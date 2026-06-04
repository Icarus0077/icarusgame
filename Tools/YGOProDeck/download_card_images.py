import argparse
import concurrent.futures
import json
import pathlib
import tempfile
import time
import urllib.error
import urllib.request


def load_cards(json_path):
    data = json.loads(json_path.read_text(encoding="utf-8"))
    return data["cards"]


def write_progress(progress_path, payload):
    progress_path.parent.mkdir(parents=True, exist_ok=True)
    progress_path.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")


def download_file(url, destination, timeout, retries):
    headers = {"User-Agent": "IcarusGameUnityImageDownloader/1.0"}
    last_error = ""

    for attempt in range(1, retries + 1):
        tmp_path = None
        try:
            request = urllib.request.Request(url, headers=headers)
            with urllib.request.urlopen(request, timeout=timeout) as response:
                status = getattr(response, "status", 200)
                if status < 200 or status >= 300:
                    raise urllib.error.HTTPError(url, status, "HTTP error", response.headers, None)

                with tempfile.NamedTemporaryFile(delete=False, dir=str(destination.parent), suffix=".tmp") as tmp:
                    tmp.write(response.read())
                    tmp_path = pathlib.Path(tmp.name)

            if tmp_path.stat().st_size <= 0:
                tmp_path.unlink(missing_ok=True)
                raise RuntimeError("empty response")

            tmp_path.replace(destination)
            return True, ""
        except Exception as exc:
            last_error = str(exc)
            if tmp_path is not None:
                tmp_path.unlink(missing_ok=True)
            if attempt < retries:
                time.sleep(min(10.0, 1.5 * attempt))

    return False, last_error


def main():
    parser = argparse.ArgumentParser(description="Download YGOPRODeck card images referenced by Unity JSON export.")
    parser.add_argument("--json", required=True, type=pathlib.Path)
    parser.add_argument("--out", required=True, type=pathlib.Path)
    parser.add_argument("--progress", required=True, type=pathlib.Path)
    parser.add_argument("--rate", type=float, default=6.0)
    parser.add_argument("--workers", type=int, default=4)
    parser.add_argument("--timeout", type=float, default=30.0)
    parser.add_argument("--retries", type=int, default=3)
    args = parser.parse_args()

    cards = load_cards(args.json)
    args.out.mkdir(parents=True, exist_ok=True)

    delay = 1.0 / max(0.1, args.rate)
    started = time.time()
    downloaded = 0
    skipped = 0
    failed = 0
    failures = []
    processed = 0
    last_database_id = ""
    last_submission = 0.0
    pending = {}

    def record_result(card_id, image_url, result):
        nonlocal downloaded, failed, failures, processed, last_database_id
        success, error = result
        processed += 1
        last_database_id = card_id

        if success:
            downloaded += 1
        else:
            failed += 1
            failures.append({"databaseId": card_id, "imageUrl": image_url, "error": error})
            failures = failures[-100:]

    def flush_completed(block):
        if not pending:
            return

        return_when = concurrent.futures.FIRST_COMPLETED if not block else concurrent.futures.ALL_COMPLETED
        done, _ = concurrent.futures.wait(pending, return_when=return_when)
        for future in done:
            card_id, image_url = pending.pop(future)
            record_result(card_id, image_url, future.result())

    def report_progress(status):
        if processed == 0:
            return

        elapsed = max(0.1, time.time() - started)
        write_progress(
            args.progress,
            {
                "status": status,
                "processed": processed,
                "total": len(cards),
                "downloaded": downloaded,
                "skipped": skipped,
                "failed": failed,
                "ratePerSecond": round(processed / elapsed, 2),
                "lastDatabaseId": last_database_id,
                "updatedAt": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
                "recentFailures": failures,
            },
        )

    max_workers = max(1, args.workers)
    with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as executor:
        for card in cards:
            card_id = str(card["databaseId"])
            destination = args.out / f"{card_id}.jpg"
            if destination.exists() and destination.stat().st_size > 0:
                skipped += 1
                processed += 1
                last_database_id = card_id
            else:
                wait_time = delay - (time.time() - last_submission)
                if wait_time > 0:
                    time.sleep(wait_time)

                future = executor.submit(download_file, card["imageUrl"], destination, args.timeout, args.retries)
                pending[future] = (card_id, card["imageUrl"])
                last_submission = time.time()

                if len(pending) >= max_workers * 2:
                    flush_completed(False)

            if processed > 0 and processed % 25 == 0:
                report_progress("running")

        flush_completed(True)
        report_progress("complete")

    return 0 if failed == 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
