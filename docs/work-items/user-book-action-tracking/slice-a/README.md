# Slice A Work Items

This folder contains implementation work items for **User Book Action Tracking (Web/API First)**, scoped to **Slice A (MVP, P0)** only.

## Scope Included
- Epic 1 — Domain + Contracts
- Epic 2 — WebApi Datasync Table + Security
- Epic 3 — Web Server Service Layer
- Epic 5 — Authorization Policy Refinement
- Epic 6 — Tests

## Execution Guide
- [Execution Order and Dependencies](./00-execution-order-and-dependencies.md)

## Work Item Index

### Epic 1 — Domain + Contracts
- [Epic 1 Task List](./epic-01-domain-contracts/epic-01-task-list.md)
- [WI-001 - Create BookUserAction entity contracts](./epic-01-domain-contracts/WI-001-create-book-user-action-entity-contracts.md)
- [WI-002 - Define action types and validation rules](./epic-01-domain-contracts/WI-002-define-action-types-and-validation-rules.md)

### Epic 2 — WebApi Datasync Table + Security
- [WI-003 - Add BookUserActions Datasync table endpoint](./epic-02-webapi-datasync-security/WI-003-add-book-user-actions-datasync-table-endpoint.md)
- [WI-004 - Add user-scoped access control provider](./epic-02-webapi-datasync-security/WI-004-add-user-scoped-access-control-provider.md)
- [WI-005 - Register Cosmos table and repository](./epic-02-webapi-datasync-security/WI-005-register-cosmos-table-and-repository.md)

### Epic 3 — Web Server Service Layer
- [WI-006 - Add server Datasync client factory and data service](./epic-03-web-server-service-layer/WI-006-add-server-datasync-client-factory-and-data-service.md)
- [WI-007 - Add web server endpoints for user book actions](./epic-03-web-server-service-layer/WI-007-add-web-server-endpoints-for-user-book-actions.md)

### Epic 5 — Authorization Policy Refinement
- [WI-010 - Formalize admin policy usage for user-scoped tracking](./epic-05-authorization-policy-refinement/WI-010-formalize-admin-policy-usage-for-user-scoped-tracking.md)

### Epic 6 — Tests
- [WI-011 - Add WebApi user-scope authorization tests](./epic-06-tests/WI-011-add-webapi-user-scope-authorization-tests.md)
- [WI-012 - Add Web service/factory/endpoint tests](./epic-06-tests/WI-012-add-web-service-factory-endpoint-tests.md)

## Shared Constraints
- Reuse existing online/offline data process conventions.
- Keep `Books` and `Authors` global/shared.
- Enforce user isolation for user-book tracking data:
  - non-admin: own data only
  - admin: cross-user access via policy
- Use existing Cosmos shared container.
