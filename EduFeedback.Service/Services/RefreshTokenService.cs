using EduFeedback.Core.DatabaseContext;
using EduFeedback.Service.Interfaces;
using EduFeedback.Service.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Services
{
    public class RefreshTokenService
    {
        private static RegistrationService _service = new RegistrationService();
        // never change this key, otherwise all the stored refresh tokens will be lost
        private static readonly string encryptionKey = "EduFeedback_Key"; // Store this securely
        public static void StoreRefreshToken(int userId, string userName, string token, DateTime expiresAt)
        {
            using (var context = new EduFeedEntities())
            {
                
                
                var encryptedToken = Encrypt(token, encryptionKey);
                var refreshToken = new RefreshToken
                {
                    UserId = userId,
                    UserName = userName,
                    RefreshToken1 = encryptedToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt
                };
                context.RefreshTokens.Add(refreshToken);
                context.SaveChanges();
            }
        }
        public static string GetRefreshToken(int userId, string userName)
        {
            using (var context = new EduFeedEntities())
            {
                //string userId = User.Identity.GetUserId();
                var refreshToken = context.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(rt => rt.CreatedAt)
                    .FirstOrDefault();
                return refreshToken != null ? Decrypt(refreshToken.RefreshToken1, encryptionKey) : null;
            }
        }
        private static string Encrypt(string plainText, string key)
        {
            using (var aes = Aes.Create())
            {
                var keyBytes = new byte[32];
                var providedKeyBytes = Encoding.UTF8.GetBytes(key);
                Array.Copy(providedKeyBytes, keyBytes, Math.Min(providedKeyBytes.Length, keyBytes.Length));


                var iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(keyBytes, iv))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        private static string Decrypt(string cipherText, string key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            using (var aes = Aes.Create())
            {
                var iv = new byte[aes.BlockSize / 8];
                var cipher = new byte[fullCipher.Length - iv.Length];

                Array.Copy(fullCipher, iv, iv.Length);
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                var keyBytes = new byte[32];
                var providedKeyBytes = Encoding.UTF8.GetBytes(key);
                Array.Copy(providedKeyBytes, keyBytes, Math.Min(providedKeyBytes.Length, keyBytes.Length));


                using (var decryptor = aes.CreateDecryptor(keyBytes, iv))
                using (var ms = new MemoryStream(cipher))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}