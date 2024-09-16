using System.Security.Cryptography;
using System.Text;

namespace TaskTracker.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public void SetPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                PasswordHash = Convert.ToBase64String(bytes);
            }
        }

        public bool ValidatePassword(string password)
        {
            using(var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hash = Convert.ToBase64String(bytes);
                return hash == PasswordHash;
            }
        }
    }
}
