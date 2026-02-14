using Microsoft.EntityFrameworkCore;
using Nexo.Contexts;
using Nexo.Helpers;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Serviços
// -------------------------

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<AuthorizationHelper>();


// PostgreSQL
builder.Services.AddDbContext<NexoContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// -------------------------
// Pipeline
// -------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// -------------------------
// Comandos customizados
// -------------------------

if (args.Contains("seed"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<NexoContext>();

    Console.WriteLine("Executando Seed...");
    await DbInitializer.SeedAsync(context);
    Console.WriteLine("Seed finalizado.");

    return;
}

if (args.Contains("migrate"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<NexoContext>();

    Console.WriteLine("Aplicando migrations...");
    await context.Database.MigrateAsync();
    Console.WriteLine("Migrations aplicadas.");

    return;
}

app.Run();
