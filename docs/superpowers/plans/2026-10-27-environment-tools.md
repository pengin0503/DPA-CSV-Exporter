# Environment tools implementation plan

Goal: add the selected environment JSON and output-folder actions, publish installable v1.0.4.
Spec: ../specs/2026-10-27-environment-tools-design.md

- [x] Add failing actual-exporter tests for the correlated environment sidecar and paired-write behavior; add GUI tests for open/copy actions, preserved export input and narrow-window fallback.
- [x] Implement EnvironmentReport metadata capture, paired output staging/rollback, and on-demand folder actions. Keep the export API and CSV schema stable.
- [ ] Run exporter, GUI, CSV and file-failure regressions; compile with RimRef and review the complete change.
- [ ] Update docs/version/workflows for v1.0.4, register PR, merge after CI and verify published ZIP/tag/DLL.

Validation record: before implementation, sidecar test failed with missing JSON, GUI test failed with missing folder controls, paired-writer test failed with missing paired behavior. After implementation, exporter 23/23, CSV/pair, GUI/action/error/narrow-window tests pass; RimRef build has zero warnings/errors.
