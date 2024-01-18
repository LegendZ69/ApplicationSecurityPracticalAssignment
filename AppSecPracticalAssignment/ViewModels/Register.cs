using System.ComponentModel.DataAnnotations;

namespace AppSecPracticalAssignment.ViewModels
{
    public class Register
    {
        [Required(ErrorMessage = "First Name is required")]
        [RegularExpression(@"^[a-zA-Z-' ]+$", ErrorMessage = "First Name must be letters or (-' )")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [RegularExpression(@"^[a-zA-Z-' ]+$", ErrorMessage = "Last Name must be letters or (-' )")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Credit Card Number is required")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Credit Card Number must be 16 digits")]
        [DataType(DataType.CreditCard)]
        public string CreditCardNo { get; set; }

        [Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^(?:\+\d{2})?[89]\d{7}$", ErrorMessage = "Mobile Number must be 8 digits and starts with either 8 or 9 with optional +65")]
        [DataType(DataType.PhoneNumber)]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Billing Address is required")]
        [RegularExpression(@"^[\w\d\s#,.-]+$", ErrorMessage = "Billing Address must be letters, digits or (#,.-)")]
        [DataType(DataType.Text)]
        public string BillingAddress { get; set; }

        [Required(ErrorMessage = "Shipping Address is required")]
        [RegularExpression(@"^[\w\d\s#,.-]+$", ErrorMessage = "Shipping Address must be letters, digits or (#,.-)")]
        [DataType(DataType.Text)]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Email Address is required")]
        [MaxLength(254, ErrorMessage = "Email Address must be at most 254 characters")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Please enter a valid Email Address")]
        [RegularExpression(@"^(?=.{1,254}$)(?=.{1,63}@)(?![.])[\w!#$%&'*+\-/=?^_`{|}~]+(\.[\w!#$%&'*+\-/=?^_`{|}~]+)*@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*(\.[a-zA-Z]{2,})$", ErrorMessage = "Email must be between 1 and 254 characters and does not contain dangerous characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
        [MaxLength(64, ErrorMessage = "Password must be at most 64 characters")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{12,64}$", ErrorMessage = "Password must be between 12 and 64 characters and contain at least an uppercase, lowercase, digit and symbol")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Photo is required")]
        [DataType(DataType.Upload)]
        [RegularExpression(@"^.+\.(jpg|JPG|jpeg|JPEG)$", ErrorMessage = "Photo must be in JPG or JPEG format")]
        public string Photo { get; set; }
    }
}
