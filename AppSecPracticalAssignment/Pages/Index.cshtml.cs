using AppSecPracticalAssignment.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppSecPracticalAssignment.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly UserManager<ApplicationUser> userManager;

        private readonly SignInManager<ApplicationUser> signInManager;

        public IndexModel(ILogger<IndexModel> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task OnGet()
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                ViewData["FirstName"] = protector.Unprotect(user.FirstName);
                ViewData["LastName"] = protector.Unprotect(user.LastName);
                ViewData["CreditCardNo"] = protector.Unprotect(user.CreditCardNo);
                ViewData["MobileNo"] = protector.Unprotect(user.PhoneNumber);
                ViewData["BillingAddress"] = protector.Unprotect(user.BillingAddress);
                ViewData["ShippingAddress"] = protector.Unprotect(user.ShippingAddress);
                ViewData["Email"] = user.Email;
                ViewData["Password"] = user.PasswordHash;
                ViewData["Photo"] = protector.Unprotect(user.Photo);
            }
        }
    }
}