using Microsoft.EntityFrameworkCore;
using SecMan.Data.SQLCipher;

namespace SecMan.UnitTests.FaultyDbConfig
{
    internal class FaultyDbContext : Db
    {
        public FaultyDbContext(DbContextOptions<Db> options) : base(options, string.Empty) { }

        public override int SaveChanges()
        {
            throw new Exception("Simulated internal server error");
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            throw new Exception("Simulated internal server error");
        }
    }
}
