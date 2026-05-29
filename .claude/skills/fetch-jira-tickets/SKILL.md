---
name: fetch-jira-tickets
description: Fetch Jira tickets by issue key or list the top 5 tickets currently assigned to you. Use whenever the user asks to "show me my tickets", "what's assigned to me", "pull up ticket <KEY>", "fetch SEPA-42", "what are my open issues", or any variant of looking up Jira work. If an issue key is provided (e.g., "fetch SEPA-42"), retrieve that specific ticket. If no key is given, return the top 5 open tickets assigned to the current user ordered by last updated.
---

# Fetch Jira Tickets

Retrieve one specific Jira ticket by key, or list the top 5 open tickets assigned to you.

## Resolve Atlassian context

Before any search or fetch, call `Atlassian:getAccessibleAtlassianResources` to get the `cloudId`. Cache it for the rest of the run — every subsequent call needs it.

## Mode selection

Look at the user's input:

- **Issue key present** (e.g., `SEPA-42`, `OH-7`) → run **Fetch by key**.
- **No key** → run **My tickets**.

If the input is ambiguous (e.g., "show me tickets" with no key), default to **My tickets**.

## Fetch by key

Call `Atlassian:getJiraIssue` with the provided issue key.

Display the result using the **Ticket card** format below.

If the issue is not found or the call returns an error, report the exact error and stop. Do not guess or search for similar keys.

## My tickets

1. Resolve the current user's account ID:
   - Call `Atlassian:atlassianUserInfo` to get the authenticated user's `accountId`.

2. Search for assigned open tickets:
   - Call `Atlassian:searchJiraIssuesUsingJql` with:
     ```
     assignee = currentUser() AND statusCategory != Done ORDER BY updated DESC
     ```
   - Limit results to **5**.
   - Request these fields: `summary`, `status`, `priority`, `issuetype`, `assignee`, `updated`, `description`, `project`.

3. If zero results are returned, say: "No open tickets assigned to you." and stop.

4. Display each result using the **Ticket card** format below.

## Ticket card format

Render each ticket as:

```
┌─ <ISSUE-KEY> ──────────────────────────────────────────┐
│ <Summary>
│
│ Project:   <Project name>
│ Type:      <Issue type>
│ Status:    <Status name>
│ Priority:  <Priority name>
│ Updated:   <updated date, formatted YYYY-MM-DD>
│
│ <First 3 lines of description, or "(no description)" if empty>
└────────────────────────────────────────────────────────┘
```

For the **My tickets** list, number each card (1 of 5, 2 of 5, …) above the top border.

Truncate long summaries at 60 characters with `…`. Truncate description previews at 200 characters with `…`.

## After displaying tickets

Offer relevant next actions based on what was shown:

- **Single ticket fetched**: "Want me to implement this ticket? (`/implement-feature <KEY>`)"
- **My tickets list**: "Want me to implement one of these? Tell me the key or number."

Only offer actions that make sense — don't offer to implement a ticket that's already In Review or Done.
