using System;

namespace AMAR.Domain.HR.Model
{
    /// <summary>
    /// مدل شخص در سیستم سازمانی (dbo.Person)
    /// </summary>
    public class Person
    {
        public int PersonId { get; set; }
        public int PersonelCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalCode { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
