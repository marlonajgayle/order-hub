# Examples and Edge Cases

Worked examples for each tier and recovery patterns for common Atlassian errors. Read the section relevant to your situation; you don't need to read top-to-bottom.

## Tier examples

### Simple (1–2 tasks)

Spec excerpt:
```markdown
# Add `/healthz` Endpoint

## Why
Kubernetes needs a liveness probe.

## What
A 200 response on `GET /healthz`.

## Tasks
### T1: Add the route
**Do:** Register a new endpoint in `Startup.cs`.
**Files:** `Startup.cs`, `tests/HealthzTests.cs`
**Verify:** `curl localhost:5000/healthz` returns 200.
```

Resulting Jira:
- **PROJ-201** (Story) — "Add `/healthz` Endpoint", labels `["aiauthored"]`, description = Why + What + spec link
- No children, no Blocks links

Annotate the spec:
```markdown
# Add `/healthz` Endpoint <!-- PROJ-201 -->
```

### Standard (3–5 tasks)

Three tasks, no file overlap → Standard, no promotion. Create:
- **PROJ-310** (Story) — parent
- **PROJ-311** (Sub-task, `parent: "PROJ-310"`) — T1
- **PROJ-312** (Sub-task, `parent: "PROJ-310"`) — T2
- **PROJ-313** (Sub-task, `parent: "PROJ-310"`) — T3

If T2 and T3 both edit `UserService.cs`, add a Blocks link:
```
createIssueLink(type: "Blocks", inwardIssue: "PROJ-312", outwardIssue: "PROJ-313")
```
PROJ-312 blocks PROJ-313, i.e. T2 must ship before T3.

### Complex (6+ tasks, or promoted)

Spec has 7 tasks. Create:
- **PROJ-420** (Epic) — parent
- **PROJ-421 … PROJ-427** (Story each) — children, linked to the Epic via the Epic Link custom field

To find the Epic Link field ID for your project:
```
Atlassian:getJiraIssueTypeMetaWithFields(cloudId, projectIdOrKey, issueTypeId)
```
Look for a field named "Epic Link" or "Parent Link". Note its ID (e.g., `customfield_10014`) and pass:
```
additional_fields: { "customfield_10014": "PROJ-420" }
```

### Promotion via cross-cutting

Spec has 2 tasks, both of which list `db/migrations/` in their Files. Two tasks normally → Simple. Overlap promotes → Standard. Result: parent Story + 2 Sub-tasks (not a single Story).

## Blocks link direction — getting it right

The Atlassian API names confuse this. Memorize:
- `inwardIssue` = the **blocker** (the issue that must be done first)
- `outwardIssue` = the **blocked** (the issue waiting on it)

Read it as: "outwardIssue is blocked by inwardIssue."

So for "T1 must ship before T2":
```
createIssueLink(
  type: "Blocks",
  inwardIssue: "<T1 key>",   // blocker
  outwardIssue: "<T2 key>"   // blocked
)
```

## Recovery patterns for common errors

### "Issue type 'Sub-task' is not valid for project"

Some projects disable Sub-tasks. Call:
```
Atlassian:getJiraProjectIssueTypesMetadata(cloudId, projectIdOrKey)
```
Look at the returned types. Common substitutes: `Task` (with a parent link via Epic Link custom field), or upgrade to Complex tier and use `Story` under an `Epic`. Tell the user what you found and which substitute you're going with before retrying.

### "Field 'priority' is required" or "Priority 'High' not found"

Priority names vary per project (some use "Highest/High/Medium/Low/Lowest", some use "P1/P2/P3", some disable priority entirely). Call:
```
Atlassian:getJiraIssueTypeMetaWithFields(cloudId, projectIdOrKey, issueTypeId)
```
Look at the `priority` field's `allowedValues`. Pick the closest match to the spec's MoSCoW (Must=highest, Should=middle, Could=lowest) and retry. If priority is absent from `allowedValues`, the project doesn't use it — omit it.

### "Field 'customfield_XXXXX' cannot be set"

You guessed an Epic Link field ID that this project doesn't have. Call `getJiraIssueTypeMetaWithFields` and look at every custom field — the Epic Link is usually obvious from the name, but on team-managed projects it may be called "Parent" instead and use `parent: "<epic-key>"` like sub-tasks do. Try that.

### Created the issue with the wrong field

`Atlassian:editJiraIssue` lets you fix it without recreating. Faster than deleting and remaking, and the issue key stays the same in your summary table.

## Field mapping cheat sheet

| Spec field                     | Atlassian field in `createJiraIssue`                                |
|--------------------------------|---------------------------------------------------------------------|
| Title (from `# Feature Name`)  | `summary`                                                           |
| Description (Why + What)       | `description`                                                       |
| Type                           | `issueTypeName: "Story"` / `"Epic"` / `"Sub-task"`                 |
| Project                        | `projectKey`                                                        |
| Sub-task parent                | `parent: "<parent-key>"`                                            |
| Story-under-Epic parent        | `additional_fields: { "<epic-link-field-id>": "<epic-key>" }`       |
| Priority                       | `additional_fields: { "priority": { "name": "<name>" } }`           |
| Labels                         | `additional_fields: { "labels": ["aiauthored", "feature"] }`        |

All non-standard fields go inside `additional_fields` — that's the one parameter that accepts arbitrary Jira fields. Don't try to pass `priority` or `labels` at the top level; they'll be silently dropped.