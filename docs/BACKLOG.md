# Candidate backlog (Jira-style)

| Key | Story | Acceptance criteria |
|---|---|---|
| AST-1 | Create a workflow ticket | Validates owner/title, writes audit row, returns 201 |
| AST-2 | Review workflow queue | Paginates tickets deterministically, displays state |
| AST-3 | Advance a ticket | Enforces Open -> InProgress -> Completed and version token |
| AST-4 | Release independent UI | Shell loads remote through manifest, runs on different port |
| AST-5 | Protect API | Follow-up: OIDC, role policies and cloud secrets management |

Deployment follow-up: replace `EnsureCreated()` with versioned migrations before using persistent production data.
