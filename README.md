# 🛒 Full-Stack E-Commerce Platform (ASP.NET MVC)

A modern, feature-rich E-Commerce web application built using **ASP.NET MVC**, **Entity Framework**, and a **Layered Architecture**. It provides a clean, animated UI for customers to browse and shop, and a powerful admin panel for managing the entire system.

---

## 🌐 Features

### 🛍️ User Side
- **Modern, responsive UI** with smooth animations
- Pages: **Home**, **Shop**, **Product Details**, **About Us**, **Contact Us**, **Cart (Popup + Full Page)**
- **Stripe Payment Integration**
- **Authentication**: Internal + External (Google & Facebook)
- **Search, Sort & Pagination** on the Shop page
- **AJAX & jQuery** support for dynamic content
- **Client-side & Server-side Input Validation**

---

### 🔐 Admin Panel  
🔓 *Access via:*  
**Email**: `admin@example.com`  
**Password**: `Admin@123`

#### 📊 Dashboard Includes:
- Total Users
- Today’s Orders
- Revenue
- Pending Issues
- **Line Graph** for sales growth
- **Bar Graph** for order overview
- **Pie Chart** for order status (Pending, Delivered, Cancelled)
- **Top Selling Products**
- Recent Orders

#### 🛠️ Admin Management:
- Users & Roles  
- Products & Categories  
- Transactions  
- Contact Messages (Inbox & Reply System)

---

## ⚙️ Architecture

- **ASP.NET MVC Pattern**
- **Entity Framework** for ORM
- **Repository Pattern** for Data Access Layer
- **Service Layer** for Business Logic
- **Controllers** for Routing & Logic
- **Views & ViewModels** for UI Rendering
- **jQuery & AJAX** for frontend interactivity
- **Logging System** – saves system events to the database
- **Global Error Handling**

---

## 📦 Technologies Used

- ASP.NET MVC 5  
- Entity Framework Core  
- Microsoft Identity (Auth)  
- Stripe API  
- SQL Server  
- Bootstrap 5  
- Chart.js  
- jQuery & AJAX  
- External Auth (Facebook & Google)

---

## 💬 Contact & Email System

- Users can message the admin via the **Contact Us** page  
- Admin can view messages and **reply** via the **Inbox**

---

## 🔍 Search, Sort, and Pagination

- **Admin Panel Index Pages**:  
  🔎 Search | 🔃 Sort | 📄 Pagination  

- **Shop Page**:  
  🔍 Search by Name | 💰 Sort by Price | 📄 Paginated Results  

---

## 🧪 Validation & Error Handling

- ✅ Client-side: Data Annotations + jQuery  
- ✅ Server-side: Handled in Controllers & Services  
- ⚠️ Global Error Handling Middleware  
- 🪵 Full **Logging System** (saved to DB)

---

## Setup Instructions

### Prerequisites
* .NET 9.0 SDK
* Visual Studio 2022 or VS Code
* SQL Server (Local or Express)

### Installation Steps

1. Clone the repository
```bash
git clone https://github.com/ahmedashraf0001/AspNetMVC-ECommercePlatform
```
2. Update the connection string in `appsettings.json` to point to your SQL Server instance
```json
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.;Initial Catalog=E-Commerce-PlatformDB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True",
    "HangfireConnection": "Data Source=.;Initial Catalog=HangfireDB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True"
  },
```
3. Configure your Stripe API keys in `appsettings.json`:
```json
  "Stripe": {
    "SecretKey": "your_secret_key",
    "Domain": "https://localhost:7048"
  }
```

5. Configure Google OAuth in `appsettings.json`:
```json
  "Authentication": {
    "Google": {
      "ClientId": "your_secret_key",
      "ClientSecret": "your_secret_key"
    },
    "Facebook": {
      "AppId": "your_secret_key",
      "AppSecret": "your_secret_key"
    }
  }
```
6. Navigate to the project directory
```bash
cd E-Commerce-Platform\E-Commerce-Platform
```
7. Restore dependencies
```bash
dotnet restore
```
8. Apply database migrations
```bash
dotnet ef database update
```
9. Run the application
```bash
dotnet run
```
10. The application will seed initial data including:
- Admin user
- Product categories
- Sample products
- Roles

## ✅ Completed Features

- Product Management  
- Stripe Integration  
- Secure Login/Register (incl. Facebook & Google)  
- Admin Dashboard with Visual Statistics  
- Contact & Reply System  
- Full CRUD for Admin  
- Sorting, Filtering, and Pagination  
- Logging & Error Handling  

---

## 📧 Contact

If you have any questions, please use the **Contact Us** page in the application.
