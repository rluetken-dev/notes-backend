using Microsoft.EntityFrameworkCore;
using Notes.Api.Models;

namespace Notes.Api.Data
{
    public class AppDb : DbContext
    {
        public AppDb(DbContextOptions<AppDb> options) : base(options)
        {
        }

        public DbSet<Note> Notes => Set<Note>();
    }
}