//! Error types for FCS generation.

use std::path::PathBuf;
use thiserror::Error;

/// Result type alias using our error type.
pub type Result<T> = std::result::Result<T, ParseError>;

/// Errors that can occur during parsing or conversion.
#[derive(Debug, Error)]
pub enum ParseError {
	/// Failed to read a file.
	#[error("failed to read file {path}: {source}")]
	IoError {
		path: PathBuf,
		#[source]
		source: std::io::Error,
	},

	/// Failed to parse JSON.
	#[error("failed to parse JSON in {path}: {source}")]
	JsonError {
		path: PathBuf,
		#[source]
		source: serde_json::Error,
	},

	/// Missing required field in datamine.
	#[error("missing required field '{field}' in {context}")]
	MissingField { field: String, context: String },

	/// Invalid data format.
	#[error("invalid data format in {context}: {message}")]
	InvalidFormat { context: String, message: String },
}

impl ParseError {
	/// Create an IO error with path context.
	pub fn io(path: impl Into<PathBuf>, source: std::io::Error) -> Self {
		Self::IoError {
			path: path.into(),
			source,
		}
	}

	/// Create a JSON parse error with path context.
	pub fn json(path: impl Into<PathBuf>, source: serde_json::Error) -> Self {
		Self::JsonError {
			path: path.into(),
			source,
		}
	}
}
