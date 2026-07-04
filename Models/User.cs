namespace PlanningPoker.Models;

/// <summary>
/// Representa o papel/grupo de um usuário na votação
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Desenvolvedor - voto conta na média
    /// </summary>
    Dev,

    /// <summary>
    /// QA/Tester - voto conta na média
    /// </summary>
    QA,

    /// <summary>
    /// Observador - não vota ou voto não conta na média
    /// </summary>
    Observer
}

/// <summary>
/// Representa um usuário participante de uma sala de Planning Poker
/// </summary>
public class User
{
    /// <summary>
    /// Identificador único do usuário
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Nome do usuário
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL do avatar do usuário (gerado via DiceBear API)
    /// </summary>
    public string AvatarUrl { get; set; } = string.Empty;

    /// <summary>
    /// Indica se o usuário é o moderador/criador da sala
    /// </summary>
    public bool IsModerator { get; set; }

    /// <summary>
    /// Papel/Grupo do usuário na votação (Dev, QA, Observador)
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Dev;

    /// <summary>
    /// Data e hora em que o usuário entrou na sala
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica se o usuário está conectado ativamente
    /// </summary>
    public bool IsConnected { get; set; } = true;
}
