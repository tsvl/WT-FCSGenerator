//! Parser for War Thunder datamine files.

pub mod data;
pub mod unittags;
pub mod vehicle;
pub mod weapon;

pub use unittags::{UnittagsMap, lookup_vehicle_id, parse_unittags, parse_unittags_str};
pub use vehicle::parse_vehicle;
pub use weapon::parse_weapon_module;
