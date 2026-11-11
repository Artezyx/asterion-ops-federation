"""Evaluate observed latency/error samples against an explicit SLO policy."""
from __future__ import annotations
import argparse
import csv
import json
import math
from pathlib import Path


def evaluate(rows: list[dict[str, str]], latency_budget_ms: float = 500.0,
             availability_target: float = 0.99) -> dict:
    if not 0 < availability_target <= 1 or latency_budget_ms <= 0:
        raise ValueError("invalid policy")
    samples: list[tuple[float, int]] = []
    for index, row in enumerate(rows, 1):
        try:
            latency = float(row["latency_ms"])
            status = int(row["status_code"])
        except (KeyError, ValueError, TypeError) as exc:
            raise ValueError(f"invalid sample at row {index}") from exc
        if not math.isfinite(latency) or latency < 0 or not 100 <= status <= 599:
            raise ValueError(f"out-of-range sample at row {index}")
        samples.append((latency, status))
    if not samples:
        raise ValueError("empty observation window")
    total = len(samples)
    good = sum(status < 500 for _, status in samples)
    ordered = sorted(latency for latency, _ in samples)
    p95_index = math.ceil(0.95 * total) - 1
    p95 = round(ordered[p95_index], 3)
    availability = good / total
    remaining = max(0, math.floor((1 - availability_target) * total) - (total - good))
    return {
        "requests": total,
        "availability": round(availability, 6),
        "p95_latency_ms": p95,
        "remaining_error_budget_requests": remaining,
        "availability_met": availability >= availability_target,
        "latency_met": p95 <= latency_budget_ms,
        "alert": availability < availability_target or p95 > latency_budget_ms,
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", required=True)
    parser.add_argument("--latency-budget-ms", type=float, default=500)
    parser.add_argument("--availability-target", type=float, default=0.99)
    args = parser.parse_args()
    with Path(args.input).open(newline="", encoding="utf-8") as handle:
        data = evaluate(list(csv.DictReader(handle)), args.latency_budget_ms, args.availability_target)
    print(json.dumps(data, indent=2))
    if data["alert"]:
        raise SystemExit(2)


if __name__ == "__main__":
    main()
