using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using AMAR.Domain.HR.Model;

namespace AMAR.DataAccess.EF.Mapping.HR
{
    public class MedicationRequestMapping : EntityTypeConfiguration<MedicationRequest>
    {
        public MedicationRequestMapping()
        {
            // نگاشت به اسکیمای gen و جدول MedicationRequest
            ToTable("MedicationRequest", "gen");

            HasKey(x => x.UniqId);
            Property(x => x.UniqId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .IsRequired();

            Property(x => x.WorkFlowHdrId).IsRequired();
            Property(x => x.PersonId).IsRequired();
            Property(x => x.ProductId).IsRequired();
            
            Property(x => x.productName)
                .HasMaxLength(250)
                .IsRequired();

            Property(x => x.Qty).IsRequired();
            
            Property(x => x.DeliveryLocation)
                .HasMaxLength(150)
                .IsOptional();

            Property(x => x.ShYear).IsOptional();
            
            Property(x => x.ShSeason)
                .HasMaxLength(50)
                .IsOptional();

            Property(x => x.Status).IsRequired();

            Property(x => x.SnapshotId).IsOptional();
            Property(x => x.SnapshotDate).IsOptional();
            Property(x => x.IssuedDate).IsOptional();
            Property(x => x.IssuedBy).HasMaxLength(100).IsOptional();
            Property(x => x.DeliveredDate).IsOptional();
            Property(x => x.DeliveredBy).HasMaxLength(100).IsOptional();
            Property(x => x.CreatedDate).IsRequired();

            // نگاشت ارتباط با موجودیت Person جهت امکان Include مستقیم
            HasOptional(x => x.Person)
                .WithMany()
                .HasForeignKey(x => x.PersonId);
        }
    }
}
