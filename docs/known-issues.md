# Known Issues & Investigation Notes

Tracking oddities uncovered during the Rust rewrite that need investigation later.

## Duplicate shells with differing `Cx` values

**Vehicles affected**: `germ_flakpanzer_IV_Ostwind`, `germ_flakpanzer_IV_Ostwind_2`, `germ_flakpanzer_IV_Ostwind_2_net`, `germ_sdkfz_6_2_flak36` (and likely others with autocannon belts).

**Symptom**: The `Data/{vehicle}.txt` files contain two `37mm_sprgr_18_belt` blocks with identical stats *except* for `Cx` (0.6 vs 0.38). The original C# code processes both and overwrites the output file, so the last one (Cx=0.38) wins.

**Possible cause**: Multiple shell types composing a belt may each carry their own `Cx`. The datamine extraction/conversion may be emitting both entries from different belt definitions. Alternatively the 0.38 may be the hardcoded default applied when `Cx` is absent from the weapon module JSON (the legacy emitter writes `proj.cx.unwrap_or(0.38)`).

**Current workaround**: Deduplicate by `output_name`, keeping the last occurrence per vehicle — matches the C# overwrite behaviour and passes the corpus test.

**TODO**: Investigate the actual datamine structure for belt weapons to determine which `Cx` is "correct" and whether we should handle belt composition differently.
