namespace RestaurantBooking.Models
{
    public class StaffAssignment
    {
        public int ID { get; set; }
        public string StaffID { get; set; }
        public int RestaurantID { get; set; }

        public Restaurant Restaurant { get; set; }
    }
}
