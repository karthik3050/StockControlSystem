using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Firebase.Database;

public class ReportModel : PageModel
{
    private readonly ILogger<ReportModel> _logger;
    private readonly FirebaseClient _firebaseClient;

    public Dictionary<string, int> DeviceTypeDistribution { get; set; }
    public Dictionary<string, int> DeletedDeviceTypeDistribution { get; set; }
    public Dictionary<string, int> DeviceLocationDistribution { get; set; }
    public int PackedCount { get; set; }

    public int ShippedCount { get; set; }
    public int DeliveredCount { get; set; }
    public ReportModel(ILogger<ReportModel> logger, FirebaseClient firebaseClient)
    {
        _logger = logger;
        _firebaseClient = firebaseClient;
        DeviceTypeDistribution = new Dictionary<string, int>();
        DeletedDeviceTypeDistribution = new Dictionary<string, int>();
        DeviceLocationDistribution = new Dictionary<string, int>();


    }

    public async Task OnGetAsync()
    {
        try
        {
            // Query the Firebase database to get the distribution of device types for active devices
            var devices = await _firebaseClient
                .Child("devices")
                .OnceAsync<Dictionary<string, object>>();

            foreach (var device in devices)
            {
                if (device.Object.TryGetValue("DeviceType", out object deviceTypeObj) && deviceTypeObj is string deviceType)
                {
                    if (DeviceTypeDistribution.ContainsKey(deviceType))
                    {
                        DeviceTypeDistribution[deviceType]++;
                    }
                    else
                    {
                        DeviceTypeDistribution.Add(deviceType, 1);
                    }
                }
                if (device.Object.TryGetValue("Location", out object locationObj) && locationObj is string location)
                {
                    if (DeviceLocationDistribution.ContainsKey(location))
                    {
                        DeviceLocationDistribution[location]++;
                    }
                    else
                    {
                        DeviceLocationDistribution.Add(location, 1);
                    }
                }
            }

            var deletedDevices = await _firebaseClient
                .Child("deletedDevices")
                .OnceAsync<Dictionary<string, object>>();

            // Count the number of deleted devices for each type
            foreach (var device in deletedDevices)
            {
                if (device.Object.TryGetValue("DeviceType", out object deviceTypeObj) && deviceTypeObj is string deviceType)
                {
                    if (DeletedDeviceTypeDistribution.ContainsKey(deviceType))
                    {
                        DeletedDeviceTypeDistribution[deviceType]++;
                    }
                    else
                    {
                        DeletedDeviceTypeDistribution.Add(deviceType, 1);
                    }
                }
            }

            // Query the Firebase database to get the counts for packed, shipped, and delivered devices
            var packedDevicesData = await _firebaseClient
                .Child("packedDevices")
                .OnceAsync<Dictionary<string, object>>();

            PackedCount = packedDevicesData.Count;

            var shippedDevicesData = await _firebaseClient
                .Child("shippedDevices")
                .OnceAsync<Dictionary<string, object>>();

            ShippedCount = shippedDevicesData.Count;

            var deliveredDevicesData = await _firebaseClient
                .Child("deliveredDevices")
                .OnceAsync<Dictionary<string, object>>();

            DeliveredCount = deliveredDevicesData.Count;

        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
        }
    }

}
