namespace PlanningPoker.Models;

/// <summary>
/// Representa um voto de um usuário em uma rodada de Planning Poker
/// </summary>
public class Vote
{
    /// <summary>
    /// ID do usuário que votou
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Valor do voto (pode ser numérico ou string como "?", "☕", etc)
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora em que o voto foi registrado
    /// </summary>
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica se o voto já foi revelado
    /// </summary>
    public bool IsRevealed { get; set; }
}
