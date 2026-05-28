# PR Body Template

Use this template when calling `gh pr create --body`. Replace bracketed sections with real content.

```markdown
## Summary
- [What changed, in one bullet per logical change]
- [Why it changed — link to the ticket's motivation]

## Verification
- [x] `<build command>` passes
- [x] `<test command>` passes
- [x] Manually verified: [specific action you took and the observed result]

## Jira
[<ISSUE-ID>](<Jira issue URL>)
```

## Section guidance

**Summary** — Bullets, not paragraphs. Each bullet should be something a reviewer can verify by looking at the diff. "Added validation to X" is good; "Improved the user experience" is not.

**Verification** — Tick the boxes for things you actually ran. The "manually verified" line should describe a real action, not "tested it works". Good: "Hit POST /products with a valid payload, got 201 and the row appeared in the database. Hit with missing `name`, got 400 with the expected error message." Bad: "Tested manually."

**Jira link** — Use the format `[OH-42](https://your-org.atlassian.net/browse/OH-42)`. Get the base URL from the Jira issue's URL when you fetched it; don't construct it from memory.

## Optional sections (add only if relevant)

```markdown
## Screenshots
[For UI changes. Drag images directly into the PR body or use markdown image syntax.]

## Breaking changes
[List any API, schema, or config changes that consumers need to know about. Skip the section entirely if there are none — don't write "None".]

## Follow-ups
[Things noticed during implementation that are out of scope for this ticket. File these as separate Jira tickets and link them here.]
```

## What NOT to include

- Don't paste the full ticket description into the PR. Link to it.
- Don't list every file changed — the file tree shows that already.
- Don't write a long rationale for design decisions. Either it's obvious from the code or it belongs in code comments where future readers will see it.
- Don't apologize for code you weren't sure about. Either you're confident enough to push it, or you should fix it before pushing.