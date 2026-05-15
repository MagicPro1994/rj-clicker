---
description: "Use when designing or reviewing modules, classes, functions, or APIs. Enforces Separation of Concerns, Law of Demeter, CQS, and SLAP."
applyTo: "**"
---

# Architecture & Design

## Separation of Concerns (SoC)

- Each module, class, or layer must handle **one distinct concern**.
- Do not mix I/O, business logic, and presentation in the same unit.
- This is the macro-level equivalent of Single Responsibility — apply it across files and layers, not just within classes.

## Law of Demeter — Don't Talk to Strangers

- A method should only interact with: itself, its own fields, its parameters, and objects it directly creates.
- Avoid train-wreck chains: `order.getCustomer().getAddress().getCity()` — each dot is a dependency on a stranger.
- If you need data from a distant object, add a method to the closer object that retrieves it.

## Command–Query Separation (CQS)

- Every method must be **either** a command (performs an action, returns void) **or** a query (returns data, has no side effects) — never both.
- A method named `getUser()` must not modify state. A method named `save()` must not return business data.
- This makes behavior predictable and eliminates a whole class of subtle side-effect bugs.

## Single Level of Abstraction (SLAP)

- A function must operate at **one conceptual level** — do not mix high-level orchestration with low-level implementation details in the same function.
- If a function calls `processOrder()` and also increments a raw array index, extract the low-level part into a named helper.
- Readers should be able to understand a function without context-switching between abstraction levels.
