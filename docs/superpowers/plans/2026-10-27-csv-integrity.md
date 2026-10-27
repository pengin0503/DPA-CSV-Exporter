# CSV integrity implementation plan

Goal: Resolve #14 and #15, retain the CSV schema, and deliver installable 1.0.3.
Spec: ../specs/2026-10-27-csv-integrity-design.md

- [x] Add actual-Exporter regression tests and verify old code fails for missing/changed fields, absent optional values and asynchronous metadata contamination.
- [x] Add cached DpaLogSchema validation; use validated fields in Exporter, preserve unknown metadata as blank, and serialize cycles from row entries with an unknown root-average cell.
- [ ] Run Exporter, CSV and GUI suites plus RimRef build; review against supplied DPA source.
- [ ] Update 1.0.3 metadata, release packaging and precise CSV documentation. Register changes in a PR closing #14 and #15; merge after CI, then verify published ZIP and closed issues.

Verification: old implementation 2/20 passed, fixed implementation 20/20 passed. Added two characterization cases for already-fixed method attribution and exact game build metadata. Existing CSV and GUI suites plus RimRef build pass with zero warnings/errors.
