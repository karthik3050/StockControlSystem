using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace scs3.Pages
{
    public class ShippedModel : PageModel
    {
        public class packedDevice
        {
            public string DeviceID { get; set; }
            public string DeviceName { get; set; }
            public string DeviceType { get; set; }
            public string Location { get; set; }
            public string SoldTo { get; set; }
            public DateOnly Date { get; set; }
            public TimeSpan Time { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }

            public string ClientAddress { get; set; }

            public string InvoiceNumber { get; set; }
            public DateOnly DueDate { get; set; }
        }

        private readonly FirebaseClient _firebaseClient;

        public List<DeletedDevice> DeletedDeviceHistory { get; set; }

        public ShippedModel(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDeletedDeviceHistory();
            return Page();
        }

        private async Task LoadDeletedDeviceHistory()
        {
            var history = await _firebaseClient.Child("packedDevices").OnceSingleAsync<Dictionary<string, DeletedDevice>>();
            DeletedDeviceHistory = new List<DeletedDevice>();

            if (history != null)
            {
                foreach (var item in history)
                {
                    DeletedDeviceHistory.Add(item.Value);
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteAndDisplayFormAsync(string[] selectedDevices)
        {
          

            foreach (var InvoiceNumber in selectedDevices)
            {
                var device = await _firebaseClient.Child("packedDevices").Child(InvoiceNumber).OnceSingleAsync<Device>();
                if (device != null)
                {
                    var deletedDevice = new packedDevice
                    {
                        DeviceID = device.DeviceID,
                        DeviceName = device.DeviceName,
                        DeviceType = device.DeviceType,
                        Location = device.Location,
                        SoldTo = device.SoldTo,
                        ClientAddress = device.ClientAddress,
                        InvoiceNumber = device.InvoiceNumber,
                        DueDate = device.DueDate,
                        Date = device.Date,
                        Time = device.Time,
                        Quantity = device.Quantity,
                        Price = device.Price




                    };
                   var outDate = DateOnly.FromDateTime(DateTime.Today);

                    // Save the deleted device under its original DeviceID
                    await _firebaseClient.Child("shippedDevices").Child(InvoiceNumber).PutAsync(deletedDevice);
                    await _firebaseClient.Child("invoice").Child(InvoiceNumber).Child("Status").PutAsync(2);
                    await _firebaseClient.Child("invoice").Child(InvoiceNumber).Child("OrderOut").PutAsync(outDate.ToString());
                    await _firebaseClient.Child("packedDevices").Child(InvoiceNumber).DeleteAsync();
                }
            }

            return RedirectToPage("Inven"); 
        }

        private string GenerateRandomInvoiceNumber(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }

 


}
