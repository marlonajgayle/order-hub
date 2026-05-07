# create-spec

Generate a clear, implementable specification for a feature.

## Feature

$ARGUMENTS

## Instructions

Read CLAUDE.md before starting. Then follow the two phases below.

---

### Phase 1 — Clarify (before writing anything)

If the feature description above does not answer all of the following, ask the ones that are still open. Group them in a single message and wait for answers before proceeding.

**Scope & motivation**
- What problem does this solve, and who is affected?
- What is the single concrete deliverable — how will we know it's done?
- What is explicitly out of scope?

**API surface** (skip if no API changes)
- What endpoints are added or modified? (method, route, request body, response shape)
- What validation rules apply to inputs?
- What HTTP status codes should be returned, including error cases?

**Data**
- Which entities or tables are created or changed?
- Are there migration concerns (nullable columns, default values, existing data)?

**Patterns & dependencies**
- Is there an existing feature in the codebase that this should mirror?
- Are any new packages required, or is everything in-tree?

**Constraints**
- Any hard constraints on the approach (performance, auth, backward compatibility)?

Once you have enough answers, move to Phase 2. Do not ask questions you can answer by reading the codebase.

---

### Phase 2 — Write the spec

Save to `.ai/specs/<feature-slug>.md` using the template below.

```markdown
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

**Verify:** `dotnet build` passes — or — Manual: [Click X, observe Y]

### T2: [Title]

**Do:** ...

**Files:** ...

**Verify:** ...

## Done

End-to-end verification after all tasks complete.

- [ ] `dotnet build` completes with no errors or warnings
- [ ] `dotnet test` passes
- [ ] Manual: [specific action → expected result]
- [ ] No regressions in [related area]
```

---

### Spec quality checklist (review before finishing)

- Could a new agent implement T1 with no other context? If not, add what's missing.
- Is every task independently committable?
- Does each Verify step have a specific expected result, not just "verify it works"?
- Is the API Contract complete enough that no design decisions are left to the implementer?
- Are open questions that block implementation flagged, not silently assumed?

### Output

1. Spec saved to `.ai/specs/<slug>.md`
2. To implement: open a fresh session, read `.ai/specs/<slug>.md`, and start with T1.
