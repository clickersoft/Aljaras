using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Aljaras.Core
{
    public class LicenseKeyGenerator 
    {
        public static bool IsProductActivated()
        {
            string? keyFile = Directory.GetFiles(GlobalVariables.AppLocation, "*.key").FirstOrDefault();
            if (keyFile != null)
            {
                string GeneratedKey = GenerateLicenseKey(Environment.MachineName);
                string? ActivationKey = File.ReadLines(keyFile).ElementAtOrDefault(1);
                if (ActivationKey == GenerateLicenseKey(GeneratedKey))
                    return true;                
            }
            return false;
        }

        public static string GenerateLicenseKey(string productIdentifier)
        {
            return FormatLicenseKey(GetMd5Sum(productIdentifier));
        }

        static string GetMd5Sum(string productIdentifier)
        {
            Encoder enc = Encoding.Unicode.GetEncoder();
            byte[] unicodeText = new byte[productIdentifier.Length * 2];
            enc.GetBytes(productIdentifier.ToCharArray(), 0, productIdentifier.Length, unicodeText, 0, true);
            byte[] result = MD5.Create().ComputeHash(unicodeText);
            StringBuilder sb = new();
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("X2"));
            }
            return sb.ToString();
        }

        static string FormatLicenseKey(string productIdentifier)
        {
            productIdentifier = productIdentifier.Substring(0, 28).ToUpper();
            char[] serialArray = productIdentifier.ToCharArray();
            StringBuilder licenseKey = new();
            int j = 0;
            for (int i = 0; i < 28; i++)
            {
                for (j = i; j < 4 + i; j++)
                {
                    licenseKey.Append(serialArray[j]);
                }
                if (j == 28)
                {
                    break;
                }
                else
                {
                    i = (j) - 1;
                    licenseKey.Append('-');
                }
            }
            return licenseKey.ToString();
        }
    }
}
