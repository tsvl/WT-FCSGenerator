use std::path::Path;

fn main() {
    let version_path = Path::new(env!("CARGO_MANIFEST_DIR"))
        .join("../../..")
        .join("VERSION");

    println!("cargo:rerun-if-changed={}", version_path.display());

    let version = std::fs::read_to_string(&version_path)
        .unwrap_or_else(|e| panic!("failed to read {}: {e}", version_path.display()));

    println!("cargo:rustc-env=PROJECT_VERSION={}", version.trim());
}
