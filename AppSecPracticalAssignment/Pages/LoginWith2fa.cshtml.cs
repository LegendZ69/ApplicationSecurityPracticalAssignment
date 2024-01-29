using AppSecPracticalAssignment.Migrations;
using AppSecPracticalAssignment.Model;
using AppSecPracticalAssignment.Services;
using AppSecPracticalAssignment.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace AppSecPracticalAssignment.Pages
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;

        private readonly IEmailSender _emailSender;

        private readonly ISMSSenderService _smsSenderService;

        private readonly IUserStore<ApplicationUser> _userStore;

        private readonly IUserEmailStore<ApplicationUser> _emailStore;

        [BindProperty]
        public LoginWith2fa L2faModel { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public LoginWith2faModel(SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginWith2faModel> logger,
            IEmailSender emailSender,
            ISMSSenderService smsSenderService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _smsSenderService = smsSenderService;
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

/*            ReturnUrl = returnUrl;
*/            RememberMe = rememberMe;

            return Page();
        }

        /*        public void OnGet() { }
        */
        public async Task<IActionResult> OnPostAsync(bool rememberMe)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            /*            returnUrl = returnUrl ?? Url.Content("~/");
            */

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = L2faModel.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            /*            var result = await _signInManager.TwoFactorSignInAsync(L2faModel.TwoFactAuthProviderName, authenticatorCode, rememberMe, L2faModel.RememberMe);
            */
            var result = await _signInManager.TwoFactorSignInAsync(L2faModel.TwoFactAuthProviderName, authenticatorCode, false, false);

            var userId = await _userManager.GetUserIdAsync(user);

            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            if (providers.Any(_ => _ == "Phone"))
            {
                L2faModel.TwoFactAuthProviderName = "Phone";
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");

                await _smsSenderService.SendSMSAsync(user.PhoneNumber, $"OTP: {token}");
            }
            else if (providers.Any(_ => _ == "Email"))
            {
                L2faModel.TwoFactAuthProviderName = "Email";
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                await _emailSender.SendEmailAsync(user.Email, "Email 2FA Code", $"<h1>{token}</h1>");
            }

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return RedirectToPage("Index");
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
/*                ModelState.AddModelError(string.Empty, "Account Locked");
*/                return RedirectToPage("Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return Page();
            }
        }
    }
}
