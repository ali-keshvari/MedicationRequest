using System.Data.Entity;
using AMAR.DataAccess.EF.Mapping.HR;
using AMAR.Domain.HR.Model;

namespace AMAR.DataAccess.EF
{
    public class AmarDbContext : DbContext
    {
        public AmarDbContext() : base("name=AmarDbContext")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public AmarDbContext(string connectionString) : base(connectionString)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<MedicationRequest> MedicationRequests { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new MedicationRequestMapping());
        }
    }
}
