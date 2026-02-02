using RuralAddress.Web.Components;
// Force rebuild for new routes
using Microsoft.EntityFrameworkCore;
using RuralAddress.Infrastructure.Data;
using RuralAddress.Core.Interfaces;
using RuralAddress.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using RuralAddress.Web.Data;
using RuralAddress.Core.Entities;
using Microsoft.AspNetCore.DataProtection; // <--- NOVO: Necessário para a vacina

// Enable legacy timestamp behavior for PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configura para aceitar os headers do Nginx
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    
    // TRUST ALL PROXIES (Required for Nginx on different IP/Docker/etc)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// --- VACINA ANTI-ZUMBI (Persistência de Chaves) ---
// Isso impede que os cookies fiquem inválidos ao reiniciar o serviço
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/www/ruraladdress/keys"))
    .SetApplicationName("RuralAddressApp");
// --------------------------------------------------

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => options.DetailedErrors = true);

// Adicione isso junto com os outros serviços, antes do builder.Build()
builder.Services.AddSingleton<RuralAddress.Web.Services.PanicStateService>();

builder.Services.AddControllers(); // API Support
builder.Services.AddSignalR();     // SignalR Support

builder.Services.AddRazorPages();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddScoped<IPropriedadeService, PropriedadeService>();
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
builder.Services.AddScoped<ISystemParameterService, SystemParameterService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, RuralAddress.Web.Services.CurrentUserService>();
builder.Services.AddScoped<RuralAddress.Web.Services.RestoreService>();

var app = builder.Build();

app.UseForwardedHeaders();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await IdentityDataInitializer.SeedData(userManager, roleManager);
        await ParameterInitializer.SeedData(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
// app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers(); // Map API
app.MapHub<RuralAddress.Web.Hubs.PanicHub>("/panichub"); // Map SignalR

app.MapRazorPages();

app.Run();