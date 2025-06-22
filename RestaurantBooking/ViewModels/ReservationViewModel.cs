using Microsoft.AspNetCore.Mvc.ModelBinding;
using RestaurantBooking.Models;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBooking.ViewModels
{
    public class ReservationViewModel
    {
        [Required]
        public int ReservationID { get; set; }

        [Required]
        public int SelectedRestaurantID { get; set; }

        [Required]
        public DateTime SelectedDate { get; set; }

        [Required]
        public int SelectedTimeSlot { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of people must be at least 1.")]
        public int NumberOfPeople { get; set; }

        [BindNever]
        public string? RestaurantName { get; set; }

        [BindNever]
        public List<TimeSlot>? AvailableTimeSlots { get; set; } = new List<TimeSlot>();
    }
}
