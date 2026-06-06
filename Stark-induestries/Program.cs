using CloudinaryDotNet;
using Stark_induestries.Entity;
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


var cloud = app.Services.GetRequiredService<CloudService>();
var queue = app.Services.GetRequiredService<ImageQueueService>();

//Retrieving valid id's from cloud
var baseIds = await cloud.GetBasePublicIds();
foreach (var id in baseIds)
    await queue.Enqueue(new ImageEntry { Identifier = id }, cloud);


//app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
