using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TesteL2;
using TesteL2.Data;
using TesteL2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Loja do Seu Manoel - API de Embalagem",
        Version = "v1",
        Description = "API para otimização de embalagem de produtos da loja de jogos do Seu Manoel"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", 
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer", 
        BearerFormat = "JWT", 
        In = ParameterLocation.Header,
        Description = "Insira 'Bearer' [espaço] e então seu token JWT no campo abaixo.\n\nExemplo: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {} 
        }
    });

});
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policyBuilder => {
        policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});



builder.Services.AddDbContext<LojaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmbalagemService, EmbalagemService>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKeyString = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKeyString))
{
    throw new ArgumentNullException(nameof(secretKeyString), "A SecretKey JWT não pode ser nula ou vazia. Verifique a configuração em appsettings.json.");
}
var secretKey = Encoding.ASCII.GetBytes(secretKeyString);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(); 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LojaContext>();
    context.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();