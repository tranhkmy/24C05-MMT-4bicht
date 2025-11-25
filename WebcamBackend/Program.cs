using WebcamBackend.Hubs;
using WebcamBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- ĐĂNG KÝ DỊCH VỤ ---
builder.Services.AddSignalR(); // Kích hoạt SignalR
builder.Services.AddHostedService<TcpServerService>(); // Kích hoạt TCP Server chạy ngầm
// --------------------------

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

app.UseRouting();

app.UseAuthorization();

// --- ĐĂNG KÝ ĐƯỜNG DẪN ---
app.MapHub<WebcamHub>("/webcamHub");
// -----------------------------------------------------------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
