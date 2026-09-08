# Etherna Index

[![License: AGPL-3.0](https://img.shields.io/badge/license-AGPL--3.0-blue)](COPYING)
[![Target framework](https://img.shields.io/badge/.NET-10-512BD4)](#building-and-testing)

**Etherna Index** is the service where content uploaded to [Swarm](https://github.com/ethersphere/bee) is
indexed and made accessible to all. It exposes a REST API to publish, search, comment and moderate videos,
backed by MongoDB for persistence and Elasticsearch for full-text search.

Etherna Index is an **application** — an ASP.NET Core server — not a library. This repository holds its full
source together with its build and deployment configuration.

## Contents

- [Features](#features)
- [Architecture](#architecture)
- [Running locally](#running-locally)
- [Configuration](#configuration)
- [Building and testing](#building-and-testing)
- [Docker](#docker)
- [Project layout](#project-layout)
- [Contributing](#contributing)
- [Issue reports](#issue-reports)
- [Questions? Problems?](#questions-problems)
- [License](#license)

## Features

- **Video indexing** — videos published on Swarm are registered by their manifest reference; manifests are
  fetched from the Swarm gateway and validated asynchronously (Hangfire) before becoming searchable.
- **REST API** — Minimal APIs under `/api/v0.3`, with an interactive Scalar reference at `/scalar/index03`
  (OAuth2 login against the Etherna SSO).
- **Full-text search** — videos and comments are indexed in Elasticsearch and kept in sync with the domain
  through events; indexes can be rebuilt from MongoDB on demand.
- **Comments and votes** — authenticated users can comment videos (with edit history) and vote them.
- **Moderation** — unsuitable-video reports, manual reviews, and video/author moderation, managed from the
  admin area; dashboards for Hangfire (`/admin/hangfire`) and Scrinium (`/admin/db`) are admin-gated.
- **Authentication against Etherna SSO** — JWT bearer for API clients and cookie + OpenID Connect for the
  web app, with a deny-banned-users policy on every request.
- **Authenticated Gateway downloads** — the Index authenticates to the Etherna Gateway with an OAuth2
  client-credentials application (scope `userApi.gateway`), so also non-offered (paid) content can be
  downloaded and indexed, billed to the owning Etherna account.
- **Observability** — structured logging to Elasticsearch through Serilog.

## Architecture

A five-project layered solution (plus three test projects):

- **`EthernaIndex.Domain`** — pure domain layer: aggregates, entities and domain events; exposes only
  DbContext interfaces, with no persistence types leaking out.
- **`EthernaIndex.Persistence`** — MongoDB persistence via Scrinium (model maps, repositories, the Index and
  shared DbContexts, BSON serializers for the Swarm value types).
- **`EthernaIndex.Services`** — application services, side effects, event handlers and Hangfire tasks.
- **`EthernaIndex.ElasticSearch`** — Elasticsearch integration: document models, indexing and search.
- **`EthernaIndex`** — the ASP.NET Core host (Minimal APIs, Razor Pages admin area, authentication,
  Hangfire, Serilog and OpenAPI wiring).

Coding conventions and deeper architecture notes live in [AGENTS.md](AGENTS.md).

## Running locally

Prerequisites:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A reachable **MongoDB** replica set (as referenced by the connection strings in
  `appsettings.Development.json`)
- A reachable **Elasticsearch** cluster (`Elastic:Urls`)
- The **Etherna SSO** server (`SsoServer:*` settings)
- A **Swarm gateway** (`Swarm:GatewayUrl`) — or none: build the `Debug-Mockup-Swarm` solution configuration
  to switch the Swarm access to in-memory mockups

Run the server:

```bash
dotnet run --project src/EthernaIndex
```

The site listens on http://localhost:42690 (https on 44357). There is no front-end build step: a plain
`dotnet run` produces a working site.

## Configuration

Configuration uses the standard ASP.NET Core model. Values are resolved, in increasing order of precedence, from:

1. `appsettings.json` — committed defaults
2. `appsettings.{ASPNETCORE_ENVIRONMENT}.json` — e.g. `appsettings.Production.json`
3. **environment variables** — per-deployment values and secrets

Hierarchical keys use a separator: the colon form (`Section:SubSection:Key`) or the double-underscore form (`Section__SubSection__Key`), which is portable across all platforms. Array entries are indexed: `Section:Items:0`, `Section:Items:1`, …

Secrets (`*Password`, `*Secret`, MongoDB credentials) must be supplied via environment variables or a secret store at deploy time — never committed. Values marked **set in prod** below have no entry in `appsettings.Production.json` and must therefore come from the environment.

### Hosting (ASP.NET Core built-ins)

| Variable | Example | Notes |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | selects the `appsettings.{Environment}.json` overlay |
| `ASPNETCORE_URLS` | `http://+:80` | listening endpoints |

### Connection strings (MongoDB — all required)

| Key | Purpose |
|---|---|
| `ConnectionStrings:IndexDb` | main Index database (videos, manifests, comments, domain data) |
| `ConnectionStrings:ServiceSharedDb` | database shared with sibling Etherna services |
| `ConnectionStrings:DataProtectionDb` | ASP.NET data-protection keys |
| `ConnectionStrings:HangfireDb` | Hangfire job storage |

### SSO server

| Key | Notes |
|---|---|
| `SsoServer:BaseUrl` | public SSO authority URL, e.g. `https://sso.etherna.io` |
| `SsoServer:AllowUnsafeConnection` | dev only — allow an http authority |
| `SsoServer:LoginPath`, `SsoServer:RegisterPath` | SSO login/register page paths (committed defaults) |
| `SsoServer:Clients:Webapp:ClientId` | web-app OIDC client id (committed) |
| `SsoServer:Clients:Webapp:Secret` | **secret** — web-app OIDC client secret |
| `SsoServer:Clients:Scalar:ClientId` | Scalar API-reference OAuth client id (committed) |
| `SsoServer:Clients:Services:ClientId` | — (**set in prod**) client-credentials application authenticating the Index to the other Etherna services ("Etherna Index to Services"), created in the SSO developer editor (owner = Etherna account); its id is generated at creation. A dev value ships in `appsettings.Development.json` |
| `SsoServer:Clients:Services:Secret` | **secret** — (**set in prod**) client-credentials application secret |

### Swarm

| Key | Example | Notes |
|---|---|---|
| `Swarm:GatewayUrl` | `http://localhost:1633` | Swarm gateway used to fetch and validate video manifests |

### Logging, search & networking

| Key | Example | Notes |
|---|---|---|
| `Elastic:Urls:<n>` | `http://elasticsearch:9200` | Elasticsearch endpoints, used by both the Serilog sink and the search indexes |
| `Elastic:Username` | `etherna_services` | Basic Auth user (omit against an unsecured cluster) |
| `Elastic:Password` | — | **secret** — Basic Auth password |
| `ForwardedHeaders:KnownNetworks:<n>` | `10.0.0.0/8` | trusted reverse-proxy networks |
| `Serilog:MinimumLevel:Default` | `Information` | global log level |
| `Serilog:MinimumLevel:Override:<Namespace>` | `Warning` | per-namespace override |

## Building and testing

```bash
dotnet restore EthernaIndex.sln
dotnet build   EthernaIndex.sln -c Release
dotnet test    EthernaIndex.sln -c Release      # runs the xUnit test projects
```

`TreatWarningsAsErrors=true` and `AnalysisMode=AllEnabledByDefault` are enabled across the solution, so
warnings break the build. To develop without a real Swarm/Bee node, build the `Debug-Mockup-Swarm` solution
configuration.

## Docker

The repository ships a `Dockerfile` that builds the server (and runs the test suite as part of the build
stage):

```bash
docker build -t etherna-index .
```

Provide the runtime configuration (connection strings, SSO clients, Swarm gateway, …) through environment
variables — see [Configuration](#configuration).

## Project layout

```
src/
  EthernaIndex.Domain          pure domain layer (aggregates, entities, domain events)
  EthernaIndex.Persistence     Scrinium persistence (model maps, repositories, DbContexts)
  EthernaIndex.Services        application services, event handlers, Hangfire tasks
  EthernaIndex.ElasticSearch   Elasticsearch integration (documents, indexing, search)
  EthernaIndex                 ASP.NET Core host (API, admin area, authentication)
test/
  EthernaIndex.Domain.Tests        xUnit + Moq domain tests
  EthernaIndex.Persistence.Tests   xUnit persistence / serialization tests
  EthernaIndex.Services.Tests      xUnit services and tasks tests
```

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) and our
[Code of Conduct](CODE_OF_CONDUCT.md) before opening a pull request.

## Issue reports

If you've discovered a bug, or have an idea for a new feature, please report it to our issue manager based on
Jira: https://etherna.atlassian.net/projects/EID.

Detailed reports with stack traces, actual and expected behaviours are welcome.

## Questions? Problems?

For questions or problems please write an email to [info@etherna.io](mailto:info@etherna.io).

## License

![AGPL Logo](https://www.gnu.org/graphics/agplv3-with-text-162x68.png)

We use the GNU Affero General Public License v3 (AGPL-3.0) for this project.
If you require a custom license, you can contact us at [license@etherna.io](mailto:license@etherna.io).
