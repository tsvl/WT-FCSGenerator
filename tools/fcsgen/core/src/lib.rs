//! Core library for FCS generation.
//!
//! This crate provides the core functionality for:
//! - Parsing War Thunder datamine files
//! - Computing ballistic trajectories and penetration curves
//! - Generating sight scripts
//!
//! See the CLI crate (`fcsgen`) for the command-line interface.

pub const VERSION: &str = env!("CARGO_PKG_VERSION");

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn version_is_set() {
        assert!(!VERSION.is_empty());
    }
}
