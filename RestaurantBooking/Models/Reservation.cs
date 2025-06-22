using Microsoft.AspNetCore.Identity;

namespace RestaurantBooking.Models
{
    public class Reservation
    {
        public int ID { get; set; }
        public string CustomerID { get; set; } // Foreign key to AspNetUsers
        public int RestaurantID { get; set; }
        public DateTime Date { get; set; }
        public int NumberOfPeople { get; set; }
        public string Status { get; set; } // Pending, Confirmed, Cancelled

        public Restaurant Restaurant { get; set; }

        // Navigation property for the relationship
        public IdentityUser Customer { get; set; }
    }
}
