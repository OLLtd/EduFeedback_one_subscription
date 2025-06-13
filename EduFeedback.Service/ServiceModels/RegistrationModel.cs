using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.ServiceModels
{
    public class RegistrationModel
    {
        public int User_ID { get; set; }

        [StringLength(200)]
        [Required(ErrorMessage = "Parent full name is required.")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Please use alphabets only")]
        public string FullName { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Parent first name is required.")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Please use alphabets only")]
        public string FirstName { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Parent last name is required.")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Please use alphabets only")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "The email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        // [Remote("CheckForDuplication", "Account", HttpMethod = "POST", ErrorMessage = "Sorry, this ID already exists")]
        public string Email_ID { get; set; }

        //[DataType(DataType.EmailAddress)]
        //[Required(ErrorMessage = "The email address is required.")]
        //[EmailAddress(ErrorMessage = "Please enter a valid email address")]
        //public string UpdateEmail { get; set; }


        [RegularExpression(@"^\d{7,11}$", ErrorMessage = "Not a valid phone number")]
        public string PhoneNo { get; set; }

        [Required(ErrorMessage = "Please select student school year.")]
        public int Year_ID { get; set; }

        public List<SchoolYearModel> SchoolYearList { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public string Type { get; set; }

        public bool Status { get; set; }

        [Display(Name = "User name")]
        public string UserName { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        //[Required(ErrorMessage = "Password is required.")]
        //[StringLength(32, ErrorMessage = "The {0} can be maximum of 32 characters long.", MinimumLength = 6)]
        //[RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{6,}$", ErrorMessage = "Password must be at least 6 characters long atleast one alphabet and including one digit.")]
        public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm password")]
        //[Required(ErrorMessage = "Confirm password is required.")]
        //[System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string EmailCode { get; set; }
        public int OrganisationId { get; set; }
    }
}
