using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SHOP_RUNNER.Services.ProductRepo;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using SHOP_RUNNER.Services.EmailService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization", // "Token"
        Type = SecuritySchemeType.ApiKey
    });
    // đoạn này ta phải tải thư viện hỗ trỡ truyền dữ liệu vào header qua swagger
    // install package 'Swashbuckle.AspNetCore.Filters'
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

/*
 cấu hình cho swagger truyền header qua objet req | nếu lúc nào KHÔNG cần sẽ đưa vào comment:

 
 */

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
builder.Services.AddScoped<IEmailService, ClassEmailRepo>();

// triển khai Authorization 403 or 401 :

// Lấy secretKey Bytes:
var secretkeyByte = Encoding.UTF8.GetBytes(builder.Configuration.GetConnectionString("SecretKey"));
// vì ta để trong object GetConnectionString thì ta lấy thế này nếu đặt secretKey ra một| key: values ( AppSettings : {secretKey: "..."}  ) | riêng ở file appsetting.json thì ta sẽ lấy thế này:
// builder.Configuration.GetSection("AppSettings:SecretKey").Value

// tải thư viện Microsoft.AspNetCore.Authentication.JwtBearer
builder.Services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme).AddJwtBearer( 
    options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretkeyByte),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    }
    
    );



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// cấu hình cho phép sử dụng file tĩnh ở thư mục updates/images:
app.UseStaticFiles( new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider( 
                        Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Images")), RequestPath = "/Images"
});

// sử dụng gọi lại cấu hình của core ra đây:
app.UseCors();

app.UseHttpsRedirection();

// sau khi triển khai cấu hình authen ở trên gọi lại middleware ở đây cho nó làm việc
app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();

app.Run();

/*
 bây giờ ta sẽ add accessToken vào header trong object req 
chúng ta sẽ sửa lại swagger một chút cho nó truyền dữ liệu qua header trong object
 
 */