namespace PlanningPoker.Models;

/// <summary>
/// Representa um disparo do canhão entre usuários
/// </summary>
public class CannonShot
{
    /// <summary>
    /// ID único do disparo
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID do usuário que disparou
    /// </summary>
    public string FromUserId { get; set; } = string.Empty;

    /// <summary>
    /// ID do usuário alvo
    /// </summary>
    public string ToUserId { get; set; } = string.Empty;

    /// <summary>
    /// Emoji/item disparado
    /// </summary>
    public string Projectile { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp do disparo
    /// </summary>
    public DateTime FiredAt { get; set; } = DateTime.UtcNow;
}
