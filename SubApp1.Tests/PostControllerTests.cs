using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using SubApp1.Controllers;
using SubApp1.DAL;
using SubApp1.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

public class PostControllerTests
{
    private readonly Mock<IPostRepository> _mockRepo;
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly Mock<ILogger<PostController>> _mockLogger;
    private readonly PostController _controller;

    public PostControllerTests()
    {
        _mockRepo = new Mock<IPostRepository>();
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<PostController>>();
        _mockEnv.Setup(env => env.WebRootPath).Returns("wwwroot");
        _controller = new PostController(_mockRepo.Object, _mockEnv.Object, _mockLogger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "test-user-id")
                }, "mock"))
            }
        };

        // Ensure the directory exists
        Directory.CreateDirectory(Path.Combine("wwwroot", "Images"));
    }

    [Fact]
    public async Task CreatePostProfile_ValidPost_ReturnsRedirectToAction()
    {
        // Arrange
        var postContent = "Test Post Content";
        var postImage = new Mock<IFormFile>();
        postImage.Setup(f => f.FileName).Returns("test.jpg");
        postImage.Setup(f => f.Length).Returns(1);
        postImage.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreatePostProfile(postContent, postImage.Object);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Profile", redirectToActionResult.ActionName);
    }

    [Fact]
    public async Task CreatePostProfile_InvalidPost_ReturnsViewWithModel()
    {
        // Arrange
        var postContent = "";
        var postImage = new Mock<IFormFile>();
        postImage.Setup(f => f.FileName).Returns("test.jpg");
        postImage.Setup(f => f.Length).Returns(1);
        postImage.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreatePostProfile(postContent, postImage.Object);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(postContent, ((Post?)viewResult.Model)?.Content);
    }

    [Fact]
    public async Task GetPost_ValidId_ReturnsViewWithModel()
    {
        // Arrange
        var postId = 1;
        var post = new Post { Id = postId, Content = "Test Post" };
        _mockRepo.Setup(repo => repo.GetPostByIdAsync(postId)).ReturnsAsync(post);

        // Act
        var result = await _controller.GetPost(postId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(post, viewResult.Model);
    }

    [Fact]
    public async Task GetPost_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var postId = 1;
        _mockRepo.Setup(repo => repo.GetPostByIdAsync(postId)).ReturnsAsync((Post?)null);

        // Act
        var result = await _controller.GetPost(postId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_ValidPost_ReturnsRedirectToAction()
    {
        // Arrange
        var postId = 1;
        var post = new Post { Id = postId, Content = "Updated Content", UserId = "test-user-id" };
        _mockRepo.Setup(repo => repo.GetPostByIdAsync(postId)).ReturnsAsync(post);
        _mockRepo.Setup(repo => repo.UpdatePostAsync(post)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EditPost(postId, post.Content, null);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Profile", redirectToActionResult.ActionName);
        Assert.Equal("Home", redirectToActionResult.ControllerName);
    }

    [Fact]
    public async Task EditPost_InvalidPost_ReturnsUnauthorized()
    {
        // Arrange
        var postId = 1;
        var post = new Post { Id = postId, Content = "Updated Content", UserId = "different-user-id" };
        _mockRepo.Setup(repo => repo.GetPostByIdAsync(postId)).ReturnsAsync(post);

        // Act
        var result = await _controller.EditPost(postId, post.Content, null);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task DeletePost_ValidId_ReturnsRedirectToAction()
    {
        // Arrange
        var postId = 1;
        var post = new Post { Id = postId, UserId = "test-user-id" };
        _mockRepo.Setup(repo => repo.GetPostByIdAsync(postId)).ReturnsAsync(post);
        _mockRepo.Setup(repo => repo.DeletePostAsync(post)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeletePost(postId);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Profile", redirectToActionResult.ActionName);
        Assert.Equal("Home", redirectToActionResult.ControllerName);
    }

    [Fact]
    public async Task DeletePost_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var postId = 1;
        _mockRepo.Setup(repo => repo.GetPostByIdAsync(postId)).ReturnsAsync((Post?)null);
        _mockRepo.Setup(repo => repo.DeletePostAsync(It.IsAny<Post?>())).ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.DeletePost(postId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
