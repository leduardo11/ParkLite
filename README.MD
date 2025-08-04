# ParkLite

Senior Software Engineer Case Study for FlashParking  
2500 Bee Caves Rd  
Bldg III Ste 400  
Austin, TX 78746

---

## Overview

ParkLite is a simple parking account management system for condominium residents. Each account may include multiple contacts and vehicles. The app provides a .NET 8 back-end API with a modern React frontend served from the same origin for security.

---

## Features

- Full CRUD for Accounts, Contacts, and Vehicles
- SQLite database (no ORM)
- Inline SQL / stored procedures
- RESTful API with pagination and search
- Responsive React UI with Bootstrap
- SPA served via ASP.NET `wwwroot` (no CORS)
- Vehicle photo upload (Base64)
- Batch deactivation via recursive CTE
- Unit tests for service layer

---

## Tech Stack

### Backend
- **.NET 8**, ASP.NET Core Web API
- **SQLite** (raw SQL, no ORM)

### Frontend
- **React 19**
- **Vite 7**
- **React Router DOM 7**
- **Bootstrap 5**

### Tooling
- **React Query 5**
- **Axios**
- **TypeScript 5**
- **ESLint**  
- **Docker** (multi-stage image)

