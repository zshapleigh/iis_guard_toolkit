using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace IISGuard.Services
{
    public static class BindingValidator
    {
        public static void ValidateBindings()
        {
            Console.WriteLine("[BindingValidator] Validating SSL bindings and certificates...");
            // Placeholder: Check for expired or missing SSL certs
            //HttpClientHandler handler = new HttpClientHandler();
            //handler.ServerCertificateCustomValidationCallback();

            //find all SSL certs and loop
            // stores and they friendly names
            var stores = new Dictionary<StoreName, string>()
            {
                {StoreName.My, "Personal"},
                {StoreName.Root, "Trusted roots"},
                {StoreName.TrustedPublisher, "Trusted publishers"}
                // and so on
                }.Select(s => new
                {
                    store = new X509Store(s.Key, StoreLocation.LocalMachine),
                    location = s.Value
                }).ToArray();

            foreach (var store in stores)
            {
                store.store.Open(OpenFlags.ReadOnly); // open each store
            }

            var list = stores.SelectMany(s => s.store.Certificates.Cast<X509Certificate2>()
                .Select(mCert => new CertDetails
                {
                    HasPrivateKey = mCert.HasPrivateKey,
                    FriendlyName = mCert.FriendlyName,
                    Name = mCert.GetNameInfo(X509NameType.SimpleName, false),
                    Location = s.location,
                    Issuer = mCert.Issuer,
                    ExpirationDate = mCert.GetExpirationDateString()
                })).ToList()
                //we only care about SSL certs
                .Where(c => c.HasPrivateKey).ToList();

            if (list.Count > 0) 
            {
                //check if SSL is still valid
                foreach (var cert in list)
                {
                    //IsSSLCertificateValid("cert here" + ".cer");
                } 
            }
        }

        private static bool IsSSLCertificateValid(string certificate)
        {
            // The path to the certificate file
            string Certificate = certificate;// "Certificate.cer";

            // Load the certificate into an X509Certificate object
            X509Certificate cert = X509Certificate.CreateFromCertFile(Certificate);

            // Get the expiration date string
            string expirationDateString = cert.GetExpirationDateString();

            // Display the value to the console
            Console.WriteLine($"Expiration Date: {expirationDateString}");

            // You can parse the date string to a DateTime object for comparison
            DateTime expirationDate = DateTime.Parse(expirationDateString);

            if (expirationDate < DateTime.Now)
            {
                Console.WriteLine("Certificate is expired!");
                return true;
            }
            else
            {
                Console.WriteLine("Certificate is valid.");
                return false;
            }
        }

        public class CertDetails
        {
            public string Name { get; set; }
            public string FriendlyName { get; set; }
            public bool HasPrivateKey { get; set; }
            public string Location { get; set; }
            public string Issuer { get; set; }
            public string ExpirationDate { get; set; }
        }


    }
}
