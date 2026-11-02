using Microsoft.AspNetCore.Identity;


namespace AppForSEII.API.Data {
    public class SeedData {
        public static void Initialize(ApplicationDbContext dbContext, IServiceProvider serviceProvider, ILogger logger) {
            List<string> rolesNames = new List<string> { "Administrator", "Employee", "Customer" };

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            try {
                SeedRoles(roleManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the roles in the Database.");
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            try {
                SeedUsers(userManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the Users in the Database.");
            }

            try {
                SeedGeneros(dbContext);
                SeedEditoriales(dbContext);
                SeedLibros(dbContext);
                SeedMetodosPago(dbContext);
                
                SeedCompras(dbContext, userManager);
                SeedCompraItems(dbContext);
                
                SeedReposiciones(dbContext, userManager);
                SeedReposicionItems(dbContext);
                
                SeedResenas(dbContext, userManager);
                SeedResenaItems(dbContext);
            }
            catch (Exception ex) {
                logger.LogError(ex, "Error occurred seeding the database with initial data.");
            }
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager, List<string> roles) {
            foreach (string roleName in roles) {
                if (!roleManager.RoleExistsAsync(roleName).Result) {
                    IdentityRole role = new IdentityRole();
                    role.Name = roleName;
                    role.NormalizedName = roleName;
                    IdentityResult roleResult = roleManager.CreateAsync(role).Result;
                }
            }
        }

        public static void SeedUsers(UserManager<ApplicationUser> userManager, List<string> roles) {
            if (userManager.FindByNameAsync("elena@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es", "Calle Principal 1");
                user.EmailConfirmed = true;
                user.UserName = "elena@uclm.es";
                user.PhoneNumber = "678371248";

                var result = userManager.CreateAsync(user, "Password1234%");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    userManager.AddToRoleAsync(user, roles[0]).Wait();
                }
            }

            if (userManager.FindByNameAsync("peter@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es", "Calle Mayor 2");
                user.EmailConfirmed = true;
                user.UserName = "peter@uclm.es";
                user.PhoneNumber = "678371249";
                var result = userManager.CreateAsync(user, "OtherPass12$");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    userManager.AddToRoleAsync(user, roles[2]).Wait();
                }
            }
        }

    public static void SeedGeneros(ApplicationDbContext dbContext)
        {
            if (!dbContext.Set<Genero>().Any())
            {
                dbContext.AddRange(
                    new Genero(0, "Fantasía"),
                    new Genero(0, "Ciencia Ficción"),
                    new Genero(0, "Terror"),
                    new Genero(0, "Misterio")
                );
                dbContext.SaveChanges();
            }
        }

        public static void SeedEditoriales(ApplicationDbContext dbContext)
        {
            if (!dbContext.Set<Editorial>().Any())
            {
                dbContext.AddRange(
                    new Editorial(0, "Minotauro"),
                    new Editorial(0, "Gigamesh"),
                    new Editorial(0, "Salamandra"),
                    new Editorial(0, "Plaza & Janés")
                );
                dbContext.SaveChanges();
            }
        }

        public static void SeedLibros(ApplicationDbContext dbContext)
        {
            if (!dbContext.Set<Libro>().Any())
            {
                var gFantasia = dbContext.Set<Genero>().First(g => g.Nombre == "Fantasía");
                var gCiencia = dbContext.Set<Genero>().First(g => g.Nombre == "Ciencia Ficción");
                var gTerror = dbContext.Set<Genero>().First(g => g.Nombre == "Terror");
                
                var edMinotauro = dbContext.Set<Editorial>().First(e => e.Nombre == "Minotauro");
                var edGigamesh = dbContext.Set<Editorial>().First(e => e.Nombre == "Gigamesh");
                var edPlaza = dbContext.Set<Editorial>().First(e => e.Nombre == "Plaza & Janés");

                dbContext.AddRange(
                    new Libro(0, "El Señor de los Anillos", "J.R.R. Tolkien", new DateTime(1954, 7, 29), 20.50m, 15.00m, 10, "Tapa dura", 4.9, gFantasia, edMinotauro),
                    new Libro(0, "Dune", "Frank Herbert", new DateTime(1965, 8, 1), 18.90m, 12.50m, 15, "Bolsillo", 4.8, gCiencia, edGigamesh),
                    new Libro(0, "El Resplandor", "Stephen King", new DateTime(1977, 1, 28), 22.00m, 16.00m, 0, "Tapa dura", 4.5, gTerror, edPlaza), // Stock = 0
                    new Libro(0, "It", "Stephen King", new DateTime(1986, 9, 15), 25.00m, 18.00m, 0, "Bolsillo", 4.6, gTerror, edPlaza) // Stock = 0
                );
                dbContext.SaveChanges();
            }
        }

        public static void SeedMetodosPago(ApplicationDbContext dbContext)
        {
            if (!dbContext.Set<MetodoPago>().Any())
            {
                Visa visa = new Visa(DateTime.Now.AddYears(2)) { NumeroTarjeta = "4123456789123456" };
                GooglePay googlePay = new GooglePay("pagos@uclm.es");
                PayPal payPal = new PayPal("600123456");

                dbContext.AddRange(visa, googlePay, payPal);
                dbContext.SaveChanges();
            }
        }

        public static void SeedCompras(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            if (!dbContext.Set<Compra>().Any())
            {
                var cliente = userManager.FindByNameAsync("peter@uclm.es").Result;
              var visa = dbContext.Set<MetodoPago>().Find(1);
        var googlePay = dbContext.Set<MetodoPago>().Find(2);

                if (cliente != null && visa != null && googlePay != null){
                     dbContext.AddRange(
                    new Compra(0, DateTime.Now.AddDays(-3), 97.70m, null, cliente.Id, cliente, visa.Id, visa, new List<CompraItem>()),
                    new Compra(0, DateTime.Now.AddDays(-1), 41.00m, "DTO10", cliente.Id, cliente, googlePay.Id, googlePay, new List<CompraItem>())
                );
                dbContext.SaveChanges();
                }
            }
        }

        public static void SeedCompraItems(ApplicationDbContext dbContext)
        {
            if (!dbContext.Set<CompraItem>().Any())
            {
                var compras = dbContext.Set<Compra>().ToList();
                if (compras.Count >= 2)
        {
                var l1 = dbContext.Set<Libro>().First(l => l.Titulo == "El Señor de los Anillos");
                var l2 = dbContext.Set<Libro>().First(l => l.Titulo == "Dune");

                dbContext.AddRange(
                    new CompraItem(2, l1.Id, l1, compras[0].Id, compras[0]),
                    new CompraItem(3, l2.Id, l2, compras[0].Id, compras[0]),
                    new CompraItem(2, l1.Id, l1, compras[1].Id, compras[1])
                );
                dbContext.SaveChanges();
                }
            }
        }

        public static void SeedReposiciones(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            if (!dbContext.Set<Reposicion>().Any())
            {
                var admin = userManager.FindByNameAsync("elena@uclm.es").Result;
                var payPal = dbContext.Set<MetodoPago>().AsEnumerable().OfType<PayPal>().First();
                var repo = new Reposicion(0, DateTime.Now, 320.00, "Reposición urgente stock 0");
                repo.ApplicationUserId = admin.Id;
                repo.Usuario = admin;
                repo.MetodoPagoId = payPal.Id;
                repo.MetodoPago = payPal;

                dbContext.Add(repo);
                dbContext.SaveChanges();
            }
        }

        public static void SeedReposicionItems(ApplicationDbContext dbContext)
        {
            if (!dbContext.Set<ReposicionItem>().Any())
            {
                var repo = dbContext.Set<Reposicion>().First();
                var l1 = dbContext.Set<Libro>().First(l => l.Titulo == "El Resplandor");
                var l2 = dbContext.Set<Libro>().First(l => l.Titulo == "It");

                dbContext.AddRange(
                    new ReposicionItem(0, 10, l1.Id, repo.Id),
                    new ReposicionItem(0, 20, l2.Id, repo.Id)
                );
                dbContext.SaveChanges();
            }
        }

        public static void SeedResenas(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            if (!dbContext.Set<Resena>().Any())
            {
                var cliente = userManager.FindByNameAsync("peter@uclm.es").Result;

                dbContext.Add(new Resena(0, DateTime.Now.AddDays(-2), cliente.Id));
                dbContext.SaveChanges();
            }
        }

        public static void SeedResenaItems(ApplicationDbContext dbContext)
        {
            if (!dbContext.Set<ResenaItem>().Any())
            {
                var resena = dbContext.Set<Resena>().First();
                var l1 = dbContext.Set<Libro>().First(l => l.Titulo == "El Señor de los Anillos");
                var l2 = dbContext.Set<Libro>().First(l => l.Titulo == "Dune");

                dbContext.AddRange(
                    new ResenaItem(l1.Id, resena.Id, "Una obra maestra que todo el mundo debe leer.", 5),
                    new ResenaItem(l2.Id, resena.Id, "Muy buen libro de ciencia ficción, me encantó.", 4)
                );
                dbContext.SaveChanges();
            }
        }
    }
}