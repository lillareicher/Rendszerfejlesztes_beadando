using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ReactApp1.Server.Data;
using ReactApp1.Server.Services;
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;
using System.Text;
using System.Net;
using System.Diagnostics.Eventing.Reader;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.WebHost.UseUrls("https://localhost:5173/");
builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = "https://localhost:7045/";
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateAudience = true,
        ValidAudience = "https://localhost:5173/",
        ValidateIssuer = true,
        ValidIssuer = "https://localhost:7045/",
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Jsdgfksdo345634lesdfgdf5jkdgfljkdgfk756lksdf")),
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Autorent",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
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
                new string[] { }
            }
        });
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //?


var app = builder.Build();

//public enum WebSocketState //reprezentációja az állapotoknak
//{
//    None = 0,
//    Connecting = 1,
//    Open = 2,
//    CloseSent = 3,
//    CloseReceived = 4,
//    Closed = 5,
//    Aborted = 6
//}

app.UseWebSockets(); // websocket
app.Map("/ws", async ctx =>
{
    if (ctx.WebSockets.IsWebSocketRequest)
    {
        using var ws = await ctx.WebSockets.AcceptWebSocketAsync();
        while (true)
        {
            var msg = "The current time is: " + DateTime.Now.ToString("HH:mm:ss");
            var bytes = Encoding.UTF8.GetBytes(msg);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted)
            {
                break;
            }
            Thread.Sleep(1000);
        }
    }
    else
    {
        ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
    ctx.Database.Migrate();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

//app.Run();
await app.RunAsync();