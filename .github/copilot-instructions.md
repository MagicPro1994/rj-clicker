# Clean Code Standards

Apply these rules to **every piece of code you generate**, regardless of language, file type, or task size.

Detailed rules are maintained in `.github/instructions/`. Each instruction file covers a specific concern and applies automatically to all code:

- `design-principles.instructions.md` — SOLID, DRY, KISS, YAGNI, STUPID, Explicit over Implicit, Impossible States
- `architecture.instructions.md` — SoC, Law of Demeter, CQS, SLAP
- `code-style.instructions.md` — Naming, Functions, Classes, Structure, Least Astonishment, Least Privilege
- `testability.instructions.md` — Determinism, injectable side effects, no global state
- `security.instructions.md` — Input validation, injection prevention, secret handling, secure defaults
- `performance.instructions.md` — Avoid unnecessary work, efficient data structures, N+1 awareness
- `output-format.instructions.md` — Implementation + design rationale + usage example
- `commit-messages.instructions.md` — Conventional Commits format, imperative mood, type/summary/body/footer rules
