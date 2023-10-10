using Microsoft.EntityFrameworkCore;
using SHOP_RUNNER.Services.ProductRepo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// connect DB:
string ConnectionString = builder.Configuration.GetConnectionString("API");
builder.Services.AddDbContext<SHOP_RUNNER.Entities.RunningShopContext>(
        option => option.UseSqlServer(ConnectionString));


// ADD CORS:
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers().AddNewtonsoftJson( options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore );


// Khái báo interface Repository:
builder.Services.AddScoped<IProductRepo, ProductClassRepo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// sử dụng gọi lại cấu hình của core ra đây:
app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();

app.Run();
