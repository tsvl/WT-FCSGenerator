//! Integration tests for Stage 1 conversion.

use std::path::PathBuf;

use fcsgen_core::{convert_vehicle, emit_legacy_txt};

/// Get the path to the examples directory.
fn examples_dir() -> PathBuf {
    PathBuf::from(env!("CARGO_MANIFEST_DIR"))
        .parent()
        .unwrap()
        .join("examples")
        .join("2.53.0.42")
}

/// Test conversion of BMP-2M against expected output.
#[test]
fn test_bmp_2m_conversion() {
    let examples = examples_dir();
    let input_root = examples.join("input");
    let vehicle_path = input_root
        .join("aces.vromfs.bin_u")
        .join("gamedata")
        .join("units")
        .join("tankmodels")
        .join("ussr_bmp_2m.blkx");

    // Skip test if examples aren't present (they're gitignored)
    if !vehicle_path.exists() {
        eprintln!("Skipping test: examples not present at {vehicle_path:?}");
        return;
    }

    // Convert
    let data = convert_vehicle(&vehicle_path, &input_root).expect("conversion should succeed");
    let output = emit_legacy_txt(&data);

    // Load expected output
    let expected_path = examples.join("output").join("ussr_bmp_2m.txt");
    let expected = std::fs::read_to_string(&expected_path).expect("should read expected output");

    // Compare
    if output != expected {
        // Print lengths for debugging
        eprintln!("=== DEBUG ===");
        eprintln!("Expected length: {} bytes, {} chars", expected.len(), expected.chars().count());
        eprintln!("Actual length:   {} bytes, {} chars", output.len(), output.chars().count());
        eprintln!("Expected ends with newline: {}", expected.ends_with('\n'));
        eprintln!("Actual ends with newline: {}", output.ends_with('\n'));
        eprintln!("Expected last 20 bytes: {:?}", expected.as_bytes().iter().rev().take(20).rev().collect::<Vec<_>>());
        eprintln!("Actual last 20 bytes: {:?}", output.as_bytes().iter().rev().take(20).rev().collect::<Vec<_>>());

        // Print diff for debugging
        eprintln!("\n=== EXPECTED ===");
        for (i, line) in expected.lines().enumerate() {
            eprintln!("{:3}: {}", i + 1, line);
        }
        eprintln!("\n=== ACTUAL ===");
        for (i, line) in output.lines().enumerate() {
            eprintln!("{:3}: {}", i + 1, line);
        }

        // Find first difference
        for (i, (exp, act)) in expected.lines().zip(output.lines()).enumerate() {
            if exp != act {
                eprintln!("\nFirst difference at line {}:", i + 1);
                eprintln!("  Expected: {exp:?}");
                eprintln!("  Actual:   {act:?}");
                break;
            }
        }

        // Check line counts
        let exp_lines: Vec<_> = expected.lines().collect();
        let act_lines: Vec<_> = output.lines().collect();
        eprintln!("\nExpected {} lines, got {} lines", exp_lines.len(), act_lines.len());

        panic!("Output does not match expected");
    }
}
