using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using TaskMarket.BackgroundJobs;
using TaskMarket.Handlers;
using TaskMarket.Helpers;
using TaskMarket.IRepositories;
using TaskMarket.Middlewares;
using TaskMarket.Models;
using TaskMarket.Repositories;

namespace TaskMarket
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Secrets come from user-secrets in Development and environment variables elsewhere
            var jwtSecret = builder.Configuration["JWT:Secret"];
            if (string.IsNullOrWhiteSpace(jwtSecret))
            {
                throw new InvalidOperationException(
                    "JWT:Secret is not configured. Run: dotnet user-secrets set \"JWT:Secret\" \"<a long random string>\"");
            }

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });
            builder.Services.AddSignalR();
            builder.Services.AddIdentity<User, IdentityRoleApplication>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // Removes the default 5-minute buffer so expiration is exact

                };

                options.Events = new JwtBearerEvents
                {
                    // Browsers can't send headers on WebSockets, so SignalR passes the token in the query string
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/chatHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var claim = context.Principal?.FindFirst("account_valid")?.Value;
                        if (!string.Equals(claim, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Fail("Account is not valid.");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5173", "http://localhost:5174") // React dev servers
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials(); // Required for the refresh-token cookie and SignalR
                });
            });


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Adds the "Authorize" button so protected endpoints can be tried from Swagger UI
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the token returned by /api/Accounts/Login"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IContractorRepository, ContractorRepository>();
            builder.Services.AddScoped<ITaskItemRepository, TaskItemRepository>();
            builder.Services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            builder.Services.AddScoped<IHiringRequestRepository, HiringRequestRepository>();
            builder.Services.AddScoped<IContractorSkillRepository, ContractorSkillRepository>();
            builder.Services.AddScoped<ITaskMediaRepository, TaskMediaRepository>();
            builder.Services.AddScoped<IJobOfferRepository, JobOfferRepository>();
            builder.Services.AddScoped<IOfferApplicationRepository, OfferApplicationRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IVerificationOfEmail, VerificationOfEmail>();
            builder.Services.AddScoped<IGenerateToken, GenerateToken>();

            // Register every handler class; nested types are compiler-generated (lambdas, async state machines)
            var handlerTypes = typeof(MessageHandler).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsNested && t.Namespace == "TaskMarket.Handlers");

            foreach (var handlerType in handlerTypes)
            {
                builder.Services.AddScoped(handlerType);
            }

            builder.Services.AddMemoryCache();
            builder.Services.AddHostedService<JobCacheRefreshService>();
            var app = builder.Build();

            // Registration assigns the Customer/Contractor roles, so they must exist
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRoleApplication>>();
                try
                {
                    await DatabaseSeeder.SeedRolesAsync(roleManager);
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Failed to seed roles; registration will fail until the database is reachable");
                }

                // Uncomment to apply migrations and seed fake data
                //var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //context.Database.Migrate();
                //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                //await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
            }

            // Configure the HTTP request pipeline.
            app.UseMiddleware<RequestTimingMiddleware>();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<ChatHub>("/chatHub");

            // Dashboard endpoint
            app.MapGet("/dashboard", () => Results.Content(DashboardHtml.Page, "text/html"));

            // Dashboard data API
            app.MapGet("/dashboard/data", async () =>
            {
                var path = Path.Combine(AppContext.BaseDirectory, "request-logs.txt");

                if (!File.Exists(path))
                    return Results.Json(Array.Empty<object>());

                var lines = await File.ReadAllLinesAsync(path);

                var logs = lines
                    .Reverse()              // newest first
                    .Take(100)
                    .Select(ParseLine)
                    .Where(l => l != null)
                    .ToList();

                return Results.Json(logs);
            });

            static object? ParseLine(string line)
            {
                // Format: [2026-06-20 14:32:01] GET /api/products → 200 in 45ms
                try
                {
                    var timestampEnd = line.IndexOf(']');
                    var timestamp = line.Substring(1, timestampEnd - 1);

                    var rest = line.Substring(timestampEnd + 2); // skip "] "
                    var parts = rest.Split(' ');

                    var method = parts[0];
                    var path = parts[1];
                    var statusCode = int.Parse(parts[3]);
                    var elapsedMs = long.Parse(parts[5].Replace("ms", ""));

                    return new
                    {
                        timestamp,
                        method,
                        path,
                        statusCode,
                        elapsedMs
                    };
                }
                catch
                {
                    return null;
                }
            }
            app.MapControllers();

            app.Run();
        }

    }
}
