using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Services;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using SmartHR.Hubs;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.MemoryStorage;

namespace SmartHR
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/smarthr-log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting web application");
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                var isDevelopment = builder.Environment.IsDevelopment();

            // 1. Database
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");

            builder.Services.AddDbContext<SmartHRContext>(options =>
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

            if (isDevelopment)
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // 2. Identity with lockout enabled (brute-force protection)
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount   = false;
                options.Password.RequireDigit           = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase       = true;
                options.Password.RequireLowercase       = true;
                options.Password.RequiredLength         = 8;
                options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers      = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SmartHRContext>();

            // 3. SignalR for real-time notifications
            builder.Services.AddSignalR();

            // Response compression helps reduce CSS/JS/HTML payload sizes.
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
                options.Level = CompressionLevel.Fastest);
            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
                options.Level = CompressionLevel.Fastest);

            // 4. Application Services & Performance Additions
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IAttendanceService, AttendanceService>();
            builder.Services.AddScoped<ILeaveService, LeaveService>();
            builder.Services.AddScoped<ISalaryService, SalaryService>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IRecruitmentService, RecruitmentService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddHealthChecks(); // Health Check for Prod Monitoring
            
            // Output caching & Rate Limiting
            builder.Services.AddOutputCache();
            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            // Hangfire for Background Jobs
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage());
            builder.Services.AddHangfireServer();

            // 5. AutoMapper
            builder.Services.AddAutoMapper(typeof(Program));

            // 6. MVC + Localization
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options => {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                });

            // Ensure Identity redirects match our custom AccountController.
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";

                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = isDevelopment
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
            });

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var cultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar-EG") };
                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures     = cultures;
                options.SupportedUICultures   = cultures;
            });

            var app = builder.Build();

            // 6. Middleware pipeline
            var locOpts = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(locOpts);

            if (app.Environment.IsDevelopment())
                app.UseMigrationsEndPoint();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseResponseCompression();

            // Security Headers Middleware
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Frame-Options", "DENY"); // Prevent Clickjacking
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff"); // Prevent MIME-type sniffing
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block"); // Enable XSS filter
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                if (!app.Environment.IsDevelopment())
                {
                    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains"); // Force HTTPS
                }
                await next();
            });

            // Static assets: cache aggressively in production; avoid caching during development.
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    var headers = ctx.Context.Response.Headers;
                    if (app.Environment.IsDevelopment())
                    {
                        headers.CacheControl = "no-store";
                        return;
                    }

                    // Most static assets are versioned via asp-append-version.
                    headers.CacheControl = "public,max-age=2592000"; // 30 days
                }
            });
            app.UseRouting();
            app.UseRateLimiter(); // Apply Rate Limiting
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Hangfire Dashboard (only accessible to authenticated users by default, can be restricted further)
            app.UseHangfireDashboard("/hangfire");

            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
            app.MapHub<NotificationHub>("/notificationHub");
            app.MapHealthChecks("/health"); // Provides an endpoint /health to verify system is alive

            // 7. Rotativa PDF generation
            RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

            // 8. Auto-migrate and Seed all comprehensive mock data and roles
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SmartHRContext>();
                await context.Database.MigrateAsync();
                await SmartHR.Data.DbSeeder.SeedDataAsync(scope.ServiceProvider);
            }

            RecurringJob.AddOrUpdate<PayrollJobService>("monthly-payroll", x => x.GenerateMonthlySalariesAsync(), "0 0 28 * *");

            app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task SeedAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger  = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var config  = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var env     = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            // Using AppRoles constants instead of magic strings
            string[] roles =
            {
                AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.HR,
                AppRoles.Manager, AppRoles.Employee, AppRoles.Client
            };

            foreach (var role in roles)
                if (!await roleMgr.RoleExistsAsync(role))
                {
                    var createRole = await roleMgr.CreateAsync(new IdentityRole(role));
                    if (!createRole.Succeeded)
                    {
                        logger.LogError(
                            "Failed to create role {Role}: {Errors}",
                            role,
                            string.Join("; ", createRole.Errors.Select(e => e.Description)));
                    }
                }

            // Admin seeding should never silently create a known credential in production.
            var seedAdminEnabled = config.GetValue<bool?>("SeedAdmin:Enabled")
                ?? env.IsDevelopment();

            if (!seedAdminEnabled)
                return;

            if (!env.IsDevelopment() && !env.IsStaging())
            {
                logger.LogWarning("SeedAdmin is enabled, but seeding is disabled outside Development/Staging.");
                return;
            }

            var adminEmail = config["SeedAdmin:Email"];
            if (string.IsNullOrWhiteSpace(adminEmail))
                adminEmail = "superadmin@smarthr.com";

            var adminPassword = config["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                if (env.IsDevelopment())
                {
                    adminPassword = "Admin@123";
                    logger.LogWarning(
                        "SeedAdmin:Password is not configured; using the development default password. " +
                        "Set it via user-secrets or environment variables.");
                }
                else
                {
                    logger.LogError("SeedAdmin:Password is missing. Admin user will not be seeded.");
                    return;
                }
            }

            if (!env.IsDevelopment() && string.Equals(adminPassword, "Admin@123", StringComparison.Ordinal))
            {
                logger.LogError(
                    "Refusing to seed SuperAdmin with the default password outside Development. " +
                    "Set SeedAdmin:Password to a strong value.");
                return;
            }

            var existingAdmin = await userMgr.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new ApplicationUser
                {
                    UserName       = adminEmail,
                    Email          = adminEmail,
                    EmailConfirmed = true,
                    FullName       = "System Super Admin",
                    HireDate       = DateTime.UtcNow,
                    IsActive       = true
                };

                var result = await userMgr.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userMgr.AddToRoleAsync(admin, AppRoles.SuperAdmin);
                    logger.LogInformation("SuperAdmin created: {Email}", adminEmail);
                }
                else
                {
                    foreach (var e in result.Errors)
                        logger.LogError("Seed error: {Desc}", e.Description);
                }
            }
            else
            {
                if (!await userMgr.IsInRoleAsync(existingAdmin, AppRoles.SuperAdmin))
                {
                    await userMgr.AddToRoleAsync(existingAdmin, AppRoles.SuperAdmin);
                    logger.LogInformation("SuperAdmin role ensured for: {Email}", adminEmail);
                }
            }
        }
    }
}
