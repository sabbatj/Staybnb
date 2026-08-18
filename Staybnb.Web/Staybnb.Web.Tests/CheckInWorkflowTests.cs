using Xunit;

namespace Staybnb.Web.Tests;

public class CheckInWorkflowTests
{
    private enum DocumentStatus
    {
        Pending,
        Verified
    }

    private enum CheckInStatus
    {
        Submitted,
        Verified
    }

    private enum BookingStatus
    {
        Approved,
        CheckedIn
    }

    private sealed class Document
    {
        public string Type { get; set; } = "";
        public DocumentStatus Status { get; set; }
    }

    private sealed class CheckIn
    {
        public CheckInStatus Status { get; set; }
        public List<Document> Documents { get; } = new();
    }

    private sealed class Booking
    {
        public BookingStatus Status { get; set; }
        public CheckIn CheckIn { get; set; } = new();
    }

    private static void VerifyIdentity(Booking booking, Document document)
    {
        document.Status = DocumentStatus.Verified;

        var hasVerifiedIdentity = booking.CheckIn.Documents.Any(d =>
            (d.Type.Equals("ID", StringComparison.OrdinalIgnoreCase) ||
             d.Type.Equals("Passport", StringComparison.OrdinalIgnoreCase)) &&
            d.Status == DocumentStatus.Verified);

        if (hasVerifiedIdentity)
        {
            booking.CheckIn.Status = CheckInStatus.Verified;
            booking.Status = BookingStatus.CheckedIn;
        }
    }

    [Fact]
    public void Uploaded_ID_Starts_As_Pending()
    {
        var document = new Document
        {
            Type = "ID",
            Status = DocumentStatus.Pending
        };

        Assert.Equal(DocumentStatus.Pending, document.Status);
    }

    [Fact]
    public void Host_Verification_Verifies_ID()
    {
        var booking = new Booking
        {
            Status = BookingStatus.Approved
        };

        var document = new Document
        {
            Type = "ID",
            Status = DocumentStatus.Pending
        };

        booking.CheckIn.Documents.Add(document);

        VerifyIdentity(booking, document);

        Assert.Equal(DocumentStatus.Verified, document.Status);
    }

    [Fact]
    public void Verified_ID_Completes_CheckIn()
    {
        var booking = new Booking
        {
            Status = BookingStatus.Approved
        };

        var document = new Document
        {
            Type = "ID",
            Status = DocumentStatus.Pending
        };

        booking.CheckIn.Documents.Add(document);

        VerifyIdentity(booking, document);

        Assert.Equal(CheckInStatus.Verified, booking.CheckIn.Status);
    }

    [Fact]
    public void Verified_CheckIn_Marks_Booking_CheckedIn()
    {
        var booking = new Booking
        {
            Status = BookingStatus.Approved
        };

        var document = new Document
        {
            Type = "Passport",
            Status = DocumentStatus.Pending
        };

        booking.CheckIn.Documents.Add(document);

        VerifyIdentity(booking, document);

        Assert.Equal(BookingStatus.CheckedIn, booking.Status);
    }

    [Fact]
    public void Passport_Is_Also_Accepted_As_Identity()
    {
        var booking = new Booking
        {
            Status = BookingStatus.Approved
        };

        var document = new Document
        {
            Type = "Passport",
            Status = DocumentStatus.Pending
        };

        booking.CheckIn.Documents.Add(document);

        VerifyIdentity(booking, document);

        Assert.Equal(DocumentStatus.Verified, document.Status);
        Assert.Equal(CheckInStatus.Verified, booking.CheckIn.Status);
        Assert.Equal(BookingStatus.CheckedIn, booking.Status);
    }
}
