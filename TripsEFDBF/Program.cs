using Microsoft.EntityFrameworkCore;
using TripsEFDBF.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddDbContext<TripsContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();


app.UseAuthorization();

app.MapControllers();

app.Run();