
using gemini_test.Services;
using Hangfire;
using Hangfire.PostgreSql;
using System.Threading.RateLimiting;

namespace light_quiz_api
{
    public static class ConfigureServices
    {
        public static void AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();

            builder.AddSwagger();
            builder.AddDatabase();

            builder.AddHangfireServices();

            //// Azure Blob Storage
            //var blobConnectionString = builder.Configuration.GetConnectionString("AzureStorage");

            //builder.Services.AddSingleton(new BlobServiceClient(blobConnectionString));
            //builder.Services.AddSingleton<IBlobService, FileBlobService>();

            builder.Services.AddScoped<IGradingService, GradingService>();
            builder.Services.AddScoped<StudentSubmissionService>();
            builder.Services.AddScoped<ShortCodeGeneratorService>();
            builder.Services.AddScoped<IUserAvatarService, UserAvatarService>();

            builder.AddGeminiServices();

            builder.AddJwtAuthentication();
        }

        private static void AddSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Light-Quiz Api",
                    Version = "v1",
                    Description = "Simple Quiz platform",
                    Contact = new OpenApiContact
                    {
                        Name = "Fares Mahmoud",
                        Email = "faresma7moud1@gmail.com"
                    }
                });

                options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));
                options.InferSecuritySchemes();

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        private static void AddDatabase(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (builder.Environment.IsDevelopment())
            {
                connectionString = builder.Configuration.GetConnectionString("DevConnection");
            }

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseSnakeCaseNamingConvention();
            });
        }

        private static void AddHangfireServices(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (builder.Environment.IsDevelopment())
            {
                connectionString = builder.Configuration.GetConnectionString("DevConnection");
            }

            builder.Services.AddHangfire(config =>
                config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString),
                new Hangfire.PostgreSql.PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",
                }));

            builder.Services.AddHangfireServer();

        }

        private static void AddGeminiServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<RateLimiter>(sp =>
            {
                var options = new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 100 // Queue Up to 100 reqs
                };
                return new FixedWindowRateLimiter(options);
            });

            builder.Services.Configure<GeminiSettings>(
              builder.Configuration.GetSection(GeminiSettings.SectionName));

            builder.Services.AddScoped<IGeminiService, GeminiService>();
        }

        private static void AddJwtAuthentication(this WebApplicationBuilder builder)
        {
            // JWT
            builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(o =>
                {
                    o.SaveToken = false;
                    o.RequireHttpsMetadata = false;

                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidAudience = builder.Configuration["JWT:Audience"],
                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
                    };
                });

            builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

            builder.Services.AddScoped<IAuthService, AuthService>();
        }
    }
}
