---
name: code-review
description: Conduct a senior-engineer code review focused on simplicity, clarity, consistency, and correctness. Use whenever the user asks for a review, says "review this", "look this over", "audit this code", "check my changes", "PR review", or any variant of asking for a second pair of eyes on code. Accepts an optional target (a file, directory, function, branch diff, or "the last commit"); if none given, reviews all uncommitted changes against the current branch. Outputs specific issues with file and line, fixed code where useful, or "looks good" when nothing's wrong — calibrated to flag what matters and stay quiet on what doesn't.
---

# Code Review

Review code the way a thoughtful senior engineer would: read closely, flag what matters, stay quiet on what doesn't, and produce something the author actually wants to act on.

## Decide what to review

The user may have named a target (a file path, a directory, a function name, "this branch", "the last commit", a PR URL). If they did, that's the scope.

If they didn't, default to reviewing uncommitted changes:
```bash
git diff           # unstaged
git diff --staged  # staged
```

If both are empty and there's a feature branch, fall back to `git diff main` (or `master`, or whatever the default branch is). If you're still empty-handed, ask the user what to review — don't review the whole repo by default.

State the scope you settled on in one line before starting. "Reviewing the diff against `main` (3 files)." This lets the user redirect you immediately if you picked wrong.

## Read the code

Read every line in scope before writing a single comment. Skim-and-comment produces bad reviews — you'll flag something on line 12 that's explained on line 47. If a `CLAUDE.md` exists at the repo root or in the relevant subdirectory, read it first; it often defines the conventions you'll be checking consistency against.

For each piece of code, ask:

**Simplicity** — Is this over-engineered? Could it be shorter without losing clarity? Are there abstractions that exist for one caller?

**Clarity** — Are names descriptive? Is the logic easy to follow on first read? Would a teammate joining next week understand this without asking?

**Consistency** — Does it match the patterns established in `CLAUDE.md` and the surrounding code? Is it idiomatic for the language? Does it reach for the codebase's existing utilities rather than reinventing them?

**Correctness** — Are edge cases handled (empty inputs, nulls, boundary values, concurrent access where relevant)? Any obvious bugs? Anything that will fail loudly in production for an input that wasn't in the test fixtures?

These four questions are the whole review. You're not running a security audit, a performance benchmark, or a style-guide enforcement pass unless the code is obviously failing on those axes.

## Calibrate what to say

This is the part most reviews get wrong. The bar for raising an issue is: **would a thoughtful colleague mention this?** Not: "could I find something to mention?"

Things worth raising:
- Bugs and edge cases the author missed
- Code that will be hard to maintain or extend
- Names that mislead
- Abstractions that aren't pulling their weight
- Inconsistencies that will trip up the next person

Things not worth raising:
- Personal style preferences the codebase doesn't enforce
- Micro-optimizations with no measured impact
- Suggestions to add abstraction "in case we need it later"
- Restating what the code does in a different way
- "You could also..." that doesn't make the code better, just different

If a section of code is fine, don't manufacture feedback for it. Silence on a file is a valid review outcome.

## Output

If the code is good, say so briefly and stop. One sentence is enough — "Looks good — clear, handles the edge cases I'd worry about, matches the rest of the module."

If there are issues, structure each one as:

> **`path/to/file.ext:42`** — One-line description of the issue.
>
> [One or two sentences explaining why, only if the why isn't obvious from the description.]
>
> ```language
> // fixed version, if helpful
> ```

Order issues roughly by importance — bugs and correctness first, then maintainability, then everything else. Within reason, group multiple issues in the same file together so the author can address them in one editing pass.

Include fixed code when it's faster than describing the fix in prose, or when the fix involves a non-obvious idiom. Skip it when the fix is trivially obvious from the description ("rename `tmp` to `pendingOrders`") — the author doesn't need you to write that out.

## What to avoid

- **Don't pad.** No "great work overall, just a few small things" preambles. The author can tell from the review whether the work is great.
- **Don't bucket everything.** You don't need Critical/Warning/Nit labels for every comment. If something is critical, say "this is a bug" in the description and put it first. Most reviews don't need a taxonomy.
- **Don't suggest changes just to suggest changes.** If your strongest critique is "I might have named this differently", skip it.
- **Don't review the requirements.** Whether the feature should exist is not your call — only how well the code accomplishes it.
- **Don't be exhaustive at the cost of being useful.** A review with five real issues is more useful than a review with five real issues buried under twenty stylistic preferences.