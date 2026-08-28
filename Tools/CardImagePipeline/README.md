# Card image pipeline

This pipeline keeps downloaded WebP/PNG/JPEG files unchanged and produces
Unity-compatible PNG files, 256-pixel thumbnails, a resumable SQLite state
database, and a versioned JSON manifest.

## Local validation

```powershell
python -m pip install -r Tools/CardImagePipeline/requirements.txt
python -m unittest discover -s Tools/CardImagePipeline -p "test_*.py" -v
```

## Convert a directory

```powershell
python Tools/CardImagePipeline/prepare_unity_cards.py `
  --source-root C:\path\to\accepted `
  --output-root C:\path\to\unity-ready `
  --workers 8 `
  --strict-card-aspect
```

The source tree is never modified. Output files are written atomically. Files
changed by a concurrently running downloader are deferred until the next run.
Unchanged successful files are skipped using the SQLite state database.

The default full-size output preserves the decoded dimensions and pixels. Set
`--max-long-edge 2048` only when a deliberate resolution cap is desired.

Expected server paths after local acceptance:

```text
source: /home/debian/official-yugioh-cards/duckduckgo-full-cards/accepted
output: /home/debian/official-yugioh-cards/unity-ready
```
