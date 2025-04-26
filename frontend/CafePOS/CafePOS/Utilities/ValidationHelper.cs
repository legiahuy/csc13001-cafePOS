using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using CafePOS.DAO;
using CafePOS.DTO;

namespace CafePOS.Utilities
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates if an email address has the correct format
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; // Email can be optional, depends on context
                
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if a phone number has the correct Vietnamese format (10-11 digits starting with 0)
        /// </summary>
        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;
                
            // Vietnamese phone number format: 10-11 digits starting with 0
            return Regex.IsMatch(phone, @"^0\d{9,10}$");
        }

        /// <summary>
        /// Validates if a string contains only letters, numbers, spaces, and Vietnamese characters
        /// </summary>
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            
            // Name should only contain letters, numbers, spaces, and Vietnamese characters
            return Regex.IsMatch(name, @"^[a-zA-Z0-9\sÀ-ỹ]+$");
        }

        /// <summary>
        /// Validates if text is within maximum length
        /// </summary>
        public static bool IsValidLength(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            
            return text.Length <= maxLength;
        }

        /// <summary>
        /// Validates if a date is not in the future
        /// </summary>
        public static bool IsValidPastDate(DateTime? date)
        {
            if (!date.HasValue)
                return false;
            
            return date.Value <= DateTime.Now;
        }

        /// <summary>
        /// Validates if a number is positive
        /// </summary>
        public static bool IsPositiveNumber(double value)
        {
            return value > 0;
        }

        /// <summary>
        /// Validates if a number is non-negative
        /// </summary>
        public static bool IsNonNegativeNumber(double value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Validates if a string can be parsed to a positive number
        /// </summary>
        public static bool IsValidPositiveNumber(string text)
        {
            if (double.TryParse(text, out double value))
            {
                return value > 0;
            }
            return false;
        }

        /// <summary>
        /// Validates if a file path is valid and accessible
        /// </summary>
        public static async Task<bool> IsValidImagePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                Uri uri = path.StartsWith("ms-appx:///") ? new Uri(path) : new Uri("ms-appx:///" + path.TrimStart('/'));
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                return file != null;
            }
            catch
            {
                return false;
            }
        }
    }
} 