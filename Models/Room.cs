using System.Collections.Concurrent;

namespace PlanningPoker.Models;

/// <summary>
/// Representa uma sala de Planning Poker com todos os seus participantes e estado
/// </summary>
public class Room
{
    /// <summary>
    /// Identificador único da sala
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8]; // 8 caracteres do GUID

    /// <summary>
    /// Nome do time que está usando a sala
    /// </summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Nome ou número da Sprint
    /// </summary>
    public string SprintName { get; set; } = string.Empty;

    /// <summary>
    /// Instruções para referência durante a votação
    /// </summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>
    /// Sequência de valores disponíveis para votação (ex: ["1", "2", "3", "5", "8", "13"])
    /// </summary>
    public List<string> VotingSequence { get; set; } = new();

    /// <summary>
    /// Data e hora de criação da sala
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data e hora da última atividade na sala (para expiração)
    /// </summary>
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Lista thread-safe de usuários na sala
    /// </summary>
    public ConcurrentDictionary<string, User> Users { get; set; } = new();

    /// <summary>
    /// Lista thread-safe de votos da rodada atual
    /// </summary>
    public ConcurrentDictionary<string, Vote> Votes { get; set; } = new();

    /// <summary>
    /// Indica se os votos foram revelados
    /// </summary>
    public bool VotesRevealed { get; set; }

    /// <summary>
    /// Número da rodada atual
    /// </summary>
    public int RoundNumber { get; set; } = 1;

    /// <summary>
    /// Último disparo do canhão (para sincronização em tempo real)
    /// </summary>
    public CannonShot? LastCannonShot { get; set; }

    /// <summary>
    /// Lista thread-safe de notificações da sala (entrada/saída de usuários, etc)
    /// </summary>
    public ConcurrentQueue<RoomNotification> Notifications { get; set; } = new();

    /// <summary>
    /// Obtém o usuário moderador da sala
    /// </summary>
    public User? GetModerator()
    {
        return Users.Values.FirstOrDefault(u => u.IsModerator);
    }

    /// <summary>
    /// Verifica se todos os usuários conectados já votaram
    /// </summary>
    public bool AllUsersVoted()
    {
        var connectedUsers = Users.Values.Where(u => u.IsConnected).ToList();
        return connectedUsers.Any() && connectedUsers.All(u => Votes.ContainsKey(u.Id));
    }

    /// <summary>
    /// Calcula a média dos votos numéricos
    /// </summary>
    public double? GetVotesAverage()
    {
        var numericVotes = Votes.Values
            .Select(v => double.TryParse(v.Value, out var num) ? (double?)num : null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        return numericVotes.Any() ? numericVotes.Average() : null;
    }

    /// <summary>
    /// Calcula a média dos votos numéricos por grupo (Dev, QA)
    /// Observadores não são incluídos no cálculo
    /// </summary>
    public double? GetVotesAverageByRole(UserRole role)
    {
        var usersInRole = Users.Values.Where(u => u.Role == role).Select(u => u.Id).ToHashSet();
        
        var numericVotes = Votes.Values
            .Where(v => usersInRole.Contains(v.UserId))
            .Select(v => double.TryParse(v.Value, out var num) ? (double?)num : null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        return numericVotes.Any() ? numericVotes.Average() : null;
    }

    /// <summary>
    /// Calcula a soma das médias de Dev + QA
    /// </summary>
    public double? GetCombinedAverage()
    {
        var devAvg = GetVotesAverageByRole(UserRole.Dev);
        var qaAvg = GetVotesAverageByRole(UserRole.QA);

        if (!devAvg.HasValue && !qaAvg.HasValue) return null;

        return (devAvg ?? 0) + (qaAvg ?? 0);
    }

    /// <summary>
    /// Arredonda um valor para o próximo número da sequência Fibonacci
    /// </summary>
    public string RoundToFibonacci(double value)
    {
        // Sequência Fibonacci estendida
        var fibonacci = new[] { 0, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };

        // Se o valor for menor que o menor Fibonacci, retorna o menor
        if (value <= fibonacci[0]) return fibonacci[0].ToString();

        // Procura o próximo valor na sequência
        for (int i = 0; i < fibonacci.Length; i++)
        {
            if (value <= fibonacci[i])
            {
                return fibonacci[i].ToString();
            }
        }

        // Se for maior que todos, retorna o maior
        return fibonacci[^1].ToString();
    }

    /// <summary>
    /// Obtém a estimativa final arredondada (soma Dev + QA arredondada para Fibonacci)
    /// </summary>
    public string? GetFinalEstimate()
    {
        var combined = GetCombinedAverage();
        if (!combined.HasValue) return null;

        return RoundToFibonacci(combined.Value);
    }

    /// <summary>
    /// Atualiza o timestamp de última atividade
    /// </summary>
    public void UpdateActivity()
    {
        LastActivity = DateTime.UtcNow;
    }
}
