using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    /// <summary>
    /// Person Domain model class
    /// </summary>
    public class Person
    {
        [Key]
        public Guid PersonID { get; set; }

        [StringLength(40)]
        public string? PersonName { get; set; }

        [StringLength(40)]
        public string? Email { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Guid? CountryID { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public string? TIN { get; set; }

        [ForeignKey("CountryID")]
        public virtual Country? Country { get; set; }

        public override string ToString()
        {
            return $"Person ID: {PersonID}, Person Name: {PersonName}, Email: {Email}, Date of Birth: {DateOfBirth?.ToString("MM/dd/yyyy")}, Gender: {Gender}, Country ID: {CountryID}, Country: {Country?.CountryName}, Address: {Address}, Receive News Letters: {ReceiveNewsLetters}";
        }
    }
}
