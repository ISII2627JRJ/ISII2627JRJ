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

                SeedCompras(dbContext, userManager);        // CU1 Comprar libros
                SeedReposiciones(dbContext, userManager);   // CU2 Reponer libros
                SeedResenas(dbContext, userManager);        // CU4 Reseñar libros
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

        // ---------------- GÉNEROS (5) ----------------
        public static void SeedGeneros(ApplicationDbContext dbContext) {
            if (!dbContext.Generos.Any()) {
                dbContext.AddRange(
                    new Genero(0, "Fantasía"),
                    new Genero(0, "Ciencia Ficción"),
                    new Genero(0, "Terror"),
                    new Genero(0, "Misterio"),
                    new Genero(0, "Romántica")
                );
                dbContext.SaveChanges();
            }
        }

        // ---------------- EDITORIALES (5) ----------------
        public static void SeedEditoriales(ApplicationDbContext dbContext) {
            if (!dbContext.Editoriales.Any()) {
                dbContext.AddRange(
                    new Editorial(0, "Minotauro"),
                    new Editorial(0, "Gigamesh"),
                    new Editorial(0, "Salamandra"),
                    new Editorial(0, "Plaza & Janés"),
                    new Editorial(0, "Planeta")
                );
                dbContext.SaveChanges();
            }
        }

        // ---------------- LIBROS (15) ----------------
        // TipoLibro debe tener entre 10 y 50 caracteres
        // Stock > 0 -> salen en Comprar y Reseñar | Stock = 0 -> salen en Reponer
        public static void SeedLibros(ApplicationDbContext dbContext) {
            if (!dbContext.Libros.Any()) {
                var gFantasia  = dbContext.Generos.First(g => g.Nombre == "Fantasía");
                var gCiencia   = dbContext.Generos.First(g => g.Nombre == "Ciencia Ficción");
                var gTerror    = dbContext.Generos.First(g => g.Nombre == "Terror");
                var gMisterio  = dbContext.Generos.First(g => g.Nombre == "Misterio");
                var gRomantica = dbContext.Generos.First(g => g.Nombre == "Romántica");

                var edMinotauro  = dbContext.Editoriales.First(e => e.Nombre == "Minotauro");
                var edGigamesh   = dbContext.Editoriales.First(e => e.Nombre == "Gigamesh");
                var edSalamandra = dbContext.Editoriales.First(e => e.Nombre == "Salamandra");
                var edPlaza      = dbContext.Editoriales.First(e => e.Nombre == "Plaza & Janés");
                var edPlaneta    = dbContext.Editoriales.First(e => e.Nombre == "Planeta");

                string tapaDura = "Edición tapa dura";
                string bolsillo = "Edición de bolsillo";
                string tapaBlanda = "Edición tapa blanda";

                dbContext.AddRange(
                    // Con stock (10)
                    new Libro(0, "El Señor de los Anillos", "J.R.R. Tolkien", new DateTime(1954, 7, 29), 20.50m, 15.00m, 10, tapaDura, 4.9, gFantasia, edMinotauro),
                    new Libro(0, "Dune", "Frank Herbert", new DateTime(1965, 8, 1), 18.90m, 12.50m, 15, bolsillo, 4.8, gCiencia, edGigamesh),
                    new Libro(0, "Harry Potter y la piedra filosofal", "J.K. Rowling", new DateTime(1997, 6, 26), 16.95m, 11.00m, 20, tapaDura, 4.7, gFantasia, edSalamandra),
                    new Libro(0, "Fundación", "Isaac Asimov", new DateTime(1951, 5, 1), 12.95m, 8.50m, 8, bolsillo, 4.5, gCiencia, edGigamesh),
                    new Libro(0, "Asesinato en el Orient Express", "Agatha Christie", new DateTime(1934, 1, 1), 10.90m, 7.00m, 12, bolsillo, 4.4, gMisterio, edPlaneta),
                    new Libro(0, "El nombre del viento", "Patrick Rothfuss", new DateTime(2007, 3, 27), 24.90m, 17.00m, 6, tapaDura, 4.8, gFantasia, edPlaza),
                    new Libro(0, "1984", "George Orwell", new DateTime(1949, 6, 8), 11.95m, 8.00m, 9, bolsillo, 4.7, gCiencia, edPlaneta),
                    new Libro(0, "Juego de tronos", "George R.R. Martin", new DateTime(1996, 8, 1), 22.90m, 15.50m, 7, tapaDura, 4.7, gFantasia, edGigamesh),
                    new Libro(0, "El código Da Vinci", "Dan Brown", new DateTime(2003, 3, 18), 14.90m, 9.50m, 11, tapaBlanda, 4.1, gMisterio, edPlaneta),
                    new Libro(0, "Drácula", "Bram Stoker", new DateTime(1897, 5, 26), 9.95m, 6.00m, 14, bolsillo, 4.3, gTerror, edSalamandra),
                    // Sin stock (5) -> para Reponer
                    new Libro(0, "El Resplandor", "Stephen King", new DateTime(1977, 1, 28), 22.00m, 16.00m, 0, tapaDura, 4.5, gTerror, edPlaza),
                    new Libro(0, "It", "Stephen King", new DateTime(1986, 9, 15), 25.00m, 18.00m, 0, bolsillo, 4.6, gTerror, edPlaza),
                    new Libro(0, "Orgullo y prejuicio", "Jane Austen", new DateTime(1813, 1, 28), 9.95m, 6.00m, 0, bolsillo, 4.3, gRomantica, edSalamandra),
                    new Libro(0, "La sombra del viento", "Carlos Ruiz Zafón", new DateTime(2001, 4, 1), 19.90m, 13.00m, 0, tapaBlanda, 4.7, gMisterio, edPlaneta),
                    new Libro(0, "Bajo la misma estrella", "John Green", new DateTime(2012, 1, 10), 15.90m, 10.00m, 0, tapaBlanda, 4.2, gRomantica, edPlaza)
                );
                dbContext.SaveChanges();
            }
        }

        // ---------------- MÉTODOS DE PAGO ----------------
        public static void SeedMetodosPago(ApplicationDbContext dbContext) {
            if (!dbContext.MetodosPago.Any()) {
                Visa visa = new Visa(DateTime.Now.AddYears(2)) { NumeroTarjeta = "4123456789123456" };
                GooglePay googlePay = new GooglePay("pagos@uclm.es");
                PayPal payPal = new PayPal("600123456");

                dbContext.AddRange(visa, googlePay, payPal);
                dbContext.SaveChanges();
            }
        }

        // ---------------- CU1: COMPRAS (4) ----------------
        public static void SeedCompras(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
            if (!dbContext.Compras.Any()) {
                var cliente = userManager.FindByNameAsync("peter@uclm.es").Result;

                var visa = dbContext.Visas.First();
                var googlePay = dbContext.GooglePays.First();
                var payPal = dbContext.PayPals.First();

                var anillos = dbContext.Libros.First(l => l.Titulo == "El Señor de los Anillos");
                var dune = dbContext.Libros.First(l => l.Titulo == "Dune");
                var harryPotter = dbContext.Libros.First(l => l.Titulo == "Harry Potter y la piedra filosofal");
                var orientExpress = dbContext.Libros.First(l => l.Titulo == "Asesinato en el Orient Express");
                var juegoTronos = dbContext.Libros.First(l => l.Titulo == "Juego de tronos");
                var dracula = dbContext.Libros.First(l => l.Titulo == "Drácula");

                // 1. Compras (PrecioTotal = suma de precio * cantidad)
                var compra1 = new Compra(0, DateTime.Now.AddDays(-20), 97.70m, null, cliente.Id, cliente, visa.Id, visa, new List<CompraItem>());               // 2*20.50 + 3*18.90
                var compra2 = new Compra(0, DateTime.Now.AddDays(-10), 41.00m, "DTO10", cliente.Id, cliente, googlePay.Id, googlePay, new List<CompraItem>());   // 2*20.50
                var compra3 = new Compra(0, DateTime.Now.AddDays(-5), 66.60m, "VERANO25", cliente.Id, cliente, payPal.Id, payPal, new List<CompraItem>());       // 2*16.95 + 3*10.90
                var compra4 = new Compra(0, DateTime.Now.AddDays(-1), 85.60m, null, cliente.Id, cliente, visa.Id, visa, new List<CompraItem>());                 // 2*22.90 + 4*9.95

                dbContext.AddRange(compra1, compra2, compra3, compra4);
                dbContext.SaveChanges();

                // 2. Libros de cada compra (cantidad > 1)
                dbContext.AddRange(
                    new CompraItem(2, anillos.Id, anillos, compra1.Id, compra1),
                    new CompraItem(3, dune.Id, dune, compra1.Id, compra1),
                    new CompraItem(2, anillos.Id, anillos, compra2.Id, compra2),
                    new CompraItem(2, harryPotter.Id, harryPotter, compra3.Id, compra3),
                    new CompraItem(3, orientExpress.Id, orientExpress, compra3.Id, compra3),
                    new CompraItem(2, juegoTronos.Id, juegoTronos, compra4.Id, compra4),
                    new CompraItem(4, dracula.Id, dracula, compra4.Id, compra4)
                );
                dbContext.SaveChanges();
            }
        }

        // ---------------- CU2: REPOSICIONES (4) ----------------
        public static void SeedReposiciones(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
            if (!dbContext.Reposiciones.Any()) {
                var admin = userManager.FindByNameAsync("elena@uclm.es").Result;

                var visa = dbContext.Visas.First();
                var googlePay = dbContext.GooglePays.First();
                var payPal = dbContext.PayPals.First();

                var resplandor = dbContext.Libros.First(l => l.Titulo == "El Resplandor");
                var it = dbContext.Libros.First(l => l.Titulo == "It");
                var orgullo = dbContext.Libros.First(l => l.Titulo == "Orgullo y prejuicio");
                var sombra = dbContext.Libros.First(l => l.Titulo == "La sombra del viento");
                var estrella = dbContext.Libros.First(l => l.Titulo == "Bajo la misma estrella");

                // 1. Reposiciones (PrecioTotal = suma de precioReposicion * cantidad, comentario de 20 a 100 caracteres)
                var repo1 = new Reposicion(0, DateTime.Now.AddDays(-15), 520.00, "Reposición urgente de libros sin stock");      // 10*16 + 20*18
                repo1.ApplicationUserId = admin.Id;
                repo1.Usuario = admin;
                repo1.MetodoPagoId = payPal.Id;
                repo1.MetodoPago = payPal;

                var repo2 = new Reposicion(0, DateTime.Now.AddDays(-8), 82.00, "Pedido mensual de clásicos agotados");          // 5*6 + 4*13
                repo2.ApplicationUserId = admin.Id;
                repo2.Usuario = admin;
                repo2.MetodoPagoId = visa.Id;
                repo2.MetodoPago = visa;

                var repo3 = new Reposicion(0, DateTime.Now.AddDays(-4), 60.00, "Reposición de novela romántica juvenil");       // 6*10
                repo3.ApplicationUserId = admin.Id;
                repo3.Usuario = admin;
                repo3.MetodoPagoId = googlePay.Id;
                repo3.MetodoPago = googlePay;

                var repo4 = new Reposicion(0, DateTime.Now.AddDays(-1), 84.00, "Segundo pedido de terror por alta demanda");    // 3*16 + 2*18
                repo4.ApplicationUserId = admin.Id;
                repo4.Usuario = admin;
                repo4.MetodoPagoId = visa.Id;
                repo4.MetodoPago = visa;

                dbContext.AddRange(repo1, repo2, repo3, repo4);
                dbContext.SaveChanges();

                // 2. Libros de cada reposición (cantidad > 1)
                dbContext.AddRange(
                    new ReposicionItem(0, 10, resplandor.Id, repo1.Id),
                    new ReposicionItem(0, 20, it.Id, repo1.Id),
                    new ReposicionItem(0, 5, orgullo.Id, repo2.Id),
                    new ReposicionItem(0, 4, sombra.Id, repo2.Id),
                    new ReposicionItem(0, 6, estrella.Id, repo3.Id),
                    new ReposicionItem(0, 3, resplandor.Id, repo4.Id),
                    new ReposicionItem(0, 2, it.Id, repo4.Id)
                );
                dbContext.SaveChanges();
            }
        }

        // ---------------- CU4: RESEÑAS (4) ----------------
        public static void SeedResenas(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
            if (!dbContext.Resenas.Any()) {
                var cliente = userManager.FindByNameAsync("peter@uclm.es").Result;

                var anillos = dbContext.Libros.First(l => l.Titulo == "El Señor de los Anillos");
                var dune = dbContext.Libros.First(l => l.Titulo == "Dune");
                var harryPotter = dbContext.Libros.First(l => l.Titulo == "Harry Potter y la piedra filosofal");
                var orientExpress = dbContext.Libros.First(l => l.Titulo == "Asesinato en el Orient Express");
                var fundacion = dbContext.Libros.First(l => l.Titulo == "Fundación");
                var orwell = dbContext.Libros.First(l => l.Titulo == "1984");
                var nombreViento = dbContext.Libros.First(l => l.Titulo == "El nombre del viento");
                var juegoTronos = dbContext.Libros.First(l => l.Titulo == "Juego de tronos");

                // 1. Reseñas (título de 10 a 20 caracteres)
                var resena1 = new Resena(0, DateTime.Now.AddDays(-12), cliente.Id) { TituloResena = "Grandes clásicos" };
                var resena2 = new Resena(0, DateTime.Now.AddDays(-9), cliente.Id) { TituloResena = "Lecturas de verano" };
                var resena3 = new Resena(0, DateTime.Now.AddDays(-6), cliente.Id) { TituloResena = "Ciencia ficción top" };
                var resena4 = new Resena(0, DateTime.Now.AddDays(-2), cliente.Id) { TituloResena = "Fantasía épica pura" };

                dbContext.AddRange(resena1, resena2, resena3, resena4);
                dbContext.SaveChanges();

                // 2. Libros reseñados (descripción opcional de 20 a 100 caracteres, calificación de 1 a 5)
                dbContext.AddRange(
                    new ResenaItem(anillos.Id, resena1.Id, "Una obra maestra que todo el mundo debe leer.", 5),
                    new ResenaItem(dune.Id, resena1.Id, "Muy buen libro de ciencia ficción, me encantó.", 4),
                    new ResenaItem(harryPotter.Id, resena2.Id, "Mágico de principio a fin, ideal para todas las edades.", 5),
                    new ResenaItem(orientExpress.Id, resena2.Id, null, 4),
                    new ResenaItem(fundacion.Id, resena3.Id, "Una saga fundamental para entender la ciencia ficción.", 4),
                    new ResenaItem(orwell.Id, resena3.Id, "Inquietante y más actual que nunca, muy recomendable.", 5),
                    new ResenaItem(nombreViento.Id, resena4.Id, "La mejor narrativa fantástica que he leído en años.", 5),
                    new ResenaItem(juegoTronos.Id, resena4.Id, null, 4)
                );
                dbContext.SaveChanges();
            }
        }
    }
}