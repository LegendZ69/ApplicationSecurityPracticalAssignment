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
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

        private readonly SignInManager<ApplicationUser> signInManager;

        private readonly ISMSSenderService _sMSSenderService;

		private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public Login LModel { get; set; }
        
        public string ReturnUrl { get; set; }

        public LoginModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ISMSSenderService sMSSenderService,
            ILogger<LoginModel> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _sMSSenderService = sMSSenderService;
            _logger = logger;
        }

        /*
                public async Task OnGetAsync(string returnUrl = null)
                {
                    *//*if (!string.IsNullOrEmpty(ErrorMessage))
                    {
                        ModelState.AddModelError(string.Empty, ErrorMessage);
                    }*//*

                    returnUrl ??= Url.Content("~/");

                    // Clear the existing external cookie to ensure a clean login process
                    await HttpContext.SignOutAsync();

                    ReturnUrl = returnUrl;
                }*/

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync() //OG
        /*        public async Task<IActionResult> OnPostAsync(string returnUrl = null) //addon
        */
        {
/*			returnUrl ??= Url.Content("~/"); //Scaffolding addon
*/
			if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("SSEmail", LModel.Email); //L4 Slide 23
                var identityResult = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, true);
                if (identityResult.Succeeded)
                {
                    var claims = new List<Claim> {
                        /*new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                        new Claim(ClaimTypes.Email, user.Email),*/

                        new Claim(ClaimTypes.Email, LModel.Email),
                        
                        new Claim("Department", "HR")
                    };

                    var i = new ClaimsIdentity(claims, "MyCookieAuth");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);

                    //YT
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = LModel.RememberMe
                    };

                    await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal, authProperties);

					_logger.LogInformation("User logged in.");

                    return RedirectToPage("Index");
                }

                //Scaffolding
                if (identityResult.RequiresTwoFactor)
                {
                    /*					return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = LModel.RememberMe });*/
                    return RedirectToPage("LoginWith2fa", new { RememberMe = LModel.RememberMe });

                }
                if (identityResult.IsLockedOut)
				{
					_logger.LogWarning("User account locked out.");
                    return RedirectToPage("Lockout");
                }
				else
				{
					ModelState.AddModelError(string.Empty, "Invalid login attempt.");
					return Page();
				}
            }
            return Page();
        }


        //reCAPTCHA
		public bool ValidateCaptcha()
		{
			string Response = HttpContext.Request.Form["g-recaptcha-response"]; //Get Response string append to Post Method
			bool Valid = false;

			//Request to Google Server
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=6LcGmTspAAAAAMt2r5WQxxXgWSkS6y25pIEFfn3v&response=" + Response);
			try
			{
				//Google Recaptcha Response
				using (WebResponse wResponse = req.GetResponse())
				{
					using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
					{
						string jsonResponse = readStream.ReadToEnd();

						//L5 error
						/*JavaScriptSerializer js = new JavaScriptSerializer();
                        MyObject data = js.Deserialize<MyObject>(jsonResponse);*/

						var data = JsonSerializer.Deserialize<RecaptchaResponse>(jsonResponse);

						Valid = Convert.ToBoolean(data.success);
					}
				}

				return Valid;
			}
			catch (WebException ex)
			{
				throw ex;
			}
		}

		// Define the RecaptchaResponse class for deserialization
		public class RecaptchaResponse
		{
			public bool success { get; set; }
		}
	}
}
