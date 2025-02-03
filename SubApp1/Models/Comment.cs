using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubApp1.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? UserId { get; set; } 
        public int PostId { get; set; } 

        public virtual Post Posts { get; set; } = null!;
        public virtual User Users { get; set; } = null!;
    }
}
