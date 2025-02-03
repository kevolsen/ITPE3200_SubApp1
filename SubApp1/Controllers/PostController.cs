using Microsoft.AspNetCore.Mvc;
using SubApp1.DAL;
using SubApp1.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SubApp1.Controllers
{
    [Authorize] // Enforce authorization for all actions in this controller
    public class PostController : Controller
    {
        // Dependency injection for the post repository and web host environment
        private readonly IPostRepository _postRepository;
        private readonly IWebHostEnvironment _env;
         private readonly ILogger<PostController> _logger;

        // Constructor to initialize the post repository and web host environment
         public PostController(IPostRepository postRepository, IWebHostEnvironment env, ILogger<PostController> logger)
        {
            _postRepository = postRepository;
            _env = env;
            _logger = logger;
        }

        // Action method to create a post and redirect to the Profile page
[HttpPost]
public async Task<IActionResult> CreatePostProfile(string PostContent, IFormFile PostImage)
{
    if (string.IsNullOrEmpty(PostContent))
    {
        // If PostContent is null or empty, return the form view with a validation error.
        ModelState.AddModelError("Content", "Content is required.");
        return View(new Post { Content = PostContent });
    }

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        _logger.LogWarning("Unauthorized attempt to create a post.");
        return Unauthorized("You must be logged in to create a post.");
    }
            // Create a new post object
            var post = new Post
            {
                Content = PostContent,
                CreatedAt = DateTime.Now,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            };

            // If an image is uploaded, save it to the server and set the image URL
            if (PostImage != null)
            {
                var fileName = Path.Combine(_env.WebRootPath, "Images", Path.GetRandomFileName() + Path.GetExtension(PostImage.FileName));
                using (var fileStream = new FileStream(fileName, FileMode.Create))
                {
                    await PostImage.CopyToAsync(fileStream);
                }
                post.ImageUrl = "/Images/" + Path.GetFileName(fileName);
            }
            // Add the post to the repository
            await _postRepository.AddPostAsync(post);
             _logger.LogInformation($"Post created successfully by user {userId}.");
            // Redirect to the Profile page
            return RedirectToAction("Profile", "Home");
        }

        // Action method to get a post by ID and display it
        [HttpGet]
        public async Task<IActionResult> GetPost(int id)
        {
            // Retrieve the post by ID from the repository
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                _logger.LogWarning($"Post with ID {id} not found.");
                return NotFound();
            }
            _logger.LogInformation($"Post with ID {id} retrieved successfully.");
            // Pass the post to the view
            return View(post);
        }

        // Action method to display the Edit Post page (authorized users only)
       
        [HttpGet]
        public async Task<IActionResult> EditPost(int id)
        {
            // Retrieve the post by ID from the repository
            var post = await _postRepository.GetPostByIdAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the post exists and if the user is authorized to edit it
            if (post == null || post.UserId != userId)
            {
                _logger.LogWarning($"Unauthorized edit attempt for post ID {id} by user {userId}.");
                return Unauthorized();
            }

            // Pass the post to the Edit Post view
            _logger.LogInformation($"Post with ID {id} opened for editing by user {userId}.");
            return View("~/Views/Home/EditPost.cshtml", post);
        }

        // Action method to edit a post and redirect to the Profile page (authorized users only)
   
        [HttpPost]
        public async Task<IActionResult> EditPost(int id, string postContent, IFormFile? postImage)
        {
            // Retrieve the post by ID from the repository
            var post = await _postRepository.GetPostByIdAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the post exists and if the user is authorized to edit it
            if (post == null || post.UserId != userId)
            {
                _logger.LogWarning($"Unauthorized edit attempt for post ID {id} by user {userId}.");
                return Unauthorized();
            }

            // Update the post content
            post.Content = postContent;

            // If a new image is uploaded, save it to the server and update the image URL
            if (postImage != null && postImage.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                var filePath = Path.Combine(uploads, postImage.FileName);
                post.ImageUrl = $"/uploads/{postImage.FileName}";

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await postImage.CopyToAsync(fileStream);
                }
            }

            // Update the post in the repository
            await _postRepository.UpdatePostAsync(post);
            _logger.LogInformation($"Post with ID {id} updated successfully by user {userId}.");
            // Redirect to the Profile page
            return RedirectToAction("Profile", "Home");
        }

        // Action method to delete a post and redirect to the Profile page (authorized users only)

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            // Retrieve the post by ID from the repository
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                 _logger.LogWarning($"Attempted to delete non-existent post ID {id}.");
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Check if the user is authorized to delete the post
            if (post.UserId != userId)
            {
                _logger.LogWarning($"Unauthorized delete attempt for post ID {id} by user {userId}.");
                return Unauthorized();
            }

            // Delete the post from the repository
            await _postRepository.DeletePostAsync(post);
            _logger.LogInformation($"Post with ID {id} deleted successfully by user {userId}.");
            // Redirect to the Profile page
            return RedirectToAction("Profile", "Home");
        }
    }
}
