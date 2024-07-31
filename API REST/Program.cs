using BLL.Interfaces;
using BLL.Managers;
using DAL.FileSystem;
using DAL.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Dependency injection for controllers

// Configure CORS (Cross-Origin Resource Sharing) to allow the frontend to access the backend
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowSpecificOrigin",  // Name of the policy to be used in the controller to enable CORS
		builder =>
		{
			builder.WithOrigins("https://153.109.124.207", 
											"https://vlbeltbsectra.hevs.ch") 
				   .AllowAnyHeader()
				   .AllowAnyMethod();
		});
});


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
// Register the Swagger services in the service container to generate the Swagger document
builder.Services.AddSwaggerGen( c =>
{
	// Define the Swagger document to be generated (the metadata of the API)
	var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";

	// Define the path to the XML file containing the documentation comments of the API controllers
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

	// Include the XML file in the Swagger document to generate the documentation of the API
	c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	// Enable the middleware to serve the generated Swagger as a JSON endpoint (the Swagger document)
	app.UseSwaggerUI( c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "API REST V1");
		c.RoutePrefix = string.Empty; // Set the Swagger UI at the root URL
	});
}

app.UseHttpsRedirection();

// Use CORS middleware
app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();
