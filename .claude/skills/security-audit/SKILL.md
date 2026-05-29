
---
name: security-audit
description: Use this skill whenever the user wants a security audit, vulnerability scan, or check for security issues in code or dependencies. Triggers include phrases like "security audit", "check for vulnerabilities", "scan dependencies", "find security issues", "audit this PR/repo for security", "check for CVEs", "look for hardcoded secrets", or any request to identify insecure patterns, vulnerable packages, or exposed credentials. Also use when the user mentions specific concerns like "SQL injection", "XSS", "auth bypass", "secret leakage", or "dependency vulnerabilities". This skill complements code-review-pr — use both for high-stakes PRs. Do NOT use this skill for general code review (use code-review-pr) or as a substitute for a professional pentest on production-critical systems.
---
 
# Security audit
 
Scan code and dependencies for known-vulnerable packages, dangerous patterns, exposed secrets, and common misconfigurations. Produce a prioritized report with reproducers and fixes.
 
## When to use
 
The user wants a security-focused pass — not a general review. Common triggers:
 
- "Run a security audit on this repo"
- "Check the dependencies for CVEs"
- "Scan for hardcoded secrets"
- "Audit this PR for security issues"
- "Find security vulnerabilities in `src/`"
- Concern-specific: "check for SQL injection", "is there XSS here", "look for auth bypass"
Pair this skill with `code-review-pr` for high-stakes PRs. This skill goes deeper on security; `code-review-pr` covers everything else.
 
## Scope
 
Decide the scope before scanning. Default scopes:
 
- **Repo-wide audit** — full codebase + dependency manifests.
- **PR audit** — only the diff and its surrounding context, plus dependency changes.
- **Path-scoped** — user named a directory or file pattern.
Ask if it's ambiguous. Scanning the whole repo when the user wanted just the PR wastes time and produces noise.
 
## Workflow
 
### 1. Inventory the target
 
Find what you're scanning:
 
```bash
# Languages present
find . -type f -name "*.py" -o -name "*.js" -o -name "*.ts" -o -name "*.go" \
  -o -name "*.rb" -o -name "*.java" -o -name "*.rs" 2>/dev/null | \
  sed 's/.*\.//' | sort -u
 
# Dependency manifests
ls package.json package-lock.json yarn.lock pnpm-lock.yaml \
   requirements.txt poetry.lock pyproject.toml Pipfile.lock \
   Gemfile.lock go.mod go.sum Cargo.lock pom.xml build.gradle 2>/dev/null
```
 
Note what you find — it determines which checks apply.
 
### 2. Run dependency vulnerability scanning
 
For each ecosystem present, use its native audit tool. These are authoritative; don't try to maintain your own CVE list.
 
| Ecosystem | Command | Notes |
|---|---|---|
| npm/yarn/pnpm | `npm audit --json` / `yarn audit --json` / `pnpm audit --json` | Use `--production` for runtime-only |
| Python (pip) | `pip-audit --format json` or `safety check --json` | Install if missing |
| Python (poetry) | `pip-audit -r <(poetry export -f requirements.txt)` | Export first |
| Ruby | `bundle audit check --update` | |
| Go | `govulncheck ./...` | Catches actual call paths, not just listed deps |
| Rust | `cargo audit --json` | |
| Java/Maven | `mvn dependency-check:check` | Slow; OWASP Dependency-Check |
| Multi-ecosystem | `osv-scanner --recursive .` | Good fallback when native tool unavailable |
 
Capture the output. Parse it for: package name, installed version, vulnerable version range, fixed version, CVE/advisory ID, severity, exploitability notes.
 
If a tool isn't installed, tell the user what to install rather than skipping. Don't fabricate vulnerability data.
 
### 3. Scan for hardcoded secrets
 
Use pattern-based detection. The cheapest fast pass:
 
```bash
# Common secret patterns — adjust as needed
rg -i --no-heading -n \
  -e 'aws_access_key_id\s*=\s*["\047]?AKIA[0-9A-Z]{16}' \
  -e 'aws_secret_access_key\s*=\s*["\047][A-Za-z0-9/+=]{40}["\047]' \
  -e 'AIza[0-9A-Za-z_\-]{35}' \
  -e 'ghp_[A-Za-z0-9]{36}' \
  -e 'gho_[A-Za-z0-9]{36}' \
  -e 'github_pat_[A-Za-z0-9_]{82}' \
  -e 'xox[baprs]-[A-Za-z0-9-]{10,}' \
  -e 'sk-[A-Za-z0-9]{32,}' \
  -e '-----BEGIN (RSA |EC |OPENSSH |DSA |PGP )?PRIVATE KEY-----' \
  -e 'eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}' \
  --type-not lock 2>/dev/null
```
 
For a more thorough scan, run `gitleaks` or `trufflehog`:
 
```bash
gitleaks detect --no-banner --report-format json --report-path /tmp/gitleaks.json
# or
trufflehog filesystem . --json
```
 
**Important**: also check git history, not just the current tree — a committed-then-deleted secret is still leaked. Use `gitleaks detect --log-opts="--all"` or `trufflehog git file://.`.
 
For each finding, note: file, line, secret type (heuristic), and whether it appears to be a real credential vs. a test fixture or placeholder. Don't reproduce the secret value in your report — reference it by file:line.
 
### 4. Scan for dangerous code patterns
 
Pattern matching catches the common dangerous shapes. This isn't exhaustive — Semgrep or CodeQL go deeper if available — but it's a fast first pass.
 
**Run Semgrep when available** (it covers most of the patterns below with vetted rules):
 
```bash
semgrep --config=auto --json --output=/tmp/semgrep.json .
# or focused
semgrep --config=p/security-audit --config=p/owasp-top-ten .
```
 
If Semgrep isn't available, run language-targeted `rg` searches. Below are the high-value patterns to look for, organized by category. The examples are illustrative — adapt to the languages present.
 
**SQL injection**
```bash
# String concatenation or f-strings in SQL queries
rg -n --type py 'execute\s*\(\s*f["\047]|execute\s*\(\s*["\047].*\+|cursor\.execute\([^,]*%\s' 
rg -n --type js 'query\s*\(\s*`[^`]*\$\{|query\s*\([^,]*\+\s*\w'
```
 
**Command injection / shell**
```bash
# Shelled-out commands with interpolated input
rg -n --type py 'os\.system\(|subprocess\.(call|run|Popen)\([^,]*shell\s*=\s*True'
rg -n --type js 'child_process\.(exec|execSync)\s*\(\s*[`"\047]'
rg -n 'eval\s*\(|exec\s*\('
```
 
**Path traversal**
```bash
rg -n 'open\s*\(.*\+\s*\w|fs\.readFile.*\+\s*\w|path\.join.*req\.(query|body|params)'
```
 
**SSRF**
```bash
# Outbound HTTP using request-derived URLs
rg -n 'requests\.(get|post|put)\s*\([^,]*req(uest)?\.|axios\.\w+\([^,]*req\.|fetch\s*\([^,]*req\.'
```
 
**Unsafe deserialization**
```bash
rg -n 'pickle\.loads|yaml\.load\s*\([^,]*(?!Loader)|Marshal\.load|ObjectInputStream'
```
 
**Crypto issues**
```bash
# Weak/broken algorithms
rg -ni 'md5|sha1\b|DES\b|RC4|ECB\b' --type-add 'src:*.{py,js,ts,go,rb,java,rs}' --type src
# Hardcoded crypto keys, IVs, or salts
rg -n 'IV\s*=\s*["\047]|salt\s*=\s*["\047]|key\s*=\s*b?["\047]\\x'
# Insecure random for security purposes
rg -n 'Math\.random\(\)|random\.random\(\)' 
```
 
**Auth & access control**
```bash
# New routes missing decorators — list and inspect manually
rg -n '@app\.(route|get|post|put|delete)|router\.(get|post|put|delete)' -A 3
# Missing permission checks on object access
rg -n 'def get_\w+_by_id|findById|find_by_id' -A 5
```
 
**XSS / unsafe HTML**
```bash
rg -n 'innerHTML\s*=|dangerouslySetInnerHTML|v-html|\|\s*safe\b|mark_safe|raw\s*\|\s*safe'
```
 
**CORS misconfiguration**
```bash
rg -n 'Access-Control-Allow-Origin.*\*|cors\s*\(\s*\{[^}]*origin\s*:\s*[\'"]?\*'
```
 
**Open redirects**
```bash
rg -n 'redirect\s*\(\s*request\.|res\.redirect\s*\([^,]*req\.'
```
 
**JWT mistakes**
```bash
rg -n 'algorithm\s*=\s*["\047]none["\047]|verify\s*=\s*False|verify_signature\s*:\s*false'
```
 
**Insecure transport / TLS**
```bash
rg -n 'verify\s*=\s*False|rejectUnauthorized\s*:\s*false|InsecureSkipVerify\s*:\s*true|http://(?!localhost|127\.0\.0\.1)'
```
 
**Debug/exposure**
```bash
rg -n 'DEBUG\s*=\s*True|console\.log\(.*password|console\.log\(.*token|print\(.*api_key'
```
 
### 5. Check configuration files
 
Skim these for known dangerous defaults:
 
- **Dockerfiles** — running as root, `--privileged` references, secrets in `ENV`, `ADD` of remote URLs, base images without pinned digests.
- **CI configs** (`.github/workflows/`, `.gitlab-ci.yml`) — `pull_request_target` triggers with checkouts of head SHA, secrets exposed to fork PRs, unpinned third-party actions (`uses: someone/action@main`).
- **K8s manifests** — `privileged: true`, `hostNetwork: true`, missing `securityContext`, `runAsUser: 0`, `automountServiceAccountToken` not disabled where unneeded.
- **Web framework configs** — debug modes on, `ALLOWED_HOSTS = ['*']`, permissive CSRF/CORS, default secret keys.
- **Cloud IaC** (Terraform, CloudFormation) — public S3 buckets, security groups open to `0.0.0.0/0` on non-public ports, unencrypted storage, IAM `*:*` policies.
### 6. Check the diff for newly-introduced issues
 
If scoped to a PR, run the pattern checks specifically against changed lines:
 
```bash
# Files changed in the PR
git diff --name-only origin/main...HEAD > /tmp/changed.txt
 
# Run targeted checks
xargs -a /tmp/changed.txt rg -n '<pattern>' 2>/dev/null
```
 
A new vulnerable pattern introduced by this PR is more urgent than a pre-existing one — surface it as such.
 
### 7. Severity assessment
 
Use the same three-level scheme as code review, with security-specific calibration:
 
| Severity | Criteria |
|---|---|
| **Critical** | Exploitable now, in production-reachable code: RCE, auth bypass, data exfiltration, known-vulnerable dependency with active exploitation, leaked production credential |
| **High** | Real vulnerability but exploitation requires preconditions, OR known-vulnerable dependency without confirmed exploit path |
| **Medium** | Risky pattern that's not directly exploitable, defense-in-depth gap, deprecated crypto, suspect config |
| **Low / informational** | Best-practice deviations, comments worth making, things to address opportunistically |
 
**Adjust for context.** A hardcoded secret in a test fixture is Low. A hardcoded secret in production config is Critical. The pattern is the same; the context decides.
 
**Flag false positives proactively.** If a match looks like a real issue but the surrounding code shows otherwise (e.g., the user input is validated three lines up), say so rather than reporting it.
 
### 8. Write the audit report
 
```markdown
# Security Audit — <target>
 
**Scope:** <repo-wide / PR #1234 / src/api/>
**Scan date:** 2026-05-28
**Tools used:** npm audit, gitleaks, semgrep (auto), manual pattern review
 
## Summary
<2–4 sentences. Total findings by severity. Overall posture. The most important thing the user should know first.>
 
Found **3 Critical**, **2 High**, **5 Medium**, **8 Low** issues. The most urgent items are a hardcoded production database password in `config/prod.env` (committed 3 months ago, still in git history) and an outdated `lodash@4.17.15` with known prototype-pollution CVE.
 
## Critical
 
### C1. Hardcoded production DB password — `config/prod.env:4`
**Type:** Credential exposure
**Evidence:** Line 4 contains `DB_PASSWORD=` followed by what appears to be a real password (40-char alphanumeric, not a placeholder pattern).
**Git history:** present since commit `a3f2c1d` (2026-02-14). All clones have it.
**Impact:** Anyone with read access to this repo (current or historical) has the production database password.
**Fix:**
1. **Rotate the password now** — assume it's compromised.
2. Remove the value from the file (use a secret manager reference).
3. Purge from git history via `git filter-repo` or BFG, then force-push and require teammates to re-clone.
4. Add `*.env` (or specific files) to `.gitignore` if not already.
 
### C2. Known-vulnerable dependency: `lodash@4.17.15`
**Advisory:** GHSA-jf85-cpcp-j695 (prototype pollution)
**Fixed in:** 4.17.21
**Reachability:** `lodash.merge` is called in `src/utils/config.js:88` with user-controlled input from the `/api/preferences` endpoint.
**Fix:** `pnpm up lodash@^4.17.21` then run tests.
 
## High
<Same structure>
 
## Medium
<Brief — file:line, type, suggested fix. No need for the full template at this severity.>
 
- **`src/auth/login.py:34`** — SHA-1 used for password hashing. Migrate to `argon2id` or `bcrypt` via a staged rehash on next login.
- **`src/api/files.py:67`** — `open(user_path)` without normalizing against a base directory; path traversal possible if `user_path` comes from request input.
 
## Low / informational
<One-line bullets.>
 
- 4 instances of `Math.random()` used in non-security contexts (UI animation, etc.) — safe but worth a comment.
- `.github/workflows/ci.yml` uses `actions/checkout@v3` unpinned; pin to a SHA for supply-chain safety.
 
## Dependencies summary
<Compact table of vulnerable packages.>
 
| Package | Installed | Vulnerable | Fixed in | Severity | Advisory |
|---|---|---|---|---|---|
| lodash | 4.17.15 | <4.17.21 | 4.17.21 | High | GHSA-jf85-cpcp-j695 |
| axios | 0.21.0 | <0.21.2 | 0.21.2 | High | GHSA-cph5-m8f7-6c5x |
| ... | | | | | |
 
## What looks good
<Specific things — not filler. Tells the user what to keep doing.>
 
- All endpoints under `/api/admin/` consistently use the `@require_admin` decorator.
- Secrets in CI come from GitHub OIDC — no long-lived credentials in repo.
 
## Not covered by this audit
<Be explicit about limits.>
 
- Runtime behavior (this is static analysis only).
- Business-logic flaws that don't match scanner patterns.
- Network and infrastructure layer.
- Third-party services this app depends on.
 
## Suggested next steps
1. Rotate and purge the leaked credential (C1) immediately.
2. Patch the High dependency advisories — they're one PR.
3. Schedule a follow-up for Medium items as part of normal sprint work.
```
 
### 9. Hand off
 
Ask what the user wants to do:
 
- "Want me to open Jira tickets for the Critical and High findings?" (Hand off to `plan-and-create-jira-tickets`.)
- "Want me to draft the fix PR for the dependency upgrades?"
- "Should I post the audit summary as a comment on the PR?"
For Critical findings — especially leaked credentials — recommend immediate action before anything else. Don't bundle a credential rotation into a "later" backlog.
 
## Tool installation hints
 
If a scanner isn't available, tell the user how to install it rather than skipping the check:
 
```bash
# Python
pip install pip-audit
pip install safety
 
# JS audit is built into npm/yarn/pnpm
 
# Multi-ecosystem
brew install gitleaks trufflehog osv-scanner semgrep
# or
go install github.com/google/osv-scanner/cmd/osv-scanner@latest
pip install semgrep
 
# Go
go install golang.org/x/vuln/cmd/govulncheck@latest
 
# Rust
cargo install cargo-audit
```
 
## Handling false positives
 
Pattern-based scanning produces noise. Reduce it before reporting:
 
- **Test fixtures:** secrets in `tests/`, `__fixtures__/`, `*_test.go`, etc. are usually placeholders. Verify by looking, then de-rate to Low or omit.
- **Documentation examples:** secrets in `*.md`, `docs/`, `examples/` are likely illustrative. Same handling.
- **Validated input:** if the input to a "dangerous" call is validated/escaped a few lines up, it's not exploitable. Don't report it.
- **Internal-only code paths:** an SSRF in code that only runs against localhost from a CLI is a different risk profile from an SSRF in a public API. Note the distinction.
Show your reasoning when down-rating — "this looks like SQL injection but the `db.escape()` wrapper on line 12 handles it" — so the user can verify your call.
 
## Edge cases
 
- **Repo has no manifests** (e.g., raw scripts): skip dependency scanning, focus on code patterns.
- **Monorepo with many manifests**: scan each. Group findings by service in the report.
- **Vendored dependencies** (`vendor/`, `node_modules/` checked in): scan them, but note that fixes require updating the vendoring process, not just bumping versions.
- **Generated code**: flag, but de-rate — fixes belong upstream.
- **Lockfile not updated for the manifest**: the audit reflects the lockfile (what's actually installed). Note if the manifest disagrees.
- **Findings already disclosed in security advisories file**: cross-reference `SECURITY.md` or similar — known and accepted risks shouldn't be re-reported as new.
## Anti-patterns to avoid
 
- **Reporting every regex match without verifying.** False positives destroy trust in the report. Look at each match before including it.
- **Fabricating CVE numbers or vulnerability details.** If a scanner didn't produce it, don't invent it. Cite the tool that found each issue.
- **Quoting the actual secret in the report.** Reference by file:line. The secret is already leaked; don't leak it again into the report.
- **Treating all dependency vulnerabilities as equal.** A High CVE in a dep you don't actually call is lower-risk than a Medium CVE in a hot path.
- **Skipping git history for secret scans.** A secret committed and "removed" is still in history.
- **Recommending a pentest as the only next step.** Pentests are good but slow; the user needs the fixable items shipped this sprint.
- **Boiling-the-ocean on a PR audit.** Stay scoped — pre-existing repo issues aren't this PR's problem unless they're touched by the diff.