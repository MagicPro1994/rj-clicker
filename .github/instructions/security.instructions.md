---
description: "Use when generating any code. Enforces OWASP Top 10 awareness, input validation, secret handling, and least privilege patterns."
applyTo: "**"
---

# Security & Best Practices

## Input Validation

- Validate and sanitize all inputs at the system boundary — never trust external data.
- Reject invalid inputs early (fail fast); do not let untrusted data propagate into business logic.
- Use allowlists over denylists wherever possible.

## Injection Prevention

- Never concatenate user input into SQL queries, shell commands, or HTML. Use parameterized queries and safe APIs.
- Treat all external input as untrusted: HTTP requests, file contents, environment variables, CLI arguments.

## Secret Handling

- Never hardcode secrets, API keys, passwords, or tokens in source code.
- Read secrets from environment variables or a secrets manager — never from config files committed to source control.
- Never log sensitive values.

## Secure Defaults

- Default to the most restrictive configuration. Relax only when explicitly required.
- Disable debugging endpoints, verbose error messages, and stack traces in production code.
- Use HTTPS, encrypted storage, and secure cookie flags by default.

## Least Privilege

- Grant only the permissions required for the task. No God objects, no admin-by-default.
- Scope token and credential lifetimes to the minimum necessary.

## Dependency Awareness

- Do not introduce a dependency to solve a trivial problem.
- Prefer well-maintained, widely-used libraries over unknown ones.
- Flag use of deprecated or unmaintained packages.
