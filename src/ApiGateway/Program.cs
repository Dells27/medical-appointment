var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. YARP — lee la configuración de rutas y clusters del appsettings.json
// ============================================================
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ============================================================
// 2. CORS — el Gateway es el único que necesita CORS ahora
// Los microservicios internos ya no necesitan preocuparse por esto
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// ============================================================
// 3. Mapea todas las rutas configuradas en el appsettings.json
// ============================================================
app.MapReverseProxy();

app.Run();