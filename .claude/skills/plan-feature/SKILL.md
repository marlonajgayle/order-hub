---
name: plan-feature
description: Generate a clear, implementable feature specification before any code is written. Use whenever the user wants to plan, scope, design, or spec out a feature — including phrases like "spec this out", "let's plan this feature", "write a design doc", "create a spec", "scope this work", or any request to think through a feature before implementing it. Also use when the user describes a new feature and you sense the work should be planned before coded. Produces a markdown spec file in `.ai/features/` with clarified scope, API contract, context, constraints, and independently-shippable tasks.
---

# Create Spec

Turn a feature idea into a clear, implementable specification that another agent (or future you) can pick up in a fresh session and execute without further context.

A good spec answers three questions before any code gets written:
1. What is the smallest, concrete thing we're delivering?
2. What does the surface look like (API, data, files touched)?
3. How will we break the work into independently-shippable tasks?

## Before you start

Read `CLAUDE.md` if one exists at the repo root or in the relevant subdirectory. It often contains conventions, tech stack details, and constraints that should shape the spec.

The workflow has two phases. Don't skip Phase 1 — a spec built on assumptions is worse than no spec at all, because it lends false confidence.

---

## Phase 1 — Clarify

Your goal here is to ask only the questions whose answers you cannot find by reading the codebase. Look first, ask second.

Group all open questions into a **single message** and wait for the user's answers before moving on. Drip-feeding questions one at a time wastes the user's attention.

The categories below are a checklist for what a complete spec needs. If the feature description already answers something, skip it.

### Scope & motivation
- What problem does this solve, and who is affected?
- What is the single concrete deliverable — how will we know it's done?
- What is explicitly out of scope?

### API surface
Skip this category entirely if no API changes are involved.
- What endpoints are added or modified? (method, route, request body, response shape)
- What validation rules apply to inputs?
- What HTTP status codes should be returned, including error cases?

### Data
- Which entities or tables are created or changed?
- Are there migration concerns (nullable columns, default values, existing data)?

### Patterns & dependencies
- Is there an existing feature in the codebase this should mirror? (If so, name the file.)
- Are any new packages required, or is everything in-tree?

### Constraints
- Any hard constraints on the approach (performance, auth, backward compatibility)?

Once you have enough answers to fill out every section of the template without guessing, move to Phase 2.

---

## Phase 2 — Write the spec

Save the spec to `.ai/features/<feature-slug>.md`. Create the directory if it doesn't exist. Use a kebab-case slug derived from the feature name (e.g., `user-password-reset.md`, not `UserPasswordReset.md` or `feature_42.md`).

Use the template in `references/spec-template.md`. Read that file before writing the spec, then fill every section. The template encodes a structure that makes specs implementable by a fresh agent — don't deviate from it without reason.

### What "good" looks like for each section

**Why** — One or two sentences. If you need a paragraph, the motivation isn't clear enough yet.

**What** — A single observable outcome, not a list of features. Acceptance criteria should be things an outside observer could verify by looking at the running system, not internal implementation details.

**API Contract** — Complete enough that the implementer makes no design decisions. Include every field, every status code, every error condition. If you find yourself writing "TBD" here, go back to Phase 1.

**Context** — Point to specific files with one-line explanations of why each matters. Avoid vague references like "the auth module" — name the file. Patterns to follow should cite an existing file as the example.

**Constraints** — "Must not" is often more useful than "must". Common entries: no new packages, don't refactor unrelated code, don't modify the public API of X.

**Tasks** — Each task ships independently and fits in one session. The Verify step must have a specific, observable expected result, not "verify it works". Good: ` test` passes, or "Click Submit, observe redirect to /confirmation". Bad: "ensure the endpoint behaves correctly".

**Done** — End-to-end checks after all tasks are complete. Should include the build, the test suite, and at least one manual end-to-end action.

---

## Quality checklist

Before handing the spec back to the user, walk through these. If any answer is "no", fix it before finishing.

- Could a fresh agent implement T1 from this spec alone, with no other conversation context? If not, what's missing from Context or T1?
- Is every task independently committable, or do some require another task to be done first in a way that's not stated?
- Does each Verify step have a specific expected result?
- Is the API Contract complete enough that no design decisions are left to the implementer?
- Are open questions that would block implementation flagged in "Open questions", not silently assumed?
- Does the slug match the feature, and is the file saved under `.ai/features/`?

## Output

After saving, tell the user:
1. The path to the spec
2. That to implement, they should open a fresh session, point it at the spec file, and start with T1

Keep this final message short — the spec itself is the deliverable.