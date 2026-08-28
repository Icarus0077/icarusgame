#!/usr/bin/env python3
"""Prepare downloaded card images for Unity without mutating the sources.

The pipeline accepts WebP, PNG and JPEG, normalizes decoded pixels to sRGB
RGB/RGBA PNG, creates a small thumbnail, records resumable state in SQLite,
and exports a versioned JSON manifest for a Unity content loader.
"""

from __future__ import annotations

import argparse
import hashlib
import io
import json
import os
import sqlite3
import sys
import threading
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Sequence

try:
    from PIL import Image, ImageCms, ImageOps, UnidentifiedImageError, features
except ImportError as error:  # pragma: no cover - exercised by deployment checks
    raise SystemExit("Pillow with WebP support is required: python -m pip install Pillow") from error


TOOL_VERSION = "1.0.0"
MANIFEST_SCHEMA_VERSION = 1
DEFAULT_EXTENSIONS = (".webp", ".png", ".jpg", ".jpeg")
CARD_ASPECT_MIN = 0.60
CARD_ASPECT_MAX = 0.75


@dataclass(frozen=True)
class PipelineOptions:
    source_root: Path
    output_root: Path
    thumbnail_long_edge: int
    max_long_edge: int
    png_compress_level: int
    strict_card_aspect: bool
    settle_seconds: float


@dataclass(frozen=True)
class ConversionTask:
    source: Path
    source_rel: str
    card_id: int
    full_rel: str
    thumbnail_rel: str | None


@dataclass
class ConversionResult:
    task: ConversionTask
    status: str
    source_size: int = 0
    source_mtime_ns: int = 0
    source_sha256: str = ""
    width: int = 0
    height: int = 0
    mode: str = ""
    has_alpha: bool = False
    full_size: int = 0
    full_sha256: str = ""
    thumbnail_width: int = 0
    thumbnail_height: int = 0
    thumbnail_size: int = 0
    thumbnail_sha256: str = ""
    warning: str = ""
    error: str = ""


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as source:
        for chunk in iter(lambda: source.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def has_alpha(image: Image.Image) -> bool:
    return "A" in image.getbands() or (image.mode == "P" and "transparency" in image.info)


def normalize_to_srgb(image: Image.Image) -> tuple[Image.Image, str]:
    image = ImageOps.exif_transpose(image)
    output_mode = "RGBA" if has_alpha(image) else "RGB"
    warning = ""
    embedded_profile = image.info.get("icc_profile")
    converted = image.convert(output_mode)
    if embedded_profile:
        try:
            source_profile = ImageCms.ImageCmsProfile(io.BytesIO(embedded_profile))
            srgb_profile = ImageCms.createProfile("sRGB")
            converted = ImageCms.profileToProfile(
                converted,
                source_profile,
                srgb_profile,
                outputMode=output_mode,
            )
        except (ImageCms.PyCMSError, OSError, ValueError) as error:
            warning = f"embedded ICC profile could not be converted: {error}"
    return converted, warning


def resized_copy(image: Image.Image, max_long_edge: int) -> Image.Image:
    if max_long_edge <= 0 or max(image.size) <= max_long_edge:
        return image.copy()
    resized = image.copy()
    resized.thumbnail((max_long_edge, max_long_edge), Image.Resampling.LANCZOS)
    return resized


def write_png_temp(image: Image.Image, destination: Path, compress_level: int) -> Path:
    destination.parent.mkdir(parents=True, exist_ok=True)
    temporary = destination.with_name(
        f".{destination.name}.tmp-{os.getpid()}-{threading.get_ident()}"
    )
    with temporary.open("wb") as output:
        image.save(output, format="PNG", compress_level=compress_level, optimize=False)
        output.flush()
        os.fsync(output.fileno())
    return temporary


def same_file_state(before: os.stat_result, after: os.stat_result) -> bool:
    return before.st_size == after.st_size and before.st_mtime_ns == after.st_mtime_ns


def convert_task(task: ConversionTask, options: PipelineOptions) -> ConversionResult:
    result = ConversionResult(task=task, status="error")
    full_destination = options.output_root / task.full_rel
    thumbnail_destination = (
        options.output_root / task.thumbnail_rel if task.thumbnail_rel else None
    )
    temporary_files: list[Path] = []
    try:
        before = task.source.stat()
        result.source_size = before.st_size
        result.source_mtime_ns = before.st_mtime_ns
        if before.st_size <= 0:
            result.status = "deferred"
            result.error = "source file is empty"
            return result
        if options.settle_seconds > 0 and time.time() - before.st_mtime < options.settle_seconds:
            result.status = "deferred"
            result.error = "source file is still inside the settle window"
            return result

        result.source_sha256 = sha256_file(task.source)
        with Image.open(task.source) as opened:
            opened.load()
            normalized, result.warning = normalize_to_srgb(opened)

        ratio = normalized.width / float(normalized.height)
        if options.strict_card_aspect and not CARD_ASPECT_MIN <= ratio <= CARD_ASPECT_MAX:
            raise ValueError(
                f"card aspect ratio {ratio:.4f} is outside "
                f"{CARD_ASPECT_MIN:.2f}-{CARD_ASPECT_MAX:.2f}"
            )

        full_image = resized_copy(normalized, options.max_long_edge)
        result.width, result.height = full_image.size
        result.mode = full_image.mode
        result.has_alpha = "A" in full_image.getbands()
        full_temp = write_png_temp(
            full_image, full_destination, options.png_compress_level
        )
        temporary_files.append(full_temp)

        thumbnail_temp = None
        if thumbnail_destination is not None:
            thumbnail = resized_copy(normalized, options.thumbnail_long_edge)
            result.thumbnail_width, result.thumbnail_height = thumbnail.size
            thumbnail_temp = write_png_temp(
                thumbnail, thumbnail_destination, options.png_compress_level
            )
            temporary_files.append(thumbnail_temp)

        after = task.source.stat()
        if not same_file_state(before, after):
            result.status = "deferred"
            result.error = "source file changed while it was being decoded"
            return result

        os.replace(full_temp, full_destination)
        temporary_files.remove(full_temp)
        if thumbnail_temp is not None and thumbnail_destination is not None:
            os.replace(thumbnail_temp, thumbnail_destination)
            temporary_files.remove(thumbnail_temp)

        result.full_size = full_destination.stat().st_size
        result.full_sha256 = sha256_file(full_destination)
        if thumbnail_destination is not None:
            result.thumbnail_size = thumbnail_destination.stat().st_size
            result.thumbnail_sha256 = sha256_file(thumbnail_destination)
        result.status = "ready"
        return result
    except (UnidentifiedImageError, OSError, ValueError) as error:
        result.error = f"{type(error).__name__}: {error}"
        return result
    finally:
        for temporary in temporary_files:
            try:
                temporary.unlink(missing_ok=True)
            except OSError:
                pass


def connect_database(path: Path) -> sqlite3.Connection:
    path.parent.mkdir(parents=True, exist_ok=True)
    connection = sqlite3.connect(path)
    connection.execute("PRAGMA journal_mode=WAL")
    connection.execute(
        """
        CREATE TABLE IF NOT EXISTS conversions (
            source_rel TEXT PRIMARY KEY,
            card_id INTEGER NOT NULL,
            source_size INTEGER NOT NULL,
            source_mtime_ns INTEGER NOT NULL,
            source_sha256 TEXT NOT NULL,
            status TEXT NOT NULL,
            full_rel TEXT NOT NULL,
            thumbnail_rel TEXT,
            width INTEGER NOT NULL,
            height INTEGER NOT NULL,
            mode TEXT NOT NULL,
            has_alpha INTEGER NOT NULL,
            full_size INTEGER NOT NULL,
            full_sha256 TEXT NOT NULL,
            thumbnail_width INTEGER NOT NULL,
            thumbnail_height INTEGER NOT NULL,
            thumbnail_size INTEGER NOT NULL,
            thumbnail_sha256 TEXT NOT NULL,
            warning TEXT NOT NULL,
            error TEXT NOT NULL,
            tool_version TEXT NOT NULL,
            processed_at TEXT NOT NULL
        )
        """
    )
    connection.commit()
    return connection


def record_result(connection: sqlite3.Connection, result: ConversionResult) -> None:
    connection.execute(
        """
        INSERT INTO conversions (
            source_rel, card_id, source_size, source_mtime_ns, source_sha256,
            status, full_rel, thumbnail_rel, width, height, mode, has_alpha,
            full_size, full_sha256, thumbnail_width, thumbnail_height,
            thumbnail_size, thumbnail_sha256, warning, error, tool_version,
            processed_at
        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ON CONFLICT(source_rel) DO UPDATE SET
            card_id=excluded.card_id,
            source_size=excluded.source_size,
            source_mtime_ns=excluded.source_mtime_ns,
            source_sha256=excluded.source_sha256,
            status=excluded.status,
            full_rel=excluded.full_rel,
            thumbnail_rel=excluded.thumbnail_rel,
            width=excluded.width,
            height=excluded.height,
            mode=excluded.mode,
            has_alpha=excluded.has_alpha,
            full_size=excluded.full_size,
            full_sha256=excluded.full_sha256,
            thumbnail_width=excluded.thumbnail_width,
            thumbnail_height=excluded.thumbnail_height,
            thumbnail_size=excluded.thumbnail_size,
            thumbnail_sha256=excluded.thumbnail_sha256,
            warning=excluded.warning,
            error=excluded.error,
            tool_version=excluded.tool_version,
            processed_at=excluded.processed_at
        """,
        (
            result.task.source_rel,
            result.task.card_id,
            result.source_size,
            result.source_mtime_ns,
            result.source_sha256,
            result.status,
            result.task.full_rel,
            result.task.thumbnail_rel,
            result.width,
            result.height,
            result.mode,
            int(result.has_alpha),
            result.full_size,
            result.full_sha256,
            result.thumbnail_width,
            result.thumbnail_height,
            result.thumbnail_size,
            result.thumbnail_sha256,
            result.warning,
            result.error,
            TOOL_VERSION,
            utc_now(),
        ),
    )
    connection.commit()


def is_current(
    connection: sqlite3.Connection,
    task: ConversionTask,
    source_stat: os.stat_result,
    output_root: Path,
) -> bool:
    row = connection.execute(
        """
        SELECT source_size, source_mtime_ns, status, full_rel, thumbnail_rel,
               tool_version
        FROM conversions WHERE source_rel = ?
        """,
        (task.source_rel,),
    ).fetchone()
    if not row:
        return False
    source_size, source_mtime_ns, status, full_rel, thumbnail_rel, tool_version = row
    return bool(
        status == "ready"
        and tool_version == TOOL_VERSION
        and source_size == source_stat.st_size
        and source_mtime_ns == source_stat.st_mtime_ns
        and (output_root / full_rel).is_file()
        and (not thumbnail_rel or (output_root / thumbnail_rel).is_file())
    )


def parse_extensions(value: str) -> tuple[str, ...]:
    parsed = []
    for item in value.split(","):
        extension = item.strip().lower()
        if not extension:
            continue
        if not extension.startswith("."):
            extension = "." + extension
        parsed.append(extension)
    if not parsed:
        raise ValueError("at least one source extension is required")
    return tuple(dict.fromkeys(parsed))


def discover_tasks(
    source_root: Path,
    extensions: Sequence[str],
    thumbnail_long_edge: int,
    limit: int,
) -> list[ConversionTask]:
    source_files = sorted(
        path
        for path in source_root.rglob("*")
        if path.is_file() and path.suffix.lower() in extensions
    )
    tasks: list[ConversionTask] = []
    seen_card_ids: dict[int, Path] = {}
    for source in source_files:
        if not source.stem.isdigit():
            raise ValueError(f"source filename must be a numeric card ID: {source}")
        card_id = int(source.stem)
        previous = seen_card_ids.get(card_id)
        if previous is not None:
            raise ValueError(
                f"duplicate card ID {card_id}: {previous} and {source}"
            )
        seen_card_ids[card_id] = source
        relative = source.relative_to(source_root)
        output_relative = relative.with_suffix(".png")
        tasks.append(
            ConversionTask(
                source=source,
                source_rel=relative.as_posix(),
                card_id=card_id,
                full_rel=(Path("cards") / output_relative).as_posix(),
                thumbnail_rel=(
                    (Path("thumbnails") / output_relative).as_posix()
                    if thumbnail_long_edge > 0
                    else None
                ),
            )
        )
        if limit > 0 and len(tasks) >= limit:
            break
    return tasks


def ensure_separate_roots(source_root: Path, output_root: Path) -> None:
    if source_root == output_root:
        raise ValueError("output root must be separate from the source root")
    try:
        output_root.relative_to(source_root)
    except ValueError:
        return
    raise ValueError("output root must not be inside the source root")


def export_manifest(
    connection: sqlite3.Connection,
    output_root: Path,
    manifest_path: Path,
) -> int:
    rows = connection.execute(
        """
        SELECT card_id, source_rel, source_sha256, full_rel, thumbnail_rel,
               width, height, mode, has_alpha, full_size, full_sha256,
               thumbnail_width, thumbnail_height, thumbnail_size,
               thumbnail_sha256
        FROM conversions
        WHERE status = 'ready'
        ORDER BY card_id
        """
    ).fetchall()
    cards = []
    for row in rows:
        (
            card_id,
            source_rel,
            source_sha256,
            full_rel,
            thumbnail_rel,
            width,
            height,
            mode,
            alpha,
            full_size,
            full_sha256,
            thumbnail_width,
            thumbnail_height,
            thumbnail_size,
            thumbnail_sha256,
        ) = row
        full_path = output_root / full_rel
        if not full_path.is_file():
            continue
        thumbnail = None
        if thumbnail_rel and (output_root / thumbnail_rel).is_file():
            thumbnail = {
                "path": thumbnail_rel,
                "width": thumbnail_width,
                "height": thumbnail_height,
                "byteCount": thumbnail_size,
                "sha256": thumbnail_sha256,
                "format": "PNG",
            }
        cards.append(
            {
                "cardId": card_id,
                "sourcePath": source_rel,
                "sourceSha256": source_sha256,
                "image": {
                    "path": full_rel,
                    "width": width,
                    "height": height,
                    "mode": mode,
                    "hasAlpha": bool(alpha),
                    "byteCount": full_size,
                    "sha256": full_sha256,
                    "format": "PNG",
                },
                "thumbnail": thumbnail,
            }
        )

    payload = {
        "schemaVersion": MANIFEST_SCHEMA_VERSION,
        "toolVersion": TOOL_VERSION,
        "generatedAt": utc_now(),
        "cardCount": len(cards),
        "cards": cards,
    }
    manifest_path.parent.mkdir(parents=True, exist_ok=True)
    temporary = manifest_path.with_name(f".{manifest_path.name}.tmp-{os.getpid()}")
    with temporary.open("w", encoding="utf-8", newline="\n") as output:
        json.dump(payload, output, ensure_ascii=False, separators=(",", ":"))
        output.write("\n")
        output.flush()
        os.fsync(output.fileno())
    os.replace(temporary, manifest_path)
    return len(cards)


def run_pipeline(args: argparse.Namespace) -> int:
    if not features.check("webp"):
        print("ERROR: this Pillow build has no WebP decoder", file=sys.stderr)
        return 2
    source_root = Path(args.source_root).expanduser().resolve()
    output_root = Path(args.output_root).expanduser().resolve()
    if not source_root.is_dir():
        print(f"ERROR: source root does not exist: {source_root}", file=sys.stderr)
        return 2
    try:
        ensure_separate_roots(source_root, output_root)
        extensions = parse_extensions(args.extensions)
        tasks = discover_tasks(
            source_root, extensions, args.thumbnail_long_edge, args.limit
        )
    except ValueError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 2

    if args.dry_run:
        print(
            json.dumps(
                {
                    "sourceRoot": str(source_root),
                    "outputRoot": str(output_root),
                    "candidateCount": len(tasks),
                    "extensions": extensions,
                },
                indent=2,
            )
        )
        return 0

    output_root.mkdir(parents=True, exist_ok=True)
    database_path = output_root / args.database_name
    manifest_path = output_root / args.manifest_name
    connection = connect_database(database_path)
    options = PipelineOptions(
        source_root=source_root,
        output_root=output_root,
        thumbnail_long_edge=args.thumbnail_long_edge,
        max_long_edge=args.max_long_edge,
        png_compress_level=args.png_compress_level,
        strict_card_aspect=args.strict_card_aspect,
        settle_seconds=args.settle_seconds,
    )

    pending: list[ConversionTask] = []
    skipped = 0
    for task in tasks:
        try:
            source_stat = task.source.stat()
        except OSError:
            pending.append(task)
            continue
        if not args.force and is_current(connection, task, source_stat, output_root):
            skipped += 1
        else:
            pending.append(task)

    counts = {"ready": 0, "deferred": 0, "error": 0, "skipped": skipped}
    if pending:
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            futures = {
                executor.submit(convert_task, task, options): task for task in pending
            }
            for completed, future in enumerate(as_completed(futures), start=1):
                result = future.result()
                record_result(connection, result)
                counts[result.status] = counts.get(result.status, 0) + 1
                if result.status == "error":
                    print(
                        f"ERROR {result.task.source_rel}: {result.error}",
                        file=sys.stderr,
                    )
                if completed % 100 == 0 or completed == len(pending):
                    print(
                        f"Processed {completed}/{len(pending)} "
                        f"(ready={counts['ready']}, deferred={counts['deferred']}, "
                        f"errors={counts['error']})"
                    )

    manifest_count = export_manifest(connection, output_root, manifest_path)
    connection.close()
    summary = {
        "sourceCount": len(tasks),
        **counts,
        "manifestCount": manifest_count,
        "manifest": str(manifest_path),
        "database": str(database_path),
    }
    print(json.dumps(summary, indent=2))
    return 2 if counts["error"] else 0


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Decode and normalize downloaded card images for Unity."
    )
    parser.add_argument("--source-root", required=True)
    parser.add_argument("--output-root", required=True)
    parser.add_argument(
        "--extensions", default=",".join(DEFAULT_EXTENSIONS),
        help="Comma-separated source extensions.",
    )
    parser.add_argument("--workers", type=int, default=min(8, os.cpu_count() or 1))
    parser.add_argument("--thumbnail-long-edge", type=int, default=256)
    parser.add_argument("--max-long-edge", type=int, default=0)
    parser.add_argument("--png-compress-level", type=int, choices=range(10), default=6)
    parser.add_argument("--settle-seconds", type=float, default=5.0)
    parser.add_argument("--strict-card-aspect", action="store_true")
    parser.add_argument("--force", action="store_true")
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--limit", type=int, default=0)
    parser.add_argument("--manifest-name", default="card-art-manifest.json")
    parser.add_argument("--database-name", default=".card-image-pipeline.sqlite3")
    return parser


def validate_arguments(parser: argparse.ArgumentParser, args: argparse.Namespace) -> None:
    if args.workers < 1 or args.workers > 64:
        parser.error("--workers must be between 1 and 64")
    if args.thumbnail_long_edge < 0:
        parser.error("--thumbnail-long-edge must be zero or greater")
    if args.max_long_edge < 0:
        parser.error("--max-long-edge must be zero or greater")
    if args.settle_seconds < 0:
        parser.error("--settle-seconds must be zero or greater")
    if args.limit < 0:
        parser.error("--limit must be zero or greater")


def main(argv: Sequence[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)
    validate_arguments(parser, args)
    return run_pipeline(args)


if __name__ == "__main__":
    raise SystemExit(main())
