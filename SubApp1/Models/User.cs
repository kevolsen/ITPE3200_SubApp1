using Microsoft.AspNetCore.Identity;
namespace SubApp1.Models
{
	public class User : IdentityUser
	{
		public string? ProfilePic { get; set; }
	}
}