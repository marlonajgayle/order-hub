# create-jira-tickets

Create Jira tickets from a spec file.

## Spec

$ARGUMENTS

## Instructions

Read the spec file at the path given above. Then follow the steps below.

---

### Complexity tiers

Count the tasks (T1, T2, …) in the spec and classify:

| Tasks | Classification | Ticket structure |
|-------|----------------|-----------------|
| 1–2   | Simple         | Single Story (no sub-tasks) |
| 3–5   | Standard       | Parent Story + one Sub-task per task |
| 6+    | Complex        | Parent Epic + one Story per task |

Cross-cutting concerns (a task that touches files already listed in another task) always promote the feature to **Standard** or higher, regardless of task count.

---

### Steps

1. **Create the parent ticket** with:
   - Title: spec `# Feature Name`
   - Description: the spec's **Why** + **What** sections, plus a link to the spec file (`Spec: .ai/specs/<slug>.md`)
   - Type: Story (Standard) or Epic (Complex)
   - Priority: Must Have = 1, Should Have = 3, Could Have = 4 — infer from spec constraints; default 3
   - Label: `aiauthored`

2. **Create child tickets** (skip for Simple):
   - One sub-task/story per task (T1, T2, …)
   - Title: task noun phrase from the spec
   - Description: the task's **Do**, **Files**, and **Verify** fields verbatim
   - Link to parent via `parentId`
   - Label: pick one of `feature`, `bug`, `cleanup`, `tech-debt`, `docs` based on task intent; also add `aiauthored`
   - Priority: inherit from parent unless the spec explicitly differs for that task

3. **Resolve file overlaps** — compare the **Files** lists across all tasks:
   - If two tasks share a file, set a `blockedBy` link so they run sequentially (earlier task blocks later one)
   - If the overlap is ambiguous (same file, different methods), flag it to the user and ask how to resolve before creating the blocking link

4. **Print a summary table** once all tickets are created:

   ```
   | ID        | Type     | Title                        | Blocks / Blocked by |
   |-----------|----------|------------------------------|---------------------|
   | PROJ-101  | Story    | <Feature Name>               | —                   |
   | PROJ-102  | Sub-task | T1: <title>                  | —                   |
   | PROJ-103  | Sub-task | T2: <title>                  | PROJ-102            |
   ```

5. **Update the spec** — annotate each task heading with its Jira ID:
   ```markdown
   ### T1: [Noun phrase] <!-- PROJ-102 -->
   ```
