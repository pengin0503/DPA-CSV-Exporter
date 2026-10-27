# Export environment and folder tools

The user chose the two previously proposed functions instead of batch capture. Export remains a single selected DPA entry. Add a same-basename JSON sidecar containing a schema version, the CSV's UTC timestamp and game build, exporter assembly version, DPA assembly/declared mod versions, and enabled mods with name, actual PackageId, declared mod version and engine load order. Unknown metadata stays null. Read only in-memory loaded metadata on explicit export; add no monitoring, hashes or ongoing profiling hooks. Keep the CSV's 29-column schema unchanged.

Stage complete JSON and CSV before publishing either; publish JSON first and CSV last. Roll back only files created by this call on failure; never erase pre-existing exports. Incomplete metadata must not result in a success notification. This is rollback on an ordinary I/O exception, not a filesystem-wide transaction across a power loss.

Add Open folder and Copy path in addon settings, plus the reserved DPA footer when width permits. The original export button geometry and input-first patch remain intact. Narrow windows keep Export and omit optional footer actions, which remain available in settings. Open folder creates the known AnalyzerExports directory and asks the OS to open it using ProcessStartInfo/UseShellExecute; Copy path uses the confirmed Unity GUIUtility.systemCopyBuffer API. Report errors through existing feedback UI. Copy never starts a process.

Alternatives considered: stuffing environment data into CSV would break consumers; one environment file overwritten for all exports would lose provenance. The chosen paired sidecar preserves each result's context. Generic Application.OpenURL(file URI) is less precise than OS folder opening.

Verify real exporter sidecar content, timestamp/build correlation, Unicode/order/unknown versions, rollback and pre-existing-file protection, GUI action routing and narrow settings. Preserve all existing tests; compile with RimRef and publish an installable v1.0.4 after review and CI.
