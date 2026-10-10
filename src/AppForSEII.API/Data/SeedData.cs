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
                SeedLibros(dbContext);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the Libros in the Database.");
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
                ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "Password1234%");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    userManager.AddToRoleAsync(user, roles[0]).Wait();
                }
            }

            if (userManager.FindByNameAsync("peter@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "OtherPass12$");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    userManager.AddToRoleAsync(user, roles[2]).Wait();
                }
            }
        }

        public static void SeedLibros(ApplicationDbContext dbContext) {
          
            if (!dbContext.Libros.Any()) { 
                
               
               Genero genero1 = new Genero { Nombre = "Fantasía" };
                Genero genero2 = new Genero { Nombre = "Ciencia Ficción" };
                Genero genero3 = new Genero { Nombre = "Misterio" };
                Genero genero4 = new Genero { Nombre = "Terror" };

                // Editoriales
                Editorial editorial1 = new Editorial { Nombre = "Minotauro" };
                Editorial editorial2 = new Editorial { Nombre = "Gigamesh" };
                Editorial editorial3 = new Editorial { Nombre = "Salamandra" };
                Editorial editorial4 = new Editorial { Nombre = "Plaza & Janés" };

                // Libros
                Libro libro1 = new Libro {
                    Titulo = "El Señor de los Anillos",
                    Autor = "J.R.R. Tolkien",
                    FechaLanzamiento = new DateTime(1954, 7, 29),
                    PrecioCompra = 20.50m, 
                    Stock = 10,
                    PrecioReposicion = 15.00m, 
                    TipoLibro = "Tapa dura", 
                    CalificacionMedia = 4.9,
                    Genero = genero1,
                    Editorial = editorial1
                };

                Libro libro2 = new Libro {
                    Titulo = "Dune",
                    Autor = "Frank Herbert",
                    FechaLanzamiento = new DateTime(1965, 8, 1),
                    PrecioCompra = 18.90m, 
                    Stock = 15,
                    PrecioReposicion = 12.50m, 
                    TipoLibro = "Bolsillo", 
                    CalificacionMedia = 4.8,
                    Genero = genero2,
                    Editorial = editorial2
                };

                Libro libro3 = new Libro {
                    Titulo = "Harry Potter y la Piedra Filosofal",
                    Autor = "J.K. Rowling",
                    FechaLanzamiento = new DateTime(1997, 6, 26),
                    PrecioCompra = 15.00m, 
                    Stock = 20,
                    PrecioReposicion = 10.00m, 
                    TipoLibro = "Tapa blanda", 
                    CalificacionMedia = 4.7,
                    Genero = genero1,
                    Editorial = editorial3
                };

                Libro libro4 = new Libro {
                    Titulo = "El Resplandor",
                    Autor = "Stephen King",
                    FechaLanzamiento = new DateTime(1977, 1, 28),
                    PrecioCompra = 22.00m, 
                    Stock = 5,
                    PrecioReposicion = 16.00m, 
                    TipoLibro = "Tapa dura", 
                    CalificacionMedia = 4.5,
                    Genero = genero4,
                    Editorial = editorial4
                };

                Libro libro5 = new Libro {
                    Titulo = "1984",
                    Autor = "George Orwell",
                    FechaLanzamiento = new DateTime(1949, 6, 8),
                    PrecioCompra = 14.50m, 
                    Stock = 30,
                    PrecioReposicion = 9.00m, 
                    TipoLibro = "Bolsillo", 
                    CalificacionMedia = 4.6,
                    Genero = genero2,
                    Editorial = editorial4
                };

                Libro libro6 = new Libro {
                    Titulo = "Diez negritos",
                    Autor = "Agatha Christie",
                    FechaLanzamiento = new DateTime(1939, 11, 6),
                    PrecioCompra = 12.00m, 
                    Stock = 12,
                    PrecioReposicion = 8.00m, 
                    TipoLibro = "Bolsillo", 
                    CalificacionMedia = 4.4,
                    Genero = genero3,
                    Editorial = editorial3
                };

                Libro libro7 = new Libro {
                    Titulo = "Drácula",
                    Autor = "Bram Stoker",
                    FechaLanzamiento = new DateTime(1897, 5, 26),
                    PrecioCompra = 16.50m, 
                    Stock = 8,
                    PrecioReposicion = 11.00m, 
                    TipoLibro = "Tapa blanda", 
                    CalificacionMedia = 4.3,
                    Genero = genero4,
                    Editorial = editorial1
                };

                Libro libro8 = new Libro {
                    Titulo = "El Juego de Ender",
                    Autor = "Orson Scott Card",
                    FechaLanzamiento = new DateTime(1985, 1, 15),
                    PrecioCompra = 19.00m, 
                    Stock = 25,
                    PrecioReposicion = 14.00m, 
                    TipoLibro = "Tapa dura", 
                    CalificacionMedia = 4.7,
                    Genero = genero2,
                    Editorial = editorial2
                };

                dbContext.AddRange(genero1, genero2, genero3, genero4);
                dbContext.AddRange(editorial1, editorial2, editorial3, editorial4);
                dbContext.AddRange(libro1, libro2, libro3, libro4, libro5, libro6, libro7, libro8);

                dbContext.SaveChanges();
            }
        }
    }
}