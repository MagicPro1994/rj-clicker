---
description: "Use when writing or reviewing any code. Covers naming conventions, function design, class structure, and code organization."
applyTo: "**"
---

# Code Style

## Naming

- Use **intention-revealing names** — names must communicate purpose without requiring a comment.
- Functions should read like sentences: `calculateTotalPrice()`, not `calc()` or `doStuff()`.
- Avoid abbreviations, single-letter variables (except well-understood loop counters `i`, `j`), and generic names like `data`, `info`, `temp`, `obj`.
- Boolean names should read as assertions: `isActive`, `hasPermission`, `canRetry`.

## Functions

- Each function must do **one thing only** (Single Responsibility).
- Keep functions short — if a function needs a comment to explain what a section does, extract that section into a named function.
- Use **guard clauses** and early returns to avoid deep nesting. No more than 2 levels of nesting inside a function body.
- Parameters: prefer 0–2. More than 3 is a signal to introduce a parameter object.

## Classes and Modules

- One class = one reason to change.
- Program to **abstractions** (interfaces, protocols, abstract classes) — not concrete implementations.
- Prefer **composition over inheritance**.
- Never create unnecessary singletons. Avoid hidden global state.
- Use **dependency injection** — dependencies should be passed in, not instantiated internally.

## Code Structure

- Prefer **immutable data** — avoid mutating inputs or shared state.
- **Fail fast**: validate inputs at the boundary. Do not let invalid data propagate.
- Comments explain **why**, not **what**. If you need to explain what the code does, rewrite it to be self-explanatory.
- Keep files and classes small and focused. Split when a file grows beyond a single coherent concept.

## Principle of Least Astonishment

- Code must behave in a way that does not surprise the reader.
- If a function name implies something simple, it must not do something complex or unexpected.
- Avoid side effects in getters, unexpected mutations in helpers, or silent no-ops where an error is expected.

## Principle of Least Privilege

- Expose only what is necessary. Keep internals private.
- Default to the most restrictive visibility and relax only when required.
- A caller should not be able to access or mutate more than it needs to complete its task.
