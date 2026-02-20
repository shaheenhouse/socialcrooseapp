using System.Text.Json;
using Marketplace.Database.Entities;
using Marketplace.Database.Entities.Social;
using Marketplace.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Database;

public static class DatabaseSeeder
{
    private const string DefaultPasswordHash =
        "$2a$12$LPbPaJ9wJ3kz9tFQ5gVFn.Yb2GzRDpYi9qDSxdXiA0c0jUZKxLfGi";

    public static async Task SeedAsync(MarketplaceDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // ──────────────────────────────────────────────
        // 1. ROLES
        // ──────────────────────────────────────────────
        var roles = new Dictionary<string, Role>
        {
            ["Admin"] = MakeRole("Admin", "Administrator", "Full system access", "#E53E3E", "shield", 100, isDefault: false),
            ["User"] = MakeRole("User", "Regular User", "Standard marketplace user", "#3182CE", "user", 10, isDefault: true),
            ["Seller"] = MakeRole("Seller", "Seller", "Can create stores and sell products/services", "#38A169", "store", 20),
            ["Freelancer"] = MakeRole("Freelancer", "Freelancer", "Can bid on projects and offer services", "#805AD5", "briefcase", 20),
            ["HRManager"] = MakeRole("HRManager", "HR Manager", "Can manage company employees and hiring", "#DD6B20", "users", 30),
        };
        context.Roles.AddRange(roles.Values);

        // ──────────────────────────────────────────────
        // 2. PERMISSIONS
        // ──────────────────────────────────────────────
        var permissions = CreatePermissions(now);
        context.Permissions.AddRange(permissions);

        // ──────────────────────────────────────────────
        // 3. ROLE ↔ PERMISSION mapping
        // ──────────────────────────────────────────────
        var rolePermissions = CreateRolePermissions(roles, permissions, now);
        context.RolePermissions.AddRange(rolePermissions);

        // ──────────────────────────────────────────────
        // 4. CATEGORIES  (25+ across Product / Service / Project / Skill)
        // ──────────────────────────────────────────────
        var categories = CreateCategories(now);
        context.Categories.AddRange(categories.Values);

        // ──────────────────────────────────────────────
        // 5. SKILLS  (50+)
        // ──────────────────────────────────────────────
        var skills = CreateSkills(categories, now);
        context.Skills.AddRange(skills.Values);

        // ──────────────────────────────────────────────
        // 6. USERS  (25)
        // ──────────────────────────────────────────────
        var users = CreateUsers(now);
        context.Users.AddRange(users.Values);

        // ──────────────────────────────────────────────
        // 7. USER ROLES
        // ──────────────────────────────────────────────
        var userRoles = CreateUserRoles(users, roles, now);
        context.UserRoles.AddRange(userRoles);

        // ──────────────────────────────────────────────
        // 8. USER PROFILES
        // ──────────────────────────────────────────────
        var profiles = CreateUserProfiles(users, now);
        context.UserProfiles.AddRange(profiles);

        // ──────────────────────────────────────────────
        // 9. STORES  (10)
        // ──────────────────────────────────────────────
        var stores = CreateStores(users, now);
        context.Stores.AddRange(stores.Values);

        // ──────────────────────────────────────────────
        // 10. PRODUCTS  (50+)
        // ──────────────────────────────────────────────
        var (products, productImages) = CreateProducts(stores, categories, now);
        context.Products.AddRange(products);
        context.ProductImages.AddRange(productImages);

        // ──────────────────────────────────────────────
        // 11. SERVICES  (30+) with packages
        // ──────────────────────────────────────────────
        var (services, servicePackages) = CreateServices(users, stores, categories, now);
        context.Services.AddRange(services);
        context.ServicePackages.AddRange(servicePackages);

        // ──────────────────────────────────────────────
        // 12. PROJECTS  (20)
        // ──────────────────────────────────────────────
        var projects = CreateProjects(users, categories, skills, now);
        context.Projects.AddRange(projects);

        // ──────────────────────────────────────────────
        // 13. COMPANIES  (10) with employees
        // ──────────────────────────────────────────────
        var (companies, companyEmployees) = CreateCompanies(users, roles, now);
        context.Companies.AddRange(companies);
        context.CompanyEmployees.AddRange(companyEmployees);

        // ──────────────────────────────────────────────
        // 14. REVIEWS  (100+)
        // ──────────────────────────────────────────────
        var reviews = CreateReviews(users, products, services, stores, now);
        context.Reviews.AddRange(reviews);

        // ──────────────────────────────────────────────
        // 15. USER SKILLS
        // ──────────────────────────────────────────────
        var userSkills = CreateUserSkills(users, skills, now);
        context.UserSkills.AddRange(userSkills);

        // ──────────────────────────────────────────────
        // 16. CONNECTIONS
        // ──────────────────────────────────────────────
        var connections = CreateConnections(users, now);
        context.Set<Connection>().AddRange(connections);

        // ──────────────────────────────────────────────
        // 17. FOLLOWS
        // ──────────────────────────────────────────────
        var follows = CreateFollows(users, stores, companies, now);
        context.Set<Follow>().AddRange(follows);

        // ──────────────────────────────────────────────
        // 18. PORTFOLIOS & RESUMES (sample data)
        // ──────────────────────────────────────────────
        var portfolios = CreatePortfolios(users, now);
        context.Portfolios.AddRange(portfolios);

        await context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════
    //  HELPER: Role builder
    // ═══════════════════════════════════════════════════
    private static Role MakeRole(string name, string display, string desc,
        string color, string icon, int priority, bool isDefault = false)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayName = display,
            Description = desc,
            IsSystemRole = true,
            IsDefault = isDefault,
            Priority = priority,
            Color = color,
            Icon = icon,
            Scope = "Global",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
        };
    }

    // ═══════════════════════════════════════════════════
    //  PERMISSIONS (18)
    // ═══════════════════════════════════════════════════
    private static List<Permission> CreatePermissions(DateTime now)
    {
        var defs = new (string Module, string Action, string Display, string Desc)[]
        {
            ("Users", "Manage", "Manage Users", "Full user management"),
            ("Users", "Read", "View Users", "View user profiles"),
            ("Users", "Update", "Edit Users", "Update user information"),
            ("Stores", "Manage", "Manage Stores", "Full store management"),
            ("Stores", "Create", "Create Store", "Create new stores"),
            ("Stores", "Read", "View Stores", "View store details"),
            ("Products", "Manage", "Manage Products", "Full product management"),
            ("Products", "Create", "Create Products", "Create new products"),
            ("Products", "Read", "View Products", "View product listings"),
            ("Services", "Manage", "Manage Services", "Full service management"),
            ("Services", "Create", "Create Services", "Create new services"),
            ("Projects", "Manage", "Manage Projects", "Full project management"),
            ("Projects", "Create", "Create Projects", "Create new projects"),
            ("Projects", "Bid", "Bid on Projects", "Submit bids on projects"),
            ("Orders", "Manage", "Manage Orders", "Full order management"),
            ("Reviews", "Manage", "Manage Reviews", "Moderate reviews"),
            ("Reviews", "Create", "Create Reviews", "Leave reviews"),
            ("Companies", "Manage", "Manage Companies", "Full company management"),
        };

        return defs.Select(d => new Permission
        {
            Id = Guid.NewGuid(),
            Name = $"{d.Module}.{d.Action}",
            DisplayName = d.Display,
            Description = d.Desc,
            Module = d.Module,
            Action = d.Action,
            Resource = d.Module,
            IsSystemPermission = true,
            CreatedAt = now,
            IsDeleted = false,
        }).ToList();
    }

    // ═══════════════════════════════════════════════════
    //  ROLE ↔ PERMISSION mapping
    // ═══════════════════════════════════════════════════
    private static List<RolePermission> CreateRolePermissions(
        Dictionary<string, Role> roles, List<Permission> perms, DateTime now)
    {
        var byName = perms.ToDictionary(p => p.Name);
        var list = new List<RolePermission>();

        void Map(string role, params string[] permNames)
        {
            foreach (var pn in permNames)
            {
                if (!byName.TryGetValue(pn, out var p)) continue;
                list.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = roles[role].Id,
                    PermissionId = p.Id,
                    CreatedAt = now,
                    IsDeleted = false,
                });
            }
        }

        Map("Admin",
            "Users.Manage", "Users.Read", "Users.Update",
            "Stores.Manage", "Stores.Create", "Stores.Read",
            "Products.Manage", "Products.Create", "Products.Read",
            "Services.Manage", "Services.Create",
            "Projects.Manage", "Projects.Create", "Projects.Bid",
            "Orders.Manage", "Reviews.Manage", "Reviews.Create",
            "Companies.Manage");

        Map("User",
            "Users.Read", "Products.Read", "Stores.Read",
            "Reviews.Create", "Projects.Create");

        Map("Seller",
            "Users.Read", "Stores.Create", "Stores.Read",
            "Products.Create", "Products.Read",
            "Services.Create", "Orders.Manage", "Reviews.Create");

        Map("Freelancer",
            "Users.Read", "Projects.Bid",
            "Services.Create", "Reviews.Create");

        Map("HRManager",
            "Users.Read", "Users.Update",
            "Companies.Manage", "Projects.Create");

        return list;
    }

    // ═══════════════════════════════════════════════════
    //  CATEGORIES  (30)
    // ═══════════════════════════════════════════════════
    private static Dictionary<string, Category> CreateCategories(DateTime now)
    {
        var cats = new Dictionary<string, Category>();
        int sort = 0;

        Category Add(string name, string type, string? parentKey = null, string? desc = null)
        {
            var slug = name.ToLower().Replace(" ", "-").Replace("&", "and");
            var c = new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug,
                Description = desc ?? $"Browse {name.ToLower()} on our marketplace",
                Type = type,
                ParentId = parentKey != null ? cats[parentKey].Id : null,
                Level = parentKey != null ? 1 : 0,
                SortOrder = sort++,
                IsActive = true,
                IsFeatured = sort <= 10,
                ImageUrl = $"https://picsum.photos/seed/{slug}/800/400",
                IconUrl = $"https://picsum.photos/seed/{slug}-icon/64/64",
                CreatedAt = now,
                IsDeleted = false,
            };
            cats[name] = c;
            return c;
        }

        // Product categories
        Add("Electronics", "Product", desc: "Laptops, phones, gadgets and accessories");
        Add("Fashion", "Product", desc: "Clothing, shoes, and accessories for all");
        Add("Home & Garden", "Product", desc: "Furniture, decor, and garden essentials");
        Add("Books", "Product", desc: "Physical and digital books across genres");
        Add("Software", "Product", desc: "Desktop, mobile, and SaaS applications");
        Add("Sports & Outdoors", "Product", desc: "Sporting goods and outdoor equipment");
        Add("Health & Beauty", "Product", desc: "Skincare, wellness, and beauty products");
        Add("Toys & Games", "Product", desc: "Fun for kids and adults alike");

        // Service categories
        Add("Web Development", "Service", desc: "Websites, web apps, and e-commerce solutions");
        Add("Mobile Development", "Service", desc: "iOS, Android, and cross-platform apps");
        Add("Design & Creative", "Service", desc: "Graphic design, branding, and creative work");
        Add("Digital Marketing", "Service", desc: "SEO, PPC, social media, and content marketing");
        Add("Writing & Content", "Service", desc: "Copywriting, articles, and content creation");
        Add("Video & Animation", "Service", desc: "Video production, editing, and animation");
        Add("Data & Analytics", "Service", desc: "Data science, BI, and analytics services");
        Add("Consulting", "Service", desc: "Business, tech, and strategy consulting");
        Add("Music & Audio", "Service", desc: "Music production, voiceover, and audio editing");

        // Project categories
        Add("Web Projects", "Project", desc: "Web application and website projects");
        Add("Mobile Projects", "Project", desc: "Mobile application development projects");
        Add("Design Projects", "Project", desc: "UI/UX and graphic design projects");
        Add("Marketing Projects", "Project", desc: "Marketing campaigns and strategy projects");
        Add("AI & ML Projects", "Project", desc: "Artificial intelligence and machine learning projects");

        // Skill categories
        Add("Programming", "Skill", desc: "Programming languages and frameworks");
        Add("Design Skills", "Skill", desc: "Design tools and methodologies");
        Add("Marketing Skills", "Skill", desc: "Marketing platforms and techniques");
        Add("Business Skills", "Skill", desc: "Business management and strategy");
        Add("Finance Skills", "Skill", desc: "Accounting, analysis, and financial tools");
        Add("DevOps Skills", "Skill", desc: "Infrastructure, CI/CD, and cloud");
        Add("Data Science", "Skill", desc: "Machine learning, analytics, and AI");
        Add("Soft Skills", "Skill", desc: "Communication, leadership, and teamwork");

        return cats;
    }

    // ═══════════════════════════════════════════════════
    //  SKILLS  (55)
    // ═══════════════════════════════════════════════════
    private static Dictionary<string, Skill> CreateSkills(
        Dictionary<string, Category> cats, DateTime now)
    {
        var skills = new Dictionary<string, Skill>();
        int sort = 0;

        Skill Add(string name, string catKey, bool verifiable = true, bool featured = false)
        {
            var slug = name.ToLower().Replace(" ", "-").Replace("#", "sharp")
                .Replace(".", "dot").Replace("/", "-").Replace("+", "plus");
            var s = new Skill
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug,
                Description = $"Professional {name} skills and expertise",
                CategoryId = cats.ContainsKey(catKey) ? cats[catKey].Id : null,
                IsVerifiable = verifiable,
                HasTest = verifiable,
                TestDurationMinutes = verifiable ? 30 : null,
                PassingScore = verifiable ? 70 : null,
                IsActive = true,
                IsFeatured = featured,
                SortOrder = sort++,
                CreatedAt = now,
                IsDeleted = false,
            };
            skills[name] = s;
            return s;
        }

        // Programming (20)
        Add("C#", "Programming", featured: true);
        Add("Java", "Programming", featured: true);
        Add("Python", "Programming", featured: true);
        Add("JavaScript", "Programming", featured: true);
        Add("TypeScript", "Programming", featured: true);
        Add("React", "Programming", featured: true);
        Add("Angular", "Programming");
        Add("Vue.js", "Programming");
        Add("Node.js", "Programming", featured: true);
        Add(".NET", "Programming", featured: true);
        Add("SQL", "Programming");
        Add("Go", "Programming");
        Add("Rust", "Programming");
        Add("PHP", "Programming");
        Add("Ruby", "Programming");
        Add("Swift", "Programming");
        Add("Kotlin", "Programming");
        Add("Flutter", "Programming");
        Add("React Native", "Programming");
        Add("Next.js", "Programming");

        // Design (8)
        Add("UI/UX Design", "Design Skills", featured: true);
        Add("Figma", "Design Skills", featured: true);
        Add("Adobe Photoshop", "Design Skills");
        Add("Adobe Illustrator", "Design Skills");
        Add("Adobe XD", "Design Skills");
        Add("Sketch", "Design Skills");
        Add("Graphic Design", "Design Skills");
        Add("Motion Graphics", "Design Skills");

        // Marketing (8)
        Add("SEO", "Marketing Skills", featured: true);
        Add("Content Marketing", "Marketing Skills");
        Add("Social Media Marketing", "Marketing Skills");
        Add("Google Ads", "Marketing Skills");
        Add("Facebook Ads", "Marketing Skills");
        Add("Email Marketing", "Marketing Skills");
        Add("Copywriting", "Marketing Skills");
        Add("Analytics", "Marketing Skills");

        // Business (6)
        Add("Project Management", "Business Skills", featured: true);
        Add("Agile/Scrum", "Business Skills");
        Add("Product Management", "Business Skills");
        Add("Business Strategy", "Business Skills");
        Add("Entrepreneurship", "Business Skills", verifiable: false);
        Add("Leadership", "Business Skills", verifiable: false);

        // Finance (5)
        Add("Accounting", "Finance Skills");
        Add("Microsoft Excel", "Finance Skills");
        Add("Financial Analysis", "Finance Skills");
        Add("Bookkeeping", "Finance Skills");
        Add("QuickBooks", "Finance Skills");

        // DevOps (5)
        Add("Docker", "DevOps Skills");
        Add("Kubernetes", "DevOps Skills");
        Add("AWS", "DevOps Skills", featured: true);
        Add("Azure", "DevOps Skills");
        Add("CI/CD", "DevOps Skills");

        // Data Science (5)
        Add("Machine Learning", "Data Science", featured: true);
        Add("TensorFlow", "Data Science");
        Add("Data Visualization", "Data Science");
        Add("R", "Data Science");
        Add("Power BI", "Data Science");

        return skills;
    }

    // ═══════════════════════════════════════════════════
    //  USERS  (25)
    // ═══════════════════════════════════════════════════
    private static Dictionary<string, User> CreateUsers(DateTime now)
    {
        var users = new Dictionary<string, User>();

        User Add(string username, string first, string last, string email,
            string? country = "US", string? city = null, string? bio = null,
            bool verifiedSeller = false, decimal reputation = 0,
            decimal avgRating = 0, int totalReviews = 0)
        {
            var u = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                FirstName = first,
                LastName = last,
                Email = email,
                PasswordHash = DefaultPasswordHash,
                PhoneNumber = $"+1555{new Random(username.GetHashCode()).Next(1000000, 9999999)}",
                AvatarUrl = $"https://api.dicebear.com/7.x/avataaars/svg?seed={username}",
                Bio = bio,
                Status = UserStatus.Active,
                EmailVerified = true,
                EmailVerifiedAt = now.AddDays(-30),
                PhoneVerified = true,
                PhoneVerifiedAt = now.AddDays(-28),
                PreferredLanguage = "en",
                TimeZone = "America/New_York",
                Currency = "USD",
                Country = country,
                City = city,
                ReputationScore = reputation,
                AverageRating = avgRating,
                TotalReviews = totalReviews,
                IsVerifiedSeller = verifiedSeller,
                IsVerifiedBuyer = true,
                LastLoginAt = now.AddHours(-new Random(username.GetHashCode()).Next(1, 72)),
                CreatedAt = now.AddDays(-new Random(username.GetHashCode()).Next(30, 365)),
                IsDeleted = false,
            };
            users[username] = u;
            return u;
        }

        // Admins (2)
        Add("admin", "System", "Admin", "admin@marketplace.com",
            city: "San Francisco", bio: "Platform administrator", reputation: 100);
        Add("moderator", "Sarah", "Moderator", "sarah.mod@marketplace.com",
            city: "New York", bio: "Community moderator and support lead", reputation: 95);

        // Freelancers (8)
        Add("john_dev", "John", "Mitchell", "john.mitchell@email.com",
            city: "Austin", bio: "Full-stack developer with 8 years of experience in .NET and React. Passionate about clean architecture.",
            reputation: 92, avgRating: 4.8m, totalReviews: 47);
        Add("priya_designer", "Priya", "Sharma", "priya.sharma@email.com",
            country: "IN", city: "Bangalore", bio: "UI/UX designer crafting beautiful digital experiences. Expert in Figma and design systems.",
            reputation: 88, avgRating: 4.9m, totalReviews: 63);
        Add("alex_data", "Alexander", "Petrov", "alex.petrov@email.com",
            country: "DE", city: "Berlin", bio: "Data scientist and ML engineer. Turning data into actionable insights.",
            reputation: 85, avgRating: 4.7m, totalReviews: 31);
        Add("maria_writer", "Maria", "Garcia", "maria.garcia@email.com",
            country: "ES", city: "Barcelona", bio: "Content strategist and copywriter. Words that convert.",
            reputation: 83, avgRating: 4.6m, totalReviews: 52);
        Add("yuki_mobile", "Yuki", "Tanaka", "yuki.tanaka@email.com",
            country: "JP", city: "Tokyo", bio: "Mobile developer specializing in Flutter and React Native. 50+ apps shipped.",
            reputation: 90, avgRating: 4.8m, totalReviews: 38);
        Add("omar_devops", "Omar", "Hassan", "omar.hassan@email.com",
            country: "AE", city: "Dubai", bio: "DevOps engineer and cloud architect. AWS and Kubernetes certified.",
            reputation: 87, avgRating: 4.7m, totalReviews: 25);
        Add("lucas_python", "Lucas", "Silva", "lucas.silva@email.com",
            country: "BR", city: "São Paulo", bio: "Python developer and AI enthusiast. Building intelligent solutions.",
            reputation: 81, avgRating: 4.5m, totalReviews: 19);
        Add("emma_market", "Emma", "Thompson", "emma.thompson@email.com",
            country: "GB", city: "London", bio: "Digital marketing specialist. SEO, PPC, and growth hacking expert.",
            reputation: 86, avgRating: 4.7m, totalReviews: 41);

        // Store Owners (5)
        Add("techstore_mike", "Michael", "Chen", "michael.chen@email.com",
            city: "Seattle", bio: "Founder of TechGadgets store. Curating the best electronics since 2019.",
            verifiedSeller: true, reputation: 91, avgRating: 4.6m, totalReviews: 156);
        Add("fashion_nina", "Nina", "Kowalski", "nina.kowalski@email.com",
            country: "PL", city: "Warsaw", bio: "Fashion enthusiast and boutique owner. Sustainable style advocate.",
            verifiedSeller: true, reputation: 89, avgRating: 4.8m, totalReviews: 203);
        Add("bookworm_raj", "Rajesh", "Patel", "rajesh.patel@email.com",
            country: "IN", city: "Mumbai", bio: "Book lover running an indie bookstore. Rare finds and bestsellers.",
            verifiedSeller: true, reputation: 84, avgRating: 4.5m, totalReviews: 87);
        Add("homecraft_lisa", "Lisa", "Anderson", "lisa.anderson@email.com",
            city: "Portland", bio: "Handcrafted home decor and artisanal goods. Made with love.",
            verifiedSeller: true, reputation: 88, avgRating: 4.7m, totalReviews: 112);
        Add("softshop_dave", "David", "Kim", "david.kim@email.com",
            city: "San Jose", bio: "Software tools and digital products. Productivity solutions for teams.",
            verifiedSeller: true, reputation: 86, avgRating: 4.4m, totalReviews: 64);

        // Company Owners / HR (5)
        Add("ceo_james", "James", "Wilson", "james.wilson@techcorp.com",
            city: "Chicago", bio: "CEO of TechCorp Solutions. Building the future of enterprise software.",
            reputation: 93, avgRating: 4.9m, totalReviews: 12);
        Add("hr_anna", "Anna", "Johansson", "anna.johansson@nordictech.com",
            country: "SE", city: "Stockholm", bio: "Head of HR at NordicTech. Passionate about talent acquisition.",
            reputation: 80);
        Add("founder_wei", "Wei", "Zhang", "wei.zhang@innovateai.com",
            country: "CN", city: "Shanghai", bio: "AI startup founder. Democratizing machine learning for businesses.",
            reputation: 88, avgRating: 4.6m, totalReviews: 8);
        Add("cto_sarah", "Sarah", "O'Brien", "sarah.obrien@cloudnine.com",
            country: "IE", city: "Dublin", bio: "CTO at CloudNine. Scaling infrastructure for millions of users.",
            reputation: 91, avgRating: 4.8m, totalReviews: 15);
        Add("hr_miguel", "Miguel", "Rodriguez", "miguel.rodriguez@latamdev.com",
            country: "MX", city: "Mexico City", bio: "HR Director at LatamDev. Connecting LATAM talent with global opportunities.",
            reputation: 79);

        // Regular Users / Buyers (5)
        Add("buyer_tom", "Thomas", "Brown", "thomas.brown@email.com",
            city: "Boston", bio: "Small business owner looking for quality products and services.",
            reputation: 70, avgRating: 4.3m, totalReviews: 8);
        Add("buyer_sophie", "Sophie", "Martin", "sophie.martin@email.com",
            country: "FR", city: "Paris", bio: "Startup founder searching for talented freelancers.",
            reputation: 75, avgRating: 4.5m, totalReviews: 14);
        Add("buyer_ahmed", "Ahmed", "Al-Rashid", "ahmed.alrashid@email.com",
            country: "SA", city: "Riyadh", bio: "E-commerce entrepreneur expanding into new markets.",
            reputation: 72, avgRating: 4.2m, totalReviews: 6);
        Add("newuser_jane", "Jane", "Doe", "jane.doe@email.com",
            city: "Denver", bio: "New to the platform, excited to explore!");
        Add("newuser_bob", "Robert", "Smith", "robert.smith@email.com",
            city: "Miami", bio: "Looking for great deals and services.");

        return users;
    }

    // ═══════════════════════════════════════════════════
    //  USER ROLES
    // ═══════════════════════════════════════════════════
    private static List<UserRole> CreateUserRoles(
        Dictionary<string, User> users, Dictionary<string, Role> roles, DateTime now)
    {
        var list = new List<UserRole>();

        void Assign(string username, params string[] roleNames)
        {
            foreach (var r in roleNames)
            {
                list.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = users[username].Id,
                    RoleId = roles[r].Id,
                    CreatedAt = now,
                    IsDeleted = false,
                });
            }
        }

        // Admins
        Assign("admin", "Admin", "User");
        Assign("moderator", "Admin", "User");

        // Freelancers
        Assign("john_dev", "User", "Freelancer");
        Assign("priya_designer", "User", "Freelancer");
        Assign("alex_data", "User", "Freelancer");
        Assign("maria_writer", "User", "Freelancer");
        Assign("yuki_mobile", "User", "Freelancer");
        Assign("omar_devops", "User", "Freelancer");
        Assign("lucas_python", "User", "Freelancer");
        Assign("emma_market", "User", "Freelancer");

        // Store owners
        Assign("techstore_mike", "User", "Seller");
        Assign("fashion_nina", "User", "Seller");
        Assign("bookworm_raj", "User", "Seller");
        Assign("homecraft_lisa", "User", "Seller");
        Assign("softshop_dave", "User", "Seller");

        // Company / HR
        Assign("ceo_james", "User", "Seller");
        Assign("hr_anna", "User", "HRManager");
        Assign("founder_wei", "User", "Seller", "Freelancer");
        Assign("cto_sarah", "User", "Seller");
        Assign("hr_miguel", "User", "HRManager");

        // Regular users
        Assign("buyer_tom", "User");
        Assign("buyer_sophie", "User");
        Assign("buyer_ahmed", "User");
        Assign("newuser_jane", "User");
        Assign("newuser_bob", "User");

        return list;
    }

    // ═══════════════════════════════════════════════════
    //  USER PROFILES
    // ═══════════════════════════════════════════════════
    private static List<UserProfile> CreateUserProfiles(
        Dictionary<string, User> users, DateTime now)
    {
        var profiles = new List<UserProfile>();

        UserProfile Add(string username, string? headline = null, string? about = null,
            decimal hourlyRate = 0, int yoe = 0, int completed = 0,
            string? website = null, string? linkedin = null, string? github = null,
            string? portfolio = null, bool available = true, string? company = null,
            string? education = null, string? certs = null)
        {
            var p = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = users[username].Id,
                Headline = headline,
                About = about ?? users[username].Bio,
                HourlyRate = hourlyRate,
                YearsOfExperience = yoe,
                CompletedProjects = completed,
                AvailableForHire = available,
                Website = website,
                LinkedInUrl = linkedin ?? $"https://linkedin.com/in/{username}",
                GitHubUrl = github,
                PortfolioUrl = portfolio,
                CompanyName = company,
                Education = education,
                Certifications = certs,
                CreatedAt = now,
                IsDeleted = false,
            };
            profiles.Add(p);
            return p;
        }

        Add("admin", "Platform Administrator", hourlyRate: 0, available: false,
            company: "SocialMarketplace");
        Add("moderator", "Community Moderator & Support Lead", hourlyRate: 0,
            available: false, company: "SocialMarketplace");

        Add("john_dev", "Senior Full-Stack Developer | .NET & React Expert",
            "I build scalable web applications using modern architectures. Experienced with microservices, CQRS, and event-driven systems. Open to interesting projects.",
            hourlyRate: 95, yoe: 8, completed: 47,
            github: "https://github.com/johnmitchell",
            portfolio: "https://johnmitchell.dev",
            education: "B.S. Computer Science, UT Austin",
            certs: "Microsoft Certified: Azure Developer Associate, AWS Solutions Architect");

        Add("priya_designer", "UI/UX Designer | Design Systems & Figma Expert",
            "Creating intuitive and beautiful user experiences for web and mobile products. Design thinking practitioner with a passion for accessibility.",
            hourlyRate: 80, yoe: 6, completed: 63,
            portfolio: "https://priyasharma.design",
            website: "https://dribbble.com/priyasharma",
            education: "M.Des Interaction Design, NID Ahmedabad",
            certs: "Google UX Design Certificate, Nielsen Norman UX Certification");

        Add("alex_data", "Data Scientist & ML Engineer",
            "Specializing in predictive analytics, NLP, and computer vision. Experience with TensorFlow, PyTorch, and production ML pipelines.",
            hourlyRate: 110, yoe: 7, completed: 31,
            github: "https://github.com/alexpetrov",
            education: "M.Sc. Machine Learning, TU Berlin",
            certs: "TensorFlow Developer Certificate, AWS ML Specialty");

        Add("maria_writer", "Content Strategist & SEO Copywriter",
            "Crafting compelling content that ranks and converts. Expertise in SaaS, tech, and e-commerce content strategy.",
            hourlyRate: 65, yoe: 5, completed: 52,
            portfolio: "https://mariagarcia.writing.com",
            education: "M.A. Journalism, Universidad Autónoma de Barcelona");

        Add("yuki_mobile", "Senior Mobile Developer | Flutter & React Native",
            "Building cross-platform mobile experiences. 50+ apps published. Expert in performance optimization and native integrations.",
            hourlyRate: 100, yoe: 9, completed: 38,
            github: "https://github.com/yukitanaka",
            portfolio: "https://yukitanaka.dev",
            education: "B.Eng. Software Engineering, University of Tokyo",
            certs: "Google Associate Android Developer, Apple Developer Certification");

        Add("omar_devops", "DevOps Engineer & Cloud Architect",
            "Designing and implementing cloud infrastructure, CI/CD pipelines, and monitoring solutions. Kubernetes and Terraform specialist.",
            hourlyRate: 120, yoe: 7, completed: 25,
            github: "https://github.com/omarhassan",
            education: "B.S. Computer Engineering, Khalifa University",
            certs: "AWS Solutions Architect Professional, CKA, CKS");

        Add("lucas_python", "Python Developer & AI Enthusiast",
            "Backend development with Django and FastAPI. Building AI-powered applications and automation tools.",
            hourlyRate: 70, yoe: 4, completed: 19,
            github: "https://github.com/lucassilva",
            education: "B.S. Computer Science, USP São Paulo");

        Add("emma_market", "Digital Marketing Specialist | Growth Hacker",
            "Data-driven marketing professional. Expertise in SEO, PPC, conversion optimization, and marketing automation.",
            hourlyRate: 85, yoe: 6, completed: 41,
            website: "https://emmathompson.marketing",
            education: "M.B.A. Marketing, London Business School",
            certs: "Google Analytics, Google Ads, HubSpot Inbound Marketing");

        Add("techstore_mike", "Tech Entrepreneur & Electronics Curator",
            hourlyRate: 0, yoe: 10, completed: 0, available: false,
            company: "TechGadgets Inc.", website: "https://techgadgets.store");

        Add("fashion_nina", "Fashion Designer & Sustainable Style Advocate",
            hourlyRate: 0, yoe: 8, completed: 0, available: false,
            company: "Nina's Boutique", website: "https://ninasboutique.fashion");

        Add("bookworm_raj", "Indie Bookstore Owner & Literary Curator",
            hourlyRate: 0, yoe: 12, completed: 0, available: false,
            company: "Raj's Book Haven", website: "https://rajsbookhaven.com");

        Add("homecraft_lisa", "Artisan & Handcrafted Home Decor Designer",
            hourlyRate: 0, yoe: 6, completed: 0, available: false,
            company: "HomeCraft Studio", website: "https://homecraftstudio.com");

        Add("softshop_dave", "Software Product Manager & Digital Tools Creator",
            hourlyRate: 0, yoe: 11, completed: 0, available: false,
            company: "SoftShop Digital", website: "https://softshop.io");

        Add("ceo_james", "CEO & Co-Founder, TechCorp Solutions",
            hourlyRate: 200, yoe: 15, completed: 12, available: false,
            company: "TechCorp Solutions", website: "https://techcorpsolutions.com",
            education: "M.B.A. Harvard Business School");

        Add("hr_anna", "Head of Human Resources, NordicTech",
            hourlyRate: 0, yoe: 9, completed: 0, available: false,
            company: "NordicTech AB", education: "M.Sc. HR Management, Stockholm University");

        Add("founder_wei", "AI Startup Founder & Researcher",
            hourlyRate: 150, yoe: 12, completed: 8, available: false,
            company: "InnovateAI", github: "https://github.com/weizhang",
            education: "Ph.D. Computer Science, Tsinghua University");

        Add("cto_sarah", "CTO, CloudNine | Infrastructure at Scale",
            hourlyRate: 180, yoe: 14, completed: 15, available: false,
            company: "CloudNine Ltd.", github: "https://github.com/sarahobrien",
            education: "M.Sc. Computer Science, Trinity College Dublin",
            certs: "AWS Solutions Architect Professional, Google Cloud Professional");

        Add("hr_miguel", "HR Director, LatamDev",
            hourlyRate: 0, yoe: 8, completed: 0, available: false,
            company: "LatamDev S.A.", education: "M.A. Organizational Psychology, UNAM");

        Add("buyer_tom", "Small Business Owner",
            hourlyRate: 0, yoe: 5, completed: 0, available: false,
            company: "Brown's Hardware");

        Add("buyer_sophie", "Startup Founder",
            hourlyRate: 0, yoe: 3, completed: 0,
            company: "Atelier Sophie", website: "https://ateliersophie.fr");

        Add("buyer_ahmed", "E-Commerce Entrepreneur",
            hourlyRate: 0, yoe: 7, completed: 0, available: false,
            company: "Al-Rashid Trading", website: "https://alrashidtrading.com");

        Add("newuser_jane", "Exploring the marketplace");
        Add("newuser_bob", "New member");

        return profiles;
    }

    // ═══════════════════════════════════════════════════
    //  STORES  (10)
    // ═══════════════════════════════════════════════════
    private static Dictionary<string, Store> CreateStores(
        Dictionary<string, User> users, DateTime now)
    {
        var stores = new Dictionary<string, Store>();

        Store Add(string key, string ownerKey, string name, string desc,
            string? city = null, string? country = "US", string? email = null,
            bool verified = true, bool featured = false)
        {
            var slug = name.ToLower().Replace(" ", "-").Replace("'", "");
            var s = new Store
            {
                Id = Guid.NewGuid(),
                OwnerId = users[ownerKey].Id,
                Name = name,
                Slug = slug,
                Description = desc,
                ShortDescription = desc.Length > 120 ? desc[..120] + "..." : desc,
                LogoUrl = $"https://picsum.photos/seed/{slug}-logo/200/200",
                BannerUrl = $"https://picsum.photos/seed/{slug}-banner/1200/400",
                Status = StoreStatus.Active,
                Email = email ?? $"contact@{slug}.com",
                City = city,
                Country = country,
                Rating = 4.5m,
                IsVerified = verified,
                VerifiedAt = verified ? now.AddDays(-60) : null,
                IsFeatured = featured,
                CommissionRate = 10m,
                ShippingPolicy = "Free shipping on orders over $50. Standard delivery 3-5 business days.",
                ReturnPolicy = "30-day return policy. Items must be in original condition.",
                CreatedAt = now.AddDays(-new Random(name.GetHashCode()).Next(30, 300)),
                IsDeleted = false,
            };
            stores[key] = s;
            return s;
        }

        Add("techgadgets", "techstore_mike", "TechGadgets",
            "Premium electronics and gadgets. Latest smartphones, laptops, and accessories at competitive prices.",
            city: "Seattle", featured: true);
        Add("ninaboutique", "fashion_nina", "Nina's Boutique",
            "Sustainable and stylish clothing for the modern wardrobe. Ethically sourced, beautifully designed.",
            city: "Warsaw", country: "PL", featured: true);
        Add("bookhaven", "bookworm_raj", "Raj's Book Haven",
            "Curated collection of bestsellers, rare finds, and indie publications. From fiction to technical books.",
            city: "Mumbai", country: "IN");
        Add("homecraft", "homecraft_lisa", "HomeCraft Studio",
            "Handcrafted home decor and artisanal furniture. Unique pieces that make your house a home.",
            city: "Portland", featured: true);
        Add("softshop", "softshop_dave", "SoftShop Digital",
            "Premium software tools and digital products for teams. Boost your productivity.",
            city: "San Jose");
        Add("sportzone", "techstore_mike", "SportZone Pro",
            "Professional sports equipment and outdoor gear. From running shoes to camping essentials.",
            city: "Seattle");
        Add("beautybox", "fashion_nina", "Beauty Box",
            "Natural skincare and premium beauty products. Cruelty-free and vegan options available.",
            city: "Warsaw", country: "PL");
        Add("codetools", "softshop_dave", "CodeTools",
            "Developer tools, IDE extensions, and coding resources. Ship faster with the right tools.",
            city: "San Jose");
        Add("greenmarket", "homecraft_lisa", "Green Market",
            "Organic and eco-friendly household products. Sustainable living made easy.",
            city: "Portland");
        Add("learnhub", "bookworm_raj", "LearnHub Store",
            "Online courses, tutorials, and educational materials. Learn any skill at your own pace.",
            city: "Mumbai", country: "IN");

        return stores;
    }

    // ═══════════════════════════════════════════════════
    //  PRODUCTS  (55) + Images
    // ═══════════════════════════════════════════════════
    private static (List<Product>, List<ProductImage>) CreateProducts(
        Dictionary<string, Store> stores, Dictionary<string, Category> cats, DateTime now)
    {
        var products = new List<Product>();
        var images = new List<ProductImage>();

        Product Add(string storeKey, string catKey, string name, string desc,
            decimal price, decimal? compareAt = null, int stock = 100,
            string? tags = null, bool featured = false, bool digital = false)
        {
            var slug = name.ToLower().Replace(" ", "-").Replace("\"", "")
                .Replace("'", "").Replace("&", "and");
            var p = new Product
            {
                Id = Guid.NewGuid(),
                StoreId = stores[storeKey].Id,
                CategoryId = cats[catKey].Id,
                Name = name,
                Slug = slug,
                Description = desc,
                ShortDescription = desc.Length > 150 ? desc[..150] + "..." : desc,
                Price = price,
                CompareAtPrice = compareAt,
                CostPrice = price * 0.6m,
                Currency = "USD",
                Status = ProductStatus.Active,
                StockQuantity = digital ? 9999 : stock,
                LowStockThreshold = 10,
                TrackInventory = !digital,
                Tags = tags,
                IsFeatured = featured,
                IsDigital = digital,
                Rating = 4.0m + (decimal)(new Random(name.GetHashCode()).NextDouble() * 1.0),
                TotalReviews = new Random(name.GetHashCode()).Next(5, 80),
                TotalSold = new Random(name.GetHashCode()).Next(10, 500),
                ViewCount = new Random(name.GetHashCode()).Next(100, 5000),
                PublishedAt = now.AddDays(-new Random(name.GetHashCode()).Next(1, 180)),
                CreatedAt = now.AddDays(-new Random(name.GetHashCode()).Next(1, 200)),
                IsDeleted = false,
            };
            products.Add(p);

            for (int i = 0; i < 3; i++)
            {
                images.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = p.Id,
                    Url = $"https://picsum.photos/seed/{slug}-{i}/400/400",
                    ThumbnailUrl = $"https://picsum.photos/seed/{slug}-{i}/150/150",
                    AltText = $"{name} - Image {i + 1}",
                    SortOrder = i,
                    IsPrimary = i == 0,
                    Width = 400,
                    Height = 400,
                    MimeType = "image/jpeg",
                    CreatedAt = now,
                    IsDeleted = false,
                });
            }
            return p;
        }

        // TechGadgets – Electronics (10)
        Add("techgadgets", "Electronics", "Pro Wireless Earbuds", "High-fidelity wireless earbuds with active noise cancellation, 30-hour battery life, and IPX5 water resistance.", 129.99m, 159.99m, 250, "audio,wireless,earbuds", featured: true);
        Add("techgadgets", "Electronics", "UltraSlim Laptop Stand", "Ergonomic aluminum laptop stand with adjustable height. Compatible with all laptops 11-17 inches.", 49.99m, tags: "laptop,ergonomic,stand");
        Add("techgadgets", "Electronics", "4K Webcam Pro", "Ultra HD 4K webcam with auto-focus, built-in ring light, and noise-cancelling microphone.", 89.99m, 119.99m, 180, "webcam,streaming,video");
        Add("techgadgets", "Electronics", "Mechanical Gaming Keyboard", "RGB mechanical keyboard with Cherry MX switches, programmable keys, and aluminum frame.", 149.99m, stock: 120, tags: "keyboard,gaming,mechanical", featured: true);
        Add("techgadgets", "Electronics", "Smart Home Hub", "Central hub for your smart home. Works with Alexa, Google Home, and HomeKit.", 79.99m, 99.99m, 200, "smart-home,iot,automation");
        Add("techgadgets", "Electronics", "Portable SSD 1TB", "Blazing-fast portable SSD with USB-C 3.2 Gen 2. Read speeds up to 2000MB/s.", 109.99m, stock: 300, tags: "storage,ssd,portable");
        Add("techgadgets", "Electronics", "Wireless Charging Pad", "15W fast wireless charging pad. Compatible with all Qi-enabled devices.", 29.99m, 39.99m, 500, "charging,wireless,qi");
        Add("techgadgets", "Electronics", "Noise Cancelling Headphones", "Premium over-ear headphones with adaptive noise cancellation and 40-hour battery.", 249.99m, 299.99m, 150, "headphones,audio,anc", featured: true);
        Add("techgadgets", "Electronics", "USB-C Hub 8-in-1", "All-in-one USB-C hub with HDMI 4K, ethernet, SD card reader, and 100W PD charging.", 59.99m, stock: 350, tags: "usb-c,hub,adapter");
        Add("techgadgets", "Electronics", "Smart Fitness Tracker", "Advanced fitness tracker with heart rate, SpO2, sleep tracking, and GPS.", 69.99m, 89.99m, 400, "fitness,wearable,health");

        // Nina's Boutique – Fashion (8)
        Add("ninaboutique", "Fashion", "Organic Cotton Tee", "Super soft organic cotton t-shirt. Available in 12 colors. Ethically manufactured.", 34.99m, tags: "cotton,organic,sustainable", featured: true);
        Add("ninaboutique", "Fashion", "Slim Fit Chinos", "Classic slim-fit chinos made from stretch organic cotton. Perfect for work or weekend.", 59.99m, 79.99m, 180, "pants,chinos,slim-fit");
        Add("ninaboutique", "Fashion", "Recycled Wool Sweater", "Cozy pullover made from 100% recycled wool. Timeless design for cold-weather layering.", 89.99m, stock: 100, tags: "sweater,wool,recycled");
        Add("ninaboutique", "Fashion", "Linen Summer Dress", "Breezy linen dress perfect for summer. Relaxed fit with side pockets.", 74.99m, 94.99m, 120, "dress,linen,summer");
        Add("ninaboutique", "Fashion", "Vegan Leather Backpack", "Stylish vegan leather backpack with laptop compartment. Water-resistant.", 119.99m, stock: 90, tags: "backpack,vegan-leather,bag", featured: true);
        Add("ninaboutique", "Fashion", "Merino Wool Scarf", "Ultra-soft merino wool scarf. Lightweight and breathable for year-round wear.", 44.99m, tags: "scarf,wool,accessory");
        Add("ninaboutique", "Fashion", "Bamboo Fiber Socks Pack", "6-pack of bamboo fiber socks. Anti-bacterial, moisture-wicking, and incredibly comfortable.", 24.99m, 34.99m, 600, "socks,bamboo,pack");
        Add("ninaboutique", "Fashion", "Minimalist Watch", "Clean and modern watch with sapphire crystal and Italian leather strap.", 159.99m, 199.99m, 75, "watch,minimalist,accessory");

        // Book Haven – Books (7)
        Add("bookhaven", "Books", "Clean Architecture", "Robert C. Martin's guide to software structure and design. A must-read for developers.", 39.99m, tags: "programming,architecture,software");
        Add("bookhaven", "Books", "Design Patterns in C#", "Comprehensive guide to implementing Gang of Four design patterns in modern C#.", 44.99m, stock: 80, tags: "csharp,design-patterns,programming");
        Add("bookhaven", "Books", "The Lean Startup", "Eric Ries' essential guide to building successful startups through continuous innovation.", 29.99m, tags: "business,startup,entrepreneurship", featured: true);
        Add("bookhaven", "Books", "Atomic Habits", "James Clear's practical guide to building good habits and breaking bad ones.", 24.99m, stock: 200, tags: "self-help,habits,productivity");
        Add("bookhaven", "Books", "System Design Interview", "In-depth guide to system design interviews. Covers distributed systems and scalability.", 49.99m, tags: "interview,system-design,tech");
        Add("bookhaven", "Books", "The Art of War", "Sun Tzu's ancient masterpiece on strategy, newly translated with modern commentary.", 14.99m, 19.99m, 150, "classic,strategy,philosophy");
        Add("bookhaven", "Books", "Data Science from Scratch", "Learn data science fundamentals using Python. Perfect for beginners and self-learners.", 34.99m, tags: "data-science,python,beginner");

        // HomeCraft Studio – Home & Garden (8)
        Add("homecraft", "Home & Garden", "Handmade Ceramic Vase", "Beautiful hand-thrown ceramic vase. Each piece is unique with natural color variations.", 64.99m, tags: "ceramic,vase,handmade", featured: true);
        Add("homecraft", "Home & Garden", "Macrame Wall Hanging", "Intricate macrame wall art made from 100% cotton rope. Adds bohemian charm.", 49.99m, stock: 60, tags: "macrame,wall-art,boho");
        Add("homecraft", "Home & Garden", "Reclaimed Wood Shelf", "Floating shelf made from reclaimed barn wood. Rustic charm with modern brackets.", 79.99m, stock: 40, tags: "shelf,reclaimed-wood,rustic");
        Add("homecraft", "Home & Garden", "Soy Candle Set", "Set of 3 hand-poured soy candles in lavender, vanilla, and cedar scents.", 34.99m, 44.99m, 200, "candles,soy,aromatherapy");
        Add("homecraft", "Home & Garden", "Woven Storage Basket", "Natural seagrass storage basket. Perfect for organizing blankets, toys, or laundry.", 39.99m, stock: 150, tags: "basket,storage,seagrass");
        Add("homecraft", "Home & Garden", "Indoor Herb Garden Kit", "Everything you need to grow fresh herbs indoors. Includes seeds, pots, and soil.", 29.99m, stock: 250, tags: "herbs,garden,indoor", featured: true);
        Add("homecraft", "Home & Garden", "Cotton Throw Blanket", "Chunky knit throw blanket in soft organic cotton. Cozy comfort for your couch.", 54.99m, 69.99m, 130, "blanket,throw,cotton");
        Add("homecraft", "Home & Garden", "Minimalist Picture Frame Set", "Set of 5 modern picture frames in mixed sizes. Black matte finish.", 44.99m, stock: 180, tags: "frames,picture,minimalist");

        // SoftShop – Software (7)
        Add("softshop", "Software", "ProjectFlow Pro", "All-in-one project management tool for agile teams. Kanban, sprints, and time tracking.", 19.99m, tags: "project-management,agile,saas", featured: true, digital: true);
        Add("softshop", "Software", "CodeSnippet Manager", "Organize, search, and share your code snippets. Supports 100+ programming languages.", 9.99m, tags: "developer-tools,snippets,productivity", digital: true);
        Add("softshop", "Software", "DesignKit UI Library", "Premium Figma and Sketch UI component library. 500+ responsive components.", 49.99m, 79.99m, tags: "design,ui-kit,figma", digital: true);
        Add("softshop", "Software", "API Monitor Dashboard", "Real-time API monitoring and alerting tool. Track uptime, latency, and errors.", 29.99m, tags: "monitoring,api,devops", digital: true);
        Add("softshop", "Software", "Database Backup Tool", "Automated database backup solution for PostgreSQL, MySQL, and MongoDB.", 39.99m, tags: "database,backup,automation", digital: true);
        Add("softshop", "Software", "Email Template Builder", "Drag-and-drop email template builder with 200+ pre-designed templates.", 14.99m, 24.99m, tags: "email,templates,marketing", digital: true);
        Add("softshop", "Software", "Invoice Generator Pro", "Professional invoice creation and management. Stripe and PayPal integration included.", 24.99m, tags: "invoice,billing,finance", digital: true);

        // SportZone – Sports & Outdoors (5)
        Add("sportzone", "Sports & Outdoors", "Trail Running Shoes", "Lightweight trail running shoes with grip-optimized outsole and waterproof membrane.", 129.99m, 159.99m, 100, "running,shoes,trail");
        Add("sportzone", "Sports & Outdoors", "Yoga Mat Premium", "Extra thick 6mm yoga mat with alignment lines. Non-slip surface, eco-friendly TPE.", 39.99m, stock: 200, tags: "yoga,mat,fitness");
        Add("sportzone", "Sports & Outdoors", "Camping Hammock", "Ultralight camping hammock with mosquito net. Packs down to the size of a water bottle.", 49.99m, 64.99m, 150, "camping,hammock,outdoor", featured: true);
        Add("sportzone", "Sports & Outdoors", "Resistance Band Set", "Set of 5 resistance bands with different tensions. Perfect for home workouts.", 24.99m, stock: 300, tags: "fitness,bands,workout");
        Add("sportzone", "Sports & Outdoors", "Insulated Water Bottle", "32oz double-wall vacuum insulated bottle. Keeps drinks cold 24hrs or hot 12hrs.", 34.99m, stock: 400, tags: "bottle,insulated,hydration");

        // Beauty Box (3)
        Add("beautybox", "Health & Beauty", "Natural Face Serum", "Vitamin C and hyaluronic acid serum. Brightens skin and reduces fine lines.", 39.99m, 54.99m, 180, "skincare,serum,natural", featured: true);
        Add("beautybox", "Health & Beauty", "Organic Lip Balm Set", "Set of 4 organic lip balms in fruit flavors. Beeswax-free and vegan.", 12.99m, stock: 500, tags: "lips,organic,vegan");
        Add("beautybox", "Health & Beauty", "Bamboo Charcoal Soap", "Detoxifying bamboo charcoal soap bar. Gentle cleansing for all skin types.", 8.99m, stock: 300, tags: "soap,charcoal,natural");

        return (products, images);
    }

    // ═══════════════════════════════════════════════════
    //  SERVICES  (32) + Packages
    // ═══════════════════════════════════════════════════
    private static (List<Service>, List<ServicePackage>) CreateServices(
        Dictionary<string, User> users, Dictionary<string, Store> stores,
        Dictionary<string, Category> cats, DateTime now)
    {
        var services = new List<Service>();
        var packages = new List<ServicePackage>();

        Service Add(string sellerKey, string catKey, string title, string desc,
            decimal basePrice, int deliveryDays, string? tags = null,
            bool featured = false, string? storeKey = null,
            (string name, decimal price, int days, string features)[]? pkgs = null)
        {
            var slug = title.ToLower().Replace(" ", "-").Replace("&", "and")
                .Replace("/", "-");
            var svc = new Service
            {
                Id = Guid.NewGuid(),
                SellerId = users[sellerKey].Id,
                StoreId = storeKey != null ? stores[storeKey].Id : null,
                CategoryId = cats[catKey].Id,
                Title = title,
                Slug = slug,
                Description = desc,
                ShortDescription = desc.Length > 150 ? desc[..150] + "..." : desc,
                BasePrice = basePrice,
                Currency = "USD",
                PricingType = "Fixed",
                Status = ProductStatus.Active,
                DeliveryTime = deliveryDays,
                DeliveryTimeUnit = "days",
                Revisions = 3,
                Tags = tags,
                Rating = 4.0m + (decimal)(new Random(title.GetHashCode()).NextDouble() * 1.0),
                TotalReviews = new Random(title.GetHashCode()).Next(5, 60),
                TotalOrders = new Random(title.GetHashCode()).Next(10, 200),
                ViewCount = new Random(title.GetHashCode()).Next(100, 3000),
                IsFeatured = featured,
                PublishedAt = now.AddDays(-new Random(title.GetHashCode()).Next(1, 150)),
                CreatedAt = now.AddDays(-new Random(title.GetHashCode()).Next(1, 180)),
                IsDeleted = false,
            };
            services.Add(svc);

            var pkgDefs = pkgs ?? new[]
            {
                ("Basic", basePrice, deliveryDays, "[\"1 revision\",\"Source file\"]"),
                ("Standard", basePrice * 2m, (int)(deliveryDays * 0.8), "[\"3 revisions\",\"Source file\",\"Commercial use\"]"),
                ("Premium", basePrice * 3.5m, (int)(deliveryDays * 0.6), "[\"Unlimited revisions\",\"Source file\",\"Commercial use\",\"Priority support\"]"),
            };

            int pkgSort = 0;
            foreach (var (pName, pPrice, pDays, pFeatures) in pkgDefs)
            {
                packages.Add(new ServicePackage
                {
                    Id = Guid.NewGuid(),
                    ServiceId = svc.Id,
                    Name = pName,
                    Description = $"{pName} package for {title}",
                    Price = pPrice,
                    DeliveryTime = Math.Max(pDays, 1),
                    Revisions = pName == "Premium" ? 99 : pName == "Standard" ? 3 : 1,
                    Features = pFeatures,
                    SortOrder = pkgSort,
                    IsPopular = pName == "Standard",
                    CreatedAt = now,
                    IsDeleted = false,
                });
                pkgSort++;
            }
            return svc;
        }

        // Web Development (6)
        Add("john_dev", "Web Development", "Full-Stack Web Application Development",
            "I will build a complete web application using .NET, React, and PostgreSQL. Includes API design, responsive UI, authentication, and deployment.",
            500, 14, "dotnet,react,fullstack,webapp", featured: true);
        Add("john_dev", "Web Development", "REST API Development with .NET",
            "Professional REST API development using ASP.NET Core. Clean architecture, documentation, unit tests, and deployment included.",
            300, 7, "api,dotnet,rest,backend");
        Add("lucas_python", "Web Development", "Python Web Application with Django",
            "Custom web application built with Django or FastAPI. Database design, REST API, admin panel, and deployment.",
            400, 10, "python,django,fastapi,webapp");
        Add("lucas_python", "Web Development", "Web Scraping & Data Pipeline",
            "Custom web scraping solution with data cleaning and storage. Supports scheduled runs and exports to CSV/JSON/database.",
            200, 5, "python,scraping,data,automation");
        Add("john_dev", "Web Development", "E-Commerce Website Development",
            "Complete e-commerce solution with product management, cart, checkout, payment integration, and admin dashboard.",
            800, 21, "ecommerce,shop,fullstack", featured: true);
        Add("lucas_python", "Web Development", "WordPress Custom Development",
            "Custom WordPress theme or plugin development. Responsive design, SEO optimized, and performance tuned.",
            250, 7, "wordpress,php,cms");

        // Mobile Development (4)
        Add("yuki_mobile", "Mobile Development", "Cross-Platform Mobile App with Flutter",
            "Beautiful and performant cross-platform mobile app built with Flutter. Single codebase for iOS and Android.",
            600, 21, "flutter,mobile,ios,android", featured: true);
        Add("yuki_mobile", "Mobile Development", "React Native App Development",
            "Native-quality mobile app using React Native. Includes push notifications, analytics, and app store submission.",
            550, 18, "react-native,mobile,javascript");
        Add("yuki_mobile", "Mobile Development", "Mobile App UI/UX Redesign",
            "Complete UI/UX redesign of your existing mobile app. Improve user engagement and retention with modern design patterns.",
            350, 10, "mobile,ui,ux,redesign");
        Add("yuki_mobile", "Mobile Development", "App Store Optimization",
            "Optimize your app listing for better visibility and downloads. Keyword research, screenshots, and description optimization.",
            150, 5, "aso,mobile,marketing");

        // Design & Creative (5)
        Add("priya_designer", "Design & Creative", "UI/UX Design for Web Applications",
            "Complete UI/UX design for your web application. User research, wireframes, high-fidelity mockups, and interactive prototypes in Figma.",
            400, 10, "ui,ux,figma,web-design", featured: true);
        Add("priya_designer", "Design & Creative", "Brand Identity Design",
            "Complete brand identity package including logo, color palette, typography, brand guidelines, and social media templates.",
            350, 7, "branding,logo,identity");
        Add("priya_designer", "Design & Creative", "Mobile App UI Design",
            "Pixel-perfect mobile app UI design in Figma. Includes design system, components, and developer handoff.",
            500, 12, "mobile,ui,figma,app-design");
        Add("priya_designer", "Design & Creative", "Design System Creation",
            "Comprehensive design system with reusable components, tokens, and documentation. Ensures consistency across your product.",
            600, 14, "design-system,figma,components");
        Add("priya_designer", "Design & Creative", "Landing Page Design",
            "High-converting landing page design. A/B test-ready with mobile-responsive layouts.",
            200, 5, "landing-page,design,conversion");

        // Digital Marketing (5)
        Add("emma_market", "Digital Marketing", "SEO Audit & Strategy",
            "Comprehensive SEO audit of your website. Technical SEO, on-page optimization, keyword research, and content strategy roadmap.",
            300, 7, "seo,audit,strategy", featured: true);
        Add("emma_market", "Digital Marketing", "Google Ads Campaign Management",
            "Full Google Ads campaign setup and management. Keyword research, ad copy, landing pages, and conversion tracking.",
            400, 5, "google-ads,ppc,sem");
        Add("emma_market", "Digital Marketing", "Social Media Marketing Strategy",
            "Custom social media strategy for your brand. Content calendar, platform optimization, and engagement tactics.",
            250, 7, "social-media,marketing,strategy");
        Add("emma_market", "Digital Marketing", "Email Marketing Setup",
            "Complete email marketing setup with automation flows. Welcome series, nurture sequences, and newsletter templates.",
            200, 5, "email,marketing,automation");
        Add("emma_market", "Digital Marketing", "Content Marketing Plan",
            "Strategic content marketing plan with topic clusters, editorial calendar, and distribution strategy.",
            350, 10, "content,marketing,strategy");

        // Writing & Content (3)
        Add("maria_writer", "Writing & Content", "SEO Blog Writing",
            "Well-researched, SEO-optimized blog posts that drive organic traffic. Includes keyword research, meta descriptions, and internal linking.",
            100, 3, "blog,seo,writing", featured: true);
        Add("maria_writer", "Writing & Content", "Website Copywriting",
            "Persuasive website copy that converts visitors into customers. Homepage, about page, services, and CTAs.",
            250, 5, "copywriting,web-copy,conversion");
        Add("maria_writer", "Writing & Content", "Technical Writing",
            "Clear and comprehensive technical documentation. API docs, user guides, and knowledge base articles.",
            200, 7, "technical-writing,documentation,api-docs");

        // Data & Analytics (3)
        Add("alex_data", "Data & Analytics", "Machine Learning Model Development",
            "Custom ML model development for your business needs. From data preprocessing to model deployment and monitoring.",
            800, 14, "machine-learning,ml,ai,model", featured: true);
        Add("alex_data", "Data & Analytics", "Data Visualization Dashboard",
            "Interactive data visualization dashboard using Power BI or custom solution. Turn your data into actionable insights.",
            400, 7, "data-viz,dashboard,power-bi");
        Add("alex_data", "Data & Analytics", "Data Pipeline Development",
            "End-to-end data pipeline design and implementation. ETL/ELT, data warehousing, and real-time streaming.",
            600, 10, "data-pipeline,etl,data-engineering");

        // Consulting (2)
        Add("ceo_james", "Consulting", "Tech Startup Strategy Consulting",
            "Strategic consulting for tech startups. Product-market fit, go-to-market strategy, and fundraising preparation.",
            200, 3, "consulting,startup,strategy");
        Add("cto_sarah", "Consulting", "Cloud Architecture Review",
            "Comprehensive review of your cloud architecture. Scalability, security, cost optimization, and best practices.",
            300, 5, "cloud,architecture,review,aws");

        // DevOps (2)
        Add("omar_devops", "Web Development", "Kubernetes Cluster Setup & Management",
            "Production-ready Kubernetes cluster setup on AWS, Azure, or GCP. Includes monitoring, logging, and CI/CD pipeline.",
            500, 7, "kubernetes,devops,cloud", featured: true);
        Add("omar_devops", "Web Development", "CI/CD Pipeline Setup",
            "Automated CI/CD pipeline using GitHub Actions or GitLab CI. Build, test, and deploy with confidence.",
            250, 5, "cicd,devops,automation");

        return (services, packages);
    }

    // ═══════════════════════════════════════════════════
    //  PROJECTS  (20)
    // ═══════════════════════════════════════════════════
    private static List<Project> CreateProjects(
        Dictionary<string, User> users, Dictionary<string, Category> cats,
        Dictionary<string, Skill> skills, DateTime now)
    {
        var projects = new List<Project>();
        int idx = 0;

        Project Add(string clientKey, string catKey, string title, string desc,
            decimal budgetMin, decimal budgetMax, int durationDays,
            string budgetType = "Fixed", string experience = "Intermediate",
            ProjectStatus status = ProjectStatus.Open, bool urgent = false,
            params string[] requiredSkills)
        {
            var slug = title.ToLower().Replace(" ", "-").Replace("&", "and")
                .Replace("/", "-");
            var skillIds = requiredSkills
                .Where(s => skills.ContainsKey(s))
                .Select(s => $"\"{skills[s].Id}\"");
            var p = new Project
            {
                Id = Guid.NewGuid(),
                ClientId = users[clientKey].Id,
                CategoryId = cats[catKey].Id,
                Title = title,
                Slug = slug,
                Description = desc,
                Status = status,
                BudgetType = budgetType,
                BudgetMin = budgetMin,
                BudgetMax = budgetMax,
                Currency = "USD",
                EstimatedDurationDays = durationDays,
                Deadline = now.AddDays(durationDays + 14),
                RequiredSkills = $"[{string.Join(",", skillIds)}]",
                ExperienceLevel = experience,
                ProjectType = durationDays > 90 ? "Ongoing" : "One-time",
                Visibility = "Public",
                BidCount = new Random(idx).Next(3, 25),
                ViewCount = new Random(idx).Next(50, 500),
                IsUrgent = urgent,
                IsFeatured = idx < 5,
                Tags = string.Join(",", requiredSkills.Take(3).Select(s => s.ToLower())),
                CreatedAt = now.AddDays(-new Random(idx).Next(1, 30)),
                IsDeleted = false,
            };
            projects.Add(p);
            idx++;
            return p;
        }

        // Web Projects
        Add("buyer_sophie", "Web Projects", "E-Commerce Platform Redesign",
            "Looking for an experienced full-stack developer to redesign our e-commerce platform. Must support multi-vendor marketplace, payment integration (Stripe), and real-time inventory management.",
            5000, 15000, 60, experience: "Expert", requiredSkills: new[] { "React", "Node.js", ".NET", "SQL" });
        Add("buyer_tom", "Web Projects", "Company Website with CMS",
            "Need a modern company website with a content management system. Should include blog, contact forms, and SEO optimization.",
            2000, 5000, 30, requiredSkills: new[] { "React", "Next.js", "TypeScript" });
        Add("buyer_ahmed", "Web Projects", "Real-Time Dashboard Application",
            "Build a real-time analytics dashboard with WebSocket support. Should display charts, metrics, and alerts with live data updates.",
            3000, 8000, 45, experience: "Expert", requiredSkills: new[] { "React", "TypeScript", ".NET", "SQL" });
        Add("ceo_james", "Web Projects", "Internal Tool for Employee Management",
            "Enterprise-grade internal tool for managing employees, PTO, performance reviews, and payroll integration. Must support RBAC.",
            10000, 25000, 90, experience: "Expert", urgent: true, requiredSkills: new[] { "C#", ".NET", "React", "SQL" });

        // Mobile Projects
        Add("buyer_sophie", "Mobile Projects", "Food Delivery Mobile App",
            "Cross-platform food delivery app with real-time order tracking, push notifications, and payment integration. Both customer and driver apps needed.",
            8000, 20000, 90, experience: "Expert", requiredSkills: new[] { "Flutter", "Node.js", "SQL" });
        Add("buyer_ahmed", "Mobile Projects", "Social Fitness App",
            "Build a social fitness app where users can share workouts, challenge friends, and track progress. Integration with Apple Health and Google Fit.",
            4000, 10000, 60, requiredSkills: new[] { "React Native", "Node.js", "TypeScript" });
        Add("ceo_james", "Mobile Projects", "Enterprise Mobile App",
            "Secure enterprise mobile app for field workers. Offline support, GPS tracking, photo capture, and data sync.",
            12000, 30000, 120, experience: "Expert", requiredSkills: new[] { "Flutter", "Kotlin", "Swift" });

        // Design Projects
        Add("buyer_sophie", "Design Projects", "SaaS Product Redesign",
            "Complete UI/UX redesign of our SaaS product. Need user research, wireframes, high-fidelity mockups, and a design system in Figma.",
            3000, 8000, 30, requiredSkills: new[] { "UI/UX Design", "Figma" });
        Add("buyer_tom", "Design Projects", "Brand Identity for Tech Startup",
            "Create a complete brand identity for our new tech startup. Logo, colors, typography, brand guidelines, and social media templates.",
            1500, 4000, 21, requiredSkills: new[] { "Graphic Design", "Adobe Illustrator", "Figma" });
        Add("cto_sarah", "Design Projects", "Developer Portal Design",
            "Design a clean, developer-friendly documentation portal. Navigation, code examples, interactive API playground.",
            2000, 5000, 21, requiredSkills: new[] { "UI/UX Design", "Figma" });

        // Marketing Projects
        Add("buyer_tom", "Marketing Projects", "SEO Strategy for E-Commerce",
            "Need a comprehensive SEO strategy for our e-commerce store. Technical audit, keyword research, content plan, and link-building strategy.",
            1000, 3000, 30, requiredSkills: new[] { "SEO", "Content Marketing", "Analytics" });
        Add("buyer_ahmed", "Marketing Projects", "Social Media Campaign Launch",
            "Plan and execute a social media campaign across Instagram, TikTok, and LinkedIn. Content creation, scheduling, and analytics reporting.",
            2000, 5000, 45, requiredSkills: new[] { "Social Media Marketing", "Content Marketing", "Copywriting" });
        Add("ceo_james", "Marketing Projects", "Lead Generation Funnel",
            "Build a complete B2B lead generation funnel. Landing pages, email sequences, LinkedIn outreach, and CRM integration.",
            3000, 8000, 60, experience: "Expert", requiredSkills: new[] { "Email Marketing", "Google Ads", "SEO" });

        // AI & ML Projects
        Add("founder_wei", "AI & ML Projects", "Sentiment Analysis API",
            "Build a sentiment analysis API for customer reviews. Should handle multiple languages and return confidence scores. Deploy as a scalable microservice.",
            4000, 10000, 45, experience: "Expert", requiredSkills: new[] { "Python", "Machine Learning", "TensorFlow" });
        Add("cto_sarah", "AI & ML Projects", "Recommendation Engine",
            "Build a product recommendation engine using collaborative filtering and content-based methods. Must handle cold-start problem.",
            6000, 15000, 60, experience: "Expert", requiredSkills: new[] { "Python", "Machine Learning", "SQL" });
        Add("buyer_sophie", "AI & ML Projects", "Chatbot for Customer Support",
            "AI-powered chatbot for our e-commerce customer support. Should handle FAQs, order tracking, and escalation to human agents.",
            3000, 8000, 45, requiredSkills: new[] { "Python", "Machine Learning" });

        // Additional varied projects
        Add("cto_sarah", "Web Projects", "Microservices Migration",
            "Migrate our monolithic application to microservices architecture. Event-driven design with Docker and Kubernetes.",
            15000, 40000, 120, budgetType: "Range", experience: "Expert", urgent: true,
            requiredSkills: new[] { "Docker", "Kubernetes", ".NET", "SQL" });
        Add("buyer_tom", "Web Projects", "Landing Page with A/B Testing",
            "High-converting landing page with A/B testing capability. Should integrate with our existing analytics and CRM.",
            500, 2000, 14, requiredSkills: new[] { "React", "Next.js", "TypeScript" });
        Add("hr_anna", "Web Projects", "Applicant Tracking System",
            "Build a custom ATS for our hiring process. Job postings, application forms, pipeline management, and interview scheduling.",
            5000, 12000, 60, experience: "Expert", requiredSkills: new[] { "React", ".NET", "SQL" });
        Add("hr_miguel", "Design Projects", "Career Portal Design",
            "Design a career portal for our company. Job listings, company culture page, and application flow design.",
            1500, 4000, 21, requiredSkills: new[] { "UI/UX Design", "Figma" });

        return projects;
    }

    // ═══════════════════════════════════════════════════
    //  COMPANIES  (10) + Employees
    // ═══════════════════════════════════════════════════
    private static (List<Company>, List<CompanyEmployee>) CreateCompanies(
        Dictionary<string, User> users, Dictionary<string, Role> roles, DateTime now)
    {
        var companies = new List<Company>();
        var employees = new List<CompanyEmployee>();

        Company Add(string ownerKey, string name, string industry, string desc,
            string city, string country, int founded, string size,
            string? website = null, params (string userKey, string title, string dept)[] emps)
        {
            var slug = name.ToLower().Replace(" ", "-").Replace("'", "");
            var c = new Company
            {
                Id = Guid.NewGuid(),
                OwnerId = users[ownerKey].Id,
                Name = name,
                Slug = slug,
                LegalName = $"{name} Inc.",
                Description = desc,
                LogoUrl = $"https://picsum.photos/seed/{slug}-logo/200/200",
                BannerUrl = $"https://picsum.photos/seed/{slug}-banner/1200/400",
                Website = website ?? $"https://{slug}.com",
                Email = $"contact@{slug}.com",
                City = city,
                Country = country,
                Industry = industry,
                FoundedYear = founded,
                CompanySize = size,
                CompanyType = "Private",
                Status = UserStatus.Active,
                IsVerified = true,
                VerifiedAt = now.AddDays(-90),
                Rating = 4.0m + (decimal)(new Random(name.GetHashCode()).NextDouble() * 1.0),
                TotalEmployees = emps.Length + 1,
                CreatedAt = now.AddDays(-new Random(name.GetHashCode()).Next(60, 365)),
                IsDeleted = false,
            };
            companies.Add(c);

            foreach (var (userKey, title, dept) in emps)
            {
                employees.Add(new CompanyEmployee
                {
                    Id = Guid.NewGuid(),
                    CompanyId = c.Id,
                    UserId = users[userKey].Id,
                    RoleId = roles["User"].Id,
                    Title = title,
                    Department = dept,
                    JoinedAt = now.AddDays(-new Random(userKey.GetHashCode()).Next(30, 300)),
                    IsActive = true,
                    CanManageStores = dept == "Engineering" || dept == "Management",
                    CanManageEmployees = dept == "HR" || dept == "Management",
                    CanManageProjects = true,
                    CanManageFinances = dept == "Finance" || dept == "Management",
                    CreatedAt = now,
                    IsDeleted = false,
                });
            }

            return c;
        }

        Add("ceo_james", "TechCorp Solutions", "Technology",
            "Enterprise software solutions for Fortune 500 companies. Specializing in cloud infrastructure and data analytics.",
            "Chicago", "US", 2015, "51-200", "https://techcorpsolutions.com",
            ("hr_anna", "VP of Engineering", "Engineering"),
            ("john_dev", "Senior Developer", "Engineering"),
            ("omar_devops", "DevOps Lead", "Engineering"));

        Add("cto_sarah", "CloudNine", "Cloud Computing",
            "Cloud infrastructure and DevOps consulting. Helping companies scale with confidence.",
            "Dublin", "IE", 2018, "11-50", "https://cloudnine.io",
            ("omar_devops", "Cloud Architect", "Engineering"));

        Add("founder_wei", "InnovateAI", "Artificial Intelligence",
            "Democratizing AI for businesses of all sizes. Custom ML solutions and AI consulting.",
            "Shanghai", "CN", 2020, "11-50", "https://innovateai.com",
            ("alex_data", "Lead Data Scientist", "AI Research"),
            ("lucas_python", "ML Engineer", "Engineering"));

        Add("hr_anna", "NordicTech", "Technology",
            "Scandinavian tech company specializing in fintech and sustainability solutions.",
            "Stockholm", "SE", 2016, "51-200", "https://nordictech.se",
            ("priya_designer", "Senior UI/UX Designer", "Design"));

        Add("hr_miguel", "LatamDev", "Software Development",
            "Connecting Latin American development talent with global opportunities. Nearshore development done right.",
            "Mexico City", "MX", 2019, "11-50", "https://latamdev.com",
            ("lucas_python", "Python Developer", "Engineering"),
            ("yuki_mobile", "Mobile Lead", "Engineering"));

        Add("ceo_james", "DataDriven Co.", "Data Analytics",
            "Business intelligence and data analytics solutions. Turn your data into competitive advantage.",
            "Chicago", "US", 2017, "11-50", "https://datadriven.co",
            ("alex_data", "Principal Data Scientist", "Analytics"));

        Add("buyer_sophie", "Atelier Sophie", "E-Commerce",
            "Curated marketplace for independent artisans and craftspeople. Unique handmade goods.",
            "Paris", "FR", 2021, "1-10", "https://ateliersophie.fr",
            ("priya_designer", "Design Consultant", "Design"));

        Add("buyer_ahmed", "Al-Rashid Trading", "E-Commerce",
            "Cross-border e-commerce company connecting Middle Eastern markets with global products.",
            "Riyadh", "SA", 2018, "11-50", "https://alrashidtrading.com",
            ("emma_market", "Marketing Consultant", "Marketing"));

        Add("techstore_mike", "GadgetLabs", "Consumer Electronics",
            "Consumer electronics brand focused on innovative and affordable tech accessories.",
            "Seattle", "US", 2019, "1-10", "https://gadgetlabs.com");

        Add("fashion_nina", "EcoStyle Group", "Fashion",
            "Sustainable fashion group committed to ethical manufacturing and circular fashion.",
            "Warsaw", "PL", 2020, "1-10", "https://ecostylegroup.com");

        return (companies, employees);
    }

    // ═══════════════════════════════════════════════════
    //  REVIEWS  (120)
    // ═══════════════════════════════════════════════════
    private static List<Review> CreateReviews(
        Dictionary<string, User> users, List<Product> products,
        List<Service> services, Dictionary<string, Store> stores, DateTime now)
    {
        var reviews = new List<Review>();
        var reviewers = new[] { "buyer_tom", "buyer_sophie", "buyer_ahmed", "newuser_jane", "newuser_bob",
            "john_dev", "priya_designer", "alex_data", "maria_writer", "yuki_mobile",
            "emma_market", "omar_devops", "lucas_python", "ceo_james", "cto_sarah" };

        var productTitles = new[]
        {
            ("Excellent product! Works exactly as described.", "Great quality, fast shipping", ""),
            ("Very good quality. Arrived on time.", "Solid build quality", "Wish it came in more colors"),
            ("Amazing value for the price. Highly recommend!", "Outstanding performance", ""),
            ("Good product but packaging could be better.", "Works well", "Packaging was damaged"),
            ("Five stars! This exceeded my expectations.", "Perfect in every way", ""),
            ("Decent product. Gets the job done.", "Functional", "Nothing exceptional"),
            ("Love it! Already ordered another one.", "Addictive quality", ""),
            ("Great for the price point. Would buy again.", "Good value", "Minor cosmetic issue"),
            ("Impressive quality. My go-to recommendation now.", "Top-notch", ""),
            ("Solid purchase. Happy with the quality.", "Reliable", "Took a while to arrive"),
        };

        var serviceTitles = new[]
        {
            ("Outstanding work! Delivered ahead of schedule.", "Professional, talented, communicative", ""),
            ("Highly skilled professional. Will hire again.", "Expert-level work", ""),
            ("Great communication throughout the project.", "Always responsive", "Slightly over deadline"),
            ("Exceeded expectations in every way.", "Incredible attention to detail", ""),
            ("Very professional service. On time and on budget.", "Reliable and skilled", ""),
            ("The quality of work was exceptional.", "Clearly an expert", ""),
            ("Good work overall. A few revisions needed.", "Competent", "Needed some back and forth"),
            ("Fantastic results! Exactly what I needed.", "Perfect execution", ""),
        };

        int reviewIdx = 0;

        // Product reviews (70)
        foreach (var product in products.Take(35))
        {
            int reviewCount = 2;
            for (int i = 0; i < reviewCount; i++)
            {
                var reviewer = reviewers[(reviewIdx + i) % reviewers.Length];
                var template = productTitles[(reviewIdx + i) % productTitles.Length];
                var rating = 3 + (reviewIdx + i) % 3;

                reviews.Add(new Review
                {
                    Id = Guid.NewGuid(),
                    ReviewerId = users[reviewer].Id,
                    ProductId = product.Id,
                    StoreId = product.StoreId,
                    ReviewType = "Product",
                    Rating = rating,
                    Title = template.Item1.Split('!')[0].Split('.')[0],
                    Content = template.Item1,
                    Pros = template.Item2,
                    Cons = string.IsNullOrEmpty(template.Item3) ? null : template.Item3,
                    IsVerifiedPurchase = i == 0,
                    IsRecommended = rating >= 4,
                    HelpfulCount = new Random(reviewIdx * 10 + i).Next(0, 30),
                    QualityRating = Math.Min(rating + 1, 5),
                    ValueRating = rating,
                    DeliveryRating = Math.Min(rating + 1, 5),
                    CreatedAt = now.AddDays(-new Random(reviewIdx * 10 + i).Next(1, 90)),
                    IsDeleted = false,
                });
                reviewIdx++;
            }
        }

        // Service reviews (40)
        foreach (var service in services.Take(20))
        {
            int reviewCount = 2;
            for (int i = 0; i < reviewCount; i++)
            {
                var reviewer = reviewers[(reviewIdx + i) % reviewers.Length];
                var template = serviceTitles[(reviewIdx + i) % serviceTitles.Length];
                var rating = 4 + (reviewIdx + i) % 2;

                reviews.Add(new Review
                {
                    Id = Guid.NewGuid(),
                    ReviewerId = users[reviewer].Id,
                    RevieweeId = service.SellerId,
                    ServiceId = service.Id,
                    ReviewType = "Service",
                    Rating = rating,
                    Title = template.Item1.Split('!')[0].Split('.')[0],
                    Content = template.Item1,
                    Pros = template.Item2,
                    Cons = string.IsNullOrEmpty(template.Item3) ? null : template.Item3,
                    IsVerifiedPurchase = true,
                    IsRecommended = true,
                    HelpfulCount = new Random(reviewIdx * 10 + i).Next(0, 20),
                    QualityRating = rating,
                    CommunicationRating = Math.Min(rating + 1, 5),
                    ProfessionalismRating = rating,
                    DeliveryRating = Math.Min(rating, 5),
                    CreatedAt = now.AddDays(-new Random(reviewIdx * 10 + i).Next(1, 60)),
                    IsDeleted = false,
                });
                reviewIdx++;
            }
        }

        // Store reviews (10)
        foreach (var store in stores.Values.Take(10))
        {
            var reviewer = reviewers[reviewIdx % reviewers.Length];
            var rating = 4 + reviewIdx % 2;
            reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = users[reviewer].Id,
                StoreId = store.Id,
                ReviewType = "Store",
                Rating = rating,
                Title = "Great store experience",
                Content = "This store consistently delivers quality products with excellent customer service. Will definitely shop here again.",
                Pros = "Great selection, fast shipping, quality products",
                IsVerifiedPurchase = true,
                IsRecommended = true,
                HelpfulCount = new Random(reviewIdx).Next(5, 40),
                QualityRating = rating,
                ValueRating = rating,
                DeliveryRating = Math.Min(rating + 1, 5),
                CreatedAt = now.AddDays(-new Random(reviewIdx).Next(1, 120)),
                IsDeleted = false,
            });
            reviewIdx++;
        }

        return reviews;
    }

    // ═══════════════════════════════════════════════════
    //  USER SKILLS
    // ═══════════════════════════════════════════════════
    private static List<UserSkill> CreateUserSkills(
        Dictionary<string, User> users, Dictionary<string, Skill> skills, DateTime now)
    {
        var userSkills = new List<UserSkill>();

        void Assign(string userKey, params (string skillName, SkillLevel level, int yoe, bool primary)[] items)
        {
            foreach (var (skillName, level, yoe, primary) in items)
            {
                if (!skills.ContainsKey(skillName)) continue;
                userSkills.Add(new UserSkill
                {
                    Id = Guid.NewGuid(),
                    UserId = users[userKey].Id,
                    SkillId = skills[skillName].Id,
                    Level = level,
                    YearsOfExperience = yoe,
                    VerificationStatus = level >= SkillLevel.Advanced
                        ? VerificationStatus.Verified : VerificationStatus.Unverified,
                    VerifiedAt = level >= SkillLevel.Advanced ? now.AddDays(-60) : null,
                    IsPrimary = primary,
                    EndorsementCount = (int)level * 5,
                    CreatedAt = now,
                    IsDeleted = false,
                });
            }
        }

        Assign("john_dev",
            ("C#", SkillLevel.Expert, 8, true),
            (".NET", SkillLevel.Expert, 8, true),
            ("React", SkillLevel.Advanced, 5, false),
            ("TypeScript", SkillLevel.Advanced, 4, false),
            ("SQL", SkillLevel.Advanced, 7, false),
            ("Docker", SkillLevel.Intermediate, 3, false),
            ("Node.js", SkillLevel.Intermediate, 3, false),
            ("AWS", SkillLevel.Intermediate, 2, false));

        Assign("priya_designer",
            ("UI/UX Design", SkillLevel.Expert, 6, true),
            ("Figma", SkillLevel.Expert, 5, true),
            ("Adobe Photoshop", SkillLevel.Advanced, 6, false),
            ("Adobe Illustrator", SkillLevel.Advanced, 5, false),
            ("Sketch", SkillLevel.Advanced, 4, false),
            ("Graphic Design", SkillLevel.Expert, 6, false),
            ("Motion Graphics", SkillLevel.Intermediate, 2, false));

        Assign("alex_data",
            ("Python", SkillLevel.Expert, 7, true),
            ("Machine Learning", SkillLevel.Expert, 5, true),
            ("TensorFlow", SkillLevel.Advanced, 4, false),
            ("SQL", SkillLevel.Advanced, 6, false),
            ("Data Visualization", SkillLevel.Advanced, 5, false),
            ("R", SkillLevel.Intermediate, 3, false),
            ("Power BI", SkillLevel.Intermediate, 2, false),
            ("AWS", SkillLevel.Intermediate, 2, false));

        Assign("maria_writer",
            ("Copywriting", SkillLevel.Expert, 5, true),
            ("SEO", SkillLevel.Advanced, 4, true),
            ("Content Marketing", SkillLevel.Advanced, 4, false),
            ("Social Media Marketing", SkillLevel.Intermediate, 3, false),
            ("Analytics", SkillLevel.Intermediate, 2, false));

        Assign("yuki_mobile",
            ("Flutter", SkillLevel.Expert, 5, true),
            ("React Native", SkillLevel.Expert, 4, true),
            ("Kotlin", SkillLevel.Advanced, 3, false),
            ("Swift", SkillLevel.Advanced, 3, false),
            ("JavaScript", SkillLevel.Advanced, 6, false),
            ("TypeScript", SkillLevel.Advanced, 4, false),
            ("UI/UX Design", SkillLevel.Intermediate, 2, false));

        Assign("omar_devops",
            ("Docker", SkillLevel.Expert, 5, true),
            ("Kubernetes", SkillLevel.Expert, 4, true),
            ("AWS", SkillLevel.Expert, 6, false),
            ("Azure", SkillLevel.Advanced, 3, false),
            ("CI/CD", SkillLevel.Expert, 5, false),
            ("Python", SkillLevel.Intermediate, 3, false),
            ("Go", SkillLevel.Intermediate, 2, false));

        Assign("lucas_python",
            ("Python", SkillLevel.Advanced, 4, true),
            ("JavaScript", SkillLevel.Intermediate, 3, false),
            ("SQL", SkillLevel.Intermediate, 3, false),
            ("Machine Learning", SkillLevel.Intermediate, 2, false),
            ("Docker", SkillLevel.Beginner, 1, false),
            ("React", SkillLevel.Beginner, 1, false));

        Assign("emma_market",
            ("SEO", SkillLevel.Expert, 6, true),
            ("Google Ads", SkillLevel.Expert, 5, true),
            ("Content Marketing", SkillLevel.Advanced, 5, false),
            ("Social Media Marketing", SkillLevel.Advanced, 4, false),
            ("Email Marketing", SkillLevel.Advanced, 4, false),
            ("Analytics", SkillLevel.Advanced, 5, false),
            ("Facebook Ads", SkillLevel.Advanced, 4, false),
            ("Copywriting", SkillLevel.Intermediate, 3, false));

        Assign("ceo_james",
            ("Project Management", SkillLevel.Expert, 12, true),
            ("Business Strategy", SkillLevel.Expert, 10, false),
            ("Leadership", SkillLevel.Expert, 12, false),
            ("Product Management", SkillLevel.Advanced, 8, false));

        Assign("cto_sarah",
            ("AWS", SkillLevel.Expert, 10, true),
            ("Docker", SkillLevel.Expert, 7, false),
            ("Kubernetes", SkillLevel.Expert, 5, false),
            ("Python", SkillLevel.Advanced, 8, false),
            (".NET", SkillLevel.Advanced, 6, false),
            ("CI/CD", SkillLevel.Expert, 8, false));

        Assign("founder_wei",
            ("Python", SkillLevel.Expert, 10, true),
            ("Machine Learning", SkillLevel.Expert, 8, true),
            ("TensorFlow", SkillLevel.Expert, 6, false),
            ("Product Management", SkillLevel.Advanced, 5, false),
            ("Leadership", SkillLevel.Advanced, 7, false));

        Assign("techstore_mike",
            ("Entrepreneurship", SkillLevel.Advanced, 10, true),
            ("Project Management", SkillLevel.Intermediate, 5, false),
            ("Microsoft Excel", SkillLevel.Advanced, 8, false));

        Assign("fashion_nina",
            ("Graphic Design", SkillLevel.Advanced, 8, true),
            ("Adobe Photoshop", SkillLevel.Advanced, 7, false),
            ("Social Media Marketing", SkillLevel.Intermediate, 4, false));

        return userSkills;
    }

    // ═══════════════════════════════════════════════════
    //  CONNECTIONS
    // ═══════════════════════════════════════════════════
    private static List<Connection> CreateConnections(
        Dictionary<string, User> users, DateTime now)
    {
        var connections = new List<Connection>();
        var pairs = new (string a, string b)[]
        {
            ("john_dev", "priya_designer"),
            ("john_dev", "yuki_mobile"),
            ("john_dev", "omar_devops"),
            ("john_dev", "lucas_python"),
            ("john_dev", "ceo_james"),
            ("john_dev", "cto_sarah"),
            ("priya_designer", "yuki_mobile"),
            ("priya_designer", "maria_writer"),
            ("priya_designer", "emma_market"),
            ("alex_data", "lucas_python"),
            ("alex_data", "founder_wei"),
            ("alex_data", "cto_sarah"),
            ("maria_writer", "emma_market"),
            ("maria_writer", "buyer_sophie"),
            ("yuki_mobile", "omar_devops"),
            ("yuki_mobile", "lucas_python"),
            ("omar_devops", "cto_sarah"),
            ("omar_devops", "ceo_james"),
            ("emma_market", "buyer_tom"),
            ("emma_market", "buyer_ahmed"),
            ("ceo_james", "cto_sarah"),
            ("ceo_james", "founder_wei"),
            ("ceo_james", "hr_anna"),
            ("cto_sarah", "founder_wei"),
            ("hr_anna", "hr_miguel"),
            ("buyer_sophie", "buyer_tom"),
            ("buyer_sophie", "buyer_ahmed"),
            ("techstore_mike", "softshop_dave"),
            ("fashion_nina", "homecraft_lisa"),
            ("bookworm_raj", "homecraft_lisa"),
        };

        foreach (var (a, b) in pairs)
        {
            connections.Add(new Connection
            {
                Id = Guid.NewGuid(),
                RequesterId = users[a].Id,
                AddresseeId = users[b].Id,
                Status = ConnectionStatus.Accepted,
                AcceptedAt = now.AddDays(-new Random((a + b).GetHashCode()).Next(1, 180)),
                CreatedAt = now.AddDays(-new Random((a + b).GetHashCode()).Next(1, 200)),
            });
        }

        // A few pending connections
        connections.Add(new Connection
        {
            Id = Guid.NewGuid(),
            RequesterId = users["newuser_jane"].Id,
            AddresseeId = users["john_dev"].Id,
            Status = ConnectionStatus.Pending,
            Message = "Hi John, I'd love to connect and discuss a project idea!",
            CreatedAt = now.AddDays(-2),
        });
        connections.Add(new Connection
        {
            Id = Guid.NewGuid(),
            RequesterId = users["newuser_bob"].Id,
            AddresseeId = users["priya_designer"].Id,
            Status = ConnectionStatus.Pending,
            Message = "Hi Priya, your portfolio is amazing. Would love to connect!",
            CreatedAt = now.AddDays(-1),
        });

        return connections;
    }

    // ═══════════════════════════════════════════════════
    //  FOLLOWS
    // ═══════════════════════════════════════════════════
    private static List<Follow> CreateFollows(
        Dictionary<string, User> users, Dictionary<string, Store> stores,
        List<Company> companies, DateTime now)
    {
        var follows = new List<Follow>();

        void UserFollowsUser(string followerKey, string followingKey)
        {
            follows.Add(new Follow
            {
                Id = Guid.NewGuid(),
                FollowerId = users[followerKey].Id,
                FollowingId = users[followingKey].Id,
                TargetType = FollowTargetType.User,
                NotificationsEnabled = true,
                CreatedAt = now.AddDays(-new Random((followerKey + followingKey).GetHashCode()).Next(1, 150)),
            });
        }

        void UserFollowsStore(string followerKey, string storeKey)
        {
            follows.Add(new Follow
            {
                Id = Guid.NewGuid(),
                FollowerId = users[followerKey].Id,
                FollowingId = stores[storeKey].Id,
                TargetType = FollowTargetType.Store,
                NotificationsEnabled = true,
                CreatedAt = now.AddDays(-new Random((followerKey + storeKey).GetHashCode()).Next(1, 120)),
            });
        }

        void UserFollowsCompany(string followerKey, Guid companyId)
        {
            follows.Add(new Follow
            {
                Id = Guid.NewGuid(),
                FollowerId = users[followerKey].Id,
                FollowingId = companyId,
                TargetType = FollowTargetType.Company,
                NotificationsEnabled = true,
                CreatedAt = now.AddDays(-new Random((followerKey + companyId.ToString()).GetHashCode()).Next(1, 100)),
            });
        }

        // User follows user
        UserFollowsUser("buyer_tom", "john_dev");
        UserFollowsUser("buyer_tom", "priya_designer");
        UserFollowsUser("buyer_sophie", "john_dev");
        UserFollowsUser("buyer_sophie", "priya_designer");
        UserFollowsUser("buyer_sophie", "yuki_mobile");
        UserFollowsUser("buyer_ahmed", "emma_market");
        UserFollowsUser("buyer_ahmed", "omar_devops");
        UserFollowsUser("newuser_jane", "john_dev");
        UserFollowsUser("newuser_jane", "priya_designer");
        UserFollowsUser("newuser_jane", "maria_writer");
        UserFollowsUser("newuser_bob", "yuki_mobile");
        UserFollowsUser("newuser_bob", "alex_data");
        UserFollowsUser("john_dev", "ceo_james");
        UserFollowsUser("john_dev", "cto_sarah");
        UserFollowsUser("priya_designer", "founder_wei");
        UserFollowsUser("alex_data", "founder_wei");
        UserFollowsUser("lucas_python", "john_dev");
        UserFollowsUser("lucas_python", "alex_data");
        UserFollowsUser("omar_devops", "cto_sarah");
        UserFollowsUser("maria_writer", "emma_market");

        // User follows stores
        UserFollowsStore("buyer_tom", "techgadgets");
        UserFollowsStore("buyer_tom", "homecraft");
        UserFollowsStore("buyer_sophie", "ninaboutique");
        UserFollowsStore("buyer_sophie", "bookhaven");
        UserFollowsStore("buyer_ahmed", "techgadgets");
        UserFollowsStore("buyer_ahmed", "softshop");
        UserFollowsStore("newuser_jane", "ninaboutique");
        UserFollowsStore("newuser_jane", "beautybox");
        UserFollowsStore("newuser_bob", "techgadgets");
        UserFollowsStore("newuser_bob", "sportzone");
        UserFollowsStore("john_dev", "softshop");
        UserFollowsStore("john_dev", "codetools");
        UserFollowsStore("priya_designer", "ninaboutique");
        UserFollowsStore("alex_data", "bookhaven");
        UserFollowsStore("maria_writer", "bookhaven");

        // User follows companies
        var companyMap = companies.ToDictionary(c => c.Slug, c => c.Id);
        if (companyMap.TryGetValue("techcorp-solutions", out var techcorpId))
        {
            UserFollowsCompany("john_dev", techcorpId);
            UserFollowsCompany("omar_devops", techcorpId);
            UserFollowsCompany("buyer_tom", techcorpId);
        }
        if (companyMap.TryGetValue("cloudnine", out var cloudnineId))
        {
            UserFollowsCompany("omar_devops", cloudnineId);
            UserFollowsCompany("john_dev", cloudnineId);
        }
        if (companyMap.TryGetValue("innovateai", out var innovateId))
        {
            UserFollowsCompany("alex_data", innovateId);
            UserFollowsCompany("lucas_python", innovateId);
        }

        return follows;
    }

    // ═══════════════════════════════════════════════════
    //  PORTFOLIOS (seed from sample project data)
    // ═══════════════════════════════════════════════════
    private static List<Portfolio> CreatePortfolios(Dictionary<string, User> users, DateTime now)
    {
        var portfolios = new List<Portfolio>();

        if (users.TryGetValue("john_dev", out var johnDev))
        {
            portfolios.Add(new Portfolio
            {
                Id = Guid.NewGuid(),
                UserId = johnDev.Id,
                Slug = "john-dev",
                IsPublic = true,
                Theme = "dark",
                PersonalInfo = """{"fullName":"John Developer","title":"Senior Full-Stack Engineer","email":"john@marketplace.dev","phone":"+1234567890","location":"San Francisco, CA","bio":"Senior Full-Stack Engineer with 10+ years of experience building scalable web applications, distributed systems, and cloud-native solutions. Expert in .NET, React, Node.js, and cloud platforms.","profileImage":"","socialLinks":[{"id":"github","platform":"github","url":"https://github.com/johndev"},{"id":"linkedin","platform":"linkedin","url":"https://linkedin.com/in/johndev"}]}""",
                Education = """[{"id":"edu-1","degree":"Master of Science","institution":"Stanford University","field":"Computer Science","startDate":"2012-09","endDate":"2014-06","current":false,"gpa":"3.9","description":"Focused on distributed systems and machine learning."},{"id":"edu-2","degree":"Bachelor of Science","institution":"UC Berkeley","field":"Computer Science","startDate":"2008-09","endDate":"2012-06","current":false,"gpa":"3.7","description":"Dean's List honors."}]""",
                Experience = """[{"id":"exp-1","title":"Senior Software Engineer","company":"TechCorp Solutions","location":"San Francisco, CA","locationType":"hybrid","startDate":"2022-01","endDate":"","current":true,"description":"Lead architect for the company's flagship SaaS platform serving 500K+ users.","technologies":[".NET Core","React","PostgreSQL","AWS","Docker","Kubernetes"],"responsibilities":["Architected microservices handling 10K+ requests/sec","Led team of 8 engineers","Reduced deployment time by 70% with CI/CD pipelines"]},{"id":"exp-2","title":"Software Engineer","company":"DataFlow Inc.","location":"Seattle, WA","locationType":"remote","startDate":"2018-06","endDate":"2021-12","current":false,"description":"Built real-time data processing pipelines for analytics platform.","technologies":["Node.js","TypeScript","Apache Kafka","Redis","MongoDB"],"responsibilities":["Designed event-driven architecture","Built real-time dashboards","Optimized query performance by 60%"]}]""",
                Skills = """[{"id":"sk-1","name":".NET Core","level":"expert","category":"Backend"},{"id":"sk-2","name":"React","level":"expert","category":"Frontend"},{"id":"sk-3","name":"Node.js","level":"expert","category":"Backend"},{"id":"sk-4","name":"TypeScript","level":"expert","category":"Languages"},{"id":"sk-5","name":"PostgreSQL","level":"expert","category":"Databases"},{"id":"sk-6","name":"AWS","level":"proficient","category":"Cloud"},{"id":"sk-7","name":"Docker","level":"proficient","category":"DevOps"},{"id":"sk-8","name":"Kubernetes","level":"proficient","category":"DevOps"},{"id":"sk-9","name":"Python","level":"proficient","category":"Languages"},{"id":"sk-10","name":"GraphQL","level":"proficient","category":"Backend"}]""",
                Roles = """[{"id":"role-1","title":"Full Stack Developer","level":"expert"},{"id":"role-2","title":"Software Architect","level":"expert"},{"id":"role-3","title":"Technical Lead","level":"proficient"}]""",
                Certifications = """[{"id":"cert-1","name":"AWS Solutions Architect","issuer":"Amazon Web Services","issueDate":"2023-03","credentialUrl":"https://aws.amazon.com/certification/"},{"id":"cert-2","name":"Microsoft Certified: Azure Developer","issuer":"Microsoft","issueDate":"2022-08"}]""",
                Projects = """[{"id":"proj-1","name":"Marketplace Platform","description":"Full-featured social marketplace with real-time messaging, escrow payments, and AI-powered search.","url":"https://marketplace.dev","technologies":["React","Next.js",".NET Core","PostgreSQL","Redis"],"highlights":["500K+ active users","Real-time notifications","AI-powered recommendations"],"startDate":"2022-01"},{"id":"proj-2","name":"DevOps Dashboard","description":"Real-time infrastructure monitoring and alerting tool for distributed systems.","technologies":["Node.js","React","Grafana","Prometheus","Kubernetes"],"highlights":["10K+ deployments tracked","99.99% uptime","Auto-scaling"],"startDate":"2020-06","endDate":"2021-12"}]""",
                Achievements = """[{"id":"ach-1","title":"Open Source Contributor of the Year","description":"Recognized for significant contributions to .NET ecosystem","date":"2024-01","issuer":"GitHub Stars"}]""",
                Languages = """[{"id":"lang-1","name":"English","proficiency":"native"},{"id":"lang-2","name":"Spanish","proficiency":"professional"}]""",
                Resumes = """[{"id":"1","name":"My Resume (Profile)","templateId":"classic","isActive":true,"isStandard":true,"createdAt":"2026-01-01T00:00:00Z","updatedAt":"2026-01-01T00:00:00Z"}]""",
                CreatedAt = now,
                IsDeleted = false
            });
        }

        if (users.TryGetValue("priya_designer", out var priya))
        {
            portfolios.Add(new Portfolio
            {
                Id = Guid.NewGuid(),
                UserId = priya.Id,
                Slug = "priya-designer",
                IsPublic = true,
                Theme = "light",
                PersonalInfo = """{"fullName":"Priya Sharma","title":"Senior UI/UX Designer","email":"priya@design.studio","phone":"+91-9876543210","location":"Mumbai, India","bio":"Creating intuitive and beautiful user experiences for web and mobile products. Design thinking practitioner with a passion for accessibility and inclusive design.","profileImage":"","socialLinks":[{"id":"dribbble","platform":"dribbble","url":"https://dribbble.com/priyasharma"},{"id":"behance","platform":"behance","url":"https://behance.net/priyasharma"},{"id":"linkedin","platform":"linkedin","url":"https://linkedin.com/in/priyasharma"}]}""",
                Education = """[{"id":"edu-1","degree":"Master of Design","institution":"NID Ahmedabad","field":"Interaction Design","startDate":"2016-07","endDate":"2018-06","current":false,"gpa":"3.8","description":"Thesis on accessible design systems for emerging markets."}]""",
                Experience = """[{"id":"exp-1","title":"Senior UI/UX Designer","company":"NordicTech","location":"Stockholm, Sweden (Remote)","locationType":"remote","startDate":"2023-01","endDate":"","current":true,"description":"Leading design for fintech products used by 2M+ users across Scandinavia.","technologies":["Figma","Framer","Storybook","TailwindCSS"],"responsibilities":["Led design system serving 5 product teams","Improved conversion rates by 35%","Mentored 4 junior designers"]},{"id":"exp-2","title":"UI/UX Designer","company":"Atelier Sophie","location":"Paris, France","locationType":"hybrid","startDate":"2020-06","endDate":"2022-12","current":false,"description":"Designed e-commerce experiences for luxury artisan marketplace.","technologies":["Figma","Adobe XD","Sketch","InVision"],"responsibilities":["Redesigned checkout flow reducing cart abandonment by 25%","Created comprehensive style guide","Conducted 50+ user interviews"]}]""",
                Skills = """[{"id":"sk-1","name":"UI/UX Design","level":"expert","category":"Design"},{"id":"sk-2","name":"Figma","level":"expert","category":"Design Tools"},{"id":"sk-3","name":"Adobe Photoshop","level":"proficient","category":"Design Tools"},{"id":"sk-4","name":"Adobe Illustrator","level":"proficient","category":"Design Tools"},{"id":"sk-5","name":"Sketch","level":"proficient","category":"Design Tools"},{"id":"sk-6","name":"Design Systems","level":"expert","category":"Design"},{"id":"sk-7","name":"User Research","level":"expert","category":"Research"},{"id":"sk-8","name":"Prototyping","level":"expert","category":"Design"},{"id":"sk-9","name":"Motion Design","level":"intermediate","category":"Design"},{"id":"sk-10","name":"TailwindCSS","level":"proficient","category":"Frontend"}]""",
                Roles = """[{"id":"role-1","title":"UI/UX Designer","level":"expert"},{"id":"role-2","title":"Design Lead","level":"proficient"},{"id":"role-3","title":"Product Designer","level":"expert"}]""",
                Certifications = """[{"id":"cert-1","name":"Google UX Design Certificate","issuer":"Google","issueDate":"2022-05"},{"id":"cert-2","name":"Nielsen Norman UX Certification","issuer":"Nielsen Norman Group","issueDate":"2023-01"}]""",
                Projects = """[{"id":"proj-1","name":"NordicPay Redesign","description":"Complete redesign of a fintech payment platform serving 2M+ users. Improved NPS score from 45 to 78.","technologies":["Figma","React","Storybook"],"highlights":["35% conversion improvement","New design system","Accessibility AA compliance"],"startDate":"2023-03"},{"id":"proj-2","name":"Artisan Marketplace UX","description":"End-to-end UX design for luxury artisan e-commerce platform.","technologies":["Figma","Adobe XD","InVision"],"highlights":["25% reduction in cart abandonment","50+ user interviews","Complete style guide"],"startDate":"2020-06","endDate":"2022-12"}]""",
                Achievements = """[]""",
                Languages = """[{"id":"lang-1","name":"English","proficiency":"fluent"},{"id":"lang-2","name":"Hindi","proficiency":"native"},{"id":"lang-3","name":"Marathi","proficiency":"native"}]""",
                Resumes = """[{"id":"1","name":"My Resume (Profile)","templateId":"modern","isActive":true,"isStandard":true,"createdAt":"2026-01-01T00:00:00Z","updatedAt":"2026-01-01T00:00:00Z"}]""",
                CreatedAt = now,
                IsDeleted = false
            });
        }

        if (users.TryGetValue("alex_data", out var alex))
        {
            portfolios.Add(new Portfolio
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Slug = "alex-data",
                IsPublic = true,
                Theme = "dark",
                PersonalInfo = """{"fullName":"Alex Chen","title":"Data Scientist & ML Engineer","email":"alex@datascience.ai","location":"New York, NY","bio":"Data Scientist specializing in NLP, computer vision, and production ML systems. Building AI-powered products that scale.","profileImage":"","socialLinks":[{"id":"github","platform":"github","url":"https://github.com/alexchen"},{"id":"linkedin","platform":"linkedin","url":"https://linkedin.com/in/alexchen"}]}""",
                Education = """[{"id":"edu-1","degree":"PhD","institution":"MIT","field":"Machine Learning","startDate":"2015-09","endDate":"2019-06","current":false,"description":"Research on transformer architectures for NLP tasks."},{"id":"edu-2","degree":"Bachelor of Science","institution":"Caltech","field":"Mathematics","startDate":"2011-09","endDate":"2015-06","current":false,"gpa":"3.95"}]""",
                Experience = """[{"id":"exp-1","title":"Senior Data Scientist","company":"InnovateAI","location":"New York, NY","locationType":"hybrid","startDate":"2021-01","endDate":"","current":true,"description":"Leading ML team building production recommendation systems and NLP pipelines.","technologies":["Python","PyTorch","TensorFlow","Spark","AWS SageMaker"],"responsibilities":["Built recommendation engine improving engagement by 40%","Deployed 15+ ML models to production","Led team of 5 data scientists"]}]""",
                Skills = """[{"id":"sk-1","name":"Python","level":"expert","category":"Languages"},{"id":"sk-2","name":"PyTorch","level":"expert","category":"ML Frameworks"},{"id":"sk-3","name":"TensorFlow","level":"expert","category":"ML Frameworks"},{"id":"sk-4","name":"NLP","level":"expert","category":"AI/ML"},{"id":"sk-5","name":"Computer Vision","level":"proficient","category":"AI/ML"},{"id":"sk-6","name":"Apache Spark","level":"proficient","category":"Big Data"},{"id":"sk-7","name":"SQL","level":"expert","category":"Databases"},{"id":"sk-8","name":"AWS SageMaker","level":"proficient","category":"Cloud"}]""",
                Roles = """[{"id":"role-1","title":"Data Scientist","level":"expert"},{"id":"role-2","title":"ML Engineer","level":"expert"},{"id":"role-3","title":"AI Researcher","level":"proficient"}]""",
                Certifications = """[]""",
                Projects = """[{"id":"proj-1","name":"Smart Recommendation Engine","description":"Built production recommendation system processing 100M+ events daily.","technologies":["Python","PyTorch","Apache Kafka","Redis"],"highlights":["40% engagement improvement","100M+ events/day","Real-time inference"],"startDate":"2021-06"}]""",
                Achievements = """[{"id":"ach-1","title":"Best Paper Award - NeurIPS","description":"Awarded for research on efficient transformer architectures","date":"2019-12","issuer":"NeurIPS"}]""",
                Languages = """[{"id":"lang-1","name":"English","proficiency":"native"},{"id":"lang-2","name":"Mandarin","proficiency":"native"}]""",
                Resumes = """[{"id":"1","name":"My Resume (Profile)","templateId":"minimal","isActive":true,"isStandard":true,"createdAt":"2026-01-01T00:00:00Z","updatedAt":"2026-01-01T00:00:00Z"}]""",
                CreatedAt = now,
                IsDeleted = false
            });
        }

        return portfolios;
    }

    // ═══════════════════════════════════════════════════
    //  SUPER ADMIN SEEDER (Almas Khan)
    // ═══════════════════════════════════════════════════
    public static async Task SeedSuperAdminAsync(MarketplaceDbContext context)
    {
        const string superAdminEmail = "almaskhanwazir@gmail.com";

        if (await context.Users.AnyAsync(u => u.Email == superAdminEmail))
            return;

        var now = DateTime.UtcNow;
        var superAdminId = Guid.Parse("fe2281b8-3ff9-47cf-8046-fb44b4d20cd5");

        // BCrypt hash of "admin123"
        const string passwordHash = "$2a$11$pCktTQ9.Tp39KS.e96AebubtCcgy5.PNaM1Q3CKUXohSmcKH7gvpG";

        // 1. User
        var user = new User
        {
            Id = superAdminId,
            Username = "almaskhanwazir",
            Email = superAdminEmail,
            PasswordHash = passwordHash,
            FirstName = "Muhammad Almas",
            LastName = "Khan",
            PhoneNumber = "+923314846647",
            AvatarUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=almaskhanwazir",
            Bio = "Senior Software Engineer with 8+ years of hands-on experience architecting and delivering scalable cloud-native backends, data integration pipelines, microservices, and high-availability enterprise systems.",
            Status = UserStatus.Active,
            EmailVerified = true,
            EmailVerifiedAt = now.AddDays(-90),
            PhoneVerified = true,
            PhoneVerifiedAt = now.AddDays(-90),
            PreferredLanguage = "en",
            TimeZone = "Asia/Karachi",
            Currency = "PKR",
            Country = "PK",
            City = "Islamabad",
            ReputationScore = 100,
            AverageRating = 5.0m,
            TotalReviews = 0,
            IsVerifiedSeller = true,
            IsVerifiedBuyer = true,
            LastLoginAt = now,
            CreatedAt = now.AddDays(-365),
            IsDeleted = false,
        };
        context.Users.Add(user);

        // 2. Assign ALL roles (super admin)
        var allRoles = await context.Roles.ToListAsync();
        foreach (var role in allRoles)
        {
            context.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = superAdminId,
                RoleId = role.Id,
                CreatedAt = now,
                IsDeleted = false,
            });
        }

        // 3. User Profile
        context.UserProfiles.Add(new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = superAdminId,
            Headline = "Senior Software Engineer | Lead Engineer | Full Stack | .NET, Node.js, React, AWS, Azure",
            About = "Senior Software Engineer with 8+ years of hands-on experience architecting and delivering scalable cloud-native backends, data integration pipelines, microservices, and high-availability enterprise systems. Expertise in .NET Core, Node.js, AWS & Azure cloud services, ETL processes, and AI integrations for real-time applications.",
            HourlyRate = 75,
            YearsOfExperience = 8,
            CompletedProjects = 25,
            AvailableForHire = true,
            Website = null,
            LinkedInUrl = "https://www.linkedin.com/in/almaskhanwazir/",
            GitHubUrl = "https://github.com/almaskhanwazir",
            PortfolioUrl = null,
            CompanyName = "Ad Astra, Inc.",
            Education = "B.S. Software Engineering, University of Science and Technology Bannu (3.57 GPA)",
            Certifications = null,
            IdVerified = true,
            IdVerifiedAt = now.AddDays(-60),
            CreatedAt = now,
            IsDeleted = false,
        });

        // 4. Wallet
        context.Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = superAdminId,
            Balance = 0,
            PendingBalance = 0,
            HeldBalance = 0,
            Currency = "PKR",
            IsActive = true,
            TotalEarned = 0,
            TotalWithdrawn = 0,
            TotalSpent = 0,
            CreatedAt = now,
            IsDeleted = false,
        });

        // 5. Portfolio with full data from JSON files
        var personalInfo = new
        {
            fullName = "Muhammad Almas Khan",
            title = "Senior Software Engineer",
            email = superAdminEmail,
            phone = "+923314846647",
            whatsapp = "+923029473494",
            location = "Islamabad, Pakistan",
            bio = user.Bio,
            profileImage = "",
            socialLinks = new[]
            {
                new { id = "github", platform = "github", url = "https://github.com/almaskhanwazir" },
                new { id = "linkedin", platform = "linkedin", url = "https://www.linkedin.com/in/almaskhanwazir/" }
            }
        };

        var education = new[]
        {
            new
            {
                degree = "Bachelor of Science",
                institution = "University of Science and Technology Bannu",
                field = "Software Engineering",
                startDate = "2014-10",
                endDate = "2018-10",
                current = false,
                gpa = "3.57",
                description = "FYP: University Online Clearance System - Created with .NET Core backend and Android app front end.",
                id = "9c8c90d2-92bc-4696-8992-fbd236e6d5e1"
            }
        };

        var experience = new object[]
        {
            new { title = "Lead Software Engineer", company = "Ad Astra, Inc.", location = "Islamabad, Pakistan", locationType = "remote", startDate = "2024-09", endDate = "", current = true,
                description = "Led development of Adastra Connect (Appointments & Interpretation Super App): Architected a comprehensive platform for appointment scheduling, live AI interpretation, human interpretation escalation, finance/billing, invoicing, and operational analytics.",
                technologies = new[] { "AWS Connect", "Lambda", "API Gateway", "RDS/Aurora", "S3", "EC2", "Kinesis", "Node.js", ".NET Core", "React", "WebRTC", "PostgreSQL", "MySQL" },
                id = "fb3c8696-2b64-4820-a82c-31302688cba9" },
            new { title = "Senior Software Engineer", company = "Ibex Global", location = "Islamabad, Pakistan", locationType = "hybrid", startDate = "2023-09", endDate = "2024-08", current = false,
                description = "Designed and implemented scalable microservices for multi-tenant enterprise workflows using modern cloud technologies and best practices.",
                technologies = new[] { ".NET Core", "Angular", "Node.js", "Serverless Framework", "AWS Lambda", "Azure Service Bus", "PostgreSQL", "SQL Server" },
                id = "a1b2c3d4-5678-90ab-cdef-123456789001" },
            new { title = "Software Engineer", company = "Systems Limited", location = "Islamabad, Pakistan", locationType = "onsite", startDate = "2021-09", endDate = "2023-09", current = false,
                description = "Worked with various tools and frameworks including .NET Core, Azure Functions, Microsoft Dynamics 365, and Azure services. Delivered PartnerLinQ ETL functions for large-scale data mapping and transformations.",
                technologies = new[] { ".NET Core", "MVC", "Azure Functions", "Microsoft Dynamics 365", "Azure DevOps", "Shopify APIs", "Blob Storage", "Active Directory", "BRE" },
                id = "a1b2c3d4-5678-90ab-cdef-123456789002" },
            new { title = "Software Engineer", company = "Innovative Solutions and Development", location = "Islamabad, Pakistan", locationType = "onsite", startDate = "2018-01", endDate = "2021-09", current = false,
                description = "Full-stack development with React.js frontend and .NET backend. Implemented modern web development practices including Agile, Scrum, DevOps, and test automation.",
                technologies = new[] { "React.js", ".NET", "MVC", "Shopify Liquid", "DevOps", "BDD Specflow", "Puppeteer", "Jest", "CI/CD" },
                id = "a1b2c3d4-5678-90ab-cdef-123456789003" }
        };

        var skills = new object[]
        {
            new { name = ".NET Core", level = "expert", category = "Backend", id = "c89f953a-95a4-49c2-94f5-60fa227090f8" },
            new { name = "C#", level = "expert", category = "Backend", id = "skill-csharp" },
            new { name = "Node.js", level = "expert", category = "Backend", id = "96e85e4b-a9a5-43a2-9e62-fa09e810bc57" },
            new { name = "React.js", level = "expert", category = "Frontend", id = "f9ca2020-1857-4d6d-9ea9-9367a4d7de49" },
            new { name = "Next.js", level = "proficient", category = "Frontend", id = "3f0c59f2-a614-4215-ac68-4aeff3829c85" },
            new { name = "Angular", level = "proficient", category = "Frontend", id = "skill-angular" },
            new { name = "TypeScript", level = "expert", category = "Languages", id = "d402cb3b-97e4-4443-ad41-b7fff9db2e1f" },
            new { name = "JavaScript", level = "expert", category = "Languages", id = "47a8afb3-63e5-4a76-84b2-e241a48dcdde" },
            new { name = "Python", level = "intermediate", category = "Languages", id = "aeb5fd41-be73-405b-a3a6-54306b26efa1" },
            new { name = "AWS Lambda", level = "expert", category = "Cloud & DevOps", id = "skill-lambda" },
            new { name = "Azure Functions", level = "expert", category = "Cloud & DevOps", id = "skill-azfunc" },
            new { name = "Docker", level = "proficient", category = "Cloud & DevOps", id = "f520e695-de9a-4555-9fb5-fd8f9feb1fb8" },
            new { name = "PostgreSQL", level = "expert", category = "Databases", id = "skill-postgres" },
            new { name = "SQL Server", level = "expert", category = "Databases", id = "skill-sqlserver" },
            new { name = "MongoDB", level = "proficient", category = "Databases", id = "skill-mongodb" },
            new { name = "Microservices", level = "expert", category = "Architecture", id = "skill-microservices" },
            new { name = "Clean Architecture", level = "expert", category = "Architecture", id = "skill-clean" },
            new { name = "REST APIs", level = "expert", category = "Architecture", id = "skill-rest" },
            new { name = "SignalR", level = "proficient", category = "AI & Integrations", id = "skill-signalr" },
            new { name = "TailwindCSS", level = "proficient", category = "Frontend", id = "skill-tailwind" },
            new { name = "Shopify/Liquid", level = "proficient", category = "E-Commerce", id = "skill-shopify" },
        };

        var roles2 = new object[]
        {
            new { title = "Lead Software Engineer", level = "expert", id = "role-lead" },
            new { title = "Full Stack Developer", level = "expert", id = "1a196df7-22db-4f6f-bb30-434f02fece58" },
            new { title = "Software Architect", level = "expert", id = "78fe4d7c-3b25-4f31-a275-d526afe73252" },
            new { title = "Backend Developer", level = "expert", id = "27667ded-2825-4d36-899c-7bff25d2cbe7" },
            new { title = "Cloud Engineer", level = "proficient", id = "role-cloud" },
        };

        var projects = new object[]
        {
            new { name = "Adastra Connect", description = "Appointments & Interpretation Super App - Comprehensive platform for appointment scheduling, live AI interpretation, human interpretation escalation, finance/billing, invoicing, and operational analytics.", url = "https://ad-astrainc.com/", technologies = new[] { "AWS Connect", "Lambda", "Node.js", ".NET Core", "React", "WebRTC", "PostgreSQL" }, highlights = new[] { "Real-time appointment orchestration", "AI speech-to-text integration", "Billing & invoice engine", "40% latency improvement" }, startDate = "2024-09", endDate = "", id = "proj-adastra" },
            new { name = "CableFinder", description = "End-to-end serviceability and quoting tool for cable providers.", url = "https://www.cablefinder.net/", technologies = new[] { "Angular", ".NET Core", "AWS Lambda", "PostgreSQL", "MongoDB" }, highlights = new[] { "Unified multi-provider systems", "Real-time quoting" }, startDate = "2023-01", endDate = "2024-08", id = "proj-cablefinder" },
            new { name = "PartnerLinQ", description = "Enterprise supply chain connectivity platform delivering end-to-end visibility, control, and limitless flexibility.", url = "https://www.partnerlinq.com/", technologies = new[] { ".NET Core", "Azure Functions", "Microsoft Dynamics 365", "Azure DevOps" }, highlights = new[] { "Large-scale ETL pipelines", "Multi-format data transformations" }, startDate = "2021-09", endDate = "2023-09", id = "proj-partnerlinq" },
            new { name = "Prodoo", description = "Online Recruitment service connecting companies with skilled freelancers worldwide.", url = "https://www.prodoo.com/", technologies = new[] { "React.js", "Next.js", "Redux", ".NET Web APIs", "Azure Cloud" }, highlights = new[] { "Freelancer-recruiter matching", "Collaboration workflows" }, startDate = "2020-01", endDate = "2021-06", id = "proj-prodoo" },
        };

        var languages = new[]
        {
            new { language = "English", proficiency = "Professional", id = "lang-en" },
            new { language = "Urdu", proficiency = "Native", id = "lang-ur" },
            new { language = "Pashto", proficiency = "Native", id = "lang-ps" },
        };

        var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        context.Portfolios.Add(new Portfolio
        {
            Id = Guid.Parse("baa07431-fa2c-45dd-ab34-c11b9432b86e"),
            UserId = superAdminId,
            Slug = "almaskhanwazir",
            IsPublic = true,
            Theme = "dark",
            PersonalInfo = JsonSerializer.Serialize(personalInfo, jsonOpts),
            Education = JsonSerializer.Serialize(education, jsonOpts),
            Experience = JsonSerializer.Serialize(experience, jsonOpts),
            Skills = JsonSerializer.Serialize(skills, jsonOpts),
            Roles = JsonSerializer.Serialize(roles2, jsonOpts),
            Certifications = "[]",
            Projects = JsonSerializer.Serialize(projects, jsonOpts),
            Achievements = "[]",
            Languages = JsonSerializer.Serialize(languages, jsonOpts),
            Resumes = """[{"id":"1","name":"My Resume (Profile)","templateId":"minimal","isActive":true,"isStandard":true,"createdAt":"2026-02-05T00:00:00Z","updatedAt":"2026-02-05T00:00:00Z"}]""",
            CreatedAt = now,
            IsDeleted = false,
        });

        // 6. Notifications - welcome
        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = superAdminId,
            Type = NotificationType.System,
            Title = "Welcome, Super Admin!",
            Message = "Your super admin account has been created. You have full access to all platform features.",
            IsRead = false,
            CreatedAt = now,
            IsDeleted = false,
        });

        await context.SaveChangesAsync();
    }
}
