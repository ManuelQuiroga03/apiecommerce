using ApiEcommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiEcommerce.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context, bool clearFirst = false)
        {
            if (clearFirst)
            {
                // Limpiar tablas para un arranque fresco
                if (await context.Products.AnyAsync())
                {
                    context.Products.RemoveRange(context.Products);
                    await context.SaveChangesAsync();
                }

                if (await context.Categories.AnyAsync())
                {
                    context.Categories.RemoveRange(context.Categories);
                    await context.SaveChangesAsync();
                }

                if (await context.UserRoles.AnyAsync())
                {
                    context.UserRoles.RemoveRange(context.UserRoles);
                    await context.SaveChangesAsync();
                }

                if (await context.ApplicationUsers.AnyAsync())
                {
                    context.ApplicationUsers.RemoveRange(context.ApplicationUsers);
                    await context.SaveChangesAsync();
                }

                if (await context.Roles.AnyAsync())
                {
                    context.Roles.RemoveRange(context.Roles);
                    await context.SaveChangesAsync();
                }
            }
            if (!await context.Roles.AnyAsync())
            {
                await context.Roles.AddRangeAsync(
                    new IdentityRole { Id = "role-admin-001", Name = "Admin", NormalizedName = "ADMIN" },
                    new IdentityRole { Id = "role-user-002", Name = "User", NormalizedName = "USER" }
                );
                await context.SaveChangesAsync();
            }

            // 2. Seeding de Usuarios Iniciales
            if (!await context.ApplicationUsers.AnyAsync())
            {
                var hasher = new PasswordHasher<ApplicationUser>();

                // Usuario Administrador
                var adminUser = new ApplicationUser
                {
                    Id = "usr-admin-001",
                    UserName = "admin@techstore.com",
                    NormalizedUserName = "ADMIN@TECHSTORE.COM",
                    Email = "admin@techstore.com",
                    NormalizedEmail = "ADMIN@TECHSTORE.COM",
                    EmailConfirmed = true,
                    Name = "TechStore Administrator",
                    SecurityStamp = Guid.NewGuid().ToString("D")
                };
                adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

                // Usuario Cliente
                var customerUser = new ApplicationUser
                {
                    Id = "usr-client-002",
                    UserName = "cliente@techstore.com",
                    NormalizedUserName = "CLIENTE@TECHSTORE.COM",
                    Email = "cliente@techstore.com",
                    NormalizedEmail = "CLIENTE@TECHSTORE.COM",
                    EmailConfirmed = true,
                    Name = "Carlos Mendoza",
                    SecurityStamp = Guid.NewGuid().ToString("D")
                };
                customerUser.PasswordHash = hasher.HashPassword(customerUser, "User123!");

                await context.ApplicationUsers.AddRangeAsync(adminUser, customerUser);
                await context.SaveChangesAsync();

                // Asignación de Roles a Usuarios
                await context.UserRoles.AddRangeAsync(
                    new IdentityUserRole<string> { UserId = "usr-admin-001", RoleId = "role-admin-001" },
                    new IdentityUserRole<string> { UserId = "usr-client-002", RoleId = "role-user-002" }
                );
                await context.SaveChangesAsync();
            }

            // 3. Seeding de Categorías
            if (!await context.Categories.AnyAsync())
            {
                await context.Categories.AddRangeAsync(
                    new Category { Name = "Laptops y Computadoras", CreationDate = DateTime.Now },
                    new Category { Name = "Periféricos y Accesorios", CreationDate = DateTime.Now },
                    new Category { Name = "Monitores y Pantallas", CreationDate = DateTime.Now },
                    new Category { Name = "Audio y Streaming", CreationDate = DateTime.Now },
                    new Category { Name = "Consolas y Gaming", CreationDate = DateTime.Now },
                    new Category { Name = "Almacenamiento y Gadgets", CreationDate = DateTime.Now }
                );
                await context.SaveChangesAsync();
            }

            // 4. Seeding de Productos
            if (!await context.Products.AnyAsync())
            {
                var catLaptops = await context.Categories.FirstAsync(c => c.Name == "Laptops y Computadoras");
                var catPerifericos = await context.Categories.FirstAsync(c => c.Name == "Periféricos y Accesorios");
                var catMonitores = await context.Categories.FirstAsync(c => c.Name == "Monitores y Pantallas");
                var catAudio = await context.Categories.FirstAsync(c => c.Name == "Audio y Streaming");
                var catGaming = await context.Categories.FirstAsync(c => c.Name == "Consolas y Gaming");
                var catGadgets = await context.Categories.FirstAsync(c => c.Name == "Almacenamiento y Gadgets");

                var products = new List<Product>
                {
                    // === LAPTOPS ===
                    new Product
                    {
                        Name = "MacBook Pro 16\" M3 Max",
                        Description = "Chip Apple M3 Max (CPU 14 núcleos, GPU 30 núcleos), 36GB RAM Unificada, 1TB SSD, Pantalla Liquid Retina XDR.",
                        Price = 2499.99m,
                        SKU = "LAP-APPL-MBP16-M3",
                        Stock = 12,
                        CategoryId = catLaptops.Id,
                        Category = catLaptops,
                        ImgUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Dell XPS 15 OLED Touch",
                        Description = "Intel Core i7-13700H, NVIDIA RTX 4060, 32GB DDR5, 1TB NVMe, Pantalla 3.5K OLED Táctil 400 nits.",
                        Price = 1899.99m,
                        SKU = "LAP-DELL-XPS15-OLED",
                        Stock = 15,
                        CategoryId = catLaptops.Id,
                        Category = catLaptops,
                        ImgUrl = "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Laptop Gamer Asus ROG Strix Scar 17",
                        Description = "AMD Ryzen 9 7945HX, RTX 4080 12GB, 32GB RAM, 1TB SSD, Pantalla QHD 240Hz 3ms, Teclado mecánico RGB por tecla.",
                        Price = 2199.50m,
                        SKU = "LAP-ASUS-ROG17-4080",
                        Stock = 6,
                        CategoryId = catLaptops.Id,
                        Category = catLaptops,
                        ImgUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Lenovo ThinkPad X1 Carbon Gen 11",
                        Description = "Ultrabook empresarial ultra liviana (1.12 kg), Intel Core i7 vPro, 16GB LPDDR5, 512GB SSD, Batería hasta 15 horas.",
                        Price = 1450.00m,
                        SKU = "LAP-LENO-X1C11-PRO",
                        Stock = 18,
                        CategoryId = catLaptops.Id,
                        Category = catLaptops,
                        ImgUrl = "https://images.unsplash.com/photo-1589561084283-930aa7b1ce50?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },

                    // === PERIFÉRICOS ===
                    new Product
                    {
                        Name = "Mouse Ergonómico Logitech MX Master 3S",
                        Description = "Sensor Darkfield 8000 DPI, Clics silenciosos, Rueda MagSpeed electromagnética, Conectividad multi-dispositivo Bluetooth y Bolt.",
                        Price = 99.99m,
                        SKU = "PER-LOGI-MXM3S-GRA",
                        Stock = 45,
                        CategoryId = catPerifericos.Id,
                        Category = catPerifericos,
                        ImgUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Teclado Mecánico Inalámbrico Keychron Q1 Pro",
                        Description = "Estructura de aluminio mecanizado CNC, Switches Gateron Jupiter Red Hot-Swappable, Conectividad Bluetooth 5.1 y cable tipo C.",
                        Price = 199.00m,
                        SKU = "PER-KEYC-Q1PRO-RED",
                        Stock = 20,
                        CategoryId = catPerifericos.Id,
                        Category = catPerifericos,
                        ImgUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Mouse Gamer Razer DeathAdder V3 Pro",
                        Description = "Sensor óptico Focus Pro 30K DPI, Switches ópticos Gen-3, Ultra ligero de 63 gramos, Tecnología inalámbrica HyperSpeed.",
                        Price = 149.99m,
                        SKU = "PER-RAZE-DAV3P-WHT",
                        Stock = 30,
                        CategoryId = catPerifericos.Id,
                        Category = catPerifericos,
                        ImgUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Teclado Logitech MX Keys S",
                        Description = "Teclado de perfil bajo para programadores, Teclas cóncavas con retroiluminación inteligente de proximidad, Multi-OS.",
                        Price = 109.99m,
                        SKU = "PER-LOGI-MXKS-BLK",
                        Stock = 25,
                        CategoryId = catPerifericos.Id,
                        Category = catPerifericos,
                        ImgUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },

                    // === MONITORES ===
                    new Product
                    {
                        Name = "Monitor Curvo Samsung Odyssey G7 32\"",
                        Description = "Resolución WQHD (2560x1440), Curvatura 1000R, Tasa de refresco 240Hz, 1ms tiempo de respuesta, HDR600 y G-Sync Compatible.",
                        Price = 699.99m,
                        SKU = "MON-SAMS-ODG7-32",
                        Stock = 10,
                        CategoryId = catMonitores.Id,
                        Category = catMonitores,
                        ImgUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Monitor Profesional LG UltraFine 27\" 4K",
                        Description = "Panel IPS Nano Color DCI-P3 98%, Resolución 3840x2160, Conexión USB-C con carga de 90W, Soporte ergonómico ajustable.",
                        Price = 449.99m,
                        SKU = "MON-LG-27UF4K-IPS",
                        Stock = 2, // Stock bajo para alertas de UI
                        CategoryId = catMonitores.Id,
                        Category = catMonitores,
                        ImgUrl = "https://images.unsplash.com/photo-1551645120-d70bfe84c826?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Monitor Gamer Asus TUF Gaming 24.5\" 165Hz",
                        Description = "Panel Fast IPS Full HD (1920x1080), 165Hz (Overclocked), 0.5ms MPRT, Extreme Low Motion Blur, FreeSync Premium.",
                        Price = 189.99m,
                        SKU = "MON-ASUS-TUF24-165",
                        Stock = 35,
                        CategoryId = catMonitores.Id,
                        Category = catMonitores,
                        ImgUrl = "https://images.unsplash.com/photo-1547082299-de196ea013d6?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },

                    // === AUDIO Y STREAMING ===
                    new Product
                    {
                        Name = "Auriculares Sony WH-1000XM5 Wireless",
                        Description = "Cancelación de ruido activa líder en la industria, Procesador V1 integrado, 8 micrófonos, 30 horas de batería y audio Hi-Res LDAC.",
                        Price = 399.99m,
                        SKU = "AUD-SONY-XM5-SLV",
                        Stock = 28,
                        CategoryId = catAudio.Id,
                        Category = catAudio,
                        ImgUrl = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Auriculares In-Ear Apple AirPods Pro 2da Gen",
                        Description = "Chip H2 con cancelación activa de ruido 2x superior, Modo ambiente adaptable, Audio espacial personalizado, Estuche MagSafe USB-C.",
                        Price = 249.00m,
                        SKU = "AUD-APPL-APP2-USBC",
                        Stock = 50,
                        CategoryId = catAudio.Id,
                        Category = catAudio,
                        ImgUrl = "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Micrófono Dinámico Shure MV7 USB/XLR",
                        Description = "Tecnología de aislamiento de voz profesional, Salidas duales USB y XLR, Panel táctil integrado para ganancia y monitoreo.",
                        Price = 249.99m,
                        SKU = "AUD-SHUR-MV7-POD",
                        Stock = 14,
                        CategoryId = catAudio.Id,
                        Category = catAudio,
                        ImgUrl = "https://images.unsplash.com/photo-1583394838336-acd977736f90?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Webcam Elgato Facecam Pro 4K60",
                        Description = "Primera cámara web del mundo con captura real 4K a 60 fps, Sensor Sony STARVIS, Lente Prime Autofocus f/2.0.",
                        Price = 299.99m,
                        SKU = "AUD-ELGA-FCAM4K-PRO",
                        Stock = 8,
                        CategoryId = catAudio.Id,
                        Category = catAudio,
                        ImgUrl = "https://images.unsplash.com/photo-1587826080692-f439cd0b70da?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },

                    // === CONSOLAS Y GAMING ===
                    new Product
                    {
                        Name = "Consola PlayStation 5 Slim 1TB",
                        Description = "Edición con lector de discos Ultra HD Blu-ray, SSD ultrarrápido de 1TB, Ray Tracing, Audio 3D Tempest y control DualSense incluido.",
                        Price = 499.99m,
                        SKU = "GAM-SONY-PS5-SLIM",
                        Stock = 22,
                        CategoryId = catGaming.Id,
                        Category = catGaming,
                        ImgUrl = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Consola Xbox Series X 1TB",
                        Description = "La Xbox más potente de la historia: 12 Teraflops de potencia gráfica, Juegos en 4K real hasta 120 FPS, 1TB SSD NVMe Custom.",
                        Price = 499.99m,
                        SKU = "GAM-MSFT-XBX-1TB",
                        Stock = 16,
                        CategoryId = catGaming.Id,
                        Category = catGaming,
                        ImgUrl = "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Control Inalámbrico DualSense Midnight Black",
                        Description = "Control oficial para PS5 con gatillos adaptativos dinámicos, retroalimentación háptica inmersiva y micrófono integrado.",
                        Price = 69.99m,
                        SKU = "GAM-SONY-DS5-MBLK",
                        Stock = 40,
                        CategoryId = catGaming.Id,
                        Category = catGaming,
                        ImgUrl = "https://images.unsplash.com/photo-1592840496694-26d035b52b48?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Control Inalámbrico Xbox Robot White",
                        Description = "Control ergonómico con agarre texturizado, Cruceta híbrida, Botón Compartir dedicado y compatibilidad con Xbox, PC y Android/iOS.",
                        Price = 59.99m,
                        SKU = "GAM-MSFT-XBC-WHT",
                        Stock = 35,
                        CategoryId = catGaming.Id,
                        Category = catGaming,
                        ImgUrl = "https://images.unsplash.com/photo-1600080972464-8e5f35f63d08?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Silla Gamer Secretlab Titan Evo",
                        Description = "Tapicería de piel sintética híbrida PRIME 2.0, Soporte lumbar integrado L-ADAPT en 4 direcciones, Reposabrazos magnéticos 4D.",
                        Price = 549.00m,
                        SKU = "GAM-SECR-TTEVO-BLK",
                        Stock = 5,
                        CategoryId = catGaming.Id,
                        Category = catGaming,
                        ImgUrl = "https://images.unsplash.com/photo-1598550476439-6847785fcea6?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Silla Ergonómica Herman Miller Aeron Remastered",
                        Description = "Referente mundial en ergonomía: Malla Pellicle 8Z que distribuye la presión, Mecanismo de inclinación armónica y soporte PostureFit SL.",
                        Price = 1295.00m,
                        SKU = "GAM-HMIL-AERON-GRY",
                        Stock = 3,
                        CategoryId = catGaming.Id,
                        Category = catGaming,
                        ImgUrl = "https://images.unsplash.com/photo-1580481077190-7361296d0658?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },

                    // === ALMACENAMIENTO Y GADGETS ===
                    new Product
                    {
                        Name = "SSD Portable SanDisk Extreme 2TB",
                        Description = "Velocidad de lectura hasta 1050 MB/s, Resistencia al agua y polvo IP55, Conexión USB 3.2 Gen 2 Type-C, Carcasa de goma resistente a caídas.",
                        Price = 159.99m,
                        SKU = "ALM-SAND-EXT2-2TB",
                        Stock = 40,
                        CategoryId = catGadgets.Id,
                        Category = catGadgets,
                        ImgUrl = "https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "SSD NVMe M.2 Samsung 990 PRO 2TB con Disipador",
                        Description = "Velocidades secuenciales de lectura/escritura hasta 7,450/6,900 MB/s con PCIe 4.0, Compatible con PlayStation 5 y PC Gaming.",
                        Price = 189.99m,
                        SKU = "ALM-SAMS-990P-2TB",
                        Stock = 25,
                        CategoryId = catGadgets.Id,
                        Category = catGadgets,
                        ImgUrl = "https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Consola Portátil Steam Deck OLED 512GB",
                        Description = "Pantalla HDR OLED de 7.4\" a 90Hz, APU personalizada AMD Zen 2 + RDNA 2, Batería de 50Wh, Wi-Fi 6E y estuche de transporte incluido.",
                        Price = 549.00m,
                        SKU = "GAD-VALV-SDEC-OLED",
                        Stock = 12,
                        CategoryId = catGadgets.Id,
                        Category = catGadgets,
                        ImgUrl = "https://images.unsplash.com/photo-1550745165-9bc0b252726f?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Apple iPad Pro 11\" M2 Wi-Fi 128GB",
                        Description = "Pantalla Liquid Retina con ProMotion y True Tone, Chip Apple M2, Compatible con Apple Pencil (2.ª gen) y Magic Keyboard.",
                        Price = 799.00m,
                        SKU = "GAD-APPL-IPP11-M2",
                        Stock = 0, // Stock agotado para probar lógica de frontend
                        CategoryId = catGadgets.Id,
                        Category = catGadgets,
                        ImgUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?auto=format&fit=crop&w=600&q=80",
                        CreationDate = DateTime.Now
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
