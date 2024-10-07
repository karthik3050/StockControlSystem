using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace scs3.Pages
{
    public class GenerateModel : PageModel
    {
        private readonly FirebaseClient _firebaseClient;

        public List<DeletedDevice> DeletedDevices { get; set; }
        public string ErrorMessage { get; set; }


        public GenerateModel(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        public async Task OnGetAsync()
        {
            await LoadDeletedDevices();
        }

        public async Task<IActionResult> OnPostAsync(string selectedDevices)
        {
            if (string.IsNullOrEmpty(selectedDevices))
            {
                ErrorMessage = "Please select at least one device.";
                await LoadDeletedDevices(); // Load devices to display them again
                return Page();
            }

            string[] selectedDeviceIds = selectedDevices.Split(','); // Split the string into an array
            return await GenerateInvoice(selectedDeviceIds);
        }



        private async Task LoadDeletedDevices()
        {
            var data = await _firebaseClient.Child("invoice").OnceSingleAsync<JObject>();

            DeletedDevices = new List<DeletedDevice>();

            if (data != null)
            {
                foreach (var property in data.Properties())
                {
                    var deletedDevice = property.Value.ToObject<DeletedDevice>();
                    DeletedDevices.Add(deletedDevice);
                }
            }
        }

        public async Task<IActionResult> GenerateInvoice(string[] selectedDevices)
        {
            if (selectedDevices == null || selectedDevices.Length == 0)
            {
               return RedirectToPage("/Generate");   

            }
            else
            {
                var invoiceFilename = "invoice.pdf";// Assign custom font resolver
                                                    //new PDF documentt
                var pdfDocument = new PdfDocument();
                var pdfPage = pdfDocument.AddPage();
                var gfx = XGraphics.FromPdfPage(pdfPage);
                // Font and Size
                XFont fontTitle = new XFont("Arial", 24);
                XFont fontTitle2 = new XFont("Arial", 14);
                XFont fontDetails = new XFont("Arial", 12);
                double yOffset = 40;
                // invoice header
                gfx.DrawString("Invoice", fontTitle, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                yOffset += fontTitle.GetHeight() + 20; // Increment yOffset

                gfx.DrawString("FROM", fontTitle2, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                yOffset += fontTitle2.GetHeight() + 5; // Increment yOffset

                gfx.DrawString("University of Leicester.", fontDetails, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                yOffset += fontDetails.GetHeight() + 2; // Increment yOffset
                gfx.DrawString("University Road", fontDetails, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                yOffset += fontDetails.GetHeight() + 2; // Increment yOffset
                gfx.DrawString("Leicester, LE1 7RH", fontDetails, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);


                yOffset += fontDetails.GetHeight() + 20; // Increment yOffset

                // Fetch invoice details from the database 
                var firstDeviceDetails = await FetchDeviceDetailsFromDatabase(selectedDevices[0]);
                Random random = new Random();
                int invoiceNumber = random.Next(100000, 1000000); // Generates a random number between 100000 and 999999

                // Format the number as a string with leading zeros if needed
                string formattedInvoiceNumber = invoiceNumber.ToString("D6");
                if (firstDeviceDetails != null)
                {
                    // Print invoice details in the PDF
                    gfx.DrawString($"Invoice #: {formattedInvoiceNumber}", fontDetails, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                    gfx.DrawString($"Date: {firstDeviceDetails.Date.ToShortDateString()}", fontDetails, XBrushes.Black, new XRect(10, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopCenter);
                    gfx.DrawString($"Due Date: {firstDeviceDetails.DueDate.ToShortDateString()}", fontDetails, XBrushes.Black, new XRect(190, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopCenter);
                    yOffset += fontDetails.GetHeight() + 20; // Increment yOffset

                    gfx.DrawString("BILL TO", fontTitle2, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                    yOffset += fontTitle2.GetHeight() + 5; // Increment yOffset
                    gfx.DrawString(firstDeviceDetails.ClientAddress, fontDetails, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                    yOffset += fontDetails.GetHeight() + 2; // Increment yOffset
                    gfx.DrawString(firstDeviceDetails.StreetName, fontDetails, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                    yOffset += fontDetails.GetHeight() + 2; // Increment yOffset
                    gfx.DrawString(firstDeviceDetails.PostCode, fontDetails, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                    yOffset += fontDetails.GetHeight() + 20; // Increment yOffset
                }


                // table header
                gfx.DrawString("Description", fontDetails, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                gfx.DrawString("Quantity", fontDetails, XBrushes.Black, new XRect(200, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                gfx.DrawString("Unit Price", fontDetails, XBrushes.Black, new XRect(350, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                gfx.DrawString("Amount", fontDetails, XBrushes.Black, new XRect(500, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                yOffset += fontDetails.GetHeight() + 5; // Increment yOffset
                gfx.DrawLine(XPens.Black, 50, yOffset, pdfPage.Width - 50, yOffset); // Draw horizontal line
                yOffset += 5;

                decimal totalamount = 0;
                // Fetch details of selected devices and draw in the table
                foreach (var InvoiceNumber in selectedDevices)
                {
                    // Fetch details based on device ID
                    var deviceDetails = await FetchDeviceDetailsFromDatabase(InvoiceNumber);

                    // Printing
                    if (deviceDetails != null)
                    {
                        decimal amount = deviceDetails.Quantity * deviceDetails.Price;
                        totalamount += amount;
                        // print device details in the table
                        gfx.DrawString(deviceDetails.DeviceName, fontDetails, XBrushes.Black, new XRect(50, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                        gfx.DrawString(deviceDetails.Quantity.ToString(), fontDetails, XBrushes.Black, new XRect(205, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                        gfx.DrawString($"{deviceDetails.Price.ToString()}.00", fontDetails, XBrushes.Black, new XRect(355, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                        gfx.DrawString($"{amount.ToString()}.00", fontDetails, XBrushes.Black, new XRect(500, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft); ;
                        yOffset += fontDetails.GetHeight() + 10; // Increase yOffset for next line REMEMBERRRRRRRRRRRRR

                    }
                }
                gfx.DrawLine(XPens.Black, 50, yOffset, pdfPage.Width - 50, yOffset); // Draw horizontal line
                yOffset += fontDetails.GetHeight() + 20; // Increase yOffset for next line REMEMBERRRRRRRRRRRRR
                 // subtotal, taxes, and total
                gfx.DrawString($"SubTotal: {totalamount}", fontDetails, XBrushes.Black, new XRect(454, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                yOffset += fontDetails.GetHeight() + 5;
                gfx.DrawString("Taxes: 0%", fontDetails, XBrushes.Black, new XRect(469, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                yOffset += fontDetails.GetHeight() + 5;
                gfx.DrawString($"Total: {totalamount}", fontDetails, XBrushes.Black, new XRect(475, yOffset, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                yOffset += fontDetails.GetHeight() + 5;



                // Saving the PDF document
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    pdfDocument.Save(memoryStream);
                    return File(memoryStream.ToArray(), "application/pdf", invoiceFilename);
                }

                
            }
          }

            // fetch device from DB method
            private async Task<DeletedDevice> FetchDeviceDetailsFromDatabase(string InvoiceNumber)
            {
                var deviceData = await _firebaseClient.Child("invoice").Child(InvoiceNumber).OnceSingleAsync<JObject>();

                if (deviceData != null)
                {

                    var deviceDetails = deviceData.ToObject<DeletedDevice>();
                    return deviceDetails;
                }

                return null;
            }
        
    }

    public class CustomFontResolver : IFontResolver     //font link
    {
        public byte[] GetFont(string faceName)
        {
            using (var ms = new MemoryStream())
            {

                using (var fs = new FileStream("Fonts/ARIAL.ttf", FileMode.Open, FileAccess.Read))
                {
                    fs.CopyTo(ms);
                }
                return ms.ToArray();
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo("Arial");
        }
    }

}
