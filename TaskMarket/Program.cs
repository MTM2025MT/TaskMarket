using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using TaskMarket.Handlers;
using TaskMarket.Helpers;
using TaskMarket.IRepositories;
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
            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    connectionString
                   )
);           

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddIdentity<User, IdentityRoleApplication>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // This is the part you were missing:
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = "TaskMarketServer", // Must match the string in your Login Controller

                    ValidateAudience = true,
                    ValidAudience = "TaskMarketClient", // Must match the string in your Login Controller

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // Removes the default 5-minute buffer so expiration is exact

                };

                options.Events = new JwtBearerEvents
                {
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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5173") // <--- REPLACE THIS with your React URL
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials(); // <--- This is REQUIRED for cookies to work
                });
            });


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
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

            var handlerTypes = typeof(MessageHandler).Assembly
    .GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "TaskMarket.Handlers");

            foreach (var handlerType in handlerTypes)
            {
                builder.Services.AddScoped(handlerType);
            }

            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                // Run the seeder
                //using (var scope = app.Services.CreateScope())
                //{
                //    var services = scope.ServiceProvider;
                //    var context = services.GetRequiredService<ApplicationDbContext>();

                //    // Ensures the database exists and applies pending migrations
                //    context.Database.Migrate();

                //    // Seed the fake data
                //    var userManager = services.GetRequiredService<UserManager<User>>();
                //    var roleManager = services.GetRequiredService<RoleManager<IdentityRoleApplication>>();
                //    await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
                //}
            }
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
           app.UseCors("CorsPolicy");
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

    }
}
