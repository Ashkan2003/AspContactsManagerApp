using CRUDExample;
using CRUDExample.Middleware;
using Serilog;


// this is a project for explayning and showing the Clean archtecture 

var builder = WebApplication.CreateBuilder(args);

//Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
{

    loggerConfiguration
    .ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration
    .ReadFrom.Services(services); //read out current app's services and make them available to serilog
});

builder.Services.ConfigureServices(builder.Configuration);


var app = builder.Build();


//create application pipeline
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseHsts(); // in order to enable https we use this code
app.UseHttpsRedirection(); // enable https for better sicurity

app.UseSerilogRequestLogging();


app.UseHttpLogging();

if (builder.Environment.IsEnvironment("Test") == false)
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

app.UseStaticFiles();
app.UseAuthentication(); // reading Identity cookie // enabling the user login functionallity
app.UseAuthorization(); // Validates access permissions of the user 
app.MapControllers(); // execute the filter pipline(action + filters)

app.Run();

public partial class Program { } //make the auto-generated Program accessible programmatically
