using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SHOP_RUNNER.Services.ProductRepo;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using SHOP_RUNNER.Services.EmailService;
using SHOP_RUNNER.Services.Cart_service;
using SHOP_RUNNER.Services.Staff_service;
using Microsoft.AspNetCore.RateLimiting;
using SHOP_RUNNER.Services.Statistics_Service;
using SHOP_RUNNER.Services.Admin_service;
using System.Text.Json;

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

// connect DB: singleton -> khởi tạo 1 lần chạy mọi nơi
string ConnectionString = builder.Configuration.GetConnectionString("API");
builder.Services.AddDbContext<SHOP_RUNNER.Entities.RunningShopContext>(
        option => option.UseSqlServer(ConnectionString));


// ADD CORS:
/*builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});*/
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(orgin =>  true);
    });
});



builder.Services.AddControllers().AddNewtonsoftJson( options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore );


// Khái báo interface Repository:
builder.Services.AddScoped<IProductRepo, ProductClassRepo>();
builder.Services.AddScoped<IEmailService, ClassEmailRepo>();
builder.Services.AddScoped<ICart_m, Cart_m_class>();
builder.Services.AddScoped<IStaff_repo, Staff_class>();
builder.Services.AddScoped<IStatistics, Statistics_class>();
builder.Services.AddScoped<IAdmin_repo, Admin_class>();

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


// CẤU HÌNH SESSION:
builder.Services.AddDistributedMemoryCache();

// phương thức để thêm dịch vụ Session vào container dịch vụ
// options là một đối tượng cho phép bạn cấu hình các tùy chọn cho Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



//CẤU HÌNH RATE LIMIT: 5 request/10s
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter(policyName: "fixedWindow", o2 => {
        // số lượng cái request cho phép trong một cái window ( trong 1 khung thời gian nào đó )
        o2.PermitLimit = 40; // cho phep 40 request / 1'
        o2.Window = TimeSpan.FromMinutes(1);
        o2.QueueLimit = 0;
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// cấu hình cho phép sử dụng file tĩnh ở thư mục updates/images:
app.UseStaticFiles();


// sử dụng gọi lại cấu hình của core ra đây:
app.UseCors();


// SỬ DỤNG RATE LIMMIT:
app.UseRateLimiter();

app.UseHttpsRedirection();

// sau khi triển khai cấu hình authen ở trên gọi lại middleware ở đây cho nó làm việc
app.UseAuthentication();


app.UseAuthorization();

// KHAI BÁO SESSION MIDDLEWARE: gọi trước MapController() -> đảm bảo middleware session đc sd trc khi yc đến controller
app.UseSession();

// Sử dụng ngăn chặn lỗi exception xảy ra:
app.UseExceptionHandler( a => a.Run( async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/json";

    var response = new { error = "fail please try again!" };

    var json = JsonSerializer.Serialize(response);

    await context.Response.WriteAsync(json);

}));

// cấu hình ngăn chặn lộ thông tin của ứng dụng:
app.Use(async (context, next) =>
{
    context.Response.OnStarting(state =>
    {
        // ép kiểu đối tượng state về kiểu HttpContext
        var ctx = (HttpContext)state;
        if (ctx.Response.Headers.ContainsKey("Server"))
        {
            ctx.Response.Headers.Remove("Server");
        }
        if (ctx.Response.Headers.ContainsKey("X-Powered-By"))
        {
            ctx.Response.Headers["X-Powered-By"] = "Do Viet Anh";
        }
        return Task.CompletedTask;


    }, context); // method will call again before it responses
    await next.Invoke();
});

app.MapControllers();

app.Run();

/*
 bây giờ ta sẽ add accessToken vào header trong object req 
chúng ta sẽ sửa lại swagger một chút cho nó truyền dữ liệu qua header trong object
 
 */