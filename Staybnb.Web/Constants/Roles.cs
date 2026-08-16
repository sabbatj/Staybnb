namespace Staybnb.Web.Constants;

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Host = "Host";
    public const string Guest = "Guest";

    public static readonly string[] All = { SuperAdmin, Admin, Host, Guest };
}
