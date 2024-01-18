using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AppSecPracticalAssignment.ViewModels;
using Microsoft.AspNetCore.Identity;
using AppSecPracticalAssignment.Model;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;
using AppSecPracticalAssignment.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;

namespace AppSecPracticalAssignment.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

		private readonly SignInManager<ApplicationUser> signInManager;

		private readonly ILogger<RegisterModel> logger;

        private readonly RoleManager<IdentityRole> roleManager;

        private readonly IEmailSender _emailSender;

        private readonly IUserStore<ApplicationUser> _userStore;

        private readonly IUserEmailStore<ApplicationUser> _emailStore;

        [BindProperty]
        public Register RModel { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IUserStore<ApplicationUser> userStore)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
			this.logger = logger;
            this.roleManager = roleManager;
            _emailSender = emailSender;
            _userStore = userStore;
            _emailStore = GetEmailStore();
		}

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                //addons Scaffolding
                /*var createUser = CreateUser();

                await _userStore.SetUserNameAsync(createUser, RModel.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(createUser, RModel.Email, CancellationToken.None);
                var result = await userManager.CreateAsync(createUser, RModel.Password);*/

                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                var user = new ApplicationUser()
                {
                    UserName = RModel.Email,
                    FirstName = protector.Protect(RModel.FirstName),
                    LastName = protector.Protect(RModel.LastName),
                    CreditCardNo = protector.Protect(RModel.CreditCardNo),
                    PhoneNumber = protector.Protect(RModel.MobileNo),
                    BillingAddress = protector.Protect(RModel.BillingAddress),
                    ShippingAddress = protector.Protect(RModel.ShippingAddress),
                    Email = RModel.Email,
                    Photo = protector.Protect(RModel.Photo),
                    
                    //YT
                    /*SecurityStamp = Guid.NewGuid().ToString(),
                    TwoFactorEnabled = true*/
                };

                IdentityRole role = await roleManager.FindByIdAsync("User");
                if (role == null)
                {
                    IdentityResult result2 = await roleManager.CreateAsync(new IdentityRole("User"));
                    if (!result2.Succeeded)
                    {
                        ModelState.AddModelError("", "Create role user failed");
                    }
                }

                var result = await userManager.CreateAsync(user, RModel.Password);
                if (result.Succeeded)
                {
                    logger.LogInformation("User created a new account with password.");

                    var userId = await userManager.GetUserIdAsync(user);
                    /*var code = await userManager.GenerateEmailConfirmationTokenAsync(user);*/
                    /*code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(RModel.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");*/

                    if (userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = RModel.Email , returnUrl = returnUrl });
                    }
                    else
                    {
                        result = await userManager.AddToRoleAsync(user, "User");
                        logger.LogInformation("Added as User role");

                        await signInManager.SignInAsync(user, false);
                        logger.LogInformation("User signed in");

                        return RedirectToPage("Index");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
