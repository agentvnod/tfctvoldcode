using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Helpers;

namespace TFCTV.Models
{
    public class SignInModel
    {
        [Required]
        [EmailValidation(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class SignUpModel
    {
        [Required]
        [EmailValidation(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Country")]
        public string CountryCode { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "City")]
        public string City { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class SignToGigyaModel
    {
        public SignInModel SignIn { get; set; }

        public SignUpModel SignUp { get; set; }
    }
}