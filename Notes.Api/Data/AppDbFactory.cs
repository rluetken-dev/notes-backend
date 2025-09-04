// AppDbFactory.cs
// Purpose: Provide a design-time factory so the EF CLI (dotnet-ef) knows
// how to construct AppDb when running migrations outside your running app.
//
// Why this fixes your error:
// - At design time there is no fully built DI container.
// - This factory tells EF exactly how to create DbContextOptions<AppDb>.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notes.Api.Data
{
    // The EF CLI will discover and use this factory automatically.
    public class AppDbFactory : IDesignTimeDbContextFactory<AppDb>
    {
        public AppDb CreateDbContext(string[] args)
        {
            // Keep it simple: same SQLite connection as in appsettings.json.
            var options = new DbContextOptionsBuilder<AppDb>()
                .UseSqlite("Data Source=notes.db")
                .Options;

            return new AppDb(options);
        }
    }
}
