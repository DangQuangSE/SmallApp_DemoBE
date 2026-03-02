using MudBlazor.Services;
using SecondBike.Application;
using SecondBike.Infrastructure;
using SecondBike.WebUI.Components;

var builder = WebApplication.CreateBuilder(args);

// ─── Framework Services ───
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// ─── MudBlazor ───
builder.Services.AddMudServices();

// ─── Application Layer (Validators) ───
builder.Services.AddApplication();

// ─── Infrastructure Layer (EF Core, Identity, Repositories, Services) ───
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ─── HTTP Request Pipeline ───
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Identity middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SecondBike.WebUI.Client._Imports).Assembly);

app.Run();
