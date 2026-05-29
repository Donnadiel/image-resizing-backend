using CloudinaryDotNet;
using Stark_induestries.Service;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.UseKestrel()
    .UseUrls("http://0.0.0.0:5000");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");

builder.Services.AddSingleton(new Cloudinary(
    new Account(
        cloudinaryConfig["CLOUD_NAME"],
        cloudinaryConfig["API_KEY"],
        cloudinaryConfig["API_SECRET"]
    )
));


builder.Services.AddSingleton<CloudService>();
builder.Services.AddSingleton<ImageQueueService>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var queue = app.Services.GetRequiredService<ImageQueueService>();
await queue.ReloadQueue();

//app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
