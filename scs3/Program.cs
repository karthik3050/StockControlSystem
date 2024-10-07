using Firebase.Database;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Google.Api;
using PdfSharp.Fonts;
using scs3.Pages;
using PdfSharp.Charting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Initialize Firebase Admin SDK
string serviceAccountJsonPath = "Keys/user.json";
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile(serviceAccountJsonPath)
});

// Provide the Firebase Realtime Database URL
string firebaseDatabaseUrl = "https://stockcontrol-f2ff7-default-rtdb.firebaseio.com";

// Create a FirebaseClient instance with the database URL
builder.Services.AddSingleton<FirebaseClient>(new FirebaseClient(firebaseDatabaseUrl));


GlobalFontSettings.FontResolver = new CustomFontResolver();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
