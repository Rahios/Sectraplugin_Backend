using BLL;
using DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Dependency injection for controllers

// Register dependencies 
// DAL
string imagesDirectoryPath = "/home/user/appBackend/data/tcga/wsi"; // SAME VALUE AS THE ONE IN THE VOLUME OF THE DOCKER-COMPOSE FILE
//builder.Services.AddSingleton<IHistoLungFS>(hfs => new HistoLungFS(imagesDirectoryPath));
// BLL
//builder.Services.AddSingleton<IHistoLungManager, HistoLungManager>(); 


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
