# Notes Backend (.NET 8 + EF Core + SQLite)

![CI](https://github.com/rluetken-dev/notes-backend/actions/workflows/ci.yml/badge.svg)
![Conventional Commits](https://img.shields.io/badge/Conventional%20Commits-1.0.0-yellow.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)

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

### Swagger Screenshot

The interactive API documentation is available via Swagger:

![Swagger UI](docs/swagger.png)

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

## 🗂️ Endpoints Overview

| Method | Route             | Description        |
|-------:|-------------------|--------------------|
| GET    | `/api/notes`      | List all notes     |
| GET    | `/api/notes/{id}` | Get note by ID     |
| POST   | `/api/notes`      | Create a new note  |
| PUT    | `/api/notes/{id}` | Update a note      |
| DELETE | `/api/notes/{id}` | Delete a note      |

---

## 🛠️ Tech Stack
- C# / .NET 8 Web API
- Entity Framework Core (Code-First, Migrations)
- SQLite (local database)
- Swagger / Swashbuckle
- GitHub Actions (CI)

---

## 📂 Project Structure

```
notes-backend/
├─ Notes.Api/        # ASP.NET Core project (controllers, models, DI, etc.)
├─ docs/             # Documentation assets (e.g., swagger.png)
└─ README.md
```

---

## 💡 Development Tips
- Run `dotnet restore` after cloning to fetch dependencies.
- Keep XML doc comments tidy to improve Swagger output.
- Run builds/tests locally before pushing (e.g., `dotnet build && dotnet test`) if you add tests.
- If you change ports, update the URLs above (check `launchSettings.json`).

---

## 📌 Next Steps
- Add authentication (JWT)
- Dockerize API
- Deploy to Azure / AWS

---

## 📜 License
This project is released under the **MIT License**.
