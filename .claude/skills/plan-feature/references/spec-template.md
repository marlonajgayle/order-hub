# Spec Template

Copy the structure below into `.ai/features/<feature-slug>.md` and fill every section. Remove a section only if the guidance for that section explicitly allows it (currently only "API Contract" — drop if there are no API changes).

The fenced markdown block below is the literal content to write to the file. Replace bracketed placeholders with real content; do not leave them as-is.

````markdown
# <Feature Name>

## Why

[1–2 sentences: what problem this solves and why it matters now.]

## What

[Concrete deliverable. The single thing that will be true when this is done.]

**Acceptance criteria:**
- [ ] [Observable outcome 1]
- [ ] [Observable outcome 2]

## API Contract

> Remove this section if no API surface changes.

### `<METHOD> /api/<route>`

**Request**
```json
{
  "field": "type — description"
}
```

**Response `200`**
```json
{
  "field": "type — description"
}
```

**Error cases**
| Status | Condition |
|--------|-----------|
| 400 | [validation failure] |
| 404 | [resource not found] |
| 409 | [conflict condition] |

## Context

**Relevant files:**
- `path/to/file.cs` — [what it does and why it matters here]

**Patterns to follow:**
- [Existing convention to match, with example file path]

**Key decisions:**
- [Tech choices, libraries, or approaches already locked in]

**Open questions:**
- [Decisions still to be made during implementation — leave blank if none]

## Constraints

**Must:**
- [Required patterns or conventions]

**Must not:**
- No new packages unless specified above
- Do not modify unrelated code
- Do not refactor existing code while implementing

**Out of scope:**
- [Adjacent features explicitly not included]

## Tasks

> Each task ships independently, fits in one session, and has a clear verify step.

### T1: [Noun phrase — what gets built]

**Do:** [Specific changes — files to create, methods to add, migrations to run]

**Files:** `path/to/file.cs`, `path/to/test.cs`

**Verify:** `build` passes — or — Manual: [Click X, observe Y]

### T2: [Title]

**Do:** ...

**Files:** ...

**Verify:** ...

## Done

End-to-end verification after all tasks complete.

- [ ] `build` completes with no errors or warnings
- [ ] `test` passes
- [ ] Manual: [specific action → expected result]
- [ ] No regressions in [related area]
````

## Notes on adapting the template

The template uses .NET commands (`dotnet build`, `dotnet test`) and C# file extensions as examples. Replace these with the right commands and extensions for the project's stack:

- Node/TypeScript → `npm run build`, `npm test`, `.ts`/`.tsx`
- Python → `pytest`, `ruff check`, `.py`
- Rust → `cargo build`, `cargo test`, `.rs`
- Go → `go build ./...`, `go test ./...`, `.go`

Check the project's `CLAUDE.md`, `package.json`, `pyproject.toml`, `Cargo.toml`, or similar for the actual commands used in the codebase, and prefer those.