using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_1.Models
{
    public class User
    {
        [Key]
        public string? username { get; set; }
        
        public string? password { get; set; }
        public string? role { get; set; }
        public string? email { get; set; }
        public bool is_question { get; set; }
        public bool is_number { get; set; }

        public User()
        {
            is_question = false;
            is_number = false;
        }
    }
}
