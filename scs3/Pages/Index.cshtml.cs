using Microsoft.AspNetCore.Mvc.RazorPages;
using Firebase.Database;
using Firebase.Database.Query;
using static Google.Cloud.Firestore.V1.StructuredAggregationQuery.Types.Aggregation.Types;
using System.Globalization;


namespace scs3.Pages
{
    public class IndexModel : PageModel
    {
        private readonly FirebaseClient _firebaseClient;

        public IndexModel(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        public int DeletedDevicesCount { get; set; }
        public int PackedDevicesCount { get; set; }
        public int ShippedDevicesCount { get; set; }
        public int DeliveredDevicesCount { get; set; }
        public int OTPCount { get; set; }
        public int UserCount { get; set; }
        public int DevicesCount { get; set; }
        public int SoldCount { get; set; } = 0;
        public int SalesCount { get; set; }
        public int PackDCount { get; set; }
        public int PackPCount { get; set; }
        public int PackTCount { get; set; }
        public int ShipDCount { get; set; }
        public int ShipPCount { get; set; }
        public int ShipTCount { get; set; }
        public int DeliverDCount { get; set; }
        public int DeliverPCount { get; set; }
        public int DeliverTCount { get; set; }

        public int ItemCount { get; set; }

        public int LowStockDevicesCount { get; set; }

        public int InCount { get; set; }
        public int OutCount { get; set; }
   


        public async Task OnGet()
        {
            
            var quantitySnapshot = await _firebaseClient.Child("deletedDevices").OnceAsync<object>();
            DeletedDevicesCount = quantitySnapshot.Count;

            var packedDevicesSnapshot = await _firebaseClient.Child("packedDevices").OnceAsync<object>();
            PackedDevicesCount = packedDevicesSnapshot.Count;

            var shippedDevicesSnapshot = await _firebaseClient.Child("shippedDevices").OnceAsync<object>();
            ShippedDevicesCount = shippedDevicesSnapshot.Count;

            var deliveredDevicesSnapshot = await _firebaseClient.Child("deliveredDevices").OnceAsync<object>();
            DeliveredDevicesCount = deliveredDevicesSnapshot.Count;

            var otpSnapshot = await _firebaseClient.Child("OTP").OnceAsync<object>();
            OTPCount = otpSnapshot.Count;

            var userSnapshot = await _firebaseClient.Child("Users").OnceAsync<object>();
            UserCount = userSnapshot.Count;

            var deviceSnapshot = await _firebaseClient.Child("devices").OnceAsync<object>();
            DevicesCount = deviceSnapshot.Count;

            var totalSnapshot = await _firebaseClient.Child("invoice").OnceAsync<object>();
            SoldCount = CalculateSoldCount(totalSnapshot);

            var deletedDevicesSnapshot = await _firebaseClient.Child("invoice").OnceAsync<object>();
            SalesCount = CalculateSalesCount(deletedDevicesSnapshot);


            var packedDCountSnapshot = await _firebaseClient.Child("packedDevices").OnceAsync<object>();
            PackDCount = CalculatePackDCount(packedDCountSnapshot);

            var itemDCountSnapshot1 = await _firebaseClient.Child("devices").OnceAsync<object>();
            ItemCount = CalculateItemCount(itemDCountSnapshot1);

            var packedPCountSnapshot = await _firebaseClient.Child("packedDevices").OnceAsync<object>();
            PackPCount = CalculatePackPCount(packedPCountSnapshot);

            var packedTCountSnapshot = await _firebaseClient.Child("packedDevices").OnceAsync<object>();
            PackTCount = CalculatePackTCount(packedTCountSnapshot);


            var shippedDCountSnapshot = await _firebaseClient.Child("shippedDevices").OnceAsync<object>();
            ShipDCount = CalculateShipDCount(shippedDCountSnapshot);

            var shippedPCountSnapshot = await _firebaseClient.Child("shippedDevices").OnceAsync<object>();
            ShipPCount = CalculateShipPCount(shippedPCountSnapshot);

            var shippedTCountSnapshot = await _firebaseClient.Child("shippedDevices").OnceAsync<object>();
            ShipTCount = CalculateShipTCount(shippedTCountSnapshot);


            var deliveredDCountSnapshot = await _firebaseClient.Child("deliveredDevices").OnceAsync<object>();
            DeliverDCount = CalculateDeliverDCount(deliveredDCountSnapshot);

            var deliveredPCountSnapshot = await _firebaseClient.Child("deliveredDevices").OnceAsync<object>();
            DeliverPCount = CalculateDeliverPCount(deliveredPCountSnapshot);

            var deliveredTCountSnapshot = await _firebaseClient.Child("deliveredDevices").OnceAsync<object>();
            DeliverTCount = CalculateDeliverTCount(deliveredTCountSnapshot);

            
            var deviceSnapshot1 = await _firebaseClient.Child("devices").OnceAsync<object>();

            // Filter devices with quantity less than 8
            var lowStockDevices = deviceSnapshot1.Where(item =>
            {
                dynamic deviceData = item.Object;
                if (deviceData.Quantity != null)
                {
                    return (int)deviceData.Quantity < 8;
                }
                return false;
            });

            
            int lowStockCount = lowStockDevices.Count();
            DevicesCount = deviceSnapshot.Count;
            LowStockDevicesCount = lowStockCount;

            InCount = CalculateDevicesPurchasedLast24Hours(deviceSnapshot);

            OutCount = CalculateOrdersOutLast24Hours(totalSnapshot);




        }


        private int CalculateOrdersOutLast24Hours(IEnumerable<FirebaseObject<object>> snapshot)
        {
            var currentDate = DateTime.Today;

            int count = 0;

            foreach (var item in snapshot)
            {
                dynamic invoiceData = item.Object;
                if (invoiceData.OrderOut != null)
                {
                    if (DateTime.TryParseExact(invoiceData.OrderOut.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime orderOutDate))
                    {
                        if (orderOutDate.Date == currentDate)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }


        private int CalculateDevicesPurchasedLast24Hours(IEnumerable<FirebaseObject<object>> snapshot)
        {
            var currentDate = DateTime.Today;

            int count = 0;

            foreach (var item in snapshot)
            {
                dynamic deviceData = item.Object;
                if (deviceData.PurchaseDate != null)
                {
                    if (DateTime.TryParseExact(deviceData.PurchaseDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime purchaseDate))
                    {
                        if (purchaseDate.Date == currentDate)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }
        private int CalculateSoldCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            int soldCount = 0;
            foreach (var item in snapshot)
            {
                dynamic deviceData = item.Object;

                if (deviceData.Quantity != null)
                {
                    soldCount += (int)deviceData.Quantity; // Add the quantity to soldCount
                }
            }
            return soldCount;
        }
        private int CalculateSalesCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            int salesCount = 0;
            foreach (var item in snapshot)
            {
                dynamic deviceData = item.Object;

                if (deviceData.Quantity != null && deviceData.Price != null)
                {
                    int quantity = (int)deviceData.Quantity;
                    int price = (int)deviceData.Price;
                    int totalPrice = quantity * price;

                    salesCount += totalPrice;
                }
            }
            return salesCount;
        }

        private int CalculatePackDCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            int packDCount = 0;
            foreach (var item in snapshot)
            {
                dynamic deviceData = item.Object;

                if (deviceData.Quantity != null)
                {
                    packDCount += (int)deviceData.Quantity;
                }
            }
            return packDCount;
        }
        private int CalculateItemCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            int itemCount = 0;
            foreach (var item in snapshot)
            {
                dynamic deviceData = item.Object;

                if (deviceData.Quantity != null)
                {
                    itemCount += (int)deviceData.Quantity;
                }
            }
            return itemCount;
        }

        private int CalculateShipDCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            int packDCount = 0;
            foreach (var item in snapshot)
            {
                dynamic deviceData = item.Object;

                if (deviceData.Quantity != null)
                {
                    packDCount += (int)deviceData.Quantity;
                }
            }
            return packDCount;
        }

        private int CalculateDeliverDCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            int packDCount = 0;
            foreach (var item in snapshot)
            {
                dynamic deviceData = item.Object;

                if (deviceData.Quantity != null)
                {
                    packDCount += (int)deviceData.Quantity;
                }
            }
            return packDCount;
        }
        private int CalculatePackPCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            int packPCount = 0;
            foreach (var item in snapshot)
            {
                // Cast to dynamic to access properties dynamically
                dynamic deviceData = item.Object;

                // Access the Quantity and Price properties if they exist
                if (deviceData.Quantity != null && deviceData.Price != null)
                {
                    int quantity = (int)deviceData.Quantity;
                    int price = (int)deviceData.Price;

                    // Add cost to packPCount
                    packPCount += quantity * price;
                }
            }
            return packPCount;
        }

        private int CalculateShipPCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            int packPCount = 0;
            foreach (var item in snapshot)
            {

                dynamic deviceData = item.Object;


                if (deviceData.Quantity != null && deviceData.Price != null)
                {
                    int quantity = (int)deviceData.Quantity;
                    int price = (int)deviceData.Price;


                    packPCount += quantity * price;
                }
            }
            return packPCount;
        }

        private int CalculateDeliverPCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            int packPCount = 0;
            foreach (var item in snapshot)
            {

                dynamic deviceData = item.Object;


                if (deviceData.Quantity != null && deviceData.Price != null)
                {
                    int quantity = (int)deviceData.Quantity;
                    int price = (int)deviceData.Price;


                    packPCount += quantity * price;
                }
            }
            return packPCount;
        }
        private int CalculatePackTCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            HashSet<string> uniqueDeviceTypes = new HashSet<string>();

            foreach (var item in snapshot)
            {
                dynamic deviceData = item.Object;

                if (deviceData.DeviceType != null)
                {
                    string deviceType = (string)deviceData.DeviceType;

                    uniqueDeviceTypes.Add(deviceType);
                }
            }

            int totalCount = uniqueDeviceTypes.Count;
            return totalCount;
        }

        private int CalculateShipTCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            HashSet<string> uniqueDeviceTypes = new HashSet<string>();

            foreach (var item in snapshot)
            {
                dynamic deviceData = item.Object;

                if (deviceData.DeviceType != null)
                {
                    string deviceType = (string)deviceData.DeviceType;


                    uniqueDeviceTypes.Add(deviceType);
                }
            }

            int totalCount = uniqueDeviceTypes.Count;
            return totalCount;
        }

        private int CalculateDeliverTCount(IEnumerable<FirebaseObject<object>> snapshot)
        {
            HashSet<string> uniqueDeviceTypes = new HashSet<string>();

            foreach (var item in snapshot)
            {
                dynamic deviceData = item.Object;

                if (deviceData.DeviceType != null)
                {
                    string deviceType = (string)deviceData.DeviceType;


                    uniqueDeviceTypes.Add(deviceType);
                }
            }

            int totalCount = uniqueDeviceTypes.Count;
            return totalCount;
        }

    }
}