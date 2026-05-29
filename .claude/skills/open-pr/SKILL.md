---
name: open-pr
description: Open a pull request with a well-structured title, description, Jira ticket link, and verification checklist. Use whenever the user says "open a PR", "create a pull request", "ship this", "push a PR", "make a PR for this branch", or any variant of turning current branch work into a reviewable PR. Can also be invoked after implement-feature if the user wants a richer PR body. Detects the Jira issue key from the branch name automatically; if none is found it asks. Requires work to be committed and a clean or staged working tree.
---

# Open PR

Turn the current branch into a pull request — right title, structured description, Jira link, and a filled-in verification checklist. Then comment the PR URL back on the Jira ticket.

## Step 0 — Pre-flight checks

Run these before touching anything else. Stop and report if any fail.

```powershell
# 1. Not on main/master
git branch --show-current

# 2. Nothing uncommitted that should be in the PR
git status --short

# 3. At least one commit ahead of main
git rev-list --count main..HEAD
```

- If on `main`/`master`: stop — "You're on `main`. Switch to your feature branch first."
- If there are untracked or modified files: list them and ask "Should these be committed before opening the PR?"
- If zero commits ahead of main: stop — "This branch has no commits ahead of `main`."

## Step 1 — Resolve the Jira issue key

**From the branch name first.** The naming convention is `feature/<KEY>-<description>`, e.g. `feature/SEPA-42-add-product-endpoint`. Extract the key with:

```powershell
git branch --show-current
```

Parse the key as the segment matching `[A-Z]+-[0-9]+` immediately after the first `/`. Examples:
- `feature/SEPA-42-add-product` → `SEPA-42`
- `feature/OH-7-fix-auth` → `OH-7`
- `fix/SEPA-100-null-ref` → `SEPA-100`

**If no key is found in the branch name**, check the most recent commit message for the same pattern (e.g. `Add endpoint (SEPA-42)`).

**If still no key**, ask once: "What's the Jira issue key for this branch? (e.g. SEPA-42)" — don't proceed without one. If the user says there is no ticket, skip all Jira steps and note "No Jira ticket" in the PR body.

## Step 2 — Fetch the Jira ticket

1. Call `Atlassian:getAccessibleAtlassianResources` → get `cloudId`.
2. Call `Atlassian:getJiraIssue` with the resolved key.
3. Extract and store:
   - `summary` — the ticket title
   - `description` — the ticket description / acceptance criteria
   - `status.name` — current status
   - `issuetype.name` — Story, Bug, Task, etc.
   - `self` URL base → derive the browse URL: `https://<host>/browse/<KEY>`

If the fetch fails (key not found, auth error), warn the user but continue with a manually-written PR body.

## Step 3 — Analyze the diff

```powershell
git diff main...HEAD --stat
git diff main...HEAD
```

Read the full diff. From it, identify:

- **What changed** — specific endpoints added/modified, classes/functions changed, config touched, tests added
- **Why it changed** — cross-reference with the ticket's acceptance criteria; infer motivation where the diff is self-evident
- **Files in scope** — list changed files grouped by layer (domain, feature, infrastructure, tests)

Use this analysis to write the Summary section — not the ticket description verbatim.

## Step 4 — Run build and tests

Use the project's canonical commands (from `CLAUDE.md`):

```powershell
dotnet build OrderHub/OrderHub.Api/OrderHub.Api.csproj /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
dotnet test
```

Record the outcome of each command — you'll use the results to tick the checklist boxes in the PR body.

If either fails, stop: "Build/tests failed — fix before opening the PR." Show the error output.

## Step 5 — Compose the PR title

Format:
```
<Ticket summary> (<ISSUE-KEY>)
```

Rules:
- Use the Jira ticket's `summary` field verbatim as the base.
- Keep the total title under 72 characters. If the summary is long, abbreviate: drop articles, shorten noun phrases, keep the key.
- Capitalize sentence-style (first word only, proper nouns).
- Examples:
  - `Add product list endpoint (SEPA-1114)`
  - `Fix null reference on order creation (OH-7)`
  - `Implement authentication middleware (SEPA-99)`

## Step 6 — Compose the PR body

Use this exact template, filling each section from the diff analysis and ticket data:

```markdown
## Summary

<!-- What changed and why. One bullet per logical change. Reviewers verify against the diff. -->
- <specific change and motivation>
- <second change if present>

## Changes

| Layer | Files |
|---|---|
| <layer name> | `<file>`, `<file>` |

## Verification

- [<x or space>] Build passes (`dotnet build`)
- [<x or space>] Tests pass (`dotnet test`)
- [ ] Manually verified: <describe the specific action and expected result a reviewer should try>

## Jira

[<ISSUE-KEY>](<browse URL>)
**Type:** <issuetype>  **Status at PR open:** <status>

## Checklist

- [ ] No hardcoded values, secrets, or debug output
- [ ] New behaviour is covered by at least one test
- [ ] Endpoint responses match the API contract in the ticket
- [ ] No breaking changes to existing routes or response shapes (or documented below)
```

**Filling the template:**

- **Summary bullets**: write from the diff, not from the ticket. "Added `GET /products` endpoint returning paginated list" is good. "Implemented the feature" is not.
- **Changes table**: group modified files by layer (`Domain`, `Features`, `Infrastructure`, `Tests`). Omit layers with no changes.
- **Verification checkboxes**: tick build and test only if Step 4 confirmed they pass. Leave the manual verification box unchecked with a concrete description of what a reviewer should test (e.g., "Hit `POST /products` with valid payload → 201; hit with missing `name` → 400").
- **Checklist**: leave all boxes unchecked — these are for the reviewer, not pre-ticked.
- **Breaking changes section**: add only if the diff shows removed fields, changed response shapes, renamed routes, or schema migrations. Otherwise omit it entirely.

## Step 7 — Open the PR

```powershell
git push -u origin <branch-name>

gh pr create `
  --base main `
  --title "<title from Step 5>" `
  --body "<body from Step 6>"
```

Capture the returned PR URL.

If `gh` is not installed or auth fails, print the full composed title and body to the console so the user can paste it manually, and note the failure.

## Step 8 — Link back to Jira

1. Transition the ticket to **In Review**:
   - Call `Atlassian:getTransitionsForJiraIssue` to find the transition ID whose name contains "Review" (case-insensitive). Pick the first match.
   - Call `Atlassian:transitionJiraIssue` with that ID.
   - If no "Review" transition exists, skip the transition silently and note it in the output.

2. Add a comment to the ticket with `Atlassian:addCommentToJiraIssue`:
   ```
   PR opened: <PR URL>
   <One-sentence summary of what was implemented>
   ```

## Step 9 — Output summary

Print to the user:

```
PR opened: <URL>
Jira <KEY> → In Review
```

If the Jira transition or comment failed, list what succeeded and what didn't so the user can do it manually.

## Edge cases

- **Draft PR**: if the user says "draft" or "WIP", add `--draft` to the `gh pr create` call and prefix the title with `[WIP] `.
- **No Jira ticket**: skip Steps 2, 8. Write "No associated Jira ticket." in place of the Jira section.
- **PR already exists for this branch**: `gh pr create` will fail with "already exists". Run `gh pr view --web` instead to open it, and offer to update the description.
- **Force push needed**: if the remote branch has diverged (e.g., rebased locally), confirm with the user before running `git push --force-with-lease`. Never force-push without explicit confirmation.
- **Multiple Jira keys in branch name**: use the first match.
