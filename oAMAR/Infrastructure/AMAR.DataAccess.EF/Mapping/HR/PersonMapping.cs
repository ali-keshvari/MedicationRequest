using System.Data.Entity.ModelConfiguration;
using AMAR.Domain.HR.Model;

namespace AMAR.DataAccess.EF.Mapping.HR
{
    public class PersonMapping : EntityTypeConfiguration<Person>
    {
        public PersonMapping()
        {
            ToTable("Person", "dbo");

            HasKey(x => x.PersonId);
            Property(x => x.PersonelCode).IsRequired();
            Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            Property(x => x.LastName).HasMaxLength(100).IsRequired();
            Property(x => x.NationalCode).HasMaxLength(20).IsOptional();
            Property(x => x.Mobile).HasMaxLength(20).IsOptional();
            Property(x => x.Email).HasMaxLength(100).IsOptional();

            Ignore(x => x.FullName);
        }
    }
}
