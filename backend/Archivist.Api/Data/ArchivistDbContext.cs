using Archivist.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Archivist.Api.Data
{
    // DbContext represents a session wtih the database
    public class ArchivistDbContext: DbContext
    {
        // EF Core uses this to create/manage a "QuickNotes" table.
        public DbSet<QuickNote> QuickNotes => Set<QuickNote>();

        // ASP.NET will supply DbContextOptions via dependency injection.
        public ArchivistDbContext(DbContextOptions<ArchivistDbContext> options)
            : base(options)
        {
        } 
    }
}
