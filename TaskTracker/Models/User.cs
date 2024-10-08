﻿using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace TaskTracker.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Хешує пароль і зберігає його як хеш
        /// </summary>
        /// <param name="password">Пароль для хешування</param>

        [JsonIgnore] // Запобігаємо циклічній серіалізації
        public ICollection<TrackedTask> Tasks { get; set; } = new List<TrackedTask>(); // Ініціалізація колекції

        public string Salt { get; set; }

        public void SetPassword(string password)
        {
            Salt = Guid.NewGuid().ToString();

            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + Salt));
                PasswordHash = Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Перевіряє, чи відповідає хешований пароль введоному паролю
        /// </summary>
        /// <param name="password">Пароль для перевірки</param>
        /// <returns>Повертає true, якщо паролі співпадають, інакше false</returns>
        public bool ValidatePassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + Salt));
                var hash = Convert.ToBase64String(bytes);
                return hash == PasswordHash;
            }
        }
    }
}
