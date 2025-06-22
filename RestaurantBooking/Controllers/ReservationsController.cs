using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBooking.Data;
using RestaurantBooking.Models;
using RestaurantBooking.ViewModels;
using System;

[Authorize]
public class ReservationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ReservationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult GetAvailableTimeSlots(int restaurantId, string date)
    {
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
        {
            return BadRequest("Invalid date format. Please use yyyy-MM-dd.");
        }

        var now = DateTime.Now;
        var maxDate = now.AddMonths(1);

        if (parsedDate.Date < now.Date || parsedDate.Date > maxDate.Date)
        {
            return BadRequest("Reservations can only be made within the next month.");
        }

        var slots = _context.TimeSlots
            .Where(ts => ts.RestaurantID == restaurantId
                         && ts.Date.Date == parsedDate.Date
                         && !ts.IsReserved
                         && (parsedDate.Date > now.Date || ts.StartTime >= now.TimeOfDay.Add(TimeSpan.FromMinutes(30))))
            .OrderBy(ts => ts.StartTime)
            .Select(ts => new
            {
                ts.ID,
                StartTime = ts.StartTime.ToString(@"hh\:mm"),
                EndTime = ts.EndTime.ToString(@"hh\:mm")
            })
            .ToList();

        return Json(slots);
    }

    // GET: Reservations/Create
    public IActionResult Book(int restaurantId)
    {
        var now = DateTime.Now;
        var restaurant = _context.Restaurants.Find(restaurantId);
        if (restaurant == null)
        {
            return NotFound();
        }

        var availableTimeSlots = _context.TimeSlots
            .Where(ts => ts.RestaurantID == restaurantId
                         && ts.Date.Date >= now.Date
                         && ts.Date.Date <= now.Date.AddMonths(1)
                         && !ts.IsReserved
                         && (ts.Date.Date > now.Date || ts.StartTime >= now.TimeOfDay.Add(TimeSpan.FromMinutes(30))))
            .OrderBy(ts => ts.Date)
            .ThenBy(ts => ts.StartTime)
            .ToList();

        var model = new ReservationViewModel
        {
            SelectedRestaurantID = restaurantId,
            SelectedDate = now.Date,
            RestaurantName = restaurant.Name,
            AvailableTimeSlots = availableTimeSlots
        };

        return View(model);
    }

    // POST: Reservations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(ReservationViewModel model)
    {
        var now = DateTime.Now;

        if (model.SelectedDate.Date < now.Date || model.SelectedDate.Date > now.Date.AddMonths(1))
        {
            ModelState.AddModelError("", "Reservations can only be made within the next month.");
        }

        var timeSlot = await _context.TimeSlots.FindAsync(model.SelectedTimeSlot);
        if (timeSlot == null || timeSlot.IsReserved
            || (model.SelectedDate.Date == now.Date && timeSlot.StartTime < now.TimeOfDay.Add(TimeSpan.FromMinutes(30))))
        {
            ModelState.AddModelError("", "The selected time slot is invalid or no longer available.");
        }

        if (ModelState.IsValid)
        {
            var reservation = new Reservation
            {
                CustomerID = _userManager.GetUserId(User),
                RestaurantID = model.SelectedRestaurantID,
                Date = model.SelectedDate.Date.Add(timeSlot.StartTime),
                NumberOfPeople = model.NumberOfPeople,
                Status = "Pending"
            };

            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            timeSlot.IsReserved = true;
            timeSlot.ReservationID = reservation.ID;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyReservations));
        }

        model.RestaurantName = _context.Restaurants.Find(model.SelectedRestaurantID)?.Name;
        model.AvailableTimeSlots = await _context.TimeSlots
            .Where(ts => ts.RestaurantID == model.SelectedRestaurantID
                         && ts.Date.Date >= now.Date
                         && ts.Date.Date <= now.Date.AddMonths(1)
                         && !ts.IsReserved
                         && (ts.Date.Date > now.Date || ts.StartTime >= now.TimeOfDay.Add(TimeSpan.FromMinutes(30))))
            .OrderBy(ts => ts.StartTime)
            .ToListAsync();

        return View(model);
    }

    public IActionResult MyReservations(bool showCancelled = false)
    {
        var userId = _userManager.GetUserId(User);
        var reservations = _context.Reservations
            .Include(r => r.Restaurant)
            .Where(r => r.CustomerID == userId && (showCancelled || r.Status != "Cancelled"))
            .ToList();

        ViewData["ShowCancelled"] = showCancelled;
        return View(reservations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int reservationId)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation == null || reservation.CustomerID != _userManager.GetUserId(User))
        {
            return NotFound();
        }

        if (reservation.Status == "Cancelled")
        {
            return BadRequest("Reservation is already cancelled.");
        }

        reservation.Status = "Cancelled";
        var timeSlot = await _context.TimeSlots.FirstOrDefaultAsync(ts => ts.ReservationID == reservationId);
        if (timeSlot != null)
        {
            timeSlot.IsReserved = false;
            timeSlot.ReservationID = null;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(MyReservations));
    }

    [HttpGet]
    public IActionResult Edit(int reservationId)
    {
        var userId = _userManager.GetUserId(User);
        var reservation = _context.Reservations
            .Include(r => r.Restaurant)
            .FirstOrDefault(r => r.ID == reservationId && r.CustomerID == userId);

        if (reservation == null)
        {
            return NotFound();
        }

        var availableTimeSlots = _context.TimeSlots
            .Where(ts => ts.RestaurantID == reservation.RestaurantID
                         && ts.Date.Date == reservation.Date.Date
                         && !ts.IsReserved)
            .OrderBy(ts => ts.StartTime)
            .ToList();

        var model = new ReservationViewModel
        {
            SelectedRestaurantID = reservation.RestaurantID,
            SelectedDate = reservation.Date.Date,
            NumberOfPeople = reservation.NumberOfPeople,
            RestaurantName = reservation.Restaurant.Name,
            AvailableTimeSlots = availableTimeSlots
        };

        return View(model);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ReservationViewModel model)
    {
        if (ModelState.IsValid)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ID == model.ReservationID && r.CustomerID == _userManager.GetUserId(User));

            if (reservation == null)
            {
                return NotFound();
            }

            // Check if the reservation can be edited
            var now = DateTime.Now;
            if (reservation.Date < now.AddMinutes(30)) // Ensure it's at least 30 minutes before the reservation time
            {
                ModelState.AddModelError("", "You cannot edit a reservation that is less than 30 minutes away.");
            }

            var oldTimeSlot = await _context.TimeSlots
                .FirstOrDefaultAsync(ts => ts.ReservationID == reservation.ID);

            var newTimeSlot = await _context.TimeSlots
                .FirstOrDefaultAsync(ts => ts.ID == model.SelectedTimeSlot && !ts.IsReserved);

            if (newTimeSlot == null)
            {
                ModelState.AddModelError("SelectedTimeSlot", "The selected time slot is no longer available.");
            }
            else
            {
                // Cancel the old reservation
                reservation.Status = "Cancelled";
                if (oldTimeSlot != null)
                {
                    oldTimeSlot.IsReserved = false;
                    oldTimeSlot.ReservationID = null;
                }

                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();

                var newReservation = new Reservation
                {
                    CustomerID = reservation.CustomerID,
                    RestaurantID = reservation.RestaurantID,
                    Date = model.SelectedDate.Date.Add(newTimeSlot.StartTime),
                    NumberOfPeople = model.NumberOfPeople,
                    Status = "Pending"
                };

                await _context.Reservations.AddAsync(newReservation);
                await _context.SaveChangesAsync();

                newTimeSlot.IsReserved = true;
                newTimeSlot.ReservationID = newReservation.ID;
                await _context.SaveChangesAsync();

                return RedirectToAction("MyReservations");
            }
        }

        model.AvailableTimeSlots = await _context.TimeSlots
            .Where(ts => ts.RestaurantID == model.SelectedRestaurantID && ts.Date.Date == model.SelectedDate.Date && !ts.IsReserved)
            .OrderBy(ts => ts.StartTime)
            .ToListAsync();

        return View(model);
    }
}