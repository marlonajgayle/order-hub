Read the Jira issue $ARGUMENTS using the Atlassian MCP tool.
Read the description and acceptance criteria for full context before writing any code.

## 1. Start

- Transition the ticket to **In Progress** via the Atlassian MCP tool.
- Create a feature branch from `main` using the naming convention:
  `feature/<ISSUE-ID>-<short-kebab-description>`
  Example: `feature/OH-42-add-product-endpoint`

## 2. Implement

- Implement the change described in the ticket.
- Follow existing patterns in the codebase — do not introduce new abstractions unless the ticket requires them.
- Run `dotnet build` and confirm it passes with zero errors.
- Run `dotnet test` and confirm all tests pass.
- Stage only relevant files and commit:
  ```
  git commit -m "<summary> (<ISSUE-ID>)"
  ```

If the build or tests fail, stop and fix the issue before proceeding.

## 3. Self-Review

Before pushing, launch a sub-agent with this exact task:

```
You are doing a pre-push self-review. Run:

  git diff main

Review every changed file and check for:

**Correctness**
- Bugs, logic errors, or unhandled edge cases
- Missing null checks or guard clauses at boundaries

**Code Quality**
- Unused imports, dead code, or references to things that don't exist
- Empty catch blocks or swallowed exceptions
- Over-engineering or unnecessary abstractions
- Hardcoded values that should be configuration

**Security**
- Exposed secrets or credentials in code
- Unsanitized user input passed to queries, commands, or file paths

**Consistency**
- Does it match existing patterns in the codebase?
- Is naming consistent with surrounding code?

Classify each issue:
- **Critical** — must fix before pushing
- **Warning** — should fix, use judgment
- **Nit** — optional improvement

If nothing is found, say "LGTM".
```

Fix every **Critical** issue. Re-run `dotnet build` and `dotnet test` after any fixes.

## 4. Ship

1. Push the branch:
   ```
   git push -u origin <branch-name>
   ```

2. Create a PR targeting `main`:
   ```
   gh pr create --base main \
     --title "<summary> (<ISSUE-ID>)" \
     --body "$(cat <<'EOF'
   ## Summary
   <bullet points describing what changed and why>

   ## Verification
   - [ ] `dotnet build` passes
   - [ ] `dotnet test` passes
   - [ ] Manually verified: <describe what you tested>

   ## Jira
   [<ISSUE-ID>](<Jira issue URL>)
   EOF
   )"
   ```

3. Transition the Jira ticket to **In Review**.

4. Add a comment on the Jira ticket with:
   - The PR URL
   - A one-sentence summary of what was implemented

If anything fails at any step, stop and report the error with full output.
