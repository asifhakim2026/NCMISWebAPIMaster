using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NCMISAPI.Configuration;
using NCMISAPI.Data;
using NCMISAPI.DTOs;
using NCMISAPI.Helpers;
using NCMISAPI.Logging;
using NCMISAPI.Middleware;
using NCMISAPI.Services;
using NCMISAPI.Swagger;
using Serilog;

SerilogConfiguration.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddNcmisSerilog();

    builder.Services.AddControllers();

    builder.Services.AddDbContext<NcmisDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    );

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
    builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection(FileStorageSettings.SectionName));
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IFeeRemissionService, FeeRemissionService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ErrorLogHelper>();
    builder.Services.AddScoped<PersonHelper>();
    builder.Services.AddScoped<IPersonService, PersonService>();
    builder.Services.AddScoped<IPersonAddressService, PersonAddressService>();
    builder.Services.AddScoped<IPersonSurveyService, PersonSurveyService>();
    builder.Services.AddScoped<IPersonFinanceService, PersonFinanceService>();
    builder.Services.AddScoped<IPersonEducationService, PersonEducationService>();
    builder.Services.AddScoped<IPersonEmploymentService, PersonEmploymentService>();
    builder.Services.AddScoped<IPersonDocumentService, PersonDocumentService>();
    builder.Services.AddScoped<IPersonFieldWorkService, PersonFieldWorkService>();
    builder.Services.AddScoped<IPersonDeceasedService, PersonDeceasedService>();
    builder.Services.AddScoped<IFileStorageService, FileStorageService>();
    builder.Services.AddScoped<ISkillService, SkillService>();
    builder.Services.AddScoped<IGeneralSetupService, GeneralSetupService>();
    builder.Services.AddScoped<IHouseHoldSurveyService, HouseHoldSurveyService>();
    builder.Services.AddScoped<IAdditionalSupportService, AdditionalSupportService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("MobilePolicy", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
        ?? throw new InvalidOperationException("Jwt configuration section is missing.");
    if (string.IsNullOrWhiteSpace(jwtSettings.Key) || jwtSettings.Key.Length < 32)
        throw new InvalidOperationException(
            "Jwt:Key must be configured and at least 32 characters. Set it in appsettings or environment.");

    var signingKey = JwtSigning.CreateSecurityKey(jwtSettings.Key);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Development: put real IdentityModel messages in WWW-Authenticate.
            options.IncludeErrorDetails = builder.Environment.IsDevelopment();
            options.RequireHttpsMetadata = false;
            // Keep JWT claim types as issued (sub, unique_name) — do not map to ClaimTypes.*.
            options.MapInboundClaims = false;

            // Prefer classic JwtSecurityTokenHandler path for HS256 + SymmetricSecurityKey.
            // .NET 8+/9 default JsonWebTokenHandler is stricter about kid/key resolution
            // and commonly surfaces "The signature key was not found" for local HMAC JWTs.
            options.UseSecurityTokenValidators = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = signingKey,
                TryAllIssuerSigningKeys = true,
                // Fallback if token kid is missing/mismatched — always return our HMAC key.
                IssuerSigningKeyResolver = (_, _, _, _) => [signingKey],
                ClockSkew = TimeSpan.FromMinutes(2),
                NameClaimType = JwtRegisteredClaimNames.UniqueName,
                RoleClaimType = "roleId"
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Warning(
                        context.Exception,
                        "JWT authentication failed: {ExceptionType}",
                        context.Exception.GetType().Name);
                    return Task.CompletedTask;
                },
                // Default challenge returns empty body — clients report that as "no data".
                OnChallenge = async context =>
                {
                    Log.Warning(
                        "JWT challenge. Error={Error}; ErrorDescription={ErrorDescription}",
                        context.Error,
                        context.ErrorDescription);

                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var message = string.IsNullOrWhiteSpace(context.ErrorDescription)
                        ? "Unauthorized. Send header Authorization: Bearer <token> using the login response field \"token\" (not refreshToken)."
                        : context.ErrorDescription;

                    var body = new ApiErrorResponseDto
                    {
                        Success = false,
                        Message = message,
                        TraceId = context.HttpContext.TraceIdentifier,
                        StatusCode = StatusCodes.Status401Unauthorized
                    };

                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(body, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var body = new ApiErrorResponseDto
                    {
                        Success = false,
                        Message = "Forbidden. Authenticated but not allowed for this resource.",
                        TraceId = context.HttpContext.TraceIdentifier,
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(body, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));
                }
            };
        });

    // Re-apply key after framework JwtBearerConfigureOptions so config binding cannot wipe it.
    builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
        .PostConfigure(options =>
        {
            options.TokenValidationParameters.IssuerSigningKey = signingKey;
            options.TokenValidationParameters.IssuerSigningKeyResolver = (_, _, _, _) => [signingKey];
            options.TokenValidationParameters.TryAllIssuerSigningKeys = true;
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("LoggedInPolicy", policy =>
            policy.RequireAuthenticatedUser());
    });

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "NCMIS API",
            Version = "v1",
            Description =
                "NCMIS mobile and web API. POST /api/auth/login → copy JSON field \"token\" → Authorize (Swagger) or header Authorization: Bearer {token}. Use https://localhost:7046 (not http) so the token is not dropped on HTTPS redirect."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Paste the login response field \"token\" ONLY (the long JWT). Do NOT type the word Bearer — Swagger adds it. Do NOT paste refreshToken."
        });

        options.OperationFilter<AuthorizeCheckOperationFilter>();
        options.OperationFilter<FileUploadOperationFilter>();
        options.ParameterFilter<FormFileParameterFilter>();

        options.MapType<IFormFile>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "binary"
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "NCMIS API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "NCMIS API";
        options.DisplayRequestDuration();
        options.EnablePersistAuthorization();
        options.EnableTryItOutByDefault();
    });

    // HTTP→HTTPS redirects often strip Authorization on mobile/HTTP clients, which looks like
    // "APIs return no data with access token". Prefer calling the HTTPS URL directly.
    // In Development, skip redirect so http://localhost:5226 still accepts Bearer tokens.
    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    var webRootPath = app.Environment.WebRootPath
        ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
    Directory.CreateDirectory(webRootPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(webRootPath)
    });

    // Serve /cdn from configurable shared folder (FileStorage:RootPath), e.g. MVC wwwroot\cdn.
    var fileStorage = app.Configuration.GetSection(FileStorageSettings.SectionName).Get<FileStorageSettings>()
        ?? new FileStorageSettings();
    var cdnPhysicalPath = !string.IsNullOrWhiteSpace(fileStorage.RootPath)
        ? Path.GetFullPath(fileStorage.RootPath.Trim())
        : Path.Combine(webRootPath, "cdn");
    Directory.CreateDirectory(cdnPhysicalPath);
    var cdnRequestPath = string.IsNullOrWhiteSpace(fileStorage.RequestPath)
        ? "/cdn"
        : "/" + fileStorage.RequestPath.Trim().Trim('/').Replace('\\', '/');
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(cdnPhysicalPath),
        RequestPath = cdnRequestPath
    });
    Log.Information("FileStorage CDN mapped. RequestPath={RequestPath}; PhysicalPath={PhysicalPath}",
        cdnRequestPath, cdnPhysicalPath);

    app.UseCors("MobilePolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    Log.Information(
        "JWT configured. KeyLength={KeyLength}; Issuer={Issuer}; Audience={Audience}; ExpiryMinutes={ExpiryMinutes}",
        jwtSettings.Key.Length,
        jwtSettings.Issuer,
        jwtSettings.Audience,
        jwtSettings.ExpiryMinutes);
    Log.Information(
        "NCMIS API starting. Environment={EnvironmentName}; ContentRoot={ContentRoot}",
        app.Environment.EnvironmentName,
        app.Environment.ContentRootPath);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "NCMIS API terminated unexpectedly.");
}
finally
{
    SerilogConfiguration.CloseAndFlush();
}

