using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace API_1.Models
{
    public class UserQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int Id { get; set; }
        public string email { get; set; }
        public string SecurityQnA { get; set; } = JsonSerializer.Serialize(new List<KeyValuePair<string, string>>());

        public List<KeyValuePair<string, string>> GetSecurityQnA()
        {
            return string.IsNullOrEmpty(SecurityQnA)
                ? new List<KeyValuePair<string, string>>()
                : JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(SecurityQnA);
        }

        // Helper method to set the list
        public void SetSecurityQnA(List<KeyValuePair<string, string>> qnaList)
        {
            SecurityQnA = JsonSerializer.Serialize(qnaList);
        }

    }
}
