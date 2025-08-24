using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TeamTrack.Models; 

namespace TeamTrack.Data
{
    public class TeamTrackDbContextFactory : IDesignTimeDbContextFactory<TeamTrackDbContext>
    {
        public TeamTrackDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TeamTrackDbContext>();

            var connectionString = "Server=.;Database=TeamTrackDB;Trusted_Connection=True;TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connectionString);

            return new TeamTrackDbContext(optionsBuilder.Options);
        }
    }
}
