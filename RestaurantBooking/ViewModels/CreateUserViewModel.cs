using System.ComponentModel.DataAnnotations;

namespace RestaurantBooking.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        public int? RestaurantId { get; set; }
    }
}