---
description: Seria pomiarów — start włącza czasową ochronę systemu pomiarowego, stop ją wyłącza
argument-hint: start | stop
---
Argument: $ARGUMENTS

Blokadę plików systemu pomiarowego włącza i wyłącza hook (reaguje na Twoją wiadomość), Ty prowadzisz zapis:
- **start:** dopisz w STATUS w „Aktywne strefy czasowe”: system pomiarowy (`BenchmarkRunner.cs`, `BenchmarkInputRecorder.cs`,
  `PerformanceMonitor.cs`, `Benchmark.unity`), data, powód. Przypomnij: aktualizacje silnika/pakietów/wtyczek wstrzymane (Z11.3);
  zadania dotykające sterowania, AI lub losowości — ostrzegaj przed ich podjęciem.
- **stop:** usuń wpis ze stref czasowych, dodaj wpis w dzienniku.
