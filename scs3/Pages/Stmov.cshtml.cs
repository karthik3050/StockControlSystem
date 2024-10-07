using Firebase.Database;
using Firebase.Database.Query;
using iText.Commons.Bouncycastle.Asn1.Ess;
using iText.Kernel.Colors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace scs3.Pages
{
    public class Device
    {
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string Location { get; set; }
        public DateOnly Date { get; set; }
        public string SoldTo { get; set; }
        public TimeSpan Time { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public string ClientAddress { get; set; }

        public string InvoiceNumber { get; set; }
        public string StreetName { get; set; }
        public string PostCode { get; set; }
        public DateOnly DueDate { get; set; }
    }

    public class DeletedDevice
    {
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string Location { get; set; }
        public string SoldTo { get; set; }
        public DateOnly Date { get; set; }
        public TimeSpan Time { get; set; }
        public int Quantity { get; set; }
         
        public int Status { get; set; }
        public decimal Price { get; set; }

        public string ClientAddress { get; set; }
        public string StreetName { get; set; }
        public string PostCode { get; set; }


        public string InvoiceNumber { get; set; }
        public DateOnly DueDate { get; set; }
    }

    public class StmovModel : PageModel
    {
        private readonly FirebaseClient _firebaseClient;

        public List<Device> FirebaseData { get; set; }
        public List<DeletedDevice> DeletedDeviceHistory { get; set; }

        public StmovModel(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await RefreshData();
            await LoadDeletedDeviceHistory();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAndDisplayFormAsync(string[] selectedDevices, string soldTo, DateOnly date, TimeSpan time, int quantity, decimal price, string cid, string cid1, string cid2, DateOnly ddate, int nid)
        {
            // Check if selected devices exist and have sufficient quantity
            if (selectedDevices != null && selectedDevices.Length > 0)
            {

                foreach (var deviceId in selectedDevices)
                {
                    var deviceRef = _firebaseClient.Child("devices").Child(deviceId);
                    var device = await deviceRef.OnceSingleAsync<Device>();
                    if (device != null && device.Quantity >= quantity)
                    {
                        // Calculate new quantity
                        int newQuantity = device.Quantity - quantity;
                        if (newQuantity != 0)
                        {
                            // Update only the quantity in devices node
                            await deviceRef.Child("Quantity").PutAsync(newQuantity);
                        }
                        else
                        {
                            await _firebaseClient.Child("devices").Child(deviceId).DeleteAsync();

                        }

                        // Storing the device in history before deleting it
                        await _firebaseClient.Child("history").Child(deviceId).PutAsync(device);


                        string invoiceNumber = GenerateRandomInvoiceNumber(6); // 6-character alphanumeric

                        // Create deleted device object
                        var deletedDevice = new DeletedDevice
                        {
                            DeviceID = device.DeviceID,
                            DeviceName = device.DeviceName,
                            DeviceType = device.DeviceType,
                            Location = device.Location,
                            SoldTo = soldTo,
                            ClientAddress = cid,
                            StreetName = cid1,
                            PostCode = cid2,
                            InvoiceNumber = invoiceNumber,
                            DueDate = ddate,
                            Date = date,
                            Time = time,
                            Quantity = quantity,
                            Price = device.Price,
                            Status = 0
                        };

                        TempData["ErrorMessage"] = "Entry Successful!";

                        // Save the deleted device under its original DeviceID
                        await _firebaseClient.Child("deletedDevices").Child(invoiceNumber).PutAsync(deletedDevice);
                        await _firebaseClient.Child("invoice").Child(invoiceNumber).PutAsync(deletedDevice);

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Quantity Exceeded";

                        
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "No Selection";

            }
            // Refresh data and history after delete
            await RefreshData();
            await LoadDeletedDeviceHistory();
            return RedirectToPage();

        }



        private async Task RefreshData()
        {
            FirebaseData = await FetchDataFromFirebaseAsync();
        }

        private async Task<List<Device>> FetchDataFromFirebaseAsync()
        {
            var data = await _firebaseClient.Child("devices").OnceSingleAsync<Dictionary<string, Device>>();
            var dataList = new List<Device>();

            if (data != null)
            {
                foreach (var item in data)
                {
                    dataList.Add(item.Value);
                }
            }

            return dataList;
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

        private string GenerateRandomInvoiceNumber(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}