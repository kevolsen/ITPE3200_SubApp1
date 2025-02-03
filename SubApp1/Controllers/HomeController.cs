using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SubApp1.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SubApp1.Controllers;

public class HomeController : Controller
{
    // Logger for logging information
    private readonly ILogger<HomeController> _logger;
        // Database context for accessing the database
    private readonly UserDbContext _context;

    // Constructor to initialize the logger and database context
    public HomeController(ILogger<HomeController> logger, UserDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Action method to display the Index page
    public IActionResult Index()
    {
        // Retrieve posts from the database, including related users and comments
        var posts = _context.Posts
            .Include(p => p.Users) // Include the related Users (for the post author)
            .Include(p => p.Comments) // Include the related Comments
            .ThenInclude(c => c.Users) // Optionally, include the user who made the comment
            .OrderByDescending(p => p.CreatedAt) // Order posts by creation date
            .ToList();

        if (posts == null || !posts.Any())
        {
            _logger.LogWarning("No posts found for the Index view.");
            return NotFound("No posts available.");
        }

        // Pass the posts (with users and comments) to the view
        return View(posts);
    }

    // Action method to display the Profile page
    public IActionResult Profile()
    {
        // Get the user ID from the claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // If user ID is not found, return Unauthorized
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Unauthorized access to Profile page.");
            return Unauthorized("You must be logged in to access this page.");
        }

        // Retrieve posts by the user from the database, including related users and comments
        var userPosts = _context.Posts
            .Where(p => p.UserId == userId)
            .Include(p => p.Users)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Users)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
        if (!userPosts.Any())
        {
            _logger.LogInformation($"User with ID {userId} has no posts.");
        }

        // Retrieve the user from the database
        var user = _context.Users?.Find(userId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            return NotFound();
        }

        // Get the profile picture URL of the user
        var profilePicUrl = user.ProfilePic ?? "/Images/default-profile.jpg";

        // Pass the profile picture URL to the view
        ViewData["ProfilePicUrl"] = profilePicUrl;

        // Pass the user's posts to the view
        return View(userPosts);
    }
    
    // Action method to display the Error page
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Pass the error information to the view
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}