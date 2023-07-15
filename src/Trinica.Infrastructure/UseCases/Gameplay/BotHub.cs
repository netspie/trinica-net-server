﻿using Corelibs.Basic.Repository;
using Mediator;
using System.Collections.Concurrent;
using Trinica.Entities.Gameplay;
using Trinica.Entities.Gameplay.Events;
using Trinica.Entities.Users;
using Trinica.UseCases.Gameplay;

namespace Trinica.Infrastructure.UseCases.Gameplay;

public class BotHub : IBotHub, 
    INotificationHandler<CardsTakenToHandEvent>,
    INotificationHandler<LayCardDownOrderCalculatedEvent>,
    INotificationHandler<GameStartedEvent>
{
    private readonly IMemoryRepository<User, UserId> _userRepository;

    public Dictionary<GameId, BotGame> Games { get; } = new();
    public ConcurrentQueue<GameEvent> Events { get; } = new ();

    public BotHub(IMemoryRepository<User, UserId> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task AddGame(
        UserId botId, GameId gameId, GameActionController gameActionController)
    {
        await _userRepository.Save(new User(botId));
        Games.Add(gameId, new(botId, gameId, gameActionController));
    }

    #region Event Enqueuing

    public ValueTask Handle(CardsTakenToHandEvent ev, CancellationToken ct) => TryEnqueue(ev);
    public ValueTask Handle(LayCardDownOrderCalculatedEvent ev, CancellationToken ct) => TryEnqueue(ev);
    public ValueTask Handle(GameStartedEvent ev, CancellationToken ct) => TryEnqueue(ev);

    private bool CanEnqueue(GameEvent ev)
    {
        if (!Games.TryGetValue(ev.GameId, out var game))
            return false;

        if (ev.PlayerId == game.BotId)
            return false;

        return true;
    }

    private ValueTask TryEnqueue(GameEvent ev)
    {
       if (CanEnqueue(ev))
            Events.Enqueue(ev);

        return ValueTask.CompletedTask;
    }

    #endregion

}