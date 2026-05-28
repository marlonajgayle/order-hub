# Self-Review Sub-Agent Prompt

Copy the block below verbatim when spawning the self-review sub-agent. If running the review yourself (no sub-agent available), use the same checklist mentally and produce the same classified output.

---

```
You are doing a pre-push self-review. Run:

  git diff main

Review every changed file and check for:

**Correctness**
- Bugs, logic errors, or unhandled edge cases
- Missing null checks or guard clauses at boundaries (function entry, external input, deserialization)
- Off-by-one errors in loops, indexing, slicing
- Race conditions in concurrent code

**Code Quality**
- Unused imports, dead code, or references to things that don't exist
- Empty catch blocks or swallowed exceptions
- Over-engineering or unnecessary abstractions
- Hardcoded values (URLs, paths, magic numbers, timeouts) that should be configuration
- Commented-out code left in
- Debug prints / console.log / TODO left in

**Security**
- Exposed secrets or credentials in code
- Unsanitized user input passed to queries, commands, or file paths (SQL injection, command injection, path traversal)
- Auth checks missing on new endpoints
- Sensitive data in logs or error messages

**Consistency**
- Does it match existing patterns in the codebase? (Check at least one sibling file.)
- Is naming consistent with surrounding code? (camelCase vs snake_case, prefixes, suffixes.)
- Does it use the codebase's existing utilities, or reinvent them?

Classify each issue:
- **Critical** — must fix before pushing (bugs, security, broken builds, dead references)
- **Warning** — should fix, use judgment (code smell, minor inconsistency, missing edge case for unlikely input)
- **Nit** — optional improvement (style preference, naming alternative)

If nothing is found, say "LGTM".

Output format:
- One section per file with issues
- Use the labels Critical / Warning / Nit before each item
- Be specific: include file path + line numbers + a one-line explanation
- Do not propose patches; just identify the issues
```

---

## Notes for the caller (not part of the sub-agent prompt)

- Always run the review against the diff with `main`, not against `HEAD~1`. The reviewer needs to see the complete set of changes for the branch, not just the last commit.
- If the diff is large (>500 lines), the reviewer may miss things. Consider asking it to focus on specific files first.
- Don't pass the ticket description to the reviewer. The review is about code quality, not requirements verification — keeping it scoped prevents the reviewer from second-guessing decisions you already made deliberately.