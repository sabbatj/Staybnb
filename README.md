
Absolutely. Here is the full updated README as plain text, ready to copy directly into your README.txt or README.md:

STAYBNB
=======

PROJECT OVERVIEW
================

Staybnb is a managed property-rental platform inspired by Airbnb, built with
ASP.NET Core MVC, Entity Framework Core, SQL Server, and ASP.NET Core Identity.

The platform supports the complete rental experience, from registering as a
Guest and discovering properties to becoming a Host, managing properties,
processing bookings, completing guest check-in, and maintaining the platform
through secure Admin and SuperAdmin controls.

Staybnb is designed around a clear separation of responsibilities between
Guests, Hosts, Admins, and SuperAdmins while providing a complete property
rental workflow.


CORE FEATURES
=============

1. IDENTITY & ROLE MANAGEMENT
-----------------------------

Staybnb uses role-based access control with four main roles:

- Guest
- Host
- Admin
- SuperAdmin

New users are automatically assigned the Guest role when they register.

Guests can apply to become Hosts through the hosting application workflow.

When an Admin approves a Host Application:

- The Guest role is removed.
- The Host role is assigned.
- The user gains access to the Host dashboard.

A user is therefore treated as either a Guest or a Host for the main
rental experience rather than having both roles simultaneously.

SuperAdmin provides the highest level of administrative access and is
responsible for managing Admin privileges.

The application uses ASP.NET Core role-based authorization to protect
role-specific areas of the platform.


2. HOSTING WORKFLOW
-------------------

Guests can apply to become Hosts by submitting a property application.

The application includes:

- Property title
- Property description
- Price
- Address
- Property image

At least one PropertyImage is required before an application can be submitted.

Once the application is reviewed and approved by an Admin, the user becomes
a Host and receives access to the Host dashboard.

Hosts can:

- Manage property listings
- Activate or deactivate properties
- Review pending booking requests
- Approve booking requests
- Reject booking requests
- Configure property-specific check-in processes
- Define check-in rules
- Define required guest documents


3. GUEST EXPERIENCE
-------------------

Guests can browse available properties through an Airbnb-inspired property
discovery experience.

Property discovery includes:

- Active property listings
- Property galleries
- City filtering
- Property-type filtering
- Property details
- Property pricing

Guests can create bookings by selecting:

- Check-in date
- Check-out date
- Number of travelers

The booking system calculates the total price using:

Total Price =
(Price Per Night x Number of Nights)
+ Cleaning Fee
+ Service Fee

The system also checks property availability before creating a booking.

Approved bookings allow Guests to complete a multi-step check-in process.

The check-in process can require Guests to submit identification documents,
including documents such as:

- ID
- Passport

Submitted documents can then be reviewed and verified by the Host.


4. ADMINISTRATION
-----------------

Staybnb provides a dedicated administrative area for managing the platform.

Admins can:

- Review Host Applications
- Approve Host Applications
- Reject Host Applications
- Monitor users
- Monitor platform activity
- Review Activity Logs

SuperAdmins have additional privileges for user management.

SuperAdmins can find existing Guests and promote them to the Admin role.

This provides a controlled administrative hierarchy while preventing ordinary
users from assigning themselves elevated privileges.


5. COMMUNICATION & NOTIFICATIONS
--------------------------------

Staybnb includes a database-backed messaging system for communication between
users.

Messaging is stored in the database and does not require real-time
communication or SignalR.

Users can:

- Access their inbox
- Read messages
- Reply to messages
- Maintain stored conversations

Staybnb also includes a notification center for important platform events.

Notifications can inform users about events such as:

- Booking Approved
- Booking Rejected
- Booking status updates
- Host Application updates
- New Message
- Other system events


6. ACTIVITY LOGGING
-------------------

Staybnb uses an ActivityLog system to provide transparency and traceability
across important platform actions.

Activity logging includes events such as:

- User logins
- Role changes
- Booking status updates
- Host application events
- Property changes
- Other significant system events

This allows administrators to monitor important security and business
activities within the platform.


USER LIFECYCLE
=============

Staybnb supports a complete user lifecycle from registration through hosting.

1. REGISTRATION

   A new user creates an account and is automatically assigned the Guest role.


2. GUEST EXPERIENCE

   The user can browse properties, filter listings, make bookings, and
   complete the guest check-in process.


3. BECOME A HOST

   A Guest can submit a Host Application containing property details and
   at least one PropertyImage.


4. APPLICATION REVIEW

   An Admin reviews the Host Application and can approve or reject it.


5. HOST APPROVAL

   When the application is approved:

   - The Guest role is removed.
   - The Host role is assigned.
   - The user receives access to the Host dashboard.


6. HOST EXPERIENCE

   The Host can manage properties, manage property availability, review
   booking requests, configure check-in requirements, and manage bookings.


7. ADMINISTRATIVE MANAGEMENT

   Admins manage applications and monitor system activity.

   SuperAdmins have additional control over Admin user management.


BOOKING WORKFLOW
================

1. PROPERTY DISCOVERY

   Guest browses active properties and selects a property.


2. BOOKING DETAILS

   Guest selects:

   - Check-in date
   - Check-out date
   - Number of travelers


3. AVAILABILITY CHECK

   The system verifies that the property is available for the requested dates.


4. PRICE CALCULATION

   The system calculates the booking total using:

   Total Price =
   (Price Per Night x Number of Nights)
   + Cleaning Fee
   + Service Fee


5. BOOKING REQUEST

   A booking request is created with a Pending status.


6. HOST REVIEW

   The Host reviews the booking request.

   The Host can:

   - Approve the booking
   - Reject the booking


7. APPROVED BOOKING

   Once approved, the Guest can begin the check-in workflow.


8. GUEST CHECK-IN

   The Guest completes the required check-in steps and submits the documents
   requested for the property.


9. DOCUMENT VERIFICATION

   The Host reviews the submitted Guest Documents, such as an ID or Passport,
   and verifies the Guest's check-in information.


HOST WORKFLOW
=============

1. GUEST

   The user starts with the Guest role.


2. HOST APPLICATION

   The Guest submits an application containing:

   - Property title
   - Property description
   - Price
   - Address
   - At least one PropertyImage


3. APPLICATION REVIEW

   An Admin reviews the application.

   The application can be:

   REJECTED
   The user remains a Guest.

   APPROVED
   The user transitions from Guest to Host.


4. ROLE TRANSITION

   After approval:

   - Guest role is removed.
   - Host role is assigned.
   - Host access becomes available.


5. HOST DASHBOARD

   The approved Host can access the Host dashboard.


6. PROPERTY MANAGEMENT

   Hosts can:

   - Create and manage property listings
   - View their properties
   - Activate or deactivate properties using IsActive


7. BOOKING MANAGEMENT

   Hosts can:

   - View pending booking requests
   - Approve bookings
   - Reject bookings
   - Manage booking status


8. CHECK-IN CONFIGURATION

   Hosts can configure the check-in process for each property, including:

   - Check-in rules
   - Check-in steps
   - Required Guest Documents


9. GUEST VERIFICATION

   Hosts can review documents submitted by Guests during the check-in
   process and verify the required information.


ADMINISTRATIVE WORKFLOW
=======================

1. ADMIN DASHBOARD

   Admins have access to a central administrative dashboard.


2. APPLICATION REVIEW

   Admins can review Host Applications and decide whether applications
   should be approved or rejected.


3. HOST ROLE TRANSITION

   When an application is approved, the system automatically changes the
   user's role from Guest to Host.


4. USER MANAGEMENT

   SuperAdmins can:

   - Find existing Guests
   - Promote Guests to Admin
   - Manage administrative access


5. SYSTEM AUDIT

   Administrators can monitor ActivityLog records to review important
   security and system events.


SECURITY & ACCESS CONTROL
=========================

Staybnb uses ASP.NET Core Identity and role-based authorization to protect
application functionality.

Role-specific areas are protected so that users only have access to the
features appropriate for their role.

The main access levels are:

GUEST
- Browse properties
- Search and filter properties
- Create bookings
- Complete guest check-in
- Submit required documents
- Use messaging and notifications

HOST
- Manage properties
- Manage property activation
- Review booking requests
- Approve or reject bookings
- Configure check-in processes
- Review guest documents

ADMIN
- Review Host Applications
- Approve or reject applications
- Monitor platform activity
- Review Activity Logs

SUPERADMIN
- Perform administrative user management
- Promote Guests to Admin
- Access elevated administrative functionality


NOTIFICATIONS
=============

Staybnb provides a notification center for important changes within the
platform.

Examples include:

- Booking Approved
- Booking Rejected
- New Message
- Host Application Approved
- Host Application Rejected
- Host Access Removed
- Other system notifications


MESSAGING
=========

Staybnb provides database-backed messaging between users.

Messages are persisted in the application's database, allowing users to
return to their inbox and review previous conversations.

The messaging system supports:

- Inbox
- Reading messages
- Sending messages
- Replying to messages
- Stored conversation history


ACTIVITY LOGGING
================

The ActivityLog system records significant actions throughout Staybnb.

Examples include:

- Login events
- Role changes
- Booking status updates
- Host Application events
- Property changes
- Other important system events

Activity logging provides administrators with an audit trail of important
actions performed within the platform.


TESTING
=======

Staybnb includes an automated xUnit test suite covering the platform's core
functionality.

The tests cover areas including:

- Identity and role management
- Hosting workflow
- Booking availability
- Guest check-in workflow
- Administrative functionality
- Communication
- Activity logging

The current test suite contains:

24 TOTAL TESTS
24 PASSED
0 FAILED
0 SKIPPED

The application and test projects can be built and tested using the .NET CLI.


TECHNOLOGY STACK
================

Backend:

- ASP.NET Core MVC
- C#
- .NET 10
- Entity Framework Core
- SQL Server

Authentication & Security:

- ASP.NET Core Identity
- Role-based authorization
- Guest, Host, Admin, and SuperAdmin roles

Testing:

- xUnit
- Microsoft.NET.Test.Sdk
- Entity Framework Core InMemory


PROJECT STRUCTURE
=================

Staybnb.Web/
|
+-- Controllers/
|   +-- AccountController.cs
|   +-- AdminController.cs
|   +-- BookingController.cs
|   +-- HostController.cs
|   +-- PropertyController.cs
|   +-- ...
|
+-- Models/
|   +-- ApplicationUser.cs
|   +-- Booking.cs
|   +-- HostApplication.cs
|   +-- HostProperty.cs
|   +-- PropertyImage.cs
|   +-- GuestDocument.cs
|   +-- CheckInProcess.cs
|   +-- ActivityLog.cs
|   +-- Message.cs
|   +-- Notification.cs
|   +-- ...
|
+-- Services/
|   +-- UserRoleService.cs
|   +-- ActivityLogService.cs
|   +-- ...
|
+-- Data/
|   +-- ApplicationDbContext.cs
|   +-- ...
|
+-- Views/
|
+-- Constants/
|
+-- Staybnb.Web.Tests/
    +-- Requirement1IdentityTests.cs
    +-- Requirement2HostingTests.cs
    +-- Requirement3GuestTests.cs
    +-- Requirement4AdminTests.cs
    +-- Requirement5Tests.cs
    +-- BookingAvailabilityTests.cs
    +-- CheckInWorkflowTests.cs


GETTING STARTED
===============

1. Restore the project dependencies:

   dotnet restore


2. Build the application:

   dotnet build


3. Run the application:

   dotnet run


4. Run the automated tests:

   dotnet test Staybnb.Web.Tests/Staybnb.Web.Tests.csproj


PROJECT GOAL
============

Staybnb provides a complete property-rental experience while maintaining a
clear separation between Guest, Host, Admin, and SuperAdmin responsibilities.

The platform brings together:

- Property discovery
- Property management
- Host applications
- Booking management
- Availability checking
- Price calculation
- Guest check-in
- Guest document verification
- Administrative management
- Messaging
- Notifications
- Activity logging
- Role-based security

The result is a complete ASP.NET Core MVC property-rental platform designed
around the interaction between Guests, Hosts, and platform administrators.
