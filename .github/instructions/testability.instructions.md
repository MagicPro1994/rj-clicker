---
description: "Use when generating any code. Ensures all output is unit-testable, deterministic, and free of hidden side effects."
applyTo: "**"
---

# Testability

- Functions must be **deterministic** — same inputs always produce same outputs.
- Avoid global state and hidden side effects.
- Side-effectful operations (I/O, network, time) must be **injectable or abstracted** so they can be replaced in tests.
