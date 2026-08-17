using Microsoft.AspNetCore.Identity;
using Staybnb.Web.Constants;
using Staybnb.Web.Models;

namespace Staybnb.Web.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (context.HostProperties.Any()) return;

        var hostEmail = "demo.host@staybnb.local";
        var demoHost = await userManager.FindByEmailAsync(hostEmail);
        if (demoHost == null)
        {
            demoHost = new ApplicationUser
            {
                UserName = hostEmail,
                Email = hostEmail,
                FirstName = "Thandeka",
                LastName = "Nkosi",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(demoHost, "DemoHost123!");
            await userManager.AddToRoleAsync(demoHost, Roles.Host);
        }

        var reviewerEmails = new[] { "reviewer1@staybnb.local", "reviewer2@staybnb.local", "reviewer3@staybnb.local" };
        var reviewers = new List<ApplicationUser>();

        foreach (var email in reviewerEmails)
        {
            var reviewer = await userManager.FindByEmailAsync(email);
            if (reviewer == null)
            {
                reviewer = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = "Guest",
                    LastName = email.Split('@')[0],
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                await userManager.CreateAsync(reviewer, "Reviewer123!");
                await userManager.AddToRoleAsync(reviewer, Roles.Guest);
            }

            reviewers.Add(reviewer);
        }

        const string cabinExtTwilight = "https://images.unsplash.com/photo-1697807650304-907257330a3e?fm=jpg&q=80&w=1200&auto=format&fit=crop";
        const string cabinExtAFrame = "https://images.unsplash.com/photo-1697462247934-47afc5541494?fm=jpg&q=80&w=1200&auto=format&fit=crop";
        const string cabinIntStove = "https://images.unsplash.com/photo-1631630259742-c0f0b17c6c10?fm=jpg&q=80&w=1200&auto=format&fit=crop";
        const string cabinIntFireplace = "https://images.unsplash.com/photo-1698933787134-af2d451985c7?fm=jpg&q=80&w=1200&auto=format&fit=crop";
        const string cabinIntOrangeChairs = "https://images.unsplash.com/photo-1768488314310-3742b3c75579?fm=jpg&q=80&w=1200&auto=format&fit=crop";
        const string villaExtPool = "https://images.unsplash.com/photo-1635108198767-81d007b8814d?fm=jpg&q=80&w=1200&auto=format&fit=crop";
        const string villaIntBright = "https://images.unsplash.com/photo-1534353641488-754bfb2d6cd7?fm=jpg&q=80&w=1200&auto=format&fit=crop";
        const string apartmentExt = "https://images.unsplash.com/photo-1760235674447-fe0cc115b697?fm=jpg&q=80&w=1200&auto=format&fit=crop";
        const string apartmentIntBedroom = "https://images.unsplash.com/photo-1718717621302-a359be21a111?fm=jpg&q=80&w=1200&auto=format&fit=crop";
        const string cityView = "https://images.unsplash.com/photo-1704037201066-67eba2f05fd5?fm=jpg&q=80&w=1200&auto=format&fit=crop";

        var properties = new List<(string Title, string Desc, decimal Price, string Address, string City, string Type, int Guests, int Beds, int Baths, string Image, int[] Ratings)>
        {
            ("Cozy Forest Cabin", "Escape to this warm cabin tucked among the trees, with a wood stove and forest views from every window.",
                1450m, "12 Pine Ridge Road", "Drakensberg", "Cabin", 4, 4, 2, cabinExtTwilight, new[] {5,4,5}),

            ("A-Frame Woodland Retreat", "A striking modern A-frame cabin surrounded by forest, perfect for a quiet weekend away.",
                1980m, "8 Fern Valley Lane", "Nelspruit", "Cabin", 6, 5, 3, cabinExtAFrame, new[] {5,5,4}),

            ("Wood Stove Hideaway", "Rustic charm meets comfort in this bushveld cabin with a classic wood stove, close to the Panorama Route.",
                1320m, "3 Acacia Trail", "Mpumalanga", "Cabin", 3, 3, 1, cabinIntStove, new[] {4,4}),

            ("Fireside Mountain Cabin", "Unwind by the fireplace in this cozy mountain cabin, ideal for cold Drakensberg evenings.",
                1650m, "19 Highland Pass", "Drakensberg", "Cabin", 5, 3, 2, cabinIntFireplace, new[] {5,4}),

            ("Colourful Cabin Escape", "A vibrant, characterful cabin interior with warm lighting - a favourite with returning guests.",
                1250m, "6 Sunbird Close", "Nelspruit", "Cabin", 4, 2, 1, cabinIntOrangeChairs, new[] {4,5,4}),

            ("Poolside Family Villa", "A spacious villa with a private pool, ideal for family getaways and group stays.",
                3200m, "21 Sunset Crescent", "Umhlanga", "Villa", 8, 5, 4, villaExtPool, new[] {5,5,5}),

            ("Bright Living Villa", "An airy, light-filled villa with an open-plan living area, perfect for entertaining.",
                2850m, "14 Palm Grove Drive", "Umhlanga", "Villa", 7, 4, 3, villaIntBright, new[] {5,4}),

            ("Waterfront City Apartment", "A stylish apartment in a modern building, close to the V&A Waterfront.",
                2100m, "45 Dockside Ave", "Cape Town", "Apartment", 4, 2, 2, apartmentExt, new[] {4,5,5,4}),

            ("Comfy City Bedroom Suite", "A stylish, centrally located apartment with a comfortable bedroom - walking distance to the beachfront.",
                1150m, "67 Marine Parade", "Durban", "Apartment", 2, 1, 1, apartmentIntBedroom, new[] {3,4,5}),

            ("Harbour View Apartment", "Wake up to harbour views in this bright apartment near the city centre.",
                1890m, "22 Bree Street", "Cape Town", "Apartment", 3, 2, 1, cityView, new[] {4,4,5 })
        };

        var random = new Random();

        var reviewComments = new[]
        {
            "Wonderful stay, the host was very responsive and the place was spotless.",
            "Beautiful location and exactly as described. Would book again!",
            "Great value for money, comfortable beds and a quiet neighbourhood.",
            "Loved every minute of our stay. Highly recommend to anyone visiting the area.",
            "Clean, cozy, and close to everything we needed."
        };

        foreach (var p in properties)
        {
            var property = new HostProperty
            {
                Title = p.Title,
                Description = p.Desc,
                PricePerNight = p.Price,
                HostId = demoHost.Id,
                Address = p.Address,
                City = p.City,
                PropertyType = p.Type,
                IsActive = true,
                MaxGuests = p.Guests,
                Bedrooms = p.Beds,
                Beds = p.Beds,
                Bathrooms = p.Baths,
                CleaningFee = 150m,
                ServiceFee = Math.Round(p.Price * 0.1m, 0),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(5, 60))
            };

            context.HostProperties.Add(property);
            await context.SaveChangesAsync();

            context.PropertyImages.Add(new PropertyImage
            {
                PropertyId = property.Id,
                ImageUrl = p.Image
            });

            for (int i = 0; i < p.Ratings.Length; i++)
            {
                context.Reviews.Add(new Review
                {
                    PropertyId = property.Id,
                    ReviewerId = reviewers[i % reviewers.Count].Id,
                    Rating = p.Ratings[i],
                    Comment = reviewComments[random.Next(reviewComments.Length)],
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
