// AppDb.cs
// Purpose: Central EF Core DbContext that manages the database connection and entity sets.
// Notes:
// - Each DbSet<T> represents a table (or view) in the database.
// - This context will be registered with Dependency Injection (DI) later in Program.cs.

using Microsoft.EntityFrameworkCore;
using Notes.Api.Models;

namespace Notes.Api.Data
{
    public class AppDb : DbContext
    {
        // The options (e.g., which provider/connection string to use) are injected by ASP.NET Core.
        public AppDb(DbContextOptions<AppDb> options) : base(options) { }

        // Represents the "Notes" table in the database. Each Note instance = one row.
        public DbSet<Note> Notes => Set<Note>();

        // Override this if you need to customize mappings (e.g., value converters, indexes).
        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     // Example: modelBuilder.Entity<Note>().HasIndex(n => n.Title);
        //     base.OnModelCreating(modelBuilder);
        // }
    }
}
