# Etherna Index

Etherna Index is the service where content uploaded to [Swarm](https://github.com/ethersphere/bee) is indexed and made accessible to all. It is an **ASP.NET Core application**: it exposes a REST API to publish, search, comment, and moderate videos, a minimal Razor Pages site for admin/moderation, and an async task engine (Hangfire) for background operations like video manifest validation. It uses MongoDB for persistence and Elasticsearch as the full-text search backend.

## Build, run, test

Target framework is **.NET 10** with `TreatWarningsAsErrors=true` and `AnalysisMode=AllEnabledByDefault` (and `Nullable=enable`, `EnableNETAnalyzers=true`) on every project — warnings break the build.

```bash
dotnet restore EthernaIndex.sln
dotnet build EthernaIndex.sln -c Release
dotnet test  EthernaIndex.sln -c Release            # runs all xUnit test projects
dotnet test test/EthernaIndex.Domain.Tests/EthernaIndex.Domain.Tests.csproj   # single project
dotnet test --filter "FullyQualifiedName~VideoTest"                           # single class
dotnet test --filter "FullyQualifiedName~VideoTest.AddVideo_WhenIsValidated"  # single test
dotnet run  --project src/EthernaIndex             # local dev server, http://localhost:42690 (https: 44357)
```

There is no frontend build step: the host serves Razor Pages plus static `wwwroot` assets (no npm, no bundler) — a plain `dotnet build` produces a runnable host.

Running the app requires a reachable **MongoDB** instance (`ConnectionStrings`: `IndexDb`, `ServiceSharedDb`, `HangfireDb`, `DataProtectionDb`), **Elasticsearch** (`Elastic:Urls`, used both by the Serilog sink and by the search indexes), the **Etherna SSO server** (`SsoServer:*` settings), and a **Swarm gateway** (`Swarm:GatewayUrl`) — see `src/EthernaIndex/appsettings.Development.json` for dev defaults, plus the `ASPNETCORE_ENVIRONMENT` variable. To develop without a real Swarm/Bee node, build the `Debug-Mockup-Swarm` solution configuration: it defines `DEBUG_MOCKUP_SWARM` in `EthernaIndex.Services`, switching `SwarmService` to in-memory mockups (`ISwarmService.SetupNewPublishedVideoManifestMockup`).

Docker: `docker build .` (uses `Dockerfile`, which also runs `dotnet test` as part of the build stage and exposes ports 80/443).

## Architecture

Five-project layered solution, plus three test projects. Root namespace is `Etherna.EthernaIndex[.<Layer>]` and the namespace mirrors the folder path under it.

- **`src/EthernaIndex.Domain`** — Pure domain layer. Aggregates live under `Models/` with the `<Name>Agg/` folder convention: `VideoAgg/` (the `VideoManifest` entity, `ValidationError`, and the versioned manifest metadata under `ManifestV1/`/`ManifestV2/` with `VideoManifestMetadataBase` as polymorphic base) and `UserAgg/` (`UserSharedInfo`); the other entities live flat under `Models/` (`Video`, `User`, `Comment`, `VideoVote`, `ManualVideoReview`, `UnsuitableVideoReport`). Base classes: `ModelBase`, `EntityModelBase`. Domain events under `Events/` (`ManifestSuccessfulValidatedEvent`, `VideoModeratedEvent`) are dispatched via `Etherna.DomainEvents`. Exposes only the `IIndexDbContext`/`ISharedDbContext` interfaces — no MongoDB types leak here.
- **`src/EthernaIndex.Persistence`** — Scrinium implementations. `IndexDbContext` (main: videos, manifests, comments, votes, users, reports, reviews) and `SharedDbContext` (user info shared with SSO and the other Etherna services), plus `ModelMaps/` (Index, SsoShared) defining how domain entities serialize. `Repositories/DomainRepository` is a generic repository that dispatches domain events on create/delete. `Serializers/` holds custom BSON `SerializerBase<T>` implementations for SwarmSdk value types (`EthAddress`, `PostageBatchId`, `SwarmAddress`, `SwarmReference`, `SwarmUri`).
- **`src/EthernaIndex.Services`** — Application services and side effects. `Domain/` holds services that orchestrate domain operations (`UserService`, `VideoService`). `Infrastructure/` holds `SwarmService`, which fetches and parses published video manifests from the Swarm gateway. `EventHandlers/` follows the `On<Event>Then<Action>Handler` convention and is auto-discovered by reflection in `ServiceCollectionExtensions.AddDomainServices` — adding a handler in this namespace registers it automatically. `Tasks/` contains the Hangfire tasks (`VideoManifestValidatorTask`, `RebuildElasticIndexesTask`) and `Queues.cs`. `Options/`/`Settings/` hold configuration objects (`SwarmOptions`, `SsoServerSettings`).
- **`src/EthernaIndex.ElasticSearch`** — Elasticsearch integration (`Elastic.Clients.Elasticsearch`). `IElasticSearchService` indexes, removes, and searches the document models under `Documents/` (`VideoDocument`, `CommentDocument`); index names are prefixed by `ElasticSearchOptions.IndexesPrefix`. Registered in DI via `AddElasticSearchServices`.
- **`src/EthernaIndex`** — ASP.NET Core host (Minimal APIs + Razor Pages). `Program.cs` wires everything: authentication against Etherna SSO, authorization policies, Scrinium (scoped db contexts over singleton engines), Hangfire (Mongo storage), Serilog → Elasticsearch, OpenAPI + Scalar API Reference, CORS, data protection. Areas: `Api` (the minimal API), `Admin` (moderation/admin pages, gated by `RequireAdministratorRolePolicy`), `Account` (login/logout). Cross-cutting host code is under `Configs/`, `Conventions/`, `Extensions/`.
- **`test/`** — xUnit + Moq: `EthernaIndex.Tests` (web project units such as the API `ExceptionHandler` mapping), `EthernaIndex.Domain.Tests` (aggregate behavior), `EthernaIndex.Persistence.Tests` (model-map deserialization tests pinning each map GUID against a stored BSON document — `IndexDbContextDeserializationTest`, `SharedDbContextDeserializationTest`), `EthernaIndex.Services.Tests` (services and tasks, e.g. `VideoManifestValidatorTaskTest`).

### API design

The API is built with **Minimal APIs**, not MVC controllers — this is the central pattern to follow when adding endpoints:

- Routes are registered in the static `IndexApiMapper` class and mounted from `Program.ConfigureApplication` via `app.MapIndexApi()`, which builds a `RouteGroupBuilder` on `/api/v0.3` tagged with the `IndexApiMarker` metadata.
- Every route delegates to the handler interface `IIndexApiHandler`, implemented by the sealed `IndexApiHandler` (registered `AddScoped` in `Program`). Handler methods return `Task<IResult>` and wrap their body in `ExceptionHandler.RunAsync(async () => { … })`, which logs the exception and maps it to the status code (400 argument/format/invalid entity type, 401 unauthorized, 404 not found, 409 duplicated manifest reference or a Mongo `TransientTransactionError` the db context couldn't retry away, 503 Swarm api errors, 500 anything else).
- JSON serialization is centralized in `Program` via `ConfigureHttpJsonOptions` (camelCase + the SwarmSdk JSON converters) — handlers return plain `Results.Json(dto)`/`Results.Ok()` with no per-call serializer options (the old `IndexV03JsonSerializerOptions` was removed; don't reintroduce per-endpoint options).
- DTOs use the `Dto` suffix (`Areas/Api/DtoModels/`), request bodies use the `Input` suffix (`Areas/Api/InputModels/`). A breaking change to an endpoint gets a new numbered DTO/endpoint (`VideoDto` → `Video2Dto`, `VideoCreateInput` → `VideoCreateInput2`); the superseded handler method stays with an `_old`/`_old2` suffix and `.IsDeprecated(...)` route metadata instead of being removed.
- There is a single OpenAPI document, `index03`, filtered to the marker via `MetadataFilterDocumentTransformer<IndexApiMarker>`, served to Scalar (with OAuth2 against SSO). Transformers live in `Configs/OpenApi/`.

### Key cross-cutting points

- **Search flows through Elasticsearch, not MongoDB.** Search endpoints query `IElasticSearchService`; the ES documents are kept in sync with the domain exclusively by event handlers (index on `ManifestSuccessfulValidatedEvent` and comment creation; remove on video deletion/moderation). `RebuildElasticIndexesTask` rebuilds the indexes from MongoDB (triggered by `POST /api/v0.3/search/rebuild`).
- **Manifest validation is asynchronous.** Publishing a video (or hitting `PUT system/validate/…`) enqueues `VideoManifestValidatorTask` on the `METADATA_VIDEO_VALIDATOR` queue; the task fetches the manifest from the Swarm gateway, validates it, records `ValidationError` entries on failure, and raises `ManifestSuccessfulValidatedEvent` on success. Don't assume a just-created manifest is valid or searchable.
- **Scrinium change tracking is snapshot based, with no annotations.** Loaded entities are proxies generated at compile time by the Scrinium source generator (shipped with the `Etherna.Scrinium.Core` package referenced by the Domain project): keep every property and method of a model `virtual`, so the proxy can intercept it for lazy loading and change candidate marking. On save, only the members whose serialization differs from the loaded document are written.
- **Db contexts are scoped.** Every consumer must be scoped or transient: a singleton capturing a db context pins it (and its identity map) for the process lifetime, and scope validation is on in every environment, so it throws at resolve. Startup code touching a db context resolves it from a scope (see `ApplicationBuilderExtensions.CreateElasticIndexes`). Entity models are always referenced, never embedded: a member of an entity type must be serialized with a reference serializer in every schema (secondary ones included), or the engine build fails.
- **Reference delete policies replace cascade handlers.** The reference serializer factories in `ModelMaps/Index/` (`VideoMap.ReferenceSerializer`, `VideoManifestMap.ReferenceSerializer`/`PreviewInfoSerializer`) take an explicit `OriginDeleteMode`, declaring what happens to the referencing documents when the referenced video or manifest is deleted through its repository (propagated in background by Scrinium's `DeleteDocDependenciesTask`): comments and votes cascade (`DeleteReferencingDocument`), moderation reports and reviews keep their reference (`KeepReference`), the manifest references of a video are removed (`RemoveReference`). Declare the policy explicitly for every new reference to a deletable entity: the library default (`RemoveReference`) writes `null` into a non-nullable member.
- **Model map IDs are fresh random GUIDs.** Every `AddModelMap<T>("<guid>")` call in `ModelMaps/` needs a brand-new, randomly generated GUID (e.g. `uuidgen`) that collides with no existing map ID anywhere in the solution — never copy, edit, or hand-craft one. The ID permanently identifies that schema version; a collision silently corrupts serialization. When you add or change a map, add a matching deserialization test in `EthernaIndex.Persistence.Tests` that pins the GUID against a sample document.
- **Queries and indexes match documents as stored, not as the maps repair them.** A secondary schema's fix function shapes an old document only in memory: `VideoManifestMap` hoists `ManifestReference` out of the `ManifestHash` / `Manifest.Hash` elements of the documents written before v0.3.15, but a filter on `ManifestReference` (the admin manifest pages, `videos/manifest/{reference}`, the validation endpoints) and the unique index on it never see those documents. Renaming a queried or indexed member therefore needs the documents physically on the active schema: `IndexDbContext.DocumentMigrationList` declares the `videoManifests` rewrite, and a migration runs only at the seeding of a new database or on demand from the Scrinium dashboard (`/admin/db`, dry run first), where it also drops the old indexes and builds the current ones.
- **Index definitions are strongly typed.** In `IndexDbContext`/`SharedDbContext` index builders, always select fields with lambda expressions, never magic strings — this keeps compile-time safety against renames.
- **Domain events fire from persistence.** `DomainRepository` dispatches `EntityCreatedEvent`/`EntityDeletedEvent` (plus custom events) on create/delete; handlers in `Services/EventHandlers/` are auto-discovered by reflection — no manual registration.
- **Hangfire queues and tasks.** Queues are declared in `Services/Tasks/Queues.cs` (`DB_MAINTENANCE`, `ELASTIC_SEARCH_MAINTENANCE`, `METADATA_VIDEO_VALIDATOR`) and pinned in `Program.AddHangfireServer` alongside `"default"`. The Hangfire **server is not started in Staging** (see the `!env.IsStaging()` guard). There are no recurring jobs: all tasks are enqueued on demand via `IBackgroundJobClient`. Dashboards: Hangfire at `/admin/hangfire`, Scrinium at `/admin/db` (both admin-gated).
- **Hangfire tasks get their own db scope and execution contexts.** Each job runs in its own DI scope (a fresh db context: no stale models shared across jobs) with an async-local execution context opened by the Scrinium Hangfire filter, plus the one of the Etherna.DomainEvents library opened by `Configs/Hangfire/DomainEventsExecutionContextFilter` (its event dispatcher throws `ExecutionContextNotFoundException` without it, and a job inherits none from a request), so there is nothing to open by hand in a task. A task cursoring over a whole collection opens a transient models scope per cursor batch (`dbContext.StartTransientModelsScope()`, see the Elastic reindex tasks): the batch and what it preloads are evicted at the scope end, so the identity map doesn't grow with the collection, while the models stay tracked for the explicit preloads (the no-cache serializer modifier would break them: no-cache models never merge a load).
- **Implicit lazy loads throw.** Both db contexts run with `ImplicitLazyLoad = ReactionMode.Throw` (`Program.cs`): reading a member a summary doesn't carry throws `ScriniumLazyLoadingException` instead of querying silently. Summaries carry: `User` references only `SharedInfoId`; `Video` references nothing; `Video.LastValidManifest` carries `IsValid`, `ManifestReference`, `CreationDateTime`; `Video.VideoManifests` items and `UnsuitableVideoReport.VideoManifest` only the id. Before reading anything else, preload explicitly with `dbContext.LoadValuesAsync(models, m => m.Member, ...)` (one `$in` query per repository): the API handler helpers `LoadLastValidManifestsAsync`/`LoadVideoManifestsAsync`, `ElasticSearchService.AddVideoAsync` and `VideoManifestValidatorTask` show the pattern. The `Video` aggregate methods `AddManifest`, `FailedManifestValidation` and `SucceededManifestValidation` read `ManifestReference`, `IsValid` and `CreationDateTime` of every `VideoManifests` item: preload them before calling those methods on a loaded video.
- **Authentication uses a policy scheme** (`CommonConsts.UserAuthenticationPolicyScheme`): requests with `Authorization: Bearer …` go to JWT bearer (authority = SSO, audience `userApi`); otherwise fall back to the Identity cookie + OpenID Connect against Etherna SSO. The default policy adds `DenyBannedAuthorizationRequirement`; API endpoints that act on behalf of a user require `UserInteractApiScopePolicy` (JWT scheme + the `EthernaScopes.UserApiIndexScopeName` scope claim).
- **User data is split across two databases**: the index-specific `User` entity (IndexDb) references `UserSharedInfo` (Ether address, ban state — ServiceSharedDb, shared with SSO). Resolve both together via `IUserService.FindUserAsync`. `SharedDbContext` is registered read-only (`IsReadOnly`): the SSO owns that database, its indexes, seeding and migrations, and any write from the Index throws `UnauthorizedAccessException`.
- **Swarm types use the SwarmSdk libraries** (`Etherna.SwarmSdk`): `SwarmReference`, `SwarmAddress`, `SwarmUri`, `EthAddress`, `PostageBatchId` are value types serialized via the custom BSON serializers in `Persistence/Serializers/` and the JSON converters wired in `Program.ConfigureHttpJsonOptions`.

## Issue tracker

Bugs and features are tracked in Jira project **EID** (https://etherna.atlassian.net/projects/EID). Branch names follow `feature/EID-<id>-<slug>` / `improve/EID-<id>-<slug>` / `fix/EID-<id>-<slug>` — match this when creating branches. Release hotfixes use `hotfix/<version>` (e.g. `hotfix/0.3.16`); `dev` is the integration branch, `main` is production.

# Coding Style

## General Principles

- Keep commits clean: only include changes strictly necessary for the task at hand.
- Never reference AI agents or assistants in commits or code — no agent names, no `Co-Authored-By` agent trailers, no "generated/assisted by" notes. Commit messages and code must read as the team's own work.
- Exceptions to these conventions are accepted when strictly necessary or when they significantly improve code quality. Justify with a comment where needed.
- All elements (usings, properties, methods, fields, enum members, etc.) are always alphabetically ordered within their respective sections.
- Prefer primary constructors whenever possible — not limited to DI services. A parameter needing a light transformation still qualifies: capture it and derive a field. Fall back to a classic constructor only when the body needs real logic that can't be expressed as a field initializer.
- Keep code clean: remove unused variables, dead code, and redundant imports.
- Every source file starts with the standard AGPL-3.0 copyright header (`// Copyright 2021-present Etherna SA` … see any existing file).

## Naming

- **Classes/Structs**: PascalCase (`Video`, `Comment`, `VideoManifest`, `ValidationError`)
- **Interfaces**: `I` prefix (`IVideoService`, `IElasticSearchService`, `IIndexDbContext`)
- **Async methods**: always `Async` suffix (`DeleteVideoAsync`, `ModerateUnsuitableVideoAsync`)
- **Properties**: PascalCase, boolean `Is` prefix (`IsValid`, `IsFrozen`, `IsEditable`)
- **Private fields**: `_camelCase` only when backing a same-named property; otherwise plain `camelCase`
- **Primary constructor parameters**: `camelCase` without underscore
- **Constants**: PascalCase (`MaxEditHistory`, `MaxLength`, `RemovedByAuthorReplaceText`), except the all-caps underscore style used for Hangfire queue names (`DB_MAINTENANCE`)
- **Enums**: PascalCase type and members (`VoteValue.Up`, `ValidationErrorType.Unknown`)
- **Namespaces**: `Etherna.EthernaIndex.<Layer>.<Feature>` (e.g. `Etherna.EthernaIndex.Domain.Models.VideoAgg`)
- **DTOs**: `Dto` suffix (`ImageDto`, `ErrorDetailDto`); **API input models**: `Input` suffix (`VideoCreateInput`)
- **Custom exceptions**: `Exception` suffix (`DuplicatedManifestReferenceException`)
- **Event handlers**: `On<Event>Then<Action>Handler`
- **Hangfire tasks**: `<Name>Task` implementing `I<Name>Task`
- **Aggregate folders**: `<Name>Agg` suffix, with version subfolders (`VideoAgg/ManifestV1/`, `VideoAgg/ManifestV2/`)

## Code Organization

- One class per file, filename matches class name
- Namespace mirrors folder structure exactly
- Block-scoped namespaces: `namespace X { ... }` — NOT file-scoped
- Using directives inside namespace block, always alphabetically ordered and kept to the minimum necessary
- No global usings

## Comments

Principal comments (generally multiline, important):
```csharp
// Capital start, ending period.
// Continued on next line if needed.
```

Secondary/separator comments:
```csharp
//no space, no capital, no ending period
```

## Member Ordering Within a Class

Use principal-style section comments:

```csharp
// Consts.
// Fields.
// Constructors and dispose.
// Properties.
// Methods.
// Helpers.
```

## Class Design

- `internal sealed` for service implementations and event handlers
- Primary constructors everywhere the constructor is a simple assignment
- Reflection-discovered types (event handlers, model-map collectors) are matched by namespace — keep them in the expected namespace

### Domain Entity Classes

- `public abstract` for base classes (`ModelBase`, `EntityModelBase`, `VideoManifestMetadataBase`)
- `virtual` on all properties and methods for Scrinium proxy support (lazy loading and change tracking), except members living in memory only (`EntityModelBase.Events`): those stay non-virtual, so reading them on a summary never triggers a load
- Use `public set` by default; use `protected set` only when the property requires validation or invariant enforcement
- Protected parameterless constructor for ORM:
  ```csharp
  #pragma warning disable CS8618
  protected EntityName() { }
  #pragma warning restore CS8618
  ```
- Collection encapsulation with backing fields
- `null!` and `default!` for ORM-initialized properties: `public virtual string Name { get; protected set; } = null!;`
- Collection expressions for nullable collection setters: `[..value ?? []]`

## Async Patterns

- Always suffix with `Async`
- `CancellationToken cancellationToken = default` as optional last parameter
- No `ConfigureAwait(false)` — ASP.NET Core app

## Null Handling

- Nullable reference types enabled
- `ArgumentNullException.ThrowIfNull()` or `?? throw new ArgumentNullException(nameof(param))`
- `is null` / `is not null` patterns
- Prefer `null` over `default` wherever the type admits it: optional parameter defaults, late-init member initializers (`= null!`, not `= default!`), returns and assignments. Keep `default` only where `null` can't apply: non-nullable value types (e.g. `CancellationToken cancellationToken = default`) and unconstrained generic type parameters.
- Switch expression with null pattern:
  ```csharp
  Metadata switch
  {
      null => null,
      VideoManifestMetadataV1 v1 => v1.Description,
      VideoManifestMetadataV2 v2 => v2.Description,
      _ => throw new InvalidOperationException()
  };
  ```

## Formatting

- Allman braces, 4-space indentation
- Expression-bodied members for computed properties and simple helpers:
  ```csharp
  public bool IsLockedOutNow => LockoutEnabled && LockoutEnd >= DateTimeOffset.UtcNow;
  private void UpdateLastValidManifest() =>
      _lastValidManifest = VideoManifests
          .Where(i => i.IsValid == true)
          .OrderByDescending(i => i.CreationDateTime)
          .FirstOrDefault();
  ```
- LINQ method chains aligned, one operation per line
- Blank line between member sections

## C# Language Features

- Switch expressions for versioned metadata dispatch
- Pattern matching: type, null, and property patterns
- Primary constructors everywhere applicable
- Collection expressions: `[]`, `[..spread]`
- Prefer collection expressions over constructors to initialize any collection: `[]` not `new()`, `["a", "b"]` not `new List<string> { "a", "b" }`. Use a constructor only when a collection expression can't express the intent (e.g. presizing capacity with `new List<T>(capacity)`).
- Target-typed `new()` for non-collection types when the type is clear from context
- Lock fields: prefer the dedicated `System.Threading.Lock` type (.NET 9+) over a plain `object` — more expressive, and the compiler enforces correct `lock` usage on it.

## LINQ

- Method syntax preferred
- Query syntax only for reflection-based type discovery (e.g. model maps)
- Async LINQ via Scrinium: `FindAsync`, `QueryElementsAsync`

## Dependency Injection

- Constructor injection exclusively
- Reflection-based event handler discovery
- `AddScoped` for services, `AddSingleton` for stateless
- Hangfire task queue attributes: `[Queue(Queues.ELASTIC_SEARCH_MAINTENANCE)]`

## Testing (xUnit + Moq)

- Test naming: `[MethodName]_[Scenario]` or descriptive (`EditCommentShouldAddHistory`)
- AAA with section comments: `// Arrange.`, `// Action.`, `// Assert.`
- Helper methods for test setup: `CreateManifest(reference, valid)`
- Moq: `new Mock<User>()`, `Setup/Returns` chains
- When adding or changing a `ModelMap`, add a deserialization test that pins the new map GUID against a representative stored document (see `IndexDbContextDeserializationTest`)
