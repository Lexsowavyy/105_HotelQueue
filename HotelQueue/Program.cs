using Microsoft.EntityFrameworkCore;
using HotelQueue.Data;
using HotelQueue.DataStructures;
using HotelQueue.DataStructures.Baseline;
using HotelQueue.Services;
using HotelQueue.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<HotelDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<NotificationService>(sp =>
    new NotificationService(sp.GetRequiredService<HotelDbContext>()));
builder.Services.AddScoped<WaitlistService>(sp =>
    new WaitlistService(sp.GetRequiredService<HotelDbContext>()));
builder.Services.AddScoped<ReservationHashTable>();
builder.Services.AddScoped<PriorityQueue>();
builder.Services.AddScoped<FIFOQueue>();

builder.Services.AddScoped<ReservationService>(sp =>
{
    return new ReservationService(
        sp.GetRequiredService<HotelDbContext>(),
        sp.GetRequiredService<ReservationHashTable>(),
        sp.GetRequiredService<PriorityQueue>(),
        sp.GetRequiredService<FIFOQueue>(),
        sp.GetRequiredService<RoomService>(),
        sp.GetRequiredService<NotificationService>(),
        sp.GetRequiredService<WaitlistService>()
    );
});

builder.Services.AddScoped<BaselineSingleQueue>();
builder.Services.AddScoped<BaselineReservationService>(sp =>
{
    return new BaselineReservationService(
        sp.GetRequiredService<HotelDbContext>(),
        sp.GetRequiredService<BaselineSingleQueue>(),
        sp.GetRequiredService<RoomService>()
    );
});

builder.Services.AddScoped<DashboardService>(sp =>
{
    return new DashboardService(
        sp.GetRequiredService<ReservationService>(),
        sp.GetRequiredService<RoomService>(),
        sp.GetRequiredService<WaitlistService>(),
        sp.GetRequiredService<NotificationService>()
    );
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
