from __future__ import annotations

import json
import sqlite3
import tempfile
import time
import unittest
from pathlib import Path

from PIL import Image

import prepare_unity_cards as pipeline


class CardImagePipelineTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name)
        self.source = self.root / "accepted"
        self.output = self.root / "unity-ready"
        (self.source / "00000-00999").mkdir(parents=True)

    def tearDown(self) -> None:
        self.temporary.cleanup()

    def run_pipeline(self, *extra: str) -> int:
        return pipeline.main(
            [
                "--source-root",
                str(self.source),
                "--output-root",
                str(self.output),
                "--workers",
                "1",
                "--settle-seconds",
                "0",
                "--thumbnail-long-edge",
                "64",
                *extra,
            ]
        )

    def test_lossless_webp_becomes_png_and_manifest_entry(self) -> None:
        source = self.source / "00000-00999" / "123.webp"
        image = Image.new("RGB", (200, 300), (24, 120, 220))
        image.save(source, format="WEBP", lossless=True, quality=100)

        self.assertEqual(0, self.run_pipeline("--strict-card-aspect"))

        full = self.output / "cards" / "00000-00999" / "123.png"
        thumbnail = self.output / "thumbnails" / "00000-00999" / "123.png"
        self.assertTrue(full.is_file())
        self.assertTrue(thumbnail.is_file())
        with Image.open(full) as converted:
            self.assertEqual("PNG", converted.format)
            self.assertEqual("RGB", converted.mode)
            self.assertEqual((200, 300), converted.size)
            self.assertEqual((24, 120, 220), converted.getpixel((50, 50)))
        with Image.open(thumbnail) as converted_thumbnail:
            self.assertEqual((43, 64), converted_thumbnail.size)

        manifest = json.loads(
            (self.output / "card-art-manifest.json").read_text(encoding="utf-8")
        )
        self.assertEqual(1, manifest["schemaVersion"])
        self.assertEqual(1, manifest["cardCount"])
        self.assertEqual(123, manifest["cards"][0]["cardId"])
        self.assertEqual("cards/00000-00999/123.png", manifest["cards"][0]["image"]["path"])

    def test_alpha_is_preserved_and_full_image_can_be_bounded(self) -> None:
        source = self.source / "00000-00999" / "456.webp"
        image = Image.new("RGBA", (400, 600), (10, 20, 30, 90))
        image.save(source, format="WEBP", lossless=True, quality=100, exact=True)

        self.assertEqual(0, self.run_pipeline("--max-long-edge", "300"))

        full = self.output / "cards" / "00000-00999" / "456.png"
        with Image.open(full) as converted:
            self.assertEqual("RGBA", converted.mode)
            self.assertEqual((200, 300), converted.size)
            self.assertEqual(90, converted.getpixel((10, 10))[3])

    def test_second_run_skips_unchanged_output(self) -> None:
        source = self.source / "00000-00999" / "789.webp"
        Image.new("RGB", (200, 300), (90, 80, 70)).save(
            source, format="WEBP", lossless=True
        )
        self.assertEqual(0, self.run_pipeline())
        full = self.output / "cards" / "00000-00999" / "789.png"
        first_mtime = full.stat().st_mtime_ns
        time.sleep(0.02)

        self.assertEqual(0, self.run_pipeline())
        self.assertEqual(first_mtime, full.stat().st_mtime_ns)

    def test_png_and_jpeg_sources_are_normalized_too(self) -> None:
        png_source = self.source / "00000-00999" / "321.png"
        jpg_source = self.source / "00000-00999" / "654.jpg"
        Image.new("RGBA", (200, 300), (1, 2, 3, 40)).save(png_source, format="PNG")
        Image.new("RGB", (200, 300), (200, 100, 50)).save(
            jpg_source, format="JPEG", quality=95
        )

        self.assertEqual(0, self.run_pipeline())

        with Image.open(self.output / "cards" / "00000-00999" / "321.png") as png:
            self.assertEqual("RGBA", png.mode)
            self.assertEqual(40, png.getpixel((20, 20))[3])
        with Image.open(self.output / "cards" / "00000-00999" / "654.png") as jpg:
            self.assertEqual("RGB", jpg.mode)
            self.assertEqual((200, 300), jpg.size)

        manifest = json.loads(
            (self.output / "card-art-manifest.json").read_text(encoding="utf-8")
        )
        self.assertEqual([321, 654], [card["cardId"] for card in manifest["cards"]])

    def test_recent_source_is_deferred_until_settled(self) -> None:
        source = self.source / "00000-00999" / "852.webp"
        Image.new("RGB", (200, 300), (20, 30, 40)).save(
            source, format="WEBP", lossless=True
        )

        result = pipeline.main(
            [
                "--source-root",
                str(self.source),
                "--output-root",
                str(self.output),
                "--workers",
                "1",
                "--settle-seconds",
                "3600",
            ]
        )

        self.assertEqual(0, result)
        self.assertFalse(
            (self.output / "cards" / "00000-00999" / "852.png").exists()
        )
        database = sqlite3.connect(self.output / ".card-image-pipeline.sqlite3")
        try:
            status, error = database.execute(
                "SELECT status, error FROM conversions WHERE source_rel = ?",
                ("00000-00999/852.webp",),
            ).fetchone()
        finally:
            database.close()
        self.assertEqual("deferred", status)
        self.assertIn("settle window", error)

    def test_corrupt_webp_is_recorded_without_partial_output(self) -> None:
        source = self.source / "00000-00999" / "999.webp"
        source.write_bytes(b"not a webp")

        self.assertEqual(2, self.run_pipeline())
        self.assertFalse(
            (self.output / "cards" / "00000-00999" / "999.png").exists()
        )
        database = sqlite3.connect(self.output / ".card-image-pipeline.sqlite3")
        try:
            status, error = database.execute(
                "SELECT status, error FROM conversions WHERE source_rel = ?",
                ("00000-00999/999.webp",),
            ).fetchone()
        finally:
            database.close()
        self.assertEqual("error", status)
        self.assertIn("UnidentifiedImageError", error)


if __name__ == "__main__":
    unittest.main()
