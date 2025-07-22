using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;

namespace Inventory.DataProtection.Helpers
{
    /// <summary>
    /// Provides helper methods for configuring ASP.NET Core Data Protection with certificate-based key encryption.
    /// </summary>
    /// <remarks>
    /// The <see cref="DataProtectionHelper"/> class centralizes the setup of data protection services, including loading an X.509 certificate from configuration and applying it to protect data protection keys.
    /// This ensures that cryptographic keys are securely stored and encrypted for the application.
    /// </remarks>
    internal static class DataProtectionHelper
    {
        /// <summary>
        /// Configures ASP.NET Core Data Protection to use an X.509 certificate for key encryption and persists keys to the file system.
        /// </summary>
        /// <param name="services">The service collection to add data protection services to.</param>
        /// <param name="configuration">The application configuration containing certificate settings.</param>
        /// <exception cref="InvalidOperationException">Thrown if required configuration values are missing.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the certificate file does not exist at the specified path.</exception>
        /// <remarks>
        /// Expects the following configuration keys:
        /// <list type="bullet">
        ///   <item><description><c>DataEncryption:CertificateDirectory</c> - Directory containing the certificate file.</description></item>
        ///   <item><description><c>DataEncryption:CertificateName</c> - Name of the certificate file.</description></item>
        ///   <item><description><c>DataEncryption:CertificatePassword</c> - Password for the certificate.</description></item>
        /// </list>
        /// Keys are persisted to <c>./DataProtection/DataProtection-Keys</c> and the application name is set to <c>inventory-protection-key</c>.
        /// </remarks>
        internal static void ConfigureDataProtection(IServiceCollection services, IConfiguration configuration)
        {
            var cert = GetPfxCertificate(configuration);

            // Use with Data Protection
            services.AddDataProtection()
                .ProtectKeysWithCertificate(cert)
                .PersistKeysToFileSystem(new DirectoryInfo("./DataProtection/DataProtection-Keys"))
                .SetApplicationName("inventory-protection-key");
        }

        internal static X509Certificate2 GetPfxCertificate(IConfiguration configuration)
        {
            var certDir = configuration["DataEncryption:CertificateDirectory"]
                ?? throw new InvalidOperationException("DataEncryption:CertificateDirectory was null");
            
            var certName = configuration["DataEncryption:CertificateName"]
                ?? throw new InvalidOperationException("DataEncryption:CertificateName was null");
            
            var certPassword = configuration["DataEncryption:CertificatePassword"]
                ?? throw new InvalidOperationException("DataEncryption:CertificatePassword was null");

            var certPath = Path.Combine(certDir, certName);
            
            if (!File.Exists(certPath))
                throw new FileNotFoundException($"Certificate file not found at path {certPath}.");
            return new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.MachineKeySet);
        }
    }
}
