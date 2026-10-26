"""Analiza CSV-ów z PerformanceMonitor: statystyki per-run i agregat."""
import csv
import glob
import os
import statistics
from pathlib import Path

CSV_DIR = Path(r"C:/Users/PC/AppData/LocalLow/FabWys/Ratocalypse 2,5")
# Skipy:
# - 214721: pierwszy run z Game Over gracza
# - 222013: miał nowy nagłówek SetPass ALE jeszcze stary format przecinek-locale → wartości rozjechane
SKIP_FILES = {"perf_20261022_214721.csv", "perf_20261022_222013.csv"}

def load_run(path: Path):
    """Zwraca listę dictów z metrykami per klatka. Zwraca None dla starego formatu CSV."""
    rows = []
    with open(path, newline="") as f:
        reader = csv.DictReader(f)
        if "SetPassCalls" not in reader.fieldnames:
            return None  # stary format — pomijamy
        for row in reader:
            rows.append({
                "t": float(row["Time(s)"]),
                "avg_fps": float(row["AvgFPS"]),
                "frame_ms": float(row["FrameTime(ms)"]),
                "setpass": int(row["SetPassCalls"]),
                "tris": int(row["Triangles"]),
                "gc": int(row["GCAlloc(B)"]),
                "mem": int(row["Memory(MB)"]),
            })
    return rows

def percentile(data, p):
    data = sorted(data)
    k = (len(data) - 1) * p / 100
    f = int(k)
    c = min(f + 1, len(data) - 1)
    return data[f] + (data[c] - data[f]) * (k - f)

def summarize_run(rows):
    # Pomijamy pierwsze 60 klatek — warmup, avgFps nie napełniony
    warm_rows = rows[60:] if len(rows) > 60 else rows
    avg_fps = [r["avg_fps"] for r in warm_rows]
    frame_ms = [r["frame_ms"] for r in warm_rows]
    setpass = [r["setpass"] for r in warm_rows if r["setpass"] >= 0]
    tris = [r["tris"] for r in warm_rows if r["tris"] >= 0]
    gc = [r["gc"] for r in warm_rows if r["gc"] >= 0]
    mem = [r["mem"] for r in warm_rows]
    return {
        "n_frames": len(rows),
        "duration": rows[-1]["t"] - rows[0]["t"] if rows else 0,
        "fps_avg": statistics.mean(avg_fps),
        "fps_p1": percentile(avg_fps, 1),
        "fps_p99": percentile(avg_fps, 99),
        "frame_ms_avg": statistics.mean(frame_ms),
        "frame_ms_p99": percentile(frame_ms, 99),
        "setpass_avg": statistics.mean(setpass) if setpass else 0,
        "tris_avg": statistics.mean(tris) if tris else 0,
        "gc_avg_B": statistics.mean(gc) if gc else 0,
        "gc_total_MB": sum(gc) / (1024 * 1024) if gc else 0,
        "mem_avg": statistics.mean(mem),
    }

def main():
    files = sorted(f for f in CSV_DIR.glob("perf_*.csv") if f.name not in SKIP_FILES)
    all_summaries = []
    valid_files = []
    for f in files:
        rows = load_run(f)
        if rows is None:  # stary format
            continue
        valid_files.append(f)
        all_summaries.append(summarize_run(rows))

    print(f"Znaleziono {len(valid_files)} runów z nowym formatem CSV (łącznie {len(files)} CSV)\n")

    print(f"{'run':<24} {'frames':>6} {'FPSavg':>7} {'FPSp1':>7} {'ms_avg':>7} {'ms_p99':>7} {'setpass':>8} {'tris_M':>7} {'GC_MB':>7}")
    print("-" * 100)
    for f, s in zip(valid_files, all_summaries):
        print(f"{f.name:<24} {s['n_frames']:>6} "
              f"{s['fps_avg']:>7.2f} {s['fps_p1']:>7.2f} "
              f"{s['frame_ms_avg']:>7.2f} {s['frame_ms_p99']:>7.2f} "
              f"{s['setpass_avg']:>8.1f} {s['tris_avg']/1e6:>7.2f} {s['gc_total_MB']:>7.2f}")

    # Agregat
    print("\n=== AGREGAT PO WSZYSTKICH RUNACH ===")
    def agg(key, fmt=".2f"):
        vals = [s[key] for s in all_summaries]
        m = statistics.mean(vals)
        sd = statistics.stdev(vals) if len(vals) > 1 else 0
        return f"{m:{fmt}} ± {sd:{fmt}}"

    print(f"AvgFPS:         {agg('fps_avg')}")
    print(f"AvgFPS p1:      {agg('fps_p1')}")
    print(f"AvgFPS p99:     {agg('fps_p99')}")
    print(f"Frame ms avg:   {agg('frame_ms_avg')}")
    print(f"Frame ms p99:   {agg('frame_ms_p99')}")
    print(f"SetPass avg:    {agg('setpass_avg', '.1f')}")
    print(f"Triangles avg:  {agg('tris_avg', '.0f')} ({statistics.mean([s['tris_avg'] for s in all_summaries])/1e6:.2f}M)")
    print(f"GC alloc total: {agg('gc_total_MB')} MB / 60s")
    print(f"Memory avg:     {agg('mem_avg', '.0f')} MB")

if __name__ == "__main__":
    main()
