using Archivist.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controller Support 
builder.Services.AddControllers();

// This tells ASP.NET how to create ArchivistDbContext for each request.
builder.Services.AddDbContext<ArchivistDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ArchivistDb"));
});

// OpenAPI/Swagger (so you can test endpoints easily)
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

}

app.UseHttpsRedirection();

app.UseCors("FrontendDev");
// This tells ASP.NET to use attribute routes in your controllers
app.MapControllers();

app.Run();
