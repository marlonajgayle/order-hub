---
name: implement-feature
description: End-to-end workflow to implement a Jira issue — fetch the ticket, transition to In Progress, create a feature branch, write the code, run build and tests, self-review the diff, push, open a PR, transition to In Review, and comment the PR link back on the ticket. Use whenever the user wants you to "implement", "work on", "do", "ship", "pick up", or "take" a specific Jira issue (e.g., "implement-feature OH-42", "let's do PROJ-118", "ship this ticket"). Requires an issue key as input; if the user didn't provide one, ask. Stops at the first build, test, or review failure rather than pushing broken work.
---

# Implement Feature

Take a Jira issue from "To Do" through to "In Review" with a pull request linked back on the ticket.

The workflow is sequential and **fails closed**: if any step (build, test, self-review, push) fails, stop and report. Don't push broken work to keep the workflow moving — that creates more work, not less.

## Get the issue key

You need a Jira issue key like `OH-42` or `PROJ-118`. The user may have given it in their prompt; if not, ask once: "Which Jira issue should I implement?" Don't guess from context.

## 1. Read and start

1. Fetch the issue with `Atlassian:getJiraIssue` (or `Atlassian:fetch` if you only have an ARI). Read the **summary**, **description**, and any **acceptance criteria** end-to-end before opening a single file. A ticket that looks like "add an endpoint" might have acceptance criteria that change the design substantially.

2. Transition the issue to **In Progress**:
   - Call `Atlassian:getTransitionsForJiraIssue` to find the transition ID for "In Progress" (the name varies — some boards use "Start Progress", "Doing", "In Development"). Match by name; don't guess the ID.
   - Call `Atlassian:transitionJiraIssue` with that ID.

3. Create a feature branch from `main`. Naming convention:
   ```
   feature/<ISSUE-ID>-<short-kebab-description>
   ```
   Example: `feature/OH-42-add-product-endpoint`

   Derive the kebab description from the ticket summary — 3-5 words, lowercased, hyphenated, no articles. "Add a /healthz endpoint for k8s liveness" → `add-healthz-endpoint`.

   ```bash
   git checkout main && git pull
   git checkout -b feature/OH-42-add-product-endpoint
   ```

## 2. Implement

1. Implement the change described in the ticket. Follow existing patterns in the codebase — don't introduce new abstractions, libraries, or directory structures unless the ticket explicitly requires them. If you're tempted to refactor surrounding code, resist; that belongs in its own ticket.

2. Run the build. Use the project's actual build command (see "Detecting the build/test commands" below). For .NET: `dotnet build`. For Node: `npm run build`. For Python: whatever the project uses (`mypy`, `ruff check`, etc.). **The build must pass with zero errors before continuing.** Warnings: use judgment based on whether the project treats warnings as errors.

3. Run the tests. Same approach — use the project's test command. **All tests must pass.** If you added behavior, add at least one test that would fail without your change.

4. Stage only files relevant to this ticket and commit:
   ```bash
   git add <specific files>
   git commit -m "<summary> (<ISSUE-ID>)"
   ```
   The `(<ISSUE-ID>)` suffix matters — most Jira/Git integrations auto-link commits via this pattern.

**If the build or tests fail at any point: stop and fix.** Don't move on to self-review or push hoping CI will sort it out.

### Detecting the build/test commands

Check the repo root in this order:
- `CLAUDE.md` — often lists the canonical commands
- `package.json` (`scripts.build`, `scripts.test`)
- `Makefile` (`make build`, `make test`)
- `*.csproj` / `*.sln` → `dotnet build`, `dotnet test`
- `Cargo.toml` → `cargo build`, `cargo test`
- `pyproject.toml` → look for `[tool.poetry.scripts]`, `tox.ini`, or `pytest`/`ruff`/`mypy` in dev-deps
- `go.mod` → `go build ./...`, `go test ./...`

If unclear, ask the user which commands to run before doing the implementation work, not after.

## 3. Self-review

Before pushing, review the diff against `main`. If a sub-agent tool is available (e.g., the Task tool in Claude Code), spawn one with the prompt in `references/self-review-prompt.md`. If sub-agents aren't available, run the same review yourself — read the prompt, then go through every changed file against the checklist.

The review is not optional. The point is to catch the things you'd notice only after stepping away — dead imports, hardcoded values, swallowed exceptions, naming that drifts from surrounding code.

Output of the review classifies issues as:
- **Critical** — must fix before pushing
- **Warning** — should fix, use judgment
- **Nit** — optional

**Fix every Critical issue, then re-run build and tests.** Warnings: fix if cheap, note them in the PR description if not. Nits: ignore unless trivial.

If the review returns "LGTM" with nothing found, continue.

## 4. Ship

1. Push the branch:
   ```bash
   git push -u origin feature/<ISSUE-ID>-<short-kebab-description>
   ```

2. Open a PR targeting `main`. If `gh` is installed, use it; otherwise check for a connected GitHub MCP server. The PR body template lives in `references/pr-body-template.md` — fill the bracketed sections.

   ```bash
   gh pr create --base main \
     --title "<ticket summary> (<ISSUE-ID>)" \
     --body "$(cat <<'EOF'
   ## Summary
   - <bullet describing what changed and why>

   ## Verification
   - [x] Build passes
   - [x] Tests pass
   - [x] Manually verified: <what you tested>

   ## Jira
   [<ISSUE-ID>](<Jira issue URL>)
   EOF
   )"
   ```

   Capture the returned PR URL — you need it for step 4.

3. Transition the Jira ticket to **In Review** the same way you transitioned to In Progress: `getTransitionsForJiraIssue` to find the right ID (names vary: "In Review", "Code Review", "Ready for Review"), then `transitionJiraIssue`.

4. Add a comment to the ticket with `Atlassian:addCommentToJiraIssue`:
   - The PR URL
   - One sentence summarizing what was implemented

   Example:
   ```
   PR: https://github.com/org/repo/pull/123
   Added GET /products endpoint with pagination, returning 200 with the page of items or 400 for invalid query params.
   ```

## On failure

If anything fails at any step — build error, test failure, push rejected, PR creation refused, Jira transition unavailable — stop immediately. Report:
- Which step failed
- The full error output (don't summarize stack traces away)
- The current branch state and whether anything was committed

Do not attempt to "work around" by skipping steps. The user can decide whether to fix, abort, or change approach.

## Detailed references

- `references/self-review-prompt.md` — the exact prompt for the self-review sub-agent
- `references/pr-body-template.md` — PR body skeleton with notes on what to put in each section