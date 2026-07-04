using System.Collections.Concurrent;
using PlanningPoker.Models;

namespace PlanningPoker.Services;

/// <summary>
/// Serviço Singleton thread-safe para gerenciar todas as salas de Planning Poker em memória
/// </summary>
public class PlanningPokerService
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    private readonly ILogger<PlanningPokerService> _logger;
    private readonly Timer _cleanupTimer;

    // Event para notificar componentes sobre mudanças em uma sala
    public event Action<string>? RoomUpdated;

    public PlanningPokerService(ILogger<PlanningPokerService> logger)
    {
        _logger = logger;

        // Timer para limpar salas inativas a cada 5 minutos
        _cleanupTimer = new Timer(CleanupInactiveRooms, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    #region Gerenciamento de Salas

    /// <summary>
    /// Cria uma nova sala de Planning Poker
    /// </summary>
    public Room CreateRoom(string teamName, string sprintName, List<string> votingSequence, string moderatorName, string instructions = "", UserRole moderatorRole = UserRole.Dev)
    {
        var room = new Room
        {
            TeamName = teamName,
            SprintName = sprintName,
            VotingSequence = votingSequence,
            Instructions = instructions
        };

        var moderator = new User
        {
            Name = moderatorName,
            IsModerator = true,
            Role = moderatorRole,
            AvatarUrl = GenerateAvatarUrl(moderatorName)
        };

        room.Users.TryAdd(moderator.Id, moderator);

        if (_rooms.TryAdd(room.Id, room))
        {
            _logger.LogInformation("Sala criada: {RoomId} - Time: {TeamName}, Sprint: {SprintName}",
                room.Id, teamName, sprintName);
            return room;
        }

        throw new InvalidOperationException("Não foi possível criar a sala. Tente novamente.");
    }

    /// <summary>
    /// Obtém uma sala pelo ID
    /// </summary>
    public Room? GetRoom(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return room;
    }

    /// <summary>
    /// Verifica se uma sala existe
    /// </summary>
    public bool RoomExists(string roomId)
    {
        return _rooms.ContainsKey(roomId);
    }

    /// <summary>
    /// Remove uma sala
    /// </summary>
    public bool RemoveRoom(string roomId)
    {
        if (_rooms.TryRemove(roomId, out var room))
        {
            _logger.LogInformation("Sala removida: {RoomId}", roomId);
            return true;
        }
        return false;
    }

    #endregion

    #region Gerenciamento de Usuários

    /// <summary>
    /// Adiciona um usuário a uma sala
    /// </summary>
    public User? JoinRoom(string roomId, string userName, UserRole role = UserRole.Dev)
    {
        var room = GetRoom(roomId);
        if (room == null) return null;

        var user = new User
        {
            Name = userName,
            IsModerator = false,
            Role = role,
            AvatarUrl = GenerateAvatarUrl(userName)
        };

        if (room.Users.TryAdd(user.Id, user))
        {
            room.UpdateActivity();
            _logger.LogInformation("Usuário {UserName} entrou na sala {RoomId}", userName, roomId);
            NotifyRoomUpdate(roomId);
            return user;
        }

        return null;
    }

    /// <summary>
    /// Remove um usuário de uma sala
    /// </summary>
    public bool LeaveRoom(string roomId, string userId)
    {
        var room = GetRoom(roomId);
        if (room == null) return false;

        if (room.Users.TryRemove(userId, out _))
        {
            // Remove também o voto do usuário se existir
            room.Votes.TryRemove(userId, out _);
            room.UpdateActivity();

            _logger.LogInformation("Usuário {UserId} saiu da sala {RoomId}", userId, roomId);
            NotifyRoomUpdate(roomId);
            return true;
        }

        return false;
    }

    #endregion

    #region Gerenciamento de Votos

    /// <summary>
    /// Registra o voto de um usuário
    /// </summary>
    public bool Vote(string roomId, string userId, string value)
    {
        var room = GetRoom(roomId);
        if (room == null || !room.Users.ContainsKey(userId)) return false;

        var vote = new Vote
        {
            UserId = userId,
            Value = value,
            IsRevealed = room.VotesRevealed
        };

        room.Votes.AddOrUpdate(userId, vote, (key, old) => vote);
        room.UpdateActivity();

        _logger.LogInformation("Voto registrado na sala {RoomId} - Usuário: {UserId}, Valor: {Value}",
            roomId, userId, value);

        NotifyRoomUpdate(roomId);
        return true;
    }

    /// <summary>
    /// Revela todos os votos da sala
    /// </summary>
    public bool RevealVotes(string roomId)
    {
        var room = GetRoom(roomId);
        if (room == null) return false;

        // Adiciona votos fictícios para quem não votou (☕ café ou 🚽 banheiro)
        var random = new Random();
        var absenteeSymbols = new[] { "☕", "🚽" };

        foreach (var user in room.Users.Values)
        {
            if (!room.Votes.ContainsKey(user.Id))
            {
                var absenteeVote = new Vote
                {
                    UserId = user.Id,
                    Value = absenteeSymbols[random.Next(absenteeSymbols.Length)],
                    IsRevealed = true
                };
                room.Votes.TryAdd(user.Id, absenteeVote);

                _logger.LogInformation("Usuário ausente na sala {RoomId} - {UserId} marcado como {Symbol}",
                    roomId, user.Id, absenteeVote.Value);
            }
        }

        room.VotesRevealed = true;

        foreach (var vote in room.Votes.Values)
        {
            vote.IsRevealed = true;
        }

        room.UpdateActivity();
        _logger.LogInformation("Votos revelados na sala {RoomId}", roomId);
        NotifyRoomUpdate(roomId);
        return true;
    }

    /// <summary>
    /// Reinicia a votação (limpa todos os votos)
    /// </summary>
    public bool ResetVoting(string roomId)
    {
        var room = GetRoom(roomId);
        if (room == null) return false;

        room.Votes.Clear();
        room.VotesRevealed = false;
        room.RoundNumber++;
        room.UpdateActivity();

        _logger.LogInformation("Votação reiniciada na sala {RoomId} - Nova rodada: {RoundNumber}",
            roomId, room.RoundNumber);

        NotifyRoomUpdate(roomId);
        return true;
    }

    #endregion

    #region Canhão de Diversão

    /// <summary>
    /// Dispara o canhão de um usuário para outro
    /// </summary>
    public bool FireCannon(string roomId, string fromUserId, string toUserId)
    {
        var room = GetRoom(roomId);
        if (room == null) return false;

        // Lista de projéteis aleatórios
        var projectiles = new[]
        {
            "🌸", "💐", "🌹", "🌺", "🌻",  // Flores
            "❤️", "💕", "💖", "💝", "💗",  // Corações
            "🪨", "⚡", "💣", "🚀", "💥",  // Pesados
            "🥥", "🍎", "🍌", "🍉", "🍕",  // Comidas
            "💰", "💵", "💎", "👑", "🏆",  // Prêmios
            "🎉", "🎊", "✨", "⭐", "🌟"   // Celebrações
        };

        var random = new Random();
        var projectile = projectiles[random.Next(projectiles.Length)];

        var shot = new CannonShot
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Projectile = projectile
        };

        room.LastCannonShot = shot;
        room.UpdateActivity();

        _logger.LogInformation("Canhão disparado na sala {RoomId} - De: {FromUserId} Para: {ToUserId} Projétil: {Projectile}",
            roomId, fromUserId, toUserId, projectile);

        NotifyRoomUpdate(roomId);
        return true;
    }

    #endregion

    #region Utilitários

    /// <summary>
    /// Gera uma URL de avatar usando DiceBear API
    /// </summary>
    private string GenerateAvatarUrl(string seed)
    {
        // Usando DiceBear API com estilo bottts (robôs fofinhos)
        return $"https://api.dicebear.com/7.x/bottts/svg?seed={Uri.EscapeDataString(seed)}";
    }

    /// <summary>
    /// Notifica todos os componentes conectados sobre uma atualização na sala
    /// </summary>
    private void NotifyRoomUpdate(string roomId)
    {
        RoomUpdated?.Invoke(roomId);
    }

    /// <summary>
    /// Remove salas inativas (sem atividade por mais de 2 horas)
    /// </summary>
    private void CleanupInactiveRooms(object? state)
    {
        var inactivityThreshold = DateTime.UtcNow.AddHours(-2);
        var inactiveRooms = _rooms.Where(r => r.Value.LastActivity < inactivityThreshold).ToList();

        foreach (var room in inactiveRooms)
        {
            if (_rooms.TryRemove(room.Key, out _))
            {
                _logger.LogInformation("Sala inativa removida: {RoomId}", room.Key);
            }
        }

        if (inactiveRooms.Any())
        {
            _logger.LogInformation("Limpeza de salas inativas: {Count} salas removidas", inactiveRooms.Count);
        }
    }

    /// <summary>
    /// Obtém estatísticas do serviço
    /// </summary>
    public (int TotalRooms, int TotalUsers) GetStatistics()
    {
        var totalRooms = _rooms.Count;
        var totalUsers = _rooms.Values.Sum(r => r.Users.Count);
        return (totalRooms, totalUsers);
    }

    #endregion
}
