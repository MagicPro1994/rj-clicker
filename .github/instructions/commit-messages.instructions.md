---
description: "Use when generating or reviewing commit messages. Enforces Conventional Commits format with consistent tone, structure, and formatting."
applyTo: "**"
---

# Commit Message Format

All generated commit messages must follow the **Conventional Commits** specification consistently.

## Structure

```
<type>(<optional scope>): <summary>

<optional body>

<optional footer>
```

## Type

Use one of the following types — no others:

| Type       | When to use |
|------------|-------------|
| `feat`     | A new feature visible to the user |
| `fix`      | A bug fix |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `docs`     | Documentation changes only |
| `style`    | Formatting, whitespace — no logic change |
| `test`     | Adding or updating tests |
| `chore`    | Build process, tooling, dependency updates |
| `perf`     | Performance improvement |
| `ci`       | CI/CD configuration changes |

## Summary Line

- Written in **imperative mood**: "add", "fix", "remove" — not "added", "fixes", "removed".
- **Lowercase** after the colon.
- **No period** at the end.
- Maximum **72 characters**.

## Body (optional)

- Separate from the summary with a **blank line**.
- Use **dash-prefixed bullet points** (`- `), one change per line.
- Each bullet written in **imperative mood**: "Add", "Fix", "Remove" — not "Added", "Fixed".
- Be specific: name the file, module, package, or behavior affected.
- Use **two-space indented sub-bullets** for closely related details.
- No period at the end of each bullet.
- Wrap lines at **72 characters**.

## Footer (optional)

- Separate from the body with a **blank line**.
- Reference issues with `Closes #<id>`, `Fixes #<id>`, or `Refs #<id>`.
- Note breaking changes as `BREAKING CHANGE: <description>`.

## Examples

```
feat(auth): add OAuth2 login support

- Add Google ID token verification via GoogleTokenVerifierService
- Add AuthService: login, refresh (with rotation), logout
  - Raw refresh tokens stored as SHA-256 hash — never persisted as plaintext
  - Rotation: old token revoked before new one issued
- Add AuthController: POST /auth/login, /auth/refresh, /auth/logout
- Add JwtAuthGuard (APP_GUARD global) — protected by default, opt-out with @Public()
- Add unit tests: AuthService (12 cases)

Closes #42
```

```
fix(api): return 404 when user is not found

- Return 404 with a structured error envelope when user lookup fails
- Remove incorrect 500 fallback in UsersController.findById
```

```
chore: update eslint to v9
```
