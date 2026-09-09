using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserService.Data;
using UserService.Entities;

var configurationPath = FindLocalConfigurationFile();

if (configurationPath is null)
{
    Console.Error.WriteLine("UserDb connection string is missing. Configure appsettings.Development.local.json.");
    return;
}

var configuration = new ConfigurationBuilder()
    .AddJsonFile(configurationPath, optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("UserDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("UserDb connection string is missing. Configure appsettings.Development.local.json.");
    return;
}

Console.WriteLine("Provision a local Administrator user.");

var name = ReadRequiredValue("Administrator name: ");
var email = ReadRequiredValue("Email: ");
var phoneNumber = ReadRequiredValue("Phone number: ");
var password = ReadPassword();

if (name is null || email is null || phoneNumber is null || password is null)
{
    Console.Error.WriteLine("Name, email, phone number, and password are required. No changes were made.");
    return;
}

var options = new DbContextOptionsBuilder<UserDbContext>()
    .UseSqlServer(connectionString)
    .Options;

try
{
    await using var database = new UserDbContext(options);

    var administratorRole = await database.Roles
        .SingleOrDefaultAsync(role => role.Name == "Administrator");

    if (administratorRole is null)
    {
        Console.Error.WriteLine("Administrator role was not found. Apply the User Service database migration first.");
        return;
    }

    var emailInUse = await database.Users.AnyAsync(user => user.Email == email);
    var phoneNumberInUse = await database.Users.AnyAsync(user => user.PhoneNumber == phoneNumber);

    if (emailInUse || phoneNumberInUse)
    {
        Console.Error.WriteLine("A user with this email or phone number already exists. No changes were made.");
        return;
    }

    var administrator = new User
    {
        Name = name,
        Email = email,
        PhoneNumber = phoneNumber,
        RoleId = administratorRole.Id,
        Role = administratorRole,
        CreatedAt = DateTime.UtcNow
    };

    var passwordHasher = new PasswordHasher<User>();
    administrator.PasswordHash = passwordHasher.HashPassword(administrator, password);

    database.Users.Add(administrator);
    await database.SaveChangesAsync();

    Console.WriteLine("Administrator user created successfully.");
}
catch (DbUpdateException)
{
    Console.Error.WriteLine("The Administrator user could not be saved. Verify the database is available and try different email and phone values.");
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Provisioning failed ({exception.GetType().Name}). Verify the UserDb configuration and database migration.");
}

static string? FindLocalConfigurationFile()
{
    var repositoryRoot = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..", ".."));

    var utilityConfiguration = Path.Combine(
        repositoryRoot,
        "scripts", "ProvisionAdministrator", "appsettings.Development.local.json");

    if (File.Exists(utilityConfiguration))
    {
        return utilityConfiguration;
    }

    var userServiceConfiguration = Path.Combine(
        repositoryRoot,
        "src", "UserService", "appsettings.Development.local.json");

    return File.Exists(userServiceConfiguration) ? userServiceConfiguration : null;
}

static string? ReadRequiredValue(string prompt)
{
    Console.Write(prompt);
    var value = Console.ReadLine()?.Trim();
    return string.IsNullOrWhiteSpace(value) ? null : value;
}

static string? ReadPassword()
{
    Console.Write("Password: ");

    if (Console.IsInputRedirected)
    {
        return Console.ReadLine();
    }

    var password = new List<char>();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace && password.Count > 0)
        {
            password.RemoveAt(password.Count - 1);
            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            password.Add(key.KeyChar);
        }
    }

    var value = new string(password.ToArray());
    return string.IsNullOrWhiteSpace(value) ? null : value;
}
