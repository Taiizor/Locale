# Architecture Decision Records

This directory contains Architecture Decision Records (ADRs) for the Locale project.

## What is an ADR?

An Architecture Decision Record (ADR) is a document that captures an important architectural decision made along with its context and consequences.

## ADR Format

Each ADR follows this structure:

```markdown
# ADR-NNNN: Title

## Status
[Proposed | Accepted | Deprecated | Superseded]

## Context
What is the issue that we're seeing that is motivating this decision or change?

## Decision
What is the change that we're proposing and/or doing?

## Consequences
What becomes easier or more difficult to do because of this change?

## Alternatives Considered
What other options were evaluated?
```

## Index

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [0001](./0001-multi-format-architecture.md) | Multi-Format Architecture | Accepted | 2024-12-06 |
| [0002](./0002-format-registry-pattern.md) | Format Registry Pattern | Accepted | 2024-12-06 |
| [0003](./0003-service-layer-design.md) | Service Layer Design | Accepted | 2024-12-06 |

## Creating a New ADR

1. Copy the template from `0000-template.md`
2. Rename with next sequential number
3. Fill in all sections
4. Submit as part of your PR
5. Update this index

## Guidelines

- **Be concise** but provide enough detail for future readers
- **Explain the why**, not just the what
- **Document alternatives** that were considered
- **Update status** when decisions change
- **Link related ADRs** when one supersedes another

## References

- [ADR GitHub Organization](https://adr.github.io/)
- [Documenting Architecture Decisions](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)