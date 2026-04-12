using KitapTakasPlatformu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KitapTakasPlatformu.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await context.Database.MigrateAsync();

            var demoEmail = "demo@kitaptakas.com";
            var demoUser = await userManager.FindByEmailAsync(demoEmail);

            if (demoUser == null)
            {
                demoUser = new ApplicationUser
                {
                    UserName = demoEmail,
                    Email = demoEmail,
                    FullName = "Demo Kullanıcı",
                    SchoolName = "Ahmet Yüksel Özemre Bilim ve Sanat Merkezi",
                    SchoolNumber = "0001",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(demoUser, "Demo123!");
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                    throw new Exception("Demo kullanıcı oluşturulamadı: " + errors);
                }
            }

            if (!await context.Books.AnyAsync())
            {
                context.Books.AddRange(
                    new Book
                    {
                        Title = "Kürk Mantolu Madonna",
                        Author = "Sabahattin Ali",
                        Category = "Roman",
                        Description = "Temiz kullanılmış, eksiksiz.",
                        Condition = "İyi",
                        IsAvailable = true,
                        CreatedAt = DateTime.Now,
                        OwnerId = demoUser.Id
                    },
                    new Book
                    {
                        Title = "Suç ve Ceza",
                        Author = "Dostoyevski",
                        Category = "Klasik",
                        Description = "Okunmuş ama temiz durumda.",
                        Condition = "Orta",
                        IsAvailable = true,
                        CreatedAt = DateTime.Now,
                        OwnerId = demoUser.Id
                    },
                    new Book
                    {
                        Title = "Şeker Portakalı",
                        Author = "Jose Mauro De Vasconcelos",
                        Category = "Roman",
                        Description = "Yeni gibi.",
                        Condition = "Yeni",
                        IsAvailable = true,
                        CreatedAt = DateTime.Now,
                        OwnerId = demoUser.Id
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}