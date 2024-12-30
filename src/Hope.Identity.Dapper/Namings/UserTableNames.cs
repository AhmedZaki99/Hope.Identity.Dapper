using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user table.
/// </summary>
public class UserTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "Users";

    private static readonly UserTableNames Default = new();


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.Id"/> property.
    /// </summary>
    public string Id { get; set; } = nameof(IdentityUser.Id);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.UserName"/> property.
    /// </summary>
    public string UserName { get; set; } = nameof(IdentityUser.UserName);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.NormalizedUserName"/> property.
    /// </summary>
    public string NormalizedUserName { get; set; } = nameof(IdentityUser.NormalizedUserName);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.Email"/> property.
    /// </summary>
    public string Email { get; set; } = nameof(IdentityUser.Email);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.NormalizedEmail"/> property.
    /// </summary>
    public string NormalizedEmail { get; set; } = nameof(IdentityUser.NormalizedEmail);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.EmailConfirmed"/> property.
    /// </summary>
    public string EmailConfirmed { get; set; } = nameof(IdentityUser.EmailConfirmed);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.PasswordHash"/> property.
    /// </summary>
    public string PasswordHash { get; set; } = nameof(IdentityUser.PasswordHash);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.SecurityStamp"/> property.
    /// </summary>
    public string SecurityStamp { get; set; } = nameof(IdentityUser.SecurityStamp);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.ConcurrencyStamp"/> property.
    /// </summary>
    public string ConcurrencyStamp { get; set; } = nameof(IdentityUser.ConcurrencyStamp);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.PhoneNumber"/> property.
    /// </summary>
    public string PhoneNumber { get; set; } = nameof(IdentityUser.PhoneNumber);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.PhoneNumberConfirmed"/> property.
    /// </summary>
    public string PhoneNumberConfirmed { get; set; } = nameof(IdentityUser.PhoneNumberConfirmed);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.TwoFactorEnabled"/> property.
    /// </summary>
    public string TwoFactorEnabled { get; set; } = nameof(IdentityUser.TwoFactorEnabled);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.LockoutEnd"/> property.
    /// </summary>
    public string LockoutEnd { get; set; } = nameof(IdentityUser.LockoutEnd);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.LockoutEnabled"/> property.
    /// </summary>
    public string LockoutEnabled { get; set; } = nameof(IdentityUser.LockoutEnabled);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.AccessFailedCount"/> property.
    /// </summary>
    public string AccessFailedCount { get; set; } = nameof(IdentityUser.AccessFailedCount);


    /// <summary>
    /// Initializes a new instance of the <see cref="UserTableNames"/> class.
    /// </summary>
    public UserTableNames() : base(DefaultPascalCaseTable) { }


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        Table = ConvertIfDefault(Table, Default.Table, convertFunction);
        Id = ConvertIfDefault(Id, Default.Id, convertFunction);
        UserName = ConvertIfDefault(UserName, Default.UserName, convertFunction);
        NormalizedUserName = ConvertIfDefault(NormalizedUserName, Default.NormalizedUserName, convertFunction);
        Email = ConvertIfDefault(Email, Default.Email, convertFunction);
        NormalizedEmail = ConvertIfDefault(NormalizedEmail, Default.NormalizedEmail, convertFunction);
        EmailConfirmed = ConvertIfDefault(EmailConfirmed, Default.EmailConfirmed, convertFunction);
        PasswordHash = ConvertIfDefault(PasswordHash, Default.PasswordHash, convertFunction);
        SecurityStamp = ConvertIfDefault(SecurityStamp, Default.SecurityStamp, convertFunction);
        ConcurrencyStamp = ConvertIfDefault(ConcurrencyStamp, Default.ConcurrencyStamp, convertFunction);
        PhoneNumber = ConvertIfDefault(PhoneNumber, Default.PhoneNumber, convertFunction);
        PhoneNumberConfirmed = ConvertIfDefault(PhoneNumberConfirmed, Default.PhoneNumberConfirmed, convertFunction);
        TwoFactorEnabled = ConvertIfDefault(TwoFactorEnabled, Default.TwoFactorEnabled, convertFunction);
        LockoutEnd = ConvertIfDefault(LockoutEnd, Default.LockoutEnd, convertFunction);
        LockoutEnabled = ConvertIfDefault(LockoutEnabled, Default.LockoutEnabled, convertFunction);
        AccessFailedCount = ConvertIfDefault(AccessFailedCount, Default.AccessFailedCount, convertFunction);
    }
}
