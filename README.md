# Notes Backend (.NET 8 + EF Core + SQLite)

![CI](https://github.com/rluetken-dev/notes-backend/actions/workflows/ci.yml/badge.svg)

A simple CRUD REST API for managing notes.  
Built with **.NET 8**, **Entity Framework Core**, and **SQLite** as part of my software engineering portfolio.

---

## ✨ Features
- CRUD endpoints for notes (`GET`, `POST`, `PUT`, `DELETE`)
- Pagination, filtering (`q`), and sorting (`sort`, `dir`) for `GET /api/notes`
- Swagger UI documentation with XML comments
- Entity Framework Core with SQLite persistence
- GitHub Actions CI (build + tests + linting)

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- (optional) [SQLite CLI](https://www.sqlite.org/download.html) for inspecting the database

### Run locally
```bash
git clone https://github.com/rluetken-dev/notes-backend.git
cd notes-backend/Notes.Api
dotnet run
```

API will be available at:  
👉 `http://localhost:5000/swagger` (Swagger UI)  
👉 `http://localhost:5000/api/notes` (base endpoint)

---

## 📚 API Overview

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
`GET /api/notes/1`

### Update Note
`PUT /api/notes/1`

```json
{
  "title": "Updated shopping list",
  "content": "Milk, Bread, Eggs, Butter"
}
```

### Delete Note
`DELETE /api/notes/1`

---

## 🛠️ Tech Stack
- C# / .NET 8 Web API
- Entity Framework Core (Code-First, Migrations)
- SQLite (local database)
- Swagger / Swashbuckle
- GitHub Actions (CI)

---

## 📌 Next Steps
- Add authentication (JWT)
- Dockerize API
- Deploy to Azure / AWS
