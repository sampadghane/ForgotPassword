using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API_1.Models
{
    public class UserNumber
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int Id { get; set; }
        public string username { get; set; }
        public string Number { get; set; }
        public string Otp { get; set; }
        public string Validity { get; set; }
    }
}
