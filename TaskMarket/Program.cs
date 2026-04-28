using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using TaskMarket.Helpers;
using TaskMarket.IRepositories;
using TaskMarket.Models;
using TaskMarket.Repositories;

namespace TaskMarket
{
    public class Program
    {
        public static void Main(string[] args)
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


            builder.Services.AddIdentity<User, IdentityRole>()
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
                options.RequireHttpsMetadata = false; // Set to true in production if possible
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    // 1. Validate the Issuer (Who created the token)
                    ValidateIssuer = true,
                    ValidIssuer = "TaskMarketServer", // Must match the string in your Login Controller

                    // 2. Validate the Audience (Who the token is for)
                    ValidateAudience = true,
                    ValidAudience = "TaskMarketClient", // Must match the string in your Login Controller

                    // 3. Validate the Secret Key (Signature)
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),

                    // 4. Ensure token hasn't expired
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Removes the default 5-minute buffer so expiration is exact
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
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
