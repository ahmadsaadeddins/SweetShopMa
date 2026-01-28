namespace SweetShopMa.Utils;

/// <summary>
/// Constants for user roles and permissions.
/// </summary>
public static class RoleConstants
{
    public const string Developer = "Developer";
    public const string Admin = "Admin";
    public const string Moderator = "Moderator";
    public const string User = "User";

    public static readonly string[] AllRoles = { Developer, Admin, Moderator, User };
}
