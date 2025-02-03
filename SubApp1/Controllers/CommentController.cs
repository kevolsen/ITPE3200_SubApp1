using Microsoft.AspNetCore.Mvc;
using SubApp1.DAL;
using SubApp1.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SubApp1.Controllers
{
    [Authorize] // Enforce authorization for all actions in this controller
    public class CommentController : Controller
    {
        // Dependency injection for the comment repository
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger<CommentController> _logger;

        // Constructor to initialize the comment repository
        public CommentController(ICommentRepository commentRepository, ILogger<CommentController> logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        // Action method to add a comment and redirect to the Index page
        [HttpPost]
        public async Task<IActionResult> AddCommentIndex(int postId, string comments)
        {
            // Get the user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Attempted to add a comment without being authenticated.");
                return Unauthorized("You must be logged in to add comments.");
            }
            // Create a new comment object
            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Comments = comments,
                CreatedAt = DateTime.Now
            };

            // Add the comment to the repository
            await _commentRepository.AddCommentAsync(comment);
            _logger.LogInformation($"Comment added successfully by user {userId} on post {postId}.");

            // Redirect to the Index page
            return RedirectToAction("Index", "Home");
        }

        // Action method to add a comment and redirect to the Profile page
        public async Task<IActionResult> AddCommentProfile(int postId, string comments)
        {
            // Get the user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Attempted to add a comment without being authenticated.");
                return Unauthorized("You must be logged in to add comments.");
            }
            // Create a new comment object
            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Comments = comments,
                CreatedAt = DateTime.Now
            };

            // Add the comment to the repository
            await _commentRepository.AddCommentAsync(comment);
             _logger.LogInformation($"Comment added successfully by user {userId} on post {postId}.");
            // Redirect to the Profile page
            return RedirectToAction("Profile", "Home");
        }

        // Action method to edit a comment and redirect to the Index page
        [HttpPost]
        public async Task<IActionResult> EditCommentIndex(int commentId, string comments)
        {
            // Get the comment by ID from the repository
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning($"Attempted to edit a non-existent comment with ID {commentId}.");
                return NotFound("Comment not found.");
            }
            // Update the comment text
            comment.Comments = comments;
            // Save the updated comment to the repository
            await _commentRepository.UpdateCommentAsync(comment);
            _logger.LogInformation($"Comment with ID {commentId} updated successfully.");
            // Redirect to the Index page
            return RedirectToAction("Index", "Home");
        }

        // Action method to edit a comment and redirect to the Profile page
        public async Task<IActionResult> EditCommentProfile(int commentId, string comments)
        {
            // Get the comment by ID from the repository
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning($"Attempted to edit a non-existent comment with ID {commentId}.");
                return NotFound("Comment not found.");
            }
            // Update the comment text
            comment.Comments = comments;
            // Save the updated comment to the repository
            await _commentRepository.UpdateCommentAsync(comment);
            _logger.LogInformation($"Comment with ID {commentId} updated successfully.");
            // Redirect to the Profile page
            return RedirectToAction("Profile", "Home");
        }

        // Action method to delete a comment and redirect to the Index page
        [HttpPost]
        public async Task<IActionResult> DeleteCommentIndex(int commentId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning($"Attempted to delete a non-existent comment with ID {commentId}.");
                return NotFound("Comment not found.");
            }
            // Delete the comment from the repository
            await _commentRepository.DeleteCommentAsync(commentId);
            _logger.LogInformation($"Comment with ID {commentId} deleted successfully.");
            // Redirect to the Index page
            return RedirectToAction("Index", "Home");
        }

        // Action method to delete a comment and redirect to the Profile page
        public async Task<IActionResult> DeleteCommentProfile(int commentId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning($"Attempted to delete a non-existent comment with ID {commentId}.");
                return NotFound("Comment not found.");
            }
            // Delete the comment from the repository
            await _commentRepository.DeleteCommentAsync(commentId);
            _logger.LogInformation($"Comment with ID {commentId} deleted successfully.");
            await _commentRepository.DeleteCommentAsync(commentId);
            // Redirect to the Profile page
            return RedirectToAction("Profile", "Home");
        }
    }
}