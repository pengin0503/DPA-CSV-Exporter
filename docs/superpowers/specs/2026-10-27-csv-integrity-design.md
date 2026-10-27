# DPA CSV integrity (#14, #15)

Reject incompatible required ProfileLog fields before creating output. Cache validation by runtime log type; keep real zeros, and leave unavailable optional metadata blank. Validate present optional fields as well, rather than quietly casting a changed field to a misleading default. The required DPA 1.7 fields are entries:int, average:double, self:double, max:float, total:float, calls:float, percent:float.

Keep the existing 29-column order and names. measurement_cycles uses each published row's entries, matching sample_entries. DPA does not publish the root-average value that belongs to a Logs batch. root_average_ms therefore stays empty (unknown), rather than substituting the current root average or inferring an approximation from a float ratio. FPS/TPS/search filter remain explicitly live export context. This sacrifices one previously unreliable value to preserve correct meaning without extra background hooks.

Alternatives: a publication transpiler could capture root metadata but couples the addon to DPA private IL and adds work to every calculation; additional live-value columns would change the schema. Neither is needed to close these two issues. No measured methods or DPA code are patched by this change.

Tests must execute the actual Exporter against DPA-compatible test models: every required field missing or wrong-typed, genuine zeros, optional fields absent, all supported measurements preserved, and an in-flight calculation that changes live counters before replacing its published Logs. Keep existing GUI and CSV tests. Publish 1.0.3 with installable ZIP after CI and review; close both issues through the merged PR.
