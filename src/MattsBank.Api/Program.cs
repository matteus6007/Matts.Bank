using MattsBank.Api.Options;
using MattsBank.Api.Services;
using MattsBank.Infrastructure.Repositories;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var defaultApiVersion = new ApiVersion(1, 0);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Matt's Bank API", Version = "v1" });
});
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = defaultApiVersion;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddVersionedApiExplorer(options =>
{
    options.SubstituteApiVersionInUrl = true;
    options.GroupNameFormat = "'v'V";
    options.DefaultApiVersion = defaultApiVersion;
    options.AssumeDefaultVersionWhenUnspecified = true;
});

builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
builder.Services.AddSingleton<ITransactionRepository, InMemoryTransactionRepository>();
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<ITransactionService, TransactionService>();
builder.Services.Configure<BankOptions>(builder.Configuration.GetSection(BankOptions.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline.
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.yaml",
                description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();
app.UseExceptionHandler("/error");
app.UseAuthorization();

app.MapControllers();

app.Run();
