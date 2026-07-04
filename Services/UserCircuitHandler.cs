using Microsoft.AspNetCore.Components.Server.Circuits;

namespace PlanningPoker.Services;

/// <summary>
/// Handler para detectar quando usuários desconectam (fecham o browser)
/// </summary>
public class UserCircuitHandler : CircuitHandler
{
    private readonly PlanningPokerService _pokerService;
    private readonly ILogger<UserCircuitHandler> _logger;

    public UserCircuitHandler(PlanningPokerService pokerService, ILogger<UserCircuitHandler> logger)
    {
        _pokerService = pokerService;
        _logger = logger;
    }

    /// <summary>
    /// Chamado quando um circuito é inicializado (usuário conecta)
    /// </summary>
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit opened: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Chamado quando a conexão é perdida (browser fechado, internet caiu, etc)
    /// </summary>
    public override async Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Connection down for circuit: {CircuitId}", circuit.Id);

        // Aguarda um pouco antes de remover (em caso de reconexão temporária)
        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

        return;
    }

    /// <summary>
    /// Chamado quando o circuito é encerrado definitivamente
    /// </summary>
    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit closed: {CircuitId}", circuit.Id);

        // Remove o usuário da sala
        _pokerService.HandleUserDisconnection(circuit.Id);

        return Task.CompletedTask;
    }
}
