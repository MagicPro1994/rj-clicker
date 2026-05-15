---
description: "Use when generating any code. Enforces passive performance awareness: efficient data structures, avoiding unnecessary work, and recognizing common bottlenecks."
applyTo: "**"
---

# Performance Awareness

## Core Rule

Write correct, readable code first. Only optimize when a bottleneck is measured — never speculatively.

## Avoid Unnecessary Work

- Do not perform expensive operations (I/O, network calls, heavy computation) inside loops unless unavoidable.
- Cache results of pure, expensive computations when the inputs are stable.
- Prefer lazy evaluation over eager evaluation when data may not be needed.

## Data Structures

- Choose data structures appropriate to the access pattern: O(1) lookup → hash map; ordered traversal → sorted list; uniqueness → set.
- Do not use a list where a set or map is semantically correct and more efficient.

## Database & Query Awareness

- Avoid N+1 query patterns — batch or join instead of querying inside a loop.
- Select only the columns you need; avoid `SELECT *`.
- Prefer indexed lookups; flag full-table scans on large datasets.

## Resource Management

- Release resources (file handles, connections, streams) as soon as they are no longer needed. Use RAII, `with`, `using`, or `try-finally` patterns.
- Do not hold locks longer than necessary.

## When NOT to Optimize

- Do not add caching, pooling, or complexity for hypothetical scale.
- Do not micro-optimize (bit tricks, manual loop unrolling) without a profiler result proving the need.
