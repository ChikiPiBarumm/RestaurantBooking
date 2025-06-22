using RestaurantBooking.Models;

namespace RestaurantBooking.Data
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;

        public DatabaseSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public void SeedTimeSlots()
        {
            var startDate = DateTime.Now.Date;
            var endDate = startDate.AddMonths(1);

            var timeSlotDuration = TimeSpan.FromMinutes(30);
            var openingTime = TimeSpan.FromHours(11); // Example: 11 AM
            var closingTime = TimeSpan.FromHours(23); // Example: 11 PM

            var restaurants = _context.Restaurants.ToList();

            foreach (var restaurant in restaurants)
            {
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    for (var time = openingTime; time < closingTime; time += timeSlotDuration)
                    {
                        var startTime = time;
                        var endTime = time + timeSlotDuration;

                        // Check if the time slot already exists
                        if (!_context.TimeSlots.Any(ts => ts.RestaurantID == restaurant.ID && ts.Date == date && ts.StartTime == startTime))
                        {
                            var timeSlot = new TimeSlot
                            {
                                RestaurantID = restaurant.ID,
                                Date = date,
                                StartTime = startTime,
                                EndTime = endTime,
                                IsReserved = false,
                                ReservationID = null
                            };

                            _context.TimeSlots.Add(timeSlot);
                        }
                    }
                }
            }

            _context.SaveChanges();
        }
    }
}
