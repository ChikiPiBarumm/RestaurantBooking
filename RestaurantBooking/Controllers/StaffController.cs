using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBooking.Data;
using RestaurantBooking.Models;

namespace RestaurantBooking.Controllers
{
    [Authorize(Roles = "Staff,Admin")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; 

        public StaffController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }

        public async Task<IActionResult> Index(string status = "Active", bool showCancelled = false)
        {
            var userEmail = User.Identity.Name;

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound("User  not found.");
            }

            var staffId = user.Id;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            IQueryable<Reservation> reservationsQuery;

            if (isAdmin)
            {
                reservationsQuery = _context.Reservations
                    .Include(r => r.Restaurant)
                    .Include(r => r.Customer);
            }
            else
            {
                var assignedRestaurantIds = await _context.StaffAssignments
                    .Where(sa => sa.StaffID == staffId)
                    .Select(sa => sa.RestaurantID)
                    .ToListAsync();

                reservationsQuery = _context.Reservations
                    .Include(r => r.Restaurant)
                    .Include(r => r.Customer)
                    .Where(r => assignedRestaurantIds.Contains(r.RestaurantID));
            }

            if (status == "Active")
            {
                reservationsQuery = reservationsQuery.Where(r => r.Status == "Pending" || r.Status == "Confirmed");
            }
            else if (status == "Cancelled")
            {
                reservationsQuery = reservationsQuery.Where(r => r.Status == "Cancelled");
            }

            if (showCancelled)
            {
                reservationsQuery = reservationsQuery.Union(
                    _context.Reservations
                        .Include(r => r.Restaurant)
                        .Include(r => r.Customer)
                        .Where(r => r.Status == "Cancelled")
                );
            }

            var reservations = await reservationsQuery.ToListAsync();

            ViewBag.SelectedStatus = status;
            ViewBag.ShowCancelled = showCancelled;

            return View(reservations);
        }


        // POST: Staff/ConfirmReservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var userEmail = User.Identity.Name;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var staffId = user.Id;

            if (!isAdmin)
            {
                // Check if the staff member is authorized to confirm this reservation
                var isAuthorized = await _context.StaffAssignments
                    .AnyAsync(sa => sa.StaffID == staffId && sa.RestaurantID == reservation.RestaurantID);

                if (!isAuthorized)
                {
                    return Forbid();
                }
            }

            reservation.Status = "Confirmed";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Staff/CancelReservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var userEmail = User.Identity.Name;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var staffId = user.Id;

            if (!isAdmin)
            {
                var isAuthorized = await _context.StaffAssignments
                    .AnyAsync(sa => sa.StaffID == staffId && sa.RestaurantID == reservation.RestaurantID);

                if (!isAuthorized)
                {
                    return Forbid();
                }
            }

            reservation.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}