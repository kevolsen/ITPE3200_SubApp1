using Microsoft.EntityFrameworkCore;
using SubApp1.Models;

namespace SubApp1.DAL
{
    public class PostRepository : IPostRepository
    {
        private readonly UserDbContext _context;

        public PostRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task AddPostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task<Post?> GetPostByIdAsync(int id)
        {
            return await _context.Posts.FindAsync(id);
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(Post post)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }
}
