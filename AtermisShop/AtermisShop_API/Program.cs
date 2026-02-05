
using AtermisShop.Application;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Infrastructure;

using AtermisShop.Domain.Users;
using AtermisShop.Infrastructure.Persistence;
using AtermisShop.Infrastructure.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Npgsql;

namespace AtermisShop_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            
            // Configure logging for better error visibility
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Add services to the container.
            try
            {
                builder.Services.AddHttpContextAccessor();
                builder.Services
                    .AddApplication()
                    .AddInfrastructure(builder.Configuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during service registration: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                        {
                            // Allow localhost for development
                            if (origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:"))
                                return true;
                            
                            // Allow main Vercel production domain
                            if (origin == "https://custom-bracelet-with-gps-website.vercel.app")
                                return true;
                            
                            // Allow all Vercel preview deployments (*.vercel.app)
                            if (origin.EndsWith(".vercel.app") && origin.StartsWith("https://"))
                                return true;

                            // Allow Artemis production frontend domain
                            if (origin == "https://www.artemis.io.vn")
                                return true;
                            
                            return false;
                        })
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials(); // Required for cookies/auth headers
                });
            });

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.WriteIndented = true;
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Artemis Shop API",
                    Version = "v1",
                    Description = "API for Artemis GPS Bracelet E-commerce Platform"
                });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Include XML comments if available
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Ignore schema properties that might cause issues
                c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
                
                // Map types correctly
                c.MapType<Guid>(() => new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Format = "uuid"
                });

                // Configure Swagger to use camelCase for property names
                c.UseAllOfToExtendReferenceSchemas();
                c.SupportNonNullableReferenceTypes();
                
                // Use camelCase for property names in Swagger
                c.DescribeAllParametersInCamelCase();
                c.SchemaFilter<Swagger.CamelCaseSchemaFilter>();
                
                // Add example values to all operations
                c.OperationFilter<Swagger.ExampleOperationFilter>();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // Enable Swagger for both Development and Production
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Artemis Shop API v1");
                c.RoutePrefix = "swagger";
                // Disable try it out in production for better security (optional)
                if (!app.Environment.IsDevelopment())
                {
                    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                }
            });

            app.UseHttpsRedirection();

            // Ensure wwwroot directory exists (for other static files if needed)
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!Directory.Exists(wwwrootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
                Console.WriteLine("Created wwwroot directory");
            }

            // Configure uploads directory. Use environment variable or default to local 'uploads'
            var volumeUploadsPath = builder.Configuration["Storage:UploadsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            var dataModelsPath = Path.Combine(volumeUploadsPath, "models3d");
            var dataNewsPath = Path.Combine(volumeUploadsPath, "news");
            
            // Create directories if they don't exist
            try
            {
                if (!Directory.Exists(dataModelsPath))
                {
                    Directory.CreateDirectory(dataModelsPath);
                    Console.WriteLine($"Created uploads directory: {dataModelsPath}");
                }
                
                if (!Directory.Exists(dataNewsPath))
                {
                    Directory.CreateDirectory(dataNewsPath);
                    Console.WriteLine($"Created news uploads directory: {dataNewsPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not create upload directories: {ex.Message}");
            }

            // Enable static files for wwwroot (if needed for other static files)
            app.UseStaticFiles();

            // Map content-type for glb/gltf and images
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".glb"] = "model/gltf-binary";
            contentTypeProvider.Mappings[".gltf"] = "model/gltf+json";
            contentTypeProvider.Mappings[".jpg"] = "image/jpeg";
            contentTypeProvider.Mappings[".jpeg"] = "image/jpeg";
            contentTypeProvider.Mappings[".png"] = "image/png";
            contentTypeProvider.Mappings[".gif"] = "image/gif";
            contentTypeProvider.Mappings[".webp"] = "image/webp";

            // Serve files from /data/uploads as /uploads/...
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(volumeUploadsPath),
                RequestPath = "/uploads",
                ContentTypeProvider = contentTypeProvider,
                // Nếu sau này có extension lạ, có thể bật thêm dòng dưới:
                // ServeUnknownFileTypes = true,
                // DefaultContentType = "application/octet-stream",
                OnPrepareResponse = ctx =>
                {
                    // Allow CORS for static files
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                }
            });

            // Enable CORS - must be before UseAuthentication and UseAuthorization
            app.UseCors("AllowFrontend");

            app.UseMiddleware<Middleware.GlobalExceptionHandlerMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            // Add root route handler for Fly.io deployment check
            app.MapGet("/", () => Results.Json(new 
            { 
                message = "Artemis Shop API is running", 
                version = "v1",
                health = "/api/health",
                swagger = "/swagger"
            }));

            app.MapControllers();

            // Apply migrations and seed admin user on startup
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    // Check if database is available
                    logger.LogInformation("Checking database connection...");
                    var canConnect = await context.Database.CanConnectAsync();
                    
                    if (!canConnect)
                    {
                        logger.LogWarning("Cannot connect to database. Application will start without applying migrations.");
                        logger.LogWarning("Please check your connection string in appsettings.json");
                        logger.LogWarning("The application will continue to run, but database operations may fail.");
                    }
                    else
                    {
                        // Apply pending migrations
                        logger.LogInformation("Applying database migrations...");
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Database migrations applied successfully.");

                        // Seed admin user
                        logger.LogInformation("Seeding admin user...");
                        await DatabaseSeeder.SeedAdminUserAsync(userService);
                        logger.LogInformation("Admin user seeding completed.");

                        // Seed customer user
                        logger.LogInformation("Seeding customer user...");
                        await DatabaseSeeder.SeedCustomerUserAsync(userService);
                        logger.LogInformation("Customer user seeding completed.");
                    }
                }
                catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "28P01")
                {
                    logger.LogError(pgEx, "Database authentication failed. Please check your connection string credentials in appsettings.json");
                    logger.LogError("The application will continue to run, but database operations will fail until the connection is fixed.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while setting up the database. The application will continue to run.");
                    // Don't throw - allow app to continue running
                }
            }

            await app.RunAsync();
        }
    }
}
