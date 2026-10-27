# Export CSV input implementation plan

Goal: Export reliably without changing the selected DPA entry; publish an installable 1.0.2 release.
Architecture: Harmony prefix reserves a footer and processes the button before DPA content. Existing C# / .NET 10 build and CSV pipeline stay in place.
Spec: ../specs/2026-10-27-export-input-design.md

- [x] Add a GUI fixture that links real ModEntry and Harmony; watch the old postfix fail when an underlying entry receives the export click.
- [x] Replace postfix with prefix(ref Rect inRect); reserve 48 pixels, draw at x+24 / yMax-44 with height 26; preserve DPA's 18px border and minimum content height of 110px.
- [x] Run GUI regression and existing CSV tests, compile against RimRef; review full diff against attached DPA source.
- [ ] Update versioned packaging and docs; CI validates the ZIP and publishes a Release asset from the exact tested source commit.
- [x] Record additional substantiated findings as separate issues, without expanding the fix.

Verification record: old postfix failed locally and in CI (run 40513059095) with stolen export click; prefix passes locally and CI (40513586662). CSV tests and public RimRef build pass without warnings. Additional findings registered as #14 and #15. One read-only review found no GUI blocker; release publication now verifies actual remote tag, published state, downloadable asset and DLL hash.
