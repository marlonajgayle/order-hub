---
name: create-jira-tickets
description: Create Jira tickets from a spec file produced by the create-spec skill (or any spec following its template). Use whenever the user wants to turn a spec, plan, or design doc into Jira issues — including phrases like "create Jira tickets for this spec", "make tickets from this", "ticket up this feature", "file the work in Jira", or any request to break a written plan into trackable Jira work. Handles tier selection (Story / Story+Sub-tasks / Epic+Stories), parent/child linking, blocking relationships from file overlaps, and writes the resulting issue keys back into the spec file. Accepts an optional spec file path; if none is given, searches `.ai/features/`.
---

# Create Jira Tickets

Turn a spec file into a structured set of Jira tickets — parent + children, linked correctly, sized to the spec's complexity, with blocking relationships inferred from file overlaps.

This skill is the natural follow-up to the `create-spec` skill, but works on any spec that uses the same `# Why / # What / ## Tasks (T1, T2, …)` shape.

## Resolve the spec file

The user may give you the spec path inline (e.g., "create tickets from `.ai/features/password-reset.md`"). If they did, use that path. If they didn't:

1. List files under `.ai/features/` (from the repo root, or the relevant subdirectory if you're working in a monorepo).
2. If exactly one file exists, use it and confirm: "Using `.ai/features/<file>.md` — sound right?"
3. If multiple exist, show the list and ask which one.
4. If the directory is missing or empty, tell the user and ask for a path.

Once you have a path, read the file before doing anything else. Everything downstream depends on knowing the task count and file lists.

## Resolve the Jira project

The spec doesn't say which Jira project to file under. If the user didn't name a project either:

1. Call `Atlassian:getAccessibleAtlassianResources` to get the cloudId.
2. Call `Atlassian:getVisibleJiraProjects` with `action: "create"` and show the user the list.
3. Ask which project. Don't guess — getting this wrong creates orphan tickets in the wrong board.

Cache the `cloudId` and `projectKey` for the rest of the run.

## Pick a complexity tier

Count the `### T1`, `### T2`, … headings under the spec's `## Tasks` section.

| Tasks | Tier      | Ticket structure                       |
|-------|-----------|----------------------------------------|
| 1–2   | Simple    | Single Story (no children)             |
| 3–5   | Standard  | Parent Story + one Sub-task per task   |
| 6+    | Complex   | Parent Epic + one Story per task       |

**Cross-cutting promotion**: if any task's **Files** list overlaps with another task's **Files** list, promote the feature one tier up (Simple → Standard, never below Standard). The overlap means the tasks coordinate, which the ticket structure should reflect.

State the chosen tier and the reason out loud before creating anything. The user may want to override.

## Map spec fields to Jira fields

The spec uses friendly names; the Atlassian tools use API names. Here's the mapping you'll need:

| Spec concept            | Atlassian tool & field                                          |
|-------------------------|-----------------------------------------------------------------|
| Parent ticket           | `Atlassian:createJiraIssue` with `issueTypeName: "Story"` or `"Epic"` |
| Child sub-task          | `Atlassian:createJiraIssue` with `issueTypeName: "Sub-task"` and `parent: "<parent-key>"` |
| Child story (under Epic)| `createJiraIssue` with `issueTypeName: "Story"` and `additional_fields: { "customfield_XXXXX": "<epic-key>" }` (Epic Link — confirm field ID for the project) |
| Priority                | `additional_fields: { "priority": { "name": "High" / "Medium" / "Low" } }` — confirm available names for the project |
| Label                   | `additional_fields: { "labels": ["aiauthored", "feature"] }`    |
| Blocking link           | `Atlassian:createIssueLink` with `type: "Blocks"`. The blocker is `inwardIssue`, the blocked task is `outwardIssue` |

If a project rejects an issue type, priority name, or custom field, call `Atlassian:getJiraProjectIssueTypesMetadata` (or `getJiraIssueTypeMetaWithFields`) and retry with what the project actually accepts. Don't keep retrying the same payload — read the error first.

## Create the parent ticket

| Field | Value |
|-------|-------|
| Title | The spec's `# Feature Name` heading text |
| Description | The spec's `## Why` and `## What` sections, concatenated, followed by a line: `Spec: <relative-path-to-spec-file>` |
| Type | `Story` for Simple/Standard, `Epic` for Complex |
| Priority | Infer from the spec's Constraints section: Must Have → "High" (or equivalent), Should Have → "Medium", Could Have → "Low". Default "Medium" if unstated. |
| Labels | `["aiauthored"]` |

Record the returned issue key — every child references it.

## Create the child tickets

Skip this section for the Simple tier.

For each task heading `### T<N>: <noun phrase>` in the spec:

| Field | Value |
|-------|-------|
| Title | `T<N>: <noun phrase>` exactly as written in the spec |
| Description | The task's **Do**, **Files**, and **Verify** fields verbatim, each under its own bold subhead |
| Type | `Sub-task` (Standard) or `Story` (Complex) |
| Parent link | `parent: "<parent-key>"` for Sub-tasks; Epic Link custom field for Stories under an Epic |
| Priority | Inherit from parent unless the spec explicitly differs for that task |
| Labels | Pick **one** of `feature`, `bug`, `cleanup`, `tech-debt`, `docs` based on the task's intent, plus `aiauthored` |

Create children in spec order (T1, T2, …) so issue keys increase monotonically — this makes the summary table readable.

## Resolve file overlaps → Blocks links

Compare the **Files** lists across all tasks (parsed from each task's `**Files:**` line). For every pair of tasks that share at least one file:

- **Clear case** (one task creates the file, the next modifies it; or the order is obvious from the spec): create a `Blocks` link with the earlier task as `inwardIssue` and the later as `outwardIssue`. The earlier task **blocks** the later one.
- **Ambiguous case** (same file, different methods, no clear order): don't guess. List the overlap to the user — "T2 and T4 both touch `auth/handlers.cs` — should T2 block T4, T4 block T2, or neither?" — and act on the answer.

Don't create `Blocks` links for the Simple tier (only one ticket).

## Print the summary table

After everything is created, output:

```
| ID        | Type     | Title                        | Blocks / Blocked by |
|-----------|----------|------------------------------|---------------------|
| PROJ-101  | Story    | <Feature Name>               | —                   |
| PROJ-102  | Sub-task | T1: <title>                  | Blocks PROJ-103     |
| PROJ-103  | Sub-task | T2: <title>                  | Blocked by PROJ-102 |
```

One row per ticket, parent first. Use the project's actual issue key prefix (not `PROJ-`).

## Write Jira IDs back into the spec

Update the spec file in place. For each task heading, append an HTML comment with the ticket key:

```markdown
### T1: Add password reset endpoint <!-- PROJ-102 -->
```

Also append the parent key to the `# Feature Name` heading the same way:

```markdown
# Password Reset <!-- PROJ-101 -->
```

This makes the spec self-locating later — a future agent looking at the spec can find the tickets without searching.

If the spec already has Jira comments on some headings, leave the existing ones alone and only annotate headings without one. Don't duplicate.

## Detailed examples and edge cases

For tier examples, field-mapping examples, and recovery from common Atlassian errors (issue-type rejection, missing custom field IDs, Sub-task disabled), see `references/examples.md`.