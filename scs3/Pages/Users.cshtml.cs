using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace scs3.Pages
{
    public class UsersModel : PageModel
    {
        private readonly FirebaseClient _firebaseClient;
        private const string FirebaseUrl = "https://stockcontrol-f2ff7-default-rtdb.firebaseio.com";

        public UsersModel()
        {
            _firebaseClient = new FirebaseClient(FirebaseUrl);
        }

        public List<UserData> Users { get; set; }
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var usersNode = _firebaseClient.Child("Users");
            var users = await usersNode.OnceAsync<UserData>();

            Users = new List<UserData>();
            foreach (var user in users)
            {
                Users.Add(user.Object);
            }
        }


        public async Task<IActionResult> OnPostGenerateOTPAsync(string otpNumber)
        {
            if (!string.IsNullOrEmpty(otpNumber) && otpNumber.Length == 4 && int.TryParse(otpNumber, out _))
            {
                // Check if the OTP already exists
                var otpNode = _firebaseClient.Child("OTP");
                var existingOTP = await otpNode.Child(otpNumber).OnceSingleAsync<int?>();

                if (existingOTP == null)
                {
                    // Save OTP to Firebase
                    await otpNode.Child(otpNumber).PutAsync(0); // Assuming "0" as default value
                }
                else
                {
                    // Return BadRequest indicating that the OTP already exists
                    return BadRequest("OTP already exists.");

                    // Handle the case where the OTP already exists
                    // For example, you can return an error message or take other appropriate action
                    // You can also choose to silently ignore and proceed if needed
                }
            }
            // Redirect to refresh the page or handle as needed
            return RedirectToPage("/Users");
        }


        public class UserData
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string OTP { get; set; }
            // Add other properties as needed
        }
    }
}
