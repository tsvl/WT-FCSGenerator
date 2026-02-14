//! Parser for War Thunder datamine files.

pub mod data;
pub mod vehicle;
pub mod weapon;

pub use vehicle::parse_vehicle;
pub use weapon::parse_weapon_module;
