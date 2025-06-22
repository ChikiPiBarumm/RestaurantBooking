# 🍽️ Restaurant Booking System

This is a web-based **Restaurant Booking System** built with the **ASP.NET MVC** Framework. It allows users to browse available time slots, make reservations, and manage bookings. Admin users can manage tables, view reservations, and configure settings.

---

## 📌 Features

- User registration and authentication
- Browse and book available restaurant tables
- Admin panel for managing reservations and tables
- Responsive UI with MVC pattern
- Entity Framework Core for data access

---

## 🛠️ Tech Stack

- ASP.NET MVC
- Entity Framework Core
- SQL Server
- Bootstrap

---

## 🚀 Getting Started

### Prerequisites

- [.NET SDK 7.0 or later](https://dotnet.microsoft.com/)
- SQL Server (local or remote instance)

---

### ⚙️ Setup Instructions

1. **Clone the repository**

   ```bash
   git clone https://github.com/yourusername/restaurant-booking-system.git
   cd restaurant-booking-system

2. **Update the connection string**

Open appsettings.json and update the DefaultConnection string to match your SQL Server settings:

   ```bash
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DB_NAME;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. **Apply migrations and create the database**

Run the following command in the terminal:

   ```bash
   dotnet ef database update
   ```  
Note: If Entity Framework tools are not installed, install them with:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. **Run the application**

    ```bash
    dotnet run
    
Open your browser and go to https://localhost:5001 or the URL shown in the terminal.
