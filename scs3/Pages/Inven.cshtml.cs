using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace scs3.Pages
{
    public class InvenModel : PageModel
    {


        [HttpPost]
        public async Task<IActionResult> SavePackedDevicesAsync(List<string> selectedDeviceIds)
        {
            try
            {
                foreach (var deviceId in selectedDeviceIds)
                {
                    // Fetch device details from the devices node
                    var deviceDetails = await _firebaseClient.Child("devices").Child(deviceId).OnceSingleAsync<Device>();

                    // Save device details to the packedDevices node
                    await _firebaseClient.Child("packedDevices").Child(deviceId).PutAsync(deviceDetails);
                }
                return Page();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }





        //Field should not be empty - warnings

        [Required(ErrorMessage = "Purchase Date is required.")]
        public DateOnly PurchaseDate { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Device Name is required.")]
        public string DeviceName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Device ID is required.")]
        public string DeviceID { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Device Type is required.")]
        public string DeviceType { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Quantity is required.")]
        public int Quantity { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Quantity is required.")]
        public int Price { get; set; }




        private readonly FirebaseClient _firebaseClient;

        public List<Device> Devices { get; set; }

        public InvenModel(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
            Devices = new List<Device>(); // Initialize the list
        }

        public async Task OnGetAsync()
        {
            // Fetch data from the database
            var devicesSnapshot = await _firebaseClient.Child("devices").OnceAsync<Device>();

            // Looping the Devices list with fetched data
            foreach (var deviceSnapshot in devicesSnapshot)
            {
                Devices.Add(deviceSnapshot.Object);
            }
        }



        public async Task<IActionResult> OnPostAsync()
        {
            
            if (!ModelState.IsValid)
            {
                return Page(); // Error page
            }

            try
            {
                // Get data from the form.
                string deviceName = DeviceName;
                string deviceID = DeviceID;
                string deviceType = DeviceType;
                string location = Location;
                int quantity = Quantity;
                int price = Price;

                 DateOnly purchaseDate = PurchaseDate;
                 purchaseDate = DateOnly.FromDateTime(DateTime.Today);

                await _firebaseClient
                    .Child("devices")
                    .Child(deviceID)
                    .PutAsync(new
                    {
                        DeviceName = deviceName,
                        DeviceID = deviceID,
                        DeviceType = deviceType,
                        Location = location,
                        Quantity = quantity,
                        Price = price,
                        PurchaseDate = purchaseDate.ToString()
                    });

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // Handling any exceptions
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }




        // To represent the device data
        public class Device
        {
            public string DeviceName { get; set; }
            public string DeviceID { get; set; }
            public string DeviceType { get; set; }
            public string Location { get; set; }
            public string Quantity { get; set; }

            public decimal Price { get; set; }
        }

         public async Task<List<DeletedDeviceHistory>> GetDeletedDeviceHistoryAsync()
        {
            var deletedDevicesSnapshot = await _firebaseClient.Child("invoice").OnceAsync<DeletedDeviceHistory>();
            var deletedDeviceHistory = new List<DeletedDeviceHistory>();
            foreach (var deviceSnapshot in deletedDevicesSnapshot)
            {
                deletedDeviceHistory.Add(deviceSnapshot.Object);
            }
            return deletedDeviceHistory;
        }

        // To represent deleted device history
        public class DeletedDeviceHistory
        {
            public string DeviceID { get; set; }
            public string DeviceName { get; set; }
            public string DeviceType { get; set; }
            public string SoldTo { get; set; }
            public DateTime Date { get; set; }
            public int Quantity { get; set; }
            public int Status { get; set; }
            public decimal Price { get; set; }
            public string ClientAddress { get; set; }
            public string InvoiceNumber { get; set; }


        }





    }

}
