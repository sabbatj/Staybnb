Sure — here is the full README content in plain TXT format, so you can copy and paste it directly into README.md:

STAYBNB
=======

Staybnb is a managed property-rental platform inspired by Airbnb, built with ASP.NET Core MVC, Entity Framework Core, and SQL Server.

The platform supports the complete lifecycle of a rental user: discovering properties as a Guest, applying to become a Host, managing properties and bookings as a Host, and maintaining the platform through secure Admin and SuperAdmin controls.


CORE FEATURES
=============

1. IDENTITY & ROLE MANAGEMENT
-----------------------------

Staybnb uses role-based access control with four roles:

- Guest – Default role for newly registered users.
- Host – Users approved to manage rental properties.
- Admin – Users responsible for managing applications and platform activity.
- SuperAdmin – Highest-level administrative role responsible for managing Admin access.

New users are automatically registered as Guests.

When a Host application is approved, the user's Guest role is removed and the Host role is assigned.

Role-protected areas use ASP.NET Core authorization to ensure users can only access functionality appropriate to their role.


2. HOSTING
----------

Guests can apply to become Hosts by submitting a property application containing:

- Property title
- Description
- Price
- Address
- Property images

A property application requires at least one image before it can be submitted.

Once approved, Hosts receive access to a dedicated Host dashboard where they can:

- Manage their property listings
- Activate or deactivate properties
- Review booking requests
- Approve or reject bookings
- Configure property-specific check-in processes
- Define check-in rules
- Define required guest documents


3. GUEST EXPERIENCE
-------------------

Guests can discover available properties through an Airbnb-inspired browsing experience.

Property discovery includes:

- Active property listings
- Property galleries
- City filtering
- Property-type filtering
- Property details
- Property pricing

Guests can make bookings by selecting:

- Check-in date
- Check-out date
- Number of travelers

The booking price is calculated using:

Total Price = (Price Per Night x Number of Nights) + Cleaning Fee + Service Fee

Approved bookings also support a multi-step guest check-in process.

Guests can submit required identification documents, such as an ID or passport, for Host verification.


4. ADMINISTRATION
-----------------

Staybnb provides a dedicated Admin dashboard for managing the platform.

Administrators can:

- Review Host and property applications
- Approve applications
- Reject applications
- Monitor users
- Monitor platform activity
- Review system activity logs

SuperAdmins have additional privileges to promote existing Guests to Admin users.


5. COMMUNICATION & NOTIFICATIONS
--------------------------------

Staybnb includes a database-backed communication system that allows users to communicate through an inbox.

Users can:

- Read messages
- Reply to messages
- Maintain conversations through stored messages

The platform also includes a notification center for important events such as:

- Booking approvals
- Booking status changes
- Host application updates
- New messages
- Other system notifications


6. ACTIVITY LOGGING
-------------------

Important system events are recorded through the ActivityLog system to provide transparency and traceability.

Logged activities include:

- User logins
- Role changes
- Booking status updates
- Host application events
- Property-related changes
- Other significant system events


TECHNOLOGY STACK
================

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Role-based authorization
- xUnit
- .NET 10


PROJECT ARCHITECTURE
====================

The application follows a structured ASP.NET Core MVC architecture.

Staybnb.Web/
|
+-- Controllers/
+-- Models/
+-- Views/
+-- Services/
+-- Data/
+-- Constants/
+-- Staybnb.Web.Tests/
+-- Program.cs
+-- appsettings.json


USER LIFECYCLE
==============

The main Staybnb user journey is:

Register
   |
   v
Guest
   |
   v
Apply to Become a Host
   |
   v
Admin Review
   |
   v
Approved
   |
   v
Host
   |
   v
Manage Properties and Bookings


Administrators and SuperAdmins provide the additional layer of platform management and security.


BOOKING WORKFLOW
================

Guest discovers an active property
   |
   v
Selects dates and number of travelers
   |
   v
System checks availability
   |
   v
System calculates total price
   |
   v
Booking request is created
   |
   v
Host reviews booking
   |
   +----> Reject
   |
   +----> Approve
             |
             v
       Guest completes
       check-in workflow
             |
             v
       Guest submits required
       identification documents
             |
             v
       Host verifies documents


HOST WORKFLOW
=============

Guest
   |
   v
Submit Host Application
   |
   v
Property details + Property Image
   |
   v
Admin Review
   |
   +----> Rejected
   |
   +----> Approved
             |
             v
       Guest role removed
             |
             v
       Host role assigned
             |
             v
       Host Dashboard
             |
             +--> Manage Properties
             |
             +--> Activate/Deactivate Properties
             |
             +--> Review Bookings
             |
             +--> Approve/Reject Bookings
             |
             +--> Configure Check-In Process


SECURITY & ACCESS CONTROL
=========================

Staybnb uses ASP.NET Core Identity and role-based authorization to protect application functionality.

Different areas of the platform are restricted according to the user's role.

Guest functionality is protected from unauthorized Host access, while Host and administrative functionality is restricted from ordinary Guests.

The platform separates permissions between:

- Guest
- Host
- Admin
- SuperAdmin

This ensures that users only have access to the functionality appropriate for their role.


TESTING
=======

Staybnb includes automated tests covering the main identity, hosting, guest, administration, check-in, booking, and communication functionality.

The test suite provides regression protection for the platform's core functionality.

Tests include coverage for:

- Identity and role management
- Hosting workflow
- Booking availability
- Guest check-in workflow
- Administrative functionality
- Communication and activity logging

The current test suite contains 24 automated tests.


GETTING STARTED
===============

Restore the project dependencies:

dotnet restore


Build the application:

dotnet build


Run the application:

dotnet run


Run the automated tests:

dotnet test Staybnb.Web.Tests/Staybnb.Web.Tests.csproj


PROJECT GOAL
============

Staybnb is designed to provide a complete property-rental experience while maintaining a clear separation between Guest, Host, Admin, and SuperAdmin responsibilities.

The platform brings together property discovery, booking, hosting, guest check-in, administration, messaging, notifications, and activity tracking into a single ASP.NET Core MVC application.
