# notes-backend (.NET 8 + EF Core + SQLite)

<p align="left">
  <a href="https://github.com/rluetken-dev/notes-backend/actions/workflows/ci.yml">
    <img alt="CI" src="https://github.com/rluetken-dev/notes-backend/actions/workflows/ci.yml/badge.svg?branch=main">
  </a>
  <img alt=".NET" src="https://img.shields.io/badge/.NET-8.0-purple">
  <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-green.svg">
  <img alt="Conventional Commits" src="https://img.shields.io/badge/Conventional%20Commits-1.0.0-yellow.svg">
  <a href="https://github.com/rluetken-dev/notes-backend/releases">
    <img alt="Release" src="https://img.shields.io/github/v/release/rluetken-dev/notes-backend?sort=semver&display_name=tag">
  </a>
</p>

A simple, production-ready **REST API** for managing notes.  
Built with **.NET 8**, **Entity Framework Core**, and **SQLite**.  
Pairs well with the optional frontend: **[notes-frontend](https://github.com/rluetken-dev/notes-frontend)**.

---

## Table of Contents
- [Features](#features)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Swagger Screenshot](#swagger-screenshot)
- [API Overview](#api-overview)
- [Endpoints Overview](#endpoints-overview)
- [Project Structure](#project-structure)
- [Commits & Releases](#commits--releases)
- [Development Tips](#development-tips)
- [Roadmap](#roadmap)
- [License](#license)
- [Related](#related)

---

## Features
- CRUD endpoints for notes (`GET`, `POST`, `PUT`, `DELETE`)
- Pagination, filtering (`q`), and sorting (`sort`, `dir`) for `GET /api/notes`
- Swagger UI with XML comments (`<summary/>` on controllers/models)
- EF Core with SQLite persistence (Code-First, Migrations)
- GitHub Actions CI (build + tests + format/lint)

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- (optional) [SQLite CLI](https://www.sqlite.org/download.html)

### Run locally
```bash
git clone https://github.com/rluetken-dev/notes-backend.git
cd notes-backend

# Restore
dotnet restore

# (optional) EF tools, if noch nicht installiert
dotnet tool install --global dotnet-ef

# DB migrieren (legt SQLite-Datei an)
dotnet ef database update --project Notes.Api

# Start
dotnet run --project Notes.Api
```

API will be available at:  
👉 `http://localhost:5000/swagger` (Swagger UI)  
👉 `http://localhost:5000/api/notes` (base endpoint)

---

## Configuration

| Setting            | Env Var                     | Default                 | Notes                    |
|-------------------:|-----------------------------|-------------------------|--------------------------|
| Connection String  | `ConnectionStrings__Default`| `Data Source=notes.db`  | SQLite DB file path      |
| ASP.NET URLs       | `ASPNETCORE_URLS`           | `http://localhost:5000` | Change port/host binding |

---

## Swagger Screenshot
The interactive API documentation is available via Swagger:

![Swagger UI](docs/swagger.png)

---

## API Overview

### Create Note
`POST /api/notes`
```json
{
  "title": "Shopping",
  "content": "Milk, Bread, Eggs"
}
```

### Get All Notes
`GET /api/notes?q=shopping&page=1&pageSize=5&sort=title&dir=asc`

- **Query params:**
  - `q`: filter term (optional)
  - `page`: page index (default 1)
  - `pageSize`: items per page (default 10, max 100)
  - `sort`: `id | title | created | updated`
  - `dir`: `asc | desc`
- **Response header:** `X-Total-Count` = total matching notes

### Get Note by ID
`GET /api/notes/{id}`

### Update Note
`PUT /api/notes/{id}`
```json
{
  "title": "Updated shopping list",
  "content": "Milk, Bread, Eggs, Butter"
}
```

### Delete Note
`DELETE /api/notes/{id}`

---

## Endpoints Overview

| Method | Route             | Description       |
|------: |-------------------|-------------------|
| GET    | `/api/notes`      | List all notes    |
| GET    | `/api/notes/{id}` | Get note by ID    |
| POST   | `/api/notes`      | Create a new note |
| PUT    | `/api/notes/{id}` | Update a note     |
| DELETE | `/api/notes/{id}` | Delete a note     |

---

## Project Structure
```
notes-backend/
├─ Notes.Api/         # ASP.NET Core project (controllers, models, DI, Swagger)
├─ docs/              # Documentation assets (e.g., swagger.png)
├─ commits.md         # Conventional Commits guide (types, scopes, examples)
└─ README.md
```

---

## Commits & Releases
This repo follows **Conventional Commits** and **SemVer**.

- See **[COMMITS.md](./COMMITS.md)** for rules, allowed types/scopes, and examples.
- Tag releases as `vX.Y.Z` and keep release notes concise (link the compare view).

---

## Development Tips
- `dotnet restore` nach dem Clonen.
- `dotnet test -c Release` lokal, bevor du pushst.
- `dotnet format --verify-no-changes` für konsistenten Stil.
- Ports in `launchSettings.json` ↔ URLs in README synchron halten.

---

## Roadmap
- Authentication (JWT)
- Dockerize API
- Deploy to Azure / AWS
- Health endpoint (`/health`) + container healthcheck

---

## License
MIT — feel free to use, learn, and extend.

---

## Related
- Frontend: https://github.com/rluetken-dev/notes-frontend
- Backend:  https://github.com/rluetken-dev/notes-backend
