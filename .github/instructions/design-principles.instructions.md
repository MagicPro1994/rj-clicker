---
description: "Use when generating, reviewing, or refactoring any code. Enforces SOLID, DRY, KISS, YAGNI, and avoidance of STUPID anti-patterns."
applyTo: "**"
---

# Design Principles

## SOLID

- **S** — One responsibility per class/module. One reason to change.
- **O** — Extend behavior through new code, not by modifying existing code.
- **L** — Subtypes must be substitutable for their base types without breaking correctness.
- **I** — Prefer small, focused interfaces over large general-purpose ones.
- **D** — Depend on abstractions, not concretions.

## DRY — Don't Repeat Yourself

- Extract any logic that appears more than once into a named function, method, or constant.
- Centralize business rules — the same rule must not exist in multiple places.
- Avoid copy-paste programming.

## KISS — Keep It Simple

- Prefer the simplest solution that correctly solves the problem.
- Avoid unnecessary abstractions, design patterns, or layers of indirection.
- Clever code is a liability. Clear code is an asset.

## YAGNI — You Aren't Gonna Need It

- Implement only what is **required right now**.
- Do not add hooks, flags, extension points, or generalization for hypothetical future needs.
- Remove speculative code if discovered.

## Anti-Patterns to Avoid (STUPID)

- **Singletons**: Do not introduce singletons unless the problem explicitly requires exactly one instance.
- **Tight coupling**: Classes must not depend directly on concrete implementations of other classes they don't own.
- **Untestability**: All generated code must be unit-testable. Avoid static calls, hidden I/O, and uninjected dependencies.
- **Premature optimization**: Write correct, clear code first. Never optimize without a measured bottleneck.
- **Indescriptive naming**: Names must reveal intent — see `code-style.instructions.md`.
- **Duplication**: See DRY above.

## Explicit Over Implicit

- Prefer explicit, visible behavior over hidden conventions or magic.
- If a function has a side effect, it must be obvious from the name or signature — never buried inside.
- Configuration, dependencies, and control flow must be traceable without reading multiple layers of indirection.

## Make Impossible States Impossible

- Use types, enums, and constraints to eliminate invalid states at the type level rather than defending against them at runtime.
- If a combination of values is meaningless, the data model should not allow it to exist.
- Prefer a precise type over a broad one with a comment warning about misuse.
