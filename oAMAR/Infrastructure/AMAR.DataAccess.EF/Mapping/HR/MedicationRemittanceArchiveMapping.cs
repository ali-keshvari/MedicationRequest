using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using AMAR.Domain.HR.Model;

namespace AMAR.DataAccess.EF.Mapping.HR
{
    public class MedicationRemittanceArchiveMapping : EntityTypeConfiguration<MedicationRemittanceArchive>
    {
        public MedicationRemittanceArchiveMapping()
        {
            ToTable("MedicationRemittanceArchive", "gen");

            HasKey(x => x.MedicationRemittanceArchiveId);
            Property(x => x.MedicationRemittanceArchiveId)
                .HasColumnName("MedicationRemittanceArchiveId")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .IsRequired();
            Ignore(x => x.ArchiveId);

            Property(x => x.RemittanceBatchId).IsRequired();
            Property(x => x.RemittanceNumber).HasMaxLength(50).IsRequired();
            Property(x => x.SnapshotId).IsOptional();
            Property(x => x.WorkFlowHdrId).IsRequired();
            Property(x => x.PersonId).IsRequired();
            Property(x => x.PersonelCode).IsOptional();
            Property(x => x.PersonFullName).HasMaxLength(200).IsOptional();
            Property(x => x.DeliveryLocation).HasMaxLength(150).IsOptional();
            Property(x => x.MedicationsSummary).IsMaxLength().IsOptional();
            Property(x => x.ProductId).IsRequired();
            Property(x => x.ProductName).HasMaxLength(250).IsRequired();
            Property(x => x.Qty).IsRequired();
            Property(x => x.IssuedDate).IsRequired();
            Property(x => x.IssuedBy).HasMaxLength(100).IsRequired();
            Property(x => x.CreatedDate).IsRequired();

            HasOptional(x => x.Person)
                .WithMany()
                .HasForeignKey(x => x.PersonId);
        }
    }
}
