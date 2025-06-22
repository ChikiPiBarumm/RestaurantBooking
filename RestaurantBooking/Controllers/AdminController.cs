using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantBooking.Data;
using RestaurantBooking.Models;
using RestaurantBooking.ViewModels;

namespace RestaurantBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }


        public async Task<IActionResult> CreateUser()
        {
            ViewBag.Restaurants = await _context.Restaurants
                .Select(r => new SelectListItem
                {
                    Value = r.ID.ToString(),
                    Text = r.Name
                })
                .ToListAsync();

            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);

                    if (model.Role == "Staff" && model.RestaurantId.HasValue)
                    {
                        var staffAssignment = new StaffAssignment
                        {
                            StaffID = user.Id,
                            RestaurantID = model.RestaurantId.Value
                        };

                        _context.StaffAssignments.Add(staffAssignment);
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(ManageUsers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Restaurants = await _context.Restaurants
                .Select(r => new SelectListItem
                {
                    Value = r.ID.ToString(),
                    Text = r.Name
                })
                .ToListAsync();

            return View(model);
        }

        // Edit User
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.Email = model.Email;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ManageUsers));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // Delete User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ManageUsers));
            }
            return BadRequest("Error deleting user.");
        }

        // Manage Restaurants
        public async Task<IActionResult> ManageRestaurants()
        {
            var restaurants = await _context.Restaurants.ToListAsync();
            return View(restaurants);
        }

        // Create Restaurant
        public IActionResult CreateRestaurant()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRestaurant(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                _context.Restaurants.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageRestaurants));
            }
            return View(restaurant);
        }

        // Edit Restaurant
        public async Task<IActionResult> EditRestaurant(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();
            return View(restaurant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRestaurant(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                _context.Restaurants.Update(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageRestaurants));
            }
            return View(restaurant);
        }

        // Delete Restaurant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageRestaurants));
        }

        // Manage Reservations
        public async Task<IActionResult> ManageReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Restaurant) 
                .Include(r => r.Customer)  
                .ToListAsync();

            return View(reservations);
        }

        [HttpGet]
        public async Task<IActionResult> CreateReservation()
        {
            ViewBag.Restaurants = await _context.Restaurants.ToListAsync();
            return View(new ReservationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReservation(ReservationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customerId = _userManager.GetUserId(User);

                var reservation = new Reservation
                {
                    CustomerID = customerId, 
                    RestaurantID = model.SelectedRestaurantID,
                    Date = model.SelectedDate,
                    NumberOfPeople = model.NumberOfPeople,
                    Status = "Pending"
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageReservations));
            }

            ViewBag.Restaurants = await _context.Restaurants.ToListAsync();
            return View(model);
        }

        // Edit Reservation
        public async Task<IActionResult> EditReservation(int id)
        {
            var reservation = await _context.Reservations.Include(r => r.Restaurant).FirstOrDefaultAsync(r => r.ID == id);
            if (reservation == null) return NotFound();
            ViewBag.Restaurants = await _context.Restaurants.ToListAsync();
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReservation(Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageReservations));
            }
            ViewBag.Restaurants = await _context.Restaurants.ToListAsync();
            return View(reservation);
        }

        // Delete Reservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageReservations));
        }
    }
}