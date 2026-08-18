namespace Staybnb.Web.Models;

public enum ApplicationStatus
{
    Pending,
    Approved,
    Rejected
}

public enum BookingStatus
{
    Pending,
    Approved,
    Rejected,
    CheckedIn,
    CheckedOut,
    Completed,
    Cancelled
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}

public enum CheckInStatus
{
    NotStarted,
    InProgress,
    Submitted,
    Verified
}

public enum DocumentStatus
{
    Pending,
    Verified,
    Rejected
}

public enum ActivityType
{
    Login,
    RoleChange,
    BookingStatusUpdate,
    HostApplication,
    PropertyChange,
    Other
}

public enum NotificationType
{
    BookingUpdate,
    HostApplicationUpdate,
    NewMessage,
    System
}
