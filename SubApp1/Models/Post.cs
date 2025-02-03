using System;
using System.ComponentModel.DataAnnotations; 
using SubApp1.Models;

namespace SubApp1.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserId { get; set; }

        public string? ImageUrl { get; set; }

        public User Users { get; set; } = null!;
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
