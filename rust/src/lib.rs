use godot::{init::ExtensionLibrary, prelude::gdextension};

mod perf_monitor;
mod random_table;

struct BenchmarkCsharpRsExt;

#[gdextension]
unsafe impl ExtensionLibrary for BenchmarkCsharpRsExt {}
