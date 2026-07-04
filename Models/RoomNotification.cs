namespace PlanningPoker.Models;

/// <summary>
/// Tipo de notificação da sala
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Informação geral (verde)
    /// </summary>
    Info,

    /// <summary>
    /// Aviso importante (vermelho)
    /// </summary>
    Warning
}

/// <summary>
/// Representa uma notificação de evento na sala (entrada/saída de usuários, etc)
/// </summary>
public class RoomNotification
{
    /// <summary>
    /// Identificador único da notificação
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Mensagem da notificação
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Tipo da notificação (Info ou Warning)
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Info;

    /// <summary>
    /// Data e hora da notificação
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
