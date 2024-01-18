using AppSecPracticalAssignment.Model;
using AppSecPracticalAssignment.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppSecPracticalAssignment.Pages
{
	public class LogoutModel : PageModel
	{
		private readonly UserManager<ApplicationUser> userManager;

		private readonly SignInManager<ApplicationUser> signInManager;

		private readonly ILogger<LogoutModel> _logger;

		public LogoutModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			_logger = logger;
		}

		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostLogoutAsync()
		{
			await signInManager.SignOutAsync();
			return RedirectToPage("Login");
		}

		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}
	}
}
