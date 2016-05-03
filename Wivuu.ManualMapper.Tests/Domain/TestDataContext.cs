using System.Data.Common;
using System.Data.Entity;

namespace Wivuu.ManualMapper.Tests.Domain
{
    public class TestDataContext : DbContext
    {
        public TestDataContext() { }
        public TestDataContext(DbConnection conn) : base(conn, false) { }

        public DbSet<TestSourceType> MySources { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
