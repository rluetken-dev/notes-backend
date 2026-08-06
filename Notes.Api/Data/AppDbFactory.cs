using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notes.Api.Data
{
    public class AppDbFactory : IDesignTimeDbContextFactory<AppDb>
    {
        public AppDb CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDb>()
                .UseSqlite("Data Source=notes.db")
                .Options;

            return new AppDb(options);
        }
    }
}