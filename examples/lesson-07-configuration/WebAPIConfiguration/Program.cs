using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using WebAPIConfiguration.Data;
using WebAPIConfiguration.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, config) => {
  config.Sources.Clear();

  var env = hostingContext.HostingEnvironment;
  config.AddIniFile("MyConfig.ini", optional: true, reloadOnChange: true)
        .AddIniFile($"MyConfig.{env.EnvironmentName}", optional: true, reloadOnChange: true);
});

// Add services to the container.
if (builder.Environment.IsDevelopment())
{
  builder.Services.AddDbContext<NetLogContext>(options =>
    options.UseInMemoryDatabase("NetLogDev"));
}
else
{
  builder.Services.AddDbContext<NetLogContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NetLog")));
}

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  
  seedData();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void seedData() {
  using(var scope = app.Services.CreateScope()) {
    using FileStream fs = File.OpenRead("MOCK_DATA.json");
    var logs = JsonSerializer.Deserialize<NetLog[]>(fs, new JsonSerializerOptions {
      PropertyNameCaseInsensitive = true,
    });

    var context = scope.ServiceProvider.GetRequiredService<NetLogContext>();
      context.Database.EnsureCreated();
      context.AddRange(logs);
      context.SaveChanges();
  }
}