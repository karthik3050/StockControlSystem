using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace scs3.Pages
{
    public class PackedModel : PageModel
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

            public int Status {  get; set; }
        }

        private readonly FirebaseClient _firebaseClient;

        public List<DeletedDevice> DeletedDeviceHistory { get; set; }

        public PackedModel(FirebaseClient firebaseClient)
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
            var history = await _firebaseClient.Child("deletedDevices").OnceSingleAsync<Dictionary<string, DeletedDevice>>();
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
                var device = await _firebaseClient.Child("deletedDevices").Child(InvoiceNumber).OnceSingleAsync<Device>();
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
                        Price = device.Price,
                        Status = 0



                    };

                    // Save the deleted device under its original DeviceID
                    await _firebaseClient.Child("packedDevices").Child(InvoiceNumber).PutAsync(deletedDevice);
                    await _firebaseClient.Child("invoice").Child(InvoiceNumber).Child("Status").PutAsync(1);
                    await _firebaseClient.Child("deletedDevices").Child(InvoiceNumber).DeleteAsync();
                }
            }

            

            return RedirectToPage("Inven"); 
        }
    }
}
