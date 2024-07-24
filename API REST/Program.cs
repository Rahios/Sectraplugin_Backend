using BLL.Interfaces;
using BLL.Managers;
using DAL.FileSystem;
using DAL.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Dependency injection for controllers

// DEPENDENCY INJECTION
	// Register the manager to be injected in the controllers, and the FS to be injected in the managers
	// DI makes singleton so the same instance is used in all the controllers that need it (it is not created every time)
// BLL
// Register the manager to be injected in the controllers
builder.Services.AddSingleton<IHelloWorldManager, HelloWorldManager>();
builder.Services.AddSingleton<ICleanUpManager, CleanUpManager>();
builder.Services.AddSingleton<IHistolungManager, HistolungManager>();
// DAL
// Set the file path in the file system to save stuff on the docker volume
string DirectoryPath = "/home/user/appBackend/"; // LOCATED ON THE DOCKER VOLUME 
// Register the FS to be injected in the managers
builder.Services.AddSingleton<IHelloWorldFS>(hfs => new HelloWorldFS(DirectoryPath));
builder.Services.AddSingleton<ICleanUpFS>(cufs => new CleanUpFS(DirectoryPath));
builder.Services.AddSingleton<IHistolungFS>(hfs => new HistolungFS(DirectoryPath));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
