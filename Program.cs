using PlanningPoker.Components;
using PlanningPoker.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar serviços Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registrar o serviço de Planning Poker como Singleton (estado compartilhado em memória)
builder.Services.AddSingleton<PlanningPokerService>();

// Registrar o CircuitHandler para detectar desconexões
builder.Services.AddScoped<UserCircuitHandler>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.Circuits.CircuitHandler, UserCircuitHandler>(
    provider => provider.GetRequiredService<UserCircuitHandler>());

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configurar pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Log de inicialização
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🎴 Planning Poker Application iniciado!");
logger.LogInformation("🌐 Acesse: http://localhost:5000 ou https://localhost:5001");

app.Run();
