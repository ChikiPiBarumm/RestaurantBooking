namespace RestaurantBooking.Models
{
    public class TimeSlot
    {
        public int ID { get; set; }
        public int RestaurantID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsReserved { get; set; }
        public int? ReservationID { get; set; }

        public Restaurant Restaurant { get; set; }
    }
}
