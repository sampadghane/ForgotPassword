using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_1.Models
{
    public class UserOtp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string otp { get; set; }
        public string validity { get; set; }
    }
}
