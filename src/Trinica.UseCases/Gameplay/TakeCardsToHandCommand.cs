﻿using Corelibs.Basic.Blocks;
using Corelibs.Basic.Repository;
using Corelibs.Basic.UseCases;
using FluentValidation;
using Mediator;
using Trinica.Entities.Gameplay;
using Trinica.Entities.Users;

namespace Trinica.UseCases.Gameplay;

public class TakeCardsToHandCommandHandler : ICommandHandler<TakeCardsToHandCommand, Result>
{
    private readonly IRepository<User, UserId> _userRepository;
    private readonly IRepository<Game, GameId> _gameRepository;
    private readonly IPublisher _publisher;

    public TakeCardsToHandCommandHandler(
        IRepository<User, UserId> userRepository,
        IRepository<Game, GameId> gameRepository,
        IPublisher publisher)
    {
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _publisher = publisher;
    }

    public async ValueTask<Result> Handle(TakeCardsToHandCommand command, CancellationToken ct)
    {
        var result = Result.Success();

        var user = await _userRepository.Get(new UserId(command.PlayerId), result);
        var game = await _gameRepository.Get(new GameId(command.GameId), result);
        if (!game.TakeCardsToHand(user.Id, command.CardsToTake.ToCardsToTake()))
            return result.Fail();

        game.CalculateLayDownOrderPerPlayer(CardType_ToString_Converter.ToTypeString);

        await _gameRepository.Save(game, result);
        await _publisher.PublishEvents(game);

        return result;
    }
}

public record TakeCardsToHandCommand(string GameId, string PlayerId, string[] CardsToTake) : ICommand<Result>;

public class TakeCardsToHandCommandValidator : AbstractValidator<TakeCardsToHandCommand>
{
    public TakeCardsToHandCommandValidator() {}
}
