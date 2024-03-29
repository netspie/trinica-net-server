﻿using Corelibs.Basic.Events;
using Mediator;
using Trinica.Entities.Gameplay;
using Trinica.Entities.Gameplay.Events;

namespace Trinica.Infrastructure.UseCases.Gameplay;

public class OutgoingGameEventsHandler(
    IRoomEventsDispatcher<GameEvent> gameEventsDispatcher,
    IRoomSetup<GameId> gameSetup) :
    INotificationHandler<GameStartedEvent>,
    INotificationHandler<GameFinishedEvent>,
    INotificationHandler<CardsTakenToHandEvent>,
    INotificationHandler<LayCardDownOrderCalculatedEvent>,
    INotificationHandler<CardsLaidDownEvent>,
    INotificationHandler<CardsLayPassedByPlayerEvent>,
    INotificationHandler<DicesPlayedEvent>,
    INotificationHandler<DicesReplayPassedEvent>,
    INotificationHandler<AssignsDicesToCardConfirmedEvent>,
    INotificationHandler<AssignTargetToCardEvent>,
    INotificationHandler<AssignTargetsToCardConfirmedEvent>
{
    private readonly IRoomEventsDispatcher<GameEvent> _gameEventsDispatcher = gameEventsDispatcher;
    private readonly IRoomSetup<GameId> _gameSetup = gameSetup;

    public ValueTask Handle(GameStartedEvent ev, CancellationToken ct) 
        => Run(() => _gameSetup.AddRoom(ev.GameId));

    public ValueTask Handle(GameFinishedEvent ev , CancellationToken ct) => 
        Run(() => _gameSetup.AddRoom(ev.GameId));

    public ValueTask Handle(CardsTakenToHandEvent ev, CancellationToken ct) => On(ev, ct);
    public ValueTask Handle(LayCardDownOrderCalculatedEvent ev, CancellationToken ct) => On(ev, ct);
    public ValueTask Handle(CardsLaidDownEvent ev, CancellationToken ct) => On(ev, ct);
    public ValueTask Handle(CardsLayPassedByPlayerEvent ev, CancellationToken ct) => On(ev, ct);
    public ValueTask Handle(DicesPlayedEvent ev, CancellationToken ct) => On(ev, ct);
    public ValueTask Handle(DicesReplayPassedEvent ev, CancellationToken ct) => On(ev, ct);
    public ValueTask Handle(AssignsDicesToCardConfirmedEvent ev, CancellationToken ct) => On(ev, ct);
    public ValueTask Handle(AssignTargetToCardEvent ev, CancellationToken ct) => On(ev, ct);
    public ValueTask Handle(AssignTargetsToCardConfirmedEvent ev, CancellationToken ct) => On(ev, ct);

    private ValueTask On(GameEvent ev, CancellationToken ct) => Run(() => _gameEventsDispatcher.Dispatch(ev));

    private ValueTask Run(Action action)
    {
        action();
        return ValueTask.CompletedTask;
    }
}
