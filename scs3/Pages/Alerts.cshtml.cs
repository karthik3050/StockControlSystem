using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace scs3.Pages
{
    public class AlertsModel : PageModel
    {
        public List<DeviceAlertViewModel> Devices { get; set; }

        public async Task OnGetAsync()
        {
            Devices = await FetchDevicesAsync();
            CheckAlerts();
        }

        private void CheckAlerts()
        {
            foreach (var device in Devices)
            {
                device.AlertStatus = (device.Quantity < 8) ? "Low Stock" : "Normal";
            }
        }

        public async Task<IActionResult> OnPostAsync(string deviceName, int newQuantity)
        {
            await ReorderDeviceAsync(deviceName, newQuantity);
            return RedirectToPage(); // Redirect back to the Alerts page
        }

        public async Task<List<DeviceAlertViewModel>> FetchDevicesAsync()
        {
            var devices = new List<DeviceAlertViewModel>();
            var firebase = new FirebaseClient("https://stockcontrol-f2ff7-default-rtdb.firebaseio.com");
            var devicesSnapshot = await firebase.Child("devices").OnceAsync<Device>();

            foreach (var deviceSnapshot in devicesSnapshot)
            {
                var device = deviceSnapshot.Object;
                devices.Add(new DeviceAlertViewModel
                {
                    DeviceName = device.DeviceName,         
                    Quantity = device.Quantity
                });
            }

            return devices;
        }

        public async Task ReorderDeviceAsync(string deviceName, int newQuantity)
        {
            var firebase = new FirebaseClient("https://stockcontrol-f2ff7-default-rtdb.firebaseio.com");
            var devices = await firebase.Child("devices").OnceAsync<Device>();

            // Find the device by name using LINQ
            var deviceToUpdate = devices.FirstOrDefault(d => d.Object.DeviceName == deviceName);
            if (deviceToUpdate != null)
            {
                deviceToUpdate.Object.Quantity += newQuantity; // Update the quantity
                await firebase.Child("devices").Child(deviceToUpdate.Key).PutAsync(deviceToUpdate.Object);
            }
        }


    }

    public class DeviceAlertViewModel
    {
        public string DeviceName { get; set; }
        public int Quantity { get; set; }
        public string AlertStatus { get; set; }
    }
}
