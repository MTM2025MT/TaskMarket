using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMarket.Models
{
    public static class DatabaseSeeder
    {
        private const string DefaultPassword = "P@ssw0rd123!";

        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRoleApplication> roleManager)
        {
            Randomizer.Seed = new Random(1337);

            await SeedRolesAsync(roleManager);

            var categories = new Faker<ServiceCategory>()
                .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Slug, (f, c) => c.Name.ToLower().Replace(" ", "-"))
                .Generate(10);

            context.ServiceCategories.AddRange(categories);
            await context.SaveChangesAsync();

            var customers = new List<Customer>();
            var customerFaker = new Faker<Customer>()
                .RuleFor(u => u.UserName, f => $"customer_{f.UniqueIndex}")
                .RuleFor(u => u.Email, f => $"customer_{f.UniqueIndex}@example.com")
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.IsActive, true)
                .RuleFor(u => u.IsVerified, true)
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(1))
                .RuleFor(c => c.StripeCustomerId, f => "cus_" + f.Random.AlphaNumeric(14))
                .RuleFor(c => c.BillingAddress, f => f.Address.FullAddress());

            foreach (var customer in customerFaker.Generate(50))
            {
                customer.EmailConfirmed = true;
                var savedCustomer = await CreateOrGetUserAsync(userManager, customer, "Customer");
                customers.Add((Customer)savedCustomer);
            }

            var contractors = new List<Contractor>();
            var contractorFaker = new Faker<Contractor>()
                .RuleFor(u => u.UserName, f => $"contractor_{f.UniqueIndex}")
                .RuleFor(u => u.Email, f => $"contractor_{f.UniqueIndex}@example.com")
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.IsActive, true)
                .RuleFor(u => u.IsVerified, true)
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(2))
                .RuleFor(c => c.BusinessName, f => f.Company.CompanyName())
                .RuleFor(c => c.Bio, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HourlyRate, f => f.Finance.Amount(20, 150))
                .RuleFor(c => c.RatingAvg, f => f.Random.Decimal(3.0m, 5.0m));

            foreach (var contractor in contractorFaker.Generate(20))
            {
                contractor.EmailConfirmed = true;
                var savedContractor = await CreateOrGetUserAsync(userManager, contractor, "Contractor");
                contractors.Add((Contractor)savedContractor);
            }

            var allUsers = customers.Cast<User>().Concat(contractors).ToList();

            var tasks = new Faker<TaskItem>()
                .RuleFor(t => t.ClientId, (f, t) => f.PickRandom(customers).Id)
                .RuleFor(t => t.CategoryId, (f, t) => f.PickRandom(categories).Id)
                .RuleFor(t => t.ContractorId, (f, t) => f.Random.Bool(0.7f) && contractors.Count > 0
                    ? f.PickRandom(contractors).Id
                    : null)
                .RuleFor(t => t.Title, f => f.Lorem.Sentence(3, 5))
                .RuleFor(t => t.Description, f => f.Lorem.Paragraphs(2))
                .RuleFor(t => t.Address, f => f.Address.StreetAddress())
                .RuleFor(t => t.Status, f => f.PickRandom<TaskStatus>())
                .RuleFor(t => t.CreatedAt, f => f.Date.Recent(30))
                .Generate(100);

            context.Tasks.AddRange(tasks);
            await context.SaveChangesAsync();

            var jobOffers = new Faker<JobOffer>()
                .RuleFor(j => j.TaskId, (f, j) => f.PickRandom(tasks).Id)
                .RuleFor(j => j.ClientId, (f, j) => tasks.First(t => t.Id == j.TaskId).ClientId)
                .RuleFor(j => j.Price, f => f.Finance.Amount(50, 1000))
                .RuleFor(j => j.Deadline, f => f.Date.Future(1))
                .RuleFor(j => j.Status, f => f.PickRandom<OfferStatus>())
                .RuleFor(j => j.CreatedAt, f => f.Date.Recent(10))
                .Generate(40);

            context.JobOffers.AddRange(jobOffers);
            await context.SaveChangesAsync();

            var faker = new Faker();

            var contractorSkills = new List<ContractorSkill>();
            var contractorSkillPairs = new HashSet<(int ContractorId, int CategoryId)>();

            foreach (var contractor in contractors)
            {
                var count = faker.Random.Int(1, Math.Min(3, categories.Count));
                var picked = faker.Random.ListItems(categories, count);

                foreach (var category in picked)
                {
                    var key = (ContractorId: contractor.Id, CategoryId: category.Id);
                    if (contractorSkillPairs.Add(key))
                    {
                        contractorSkills.Add(new ContractorSkill
                        {
                            ContractorId = contractor.Id,
                            CategoryId = category.Id
                        });
                    }
                }
            }

            context.ContractorSkills.AddRange(contractorSkills);
            await context.SaveChangesAsync();

            var taskMedias = new Faker<TaskMedia>()
                .RuleFor(tm => tm.TaskId, (f, tm) => f.PickRandom(tasks).Id)
                .RuleFor(tm => tm.MediaUrl, f => f.Image.PicsumUrl())
                .RuleFor(tm => tm.MediaType, f => f.PickRandom(new[] { "Image", "Video" }))
                .RuleFor(tm => tm.CreatedAt, f => f.Date.Recent(30))
                .Generate(200);

            context.TaskMedias.AddRange(taskMedias);
            await context.SaveChangesAsync();

            var offerApplications = new Faker<OfferApplication>()
                .RuleFor(a => a.OfferId, (f, a) => f.PickRandom(jobOffers).Id)
                .RuleFor(a => a.ContractorId, (f, a) => f.PickRandom(contractors).Id)
                .RuleFor(a => a.Message, f => f.Lorem.Sentence())
                .RuleFor(a => a.PriceBid, f => f.Random.Bool(0.3f) ? f.Finance.Amount(50, 1000) : null)
                .RuleFor(a => a.Status, f => f.PickRandom<ApplicationStatus>())
                .RuleFor(a => a.CreatedAt, f => f.Date.Recent(15))
                .Generate(60);

            context.OfferApplications.AddRange(offerApplications);
            await context.SaveChangesAsync();

            var hiringRequests = new Faker<HiringRequest>()
                .RuleFor(h => h.TaskId, (f, h) => f.PickRandom(tasks).Id)
                .RuleFor(h => h.ClientId, (f, h) => tasks.First(t => t.Id == h.TaskId).ClientId)
                .RuleFor(h => h.ContractorId, (f, h) => f.PickRandom(contractors).Id)
                .RuleFor(h => h.Message, f => f.Lorem.Sentence())
                .RuleFor(h => h.PriceOffered, f => f.Finance.Amount(50, 500))
                .RuleFor(h => h.Status, f => f.PickRandom<HiringRequestStatus>())
                .RuleFor(h => h.SentAt, f => f.Date.Recent(20))
                .RuleFor(h => h.RespondedAt, (f, h) => h.Status == HiringRequestStatus.Pending ? null : f.Date.Recent(10))
                .Generate(60);

            context.HiringRequests.AddRange(hiringRequests);
            await context.SaveChangesAsync();

            var messages = new List<Message>();
            foreach (var task in tasks)
            {
                var messageCount = faker.Random.Int(1, 5);
                for (var i = 0; i < messageCount; i++)
                {
                    int senderId;
                    int receiverId;

                    if (task.ContractorId.HasValue && faker.Random.Bool(0.8f))
                    {
                        senderId = faker.Random.Bool() ? task.ClientId : task.ContractorId.Value;
                        receiverId = senderId == task.ClientId ? task.ContractorId.Value : task.ClientId;
                    }
                    else
                    {
                        var sender = faker.PickRandom(allUsers).Id;
                        var receiver = faker.PickRandom(allUsers.Where(u => u.Id != sender).ToList()).Id;
                        senderId = sender;
                        receiverId = receiver;
                    }

                    messages.Add(new Message
                    {
                        TaskId = task.Id,
                        SenderId = senderId,
                        ReceiverId = receiverId,
                        Content = faker.Lorem.Sentence(),
                        IsRead = faker.Random.Bool(0.5f),
                        SentAt = faker.Date.Recent(15)
                    });
                }
            }

            context.Messages.AddRange(messages);
            await context.SaveChangesAsync();

            var tasksWithContractor = tasks.Where(t => t.ContractorId.HasValue).ToList();
            if (tasksWithContractor.Count > 0)
            {
                var payments = new Faker<Payment>()
                    .RuleFor(p => p.TaskId, (f, p) => f.PickRandom(tasksWithContractor).Id)
                    .RuleFor(p => p.PayerId, (f, p) => tasksWithContractor.First(t => t.Id == p.TaskId).ClientId)
                    .RuleFor(p => p.PayeeId, (f, p) => tasksWithContractor.First(t => t.Id == p.TaskId).ContractorId!.Value)
                    .RuleFor(p => p.Amount, f => f.Finance.Amount(50, 1000))
                    .RuleFor(p => p.Status, f => f.PickRandom<PaymentStatus>())
                    .RuleFor(p => p.CreatedAt, f => f.Date.Recent(10))
                    .Generate(40);

                context.Payments.AddRange(payments);

                var reviews = new Faker<Review>()
                    .RuleFor(r => r.TaskId, (f, r) => f.PickRandom(tasksWithContractor).Id)
                    .RuleFor(r => r.ReviewerId, (f, r) => tasksWithContractor.First(t => t.Id == r.TaskId).ClientId)
                    .RuleFor(r => r.TargetId, (f, r) => tasksWithContractor.First(t => t.Id == r.TaskId).ContractorId!.Value)
                    .RuleFor(r => r.Rating, f => f.Random.Int(1, 5))
                    .RuleFor(r => r.Comment, f => f.Lorem.Sentence())
                    .RuleFor(r => r.CreatedAt, f => f.Date.Recent(10))
                    .Generate(40);

                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();
            }
        }

        // Roles are required by registration, so this runs on every startup
        public static async Task SeedRolesAsync(RoleManager<IdentityRoleApplication> roleManager)
        {
            await EnsureRoleExistsAsync(roleManager, "Customer");
            await EnsureRoleExistsAsync(roleManager, "Contractor");
        }

        private static async Task EnsureRoleExistsAsync(
            RoleManager<IdentityRoleApplication> roleManager,
            string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRoleApplication
                {
                    Name = roleName
                });

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
                }
            }
        }

        private static async Task<User> CreateOrGetUserAsync(
            UserManager<User> userManager,
            User user,
            string roleName)
        {
            var existing = await userManager.FindByNameAsync(user.UserName);
            if (existing is null && !string.IsNullOrWhiteSpace(user.Email))
            {
                existing = await userManager.FindByEmailAsync(user.Email);
            }

            if (existing is not null)
            {
                if (!await userManager.IsInRoleAsync(existing, roleName))
                {
                    var roleResult = await userManager.AddToRoleAsync(existing, roleName);
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to add user '{existing.UserName}' to role '{roleName}': {errors}");
                    }
                }

                return existing;
            }

            var createResult = await userManager.CreateAsync(user, DefaultPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user '{user.UserName}': {errors}");
            }

            var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to add user '{user.UserName}' to role '{roleName}': {errors}");
            }

            return user;
        }
    }
}
