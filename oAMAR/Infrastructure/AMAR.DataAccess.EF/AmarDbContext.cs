using System.Data.Entity;
using AMAR.DataAccess.EF.Mapping.HR;
using AMAR.Domain.HR.Model;

namespace AMAR.DataAccess.EF
{
    /// <summary>
    /// کلاس زمینه دیتابیس با نام درخواستی dbContext
    /// </summary>
    public class dbContext : DbContext
    {
        public dbContext() : base("name=AmarDbContext")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public dbContext(string connectionString) : base(connectionString)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<MedicationRequest> MedicationRequests { get; set; }
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<MedicationRemittanceArchive> MedicationRemittanceArchives { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new MedicationRequestMapping());
            modelBuilder.Configurations.Add(new PersonMapping());
            modelBuilder.Configurations.Add(new MedicationRemittanceArchiveMapping());
        }
    }

    /// <summary>
    /// جهت سازگاری به نام قبلی AmarDbContext ارث‌بری شده است
    /// </summary>
    public class AmarDbContext : dbContext
    {
        public AmarDbContext() : base() { }
        public AmarDbContext(string connectionString) : base(connectionString) { }
    }
}
