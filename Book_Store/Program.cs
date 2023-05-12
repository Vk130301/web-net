using Book_Store.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BookManagementContext>(option => option.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaltConnectionString")
    ));

builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));
builder.Services.AddSession();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(p =>
    {
        p.Cookie.Name = "UserLoginCookie";
        p.ExpireTimeSpan = TimeSpan.FromDays(1);
        p.LoginPath = "/dang-nhap.html";
        p.LogoutPath = "/dang-xuat/html";
        p.AccessDeniedPath = "/not-found.html";
    });

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddMvc().AddNToastNotifyToastr(new ToastrOptions()
{
    ProgressBar = true,
    TimeOut = 5000,
    PositionClass = ToastPositions.TopRight
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseNToastNotify();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
