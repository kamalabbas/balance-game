using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RollABall.Licensing
{
    public static class LicenseService
    {
        public const string ProductId = "rollaball";
        private const string PublicKeyResourceName = "license_public_key";
        private const string KeyPrefix = "ROLLABALL1";
        private const string LicenseFileName = "license.key";
        private const string MachineCodeFileName = "machine_code.txt";

        public static bool IsActivated(out string reason)
        {
#if UNITY_EDITOR
            reason = "Editor mode (no license needed).";
            return true;
#else
            if (TryLoadActivationKeyFromFile(out string fileKey, out string filePath))
            {
                if (TryValidateActivationKey(fileKey, out reason))
                {
                    return true;
                }

                EnsureMachineCodeFileWritten();
                reason = $"License file invalid: {filePath}";
                return false;
            }

            EnsureMachineCodeFileWritten();
            reason = "Not activated. Put license.key next to the .exe (machine_code.txt was generated).";
            return false;
#endif
        }

        public static void SaveActivationKey(string activationKey)
        {
            if (string.IsNullOrWhiteSpace(activationKey))
            {
                return;
            }

            string dir = GetExeDirectorySafe();
            if (string.IsNullOrWhiteSpace(dir))
            {
                return;
            }

            try
            {
                string path = Path.Combine(dir, LicenseFileName);
                File.WriteAllText(path, activationKey.Trim());
            }
            catch
            {
                // ignore
            }
        }

        public static void ClearActivationKey()
        {
            string dir = GetExeDirectorySafe();
            if (string.IsNullOrWhiteSpace(dir))
            {
                return;
            }

            try
            {
                string path = Path.Combine(dir, LicenseFileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // ignore
            }
        }

        public static string GetMachineCode()
        {
            string installId = GetInstallIdSafe();
            string deviceId = SystemInfo.deviceUniqueIdentifier ?? string.Empty;

            string raw = $"{ProductId}|{installId}|{deviceId}";
            byte[] rawBytes = Encoding.UTF8.GetBytes(raw);

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(rawBytes);
                return Base32Encode(hash);
            }
        }

        private static bool TryLoadActivationKeyFromFile(out string activationKey, out string pathUsed)
        {
            activationKey = string.Empty;
            pathUsed = string.Empty;

            foreach (string dir in new[] { GetExeDirectorySafe() })
            {
                if (string.IsNullOrWhiteSpace(dir))
                {
                    continue;
                }

                string path = Path.Combine(dir, LicenseFileName);
                try
                {
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    string text = (File.ReadAllText(path) ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        pathUsed = path;
                        return false;
                    }

                    activationKey = text;
                    pathUsed = path;
                    return true;
                }
                catch
                {
                    // ignore and continue
                }
            }

            return false;
        }

        private static void EnsureMachineCodeFileWritten()
        {
            string code = GetMachineCode();
            if (string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            foreach (string dir in new[] { GetExeDirectorySafe(), Application.persistentDataPath })
            {
                if (string.IsNullOrWhiteSpace(dir))
                {
                    continue;
                }

                try
                {
                    Directory.CreateDirectory(dir);
                    string path = Path.Combine(dir, MachineCodeFileName);
                    if (!File.Exists(path))
                    {
                        File.WriteAllText(path, code);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        public static bool TryValidateActivationKey(string activationKey, out string reason)
        {
            if (string.IsNullOrWhiteSpace(activationKey))
            {
                reason = "Activation key is empty.";
                return false;
            }

            activationKey = activationKey.Trim();

            string[] parts = activationKey.Split('.');
            if (parts.Length != 3 || !string.Equals(parts[0], KeyPrefix, StringComparison.Ordinal))
            {
                reason = "Activation key format is invalid.";
                return false;
            }

            byte[] payloadBytes;
            byte[] signatureBytes;
            try
            {
                payloadBytes = Base64UrlDecode(parts[1]);
                signatureBytes = Base64UrlDecode(parts[2]);
            }
            catch
            {
                reason = "Activation key is malformed.";
                return false;
            }

            string publicKeyXml = LoadPublicKeyXml();
            if (string.IsNullOrWhiteSpace(publicKeyXml))
            {
                reason = "Public key missing (license_public_key).";
                return false;
            }

            bool signatureValid;
            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(publicKeyXml);
                    signatureValid = rsa.VerifyData(payloadBytes, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
                }
            }
            catch (Exception ex)
            {
                reason = "Signature verification failed: " + ex.Message;
                return false;
            }

            if (!signatureValid)
            {
                reason = "Activation key signature is invalid.";
                return false;
            }

            ActivationPayload payload;
            try
            {
                string payloadJson = Encoding.UTF8.GetString(payloadBytes);
                payload = JsonUtility.FromJson<ActivationPayload>(payloadJson);
            }
            catch
            {
                reason = "Activation payload is invalid.";
                return false;
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.product) || string.IsNullOrWhiteSpace(payload.machine))
            {
                reason = "Activation payload is incomplete.";
                return false;
            }

            if (!string.Equals(payload.product, ProductId, StringComparison.Ordinal))
            {
                reason = "Activation key is for a different product.";
                return false;
            }

            string currentMachine = GetMachineCode();
            if (!string.Equals(payload.machine, currentMachine, StringComparison.Ordinal))
            {
                reason = "Activation key is for a different PC.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(payload.expiresAt))
            {
                if (!TryParseIsoUtc(payload.expiresAt, out DateTime expiresUtc))
                {
                    reason = "Activation key expiry is invalid.";
                    return false;
                }

                if (DateTime.UtcNow > expiresUtc)
                {
                    reason = "Activation key is expired.";
                    return false;
                }
            }

            reason = "Activated.";
            return true;
        }

        private static string LoadPublicKeyXml()
        {
            TextAsset publicKeyAsset = Resources.Load<TextAsset>(PublicKeyResourceName);
            return publicKeyAsset != null ? publicKeyAsset.text : string.Empty;
        }

        private static string GetInstallIdSafe()
        {
            try
            {
                string dir = Application.persistentDataPath;
                if (string.IsNullOrWhiteSpace(dir))
                {
                    return string.Empty;
                }

                string path = Path.Combine(dir, "install_id.txt");
                if (File.Exists(path))
                {
                    return (File.ReadAllText(path) ?? string.Empty).Trim();
                }

                string installId = Guid.NewGuid().ToString("N");
                Directory.CreateDirectory(dir);
                File.WriteAllText(path, installId);
                return installId;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetExeDirectorySafe()
        {
            try
            {
                // In a Windows build: <GameName>_Data is at Application.dataPath, exe is parent folder.
                string dataPath = Application.dataPath;
                if (string.IsNullOrWhiteSpace(dataPath))
                {
                    return string.Empty;
                }

                DirectoryInfo dir = Directory.GetParent(dataPath);
                return dir != null ? dir.FullName : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool TryParseIsoUtc(string text, out DateTime utc)
        {
            if (DateTime.TryParseExact(text,
                    "yyyy-MM-dd'T'HH:mm:ss'Z'",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out utc))
            {
                return true;
            }

            if (DateTime.TryParseExact(text,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out utc))
            {
                utc = new DateTime(utc.Year, utc.Month, utc.Day, 23, 59, 59, DateTimeKind.Utc);
                return true;
            }

            return false;
        }

        private static string Base32Encode(byte[] data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            if (data == null || data.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder result = new StringBuilder((data.Length + 4) / 5 * 8);

            int buffer = data[0];
            int next = 1;
            int bitsLeft = 8;
            while (bitsLeft > 0 || next < data.Length)
            {
                if (bitsLeft < 5)
                {
                    if (next < data.Length)
                    {
                        buffer <<= 8;
                        buffer |= data[next++] & 0xFF;
                        bitsLeft += 8;
                    }
                    else
                    {
                        int pad = 5 - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }

                int index = (buffer >> (bitsLeft - 5)) & 0x1F;
                bitsLeft -= 5;
                result.Append(alphabet[index]);
            }

            return result.ToString();
        }

        private static byte[] Base64UrlDecode(string text)
        {
            string padded = text.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }

        [Serializable]
        private class ActivationPayload
        {
            public string product;
            public string machine;
            public string issuedAt;
            public string expiresAt;
        }
    }
}
