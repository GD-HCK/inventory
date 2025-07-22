using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Reflection;
using System.Text;

namespace DataLibrary.Classes
{
    /// <summary>
    /// Represents an application user account with extended properties and logic for authentication and authorization.
    /// </summary>
    /// <remarks>
    /// The <see cref="Account"/> class extends <see cref="IdentityUser"/> to provide additional fields such as password, API key, allowed IP addresses, roles, and account validity dates.
    /// It supports random name assignment from an embedded CSV resource, secure password generation, and IP address validation/restriction.
    /// This class is intended for use in authentication and authorization scenarios, supporting both API key and basic authentication schemes.
    /// </remarks>
    public class Account : IdentityUser
    {
        /// <summary>
        /// Gets or sets the UTC date and time when the account was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the UTC date and time when the account expires.
        /// </summary>
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddYears(1);

        /// <summary>
        /// Gets or sets the base64-encoded string of the username and password.
        /// </summary>
        public string? Base64Encoded { get; set; }

        /// <summary>
        /// Gets or sets the plain text password for the account (for initial provisioning only).
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the API key associated with the account.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the allowed IP addresses or ranges for this account, as a comma-separated string.
        /// </summary>
        public string? AllowedIpAddresses { get; set; }

        /// <summary>
        /// Gets or sets the display name for the account.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the roles assigned to the account.
        /// </summary>
        public string[]? Roles { get; set; }

        /// <summary>
        /// List of names loaded from <see cref="LoadNames"/>.
        /// </summary>
        private static readonly string[] names = LoadNames();

        /// <summary>
        /// Loads a list of example names from an embedded CSV resource for use in account generation.
        /// </summary>
        /// <remarks>
        /// Reads the <c>DataLibrary.MocData.ExampleNames.csv</c> embedded resource from the executing assembly,
        /// trims each line, and returns the names as a string array. Throws a <see cref="FileNotFoundException"/> if the resource is not found.
        /// </remarks>
        /// <returns>An array of names loaded from the embedded CSV resource.</returns>

        private static string[] LoadNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "DataLibrary.MocData.ExampleNames.csv"; // Adjust namespace/path as needed

            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Resource '{resourceName}' not found.");
            using var reader = new StreamReader(stream);
            var lines = new List<string>();
            while (!reader.EndOfStream)
                lines.Add(reader.ReadLine()!.Trim(','));
            return [.. lines];
        }

        /// <summary>
        /// Scrambles the characters in the input string using a random shuffle.
        /// </summary>
        /// <param name="input">The string to scramble.</param>
        /// <returns>A new string with the characters randomly shuffled.</returns>

        public static string ScrambleString(string input)
        {
            var chars = input.ToCharArray();
            var rng = new Random();
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            return new string(chars);
        }

        /// <summary>
        /// Generates a random password containing lowercase, uppercase, special characters, and numbers.
        /// </summary>
        /// <returns>A randomly generated password string.</returns>
        public static string GeneratePassword()
        {
            var lower = "abcdefghijklmnopqrstuvwxyz";
            var upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var special = "!@#$%^&*()_+";
            var numbers = "0123456789";
            List<string> charsList = [lower, upper, special, numbers];

            string password = string.Empty;

            var random = new Random();

            foreach (var chars in charsList)
            {
                password += new string([.. Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)])]);
            }

            password = ScrambleString(password);

            return password;
        }

        /// <summary>
        /// Validates whether the specified IP address is allowed for this account.
        /// </summary>
        /// <param name="ipaddress">The IP address to validate.</param>
        /// <returns><c>true</c> if the IP address is allowed; otherwise, <c>false</c>.</returns>

        public bool ValidateIpAddress(IPAddress ipaddress)
        {
            if (string.IsNullOrEmpty(AllowedIpAddresses))
            {
                return true; // No restrictions, allow all IPs
            }
            var allowedIpsRanges = AllowedIpAddresses!.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var allowedIpsRange in allowedIpsRanges)
            {
                string[] ips = allowedIpsRange.Trim().Split('-');
                foreach (var ip in ips)
                {
                    if (IPAddress.TryParse(ip.Trim(), out var ipParsed))
                    {
                        if (ipParsed.Equals(ipaddress))
                        {
                            return true; // IP address is allowed
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the account properties, including username, password, API key, allowed IP addresses, and roles.
        /// </summary>
        /// <param name="ipAddress">The IP address to restrict, or null for no restriction.</param>
        /// <param name="restrictIp">Whether to restrict to a single IP address.</param>
        /// <param name="restrictRange">Whether to restrict to an IP range.</param>
        /// <param name="roles">The roles to assign to the account.</param>

        public void SetProperties(IPAddress? ipAddress, bool restrictIp, bool restrictRange, string[] roles)
        {
            var guid = Guid.NewGuid().ToString("N"); // Generate a random string
            var guidShort = guid[..5]; // Shorten the GUID for username
            Password = GeneratePassword();
            Name = names[new Random().Next(names.Length)];
            var NameWithNoTitle = string.Join(".", Name.Split(' ').Skip(1)); // Remove title from name
            UserName = NameWithNoTitle + '.' + guidShort; // Create a username from the name and a short GUID
            ApiKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(guid));
            Base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ':' + Password));
            Email = UserName + "@example.com";
            Roles = roles;

            if (ipAddress != null && !(ipAddress.ToString().Equals("127.0.0.1")) && (restrictIp || restrictRange))
            {
                var restriction = restrictIp && restrictRange || restrictRange ? "range" : "ip";
                switch (restriction)
                {
                    case "ip":
                        AllowedIpAddresses = ipAddress.ToString();
                        break;
                    case "range":
                        var octets = ipAddress.ToString().Split('.');
                        var networkId = string.Join('.', octets.Take(3)); // Use the first three octets for a /24 network
                        AllowedIpAddresses = $"{networkId}.1-{networkId}.254";
                        break;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Account"/> class.
        /// </summary>
        public Account() { }
    }
}
