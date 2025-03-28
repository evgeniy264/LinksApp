using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinksApp.Models
{
    [Table("links")]
    
    public class Link
    {
        [Key]
        public required string Guid { get; set; }
        public DateOnly Date_of_generation { get; set; }
        public DateOnly Date_of_expiration { get; set; }

        [Range(1, 365, ErrorMessage = "Срок действия ссылки должен быть не менее 1 и не более 365 дней")]
        public int Days_of_active { get; set; }

        
        public string? Text_content { get; set; }
        public string? File_name { get; set; }

    }
}
