﻿using Corelibs.Basic.Blocks;
using Corelibs.Basic.Collections;
using Corelibs.Basic.DDD;
using Corelibs.Basic.Repository;
using Corelibs.Basic.UseCases;
using Mediator;
using Trinica.Entities.Decks;
using Trinica.Entities.Gameplay;
using Trinica.Entities.Gameplay.Cards;
using Trinica.Entities.Users;

namespace Trinica.UseCases.Gameplay;

public class StartBotGameCommandHandler : ICommandHandler<StartBotGameCommand, Result>
{
    private readonly IRepository<User, UserId> _userRepository;
    private readonly IRepository<Game, GameId> _gameRepository;
    private readonly IBotHub _botHub;
    private readonly IPublisher _publisher;

    public StartBotGameCommandHandler(
        IBotHub botHub,
        IRepository<User, UserId> userRepository,
        IRepository<Game, GameId> gameRepository,
        IPublisher publisher)
    {
        _botHub = botHub;
        _userRepository = userRepository;
        _gameRepository = gameRepository;
        _publisher = publisher;
    }

    public async ValueTask<Result> Handle(StartBotGameCommand command, CancellationToken ct)
    {
        var result = Result.Success();

        var user = await _userRepository.Get(new UserId(command.PlayerId), result);

        Game game;
        if (user.LastGameId is not null)
        {
            var getGameResult = await _gameRepository.GetBy(user.LastGameId);
            game = getGameResult.Get();
            if (result.IsSuccess && game is not null)
                return result.Fail("Can't start another game while already in one.");
        }

        var decks = GetDecks();

        var player = CreatePlayer(decks.Item1.Item1, decks.Item1.Item2, user.Id);
        var bot = CreatePlayer(decks.Item2.Item1, decks.Item2.Item2);
        game = new Game(EntityId.New<GameId>(), [ player, bot ]);
        if (!game.StartGame(player.Id))
            return result.Fail();

        if (!game.StartGame(bot.Id))
            return result.Fail();

        if (!game.TakeCardsToCommonPool())
            return result.Fail();

        await _botHub.AddGame(bot.Id, game.Id, game.ActionController);

        await _gameRepository.Save(game, result);

        user.ChangeLastGame(game.Id);
        await _userRepository.Save(user, result);

        await _publisher.PublishEvents(game);

        return result;
    }

    public static Player CreatePlayer(HeroCard hero, FieldDeck fieldDeck, UserId userId = null) 
    {
        var statistics = new StatisticPointGroup(
            attack: new(10),
            hp: new(10),
            speed: new(10),
            power: new(10));

        return new Player(userId ?? EntityId.New<UserId>(), EntityId.New<DeckId>(), hero, fieldDeck);
    }

    public static ((HeroCard, FieldDeck), (HeroCard, FieldDeck)) GetDecks()
    {
        var all = DefaultCards.All.Shuffle().ToRemoveOnlyList();

        var hero1 = all.Take(c => c is HeroCard) as HeroCard;
        var hero2 = all.Take(c => c is HeroCard) as HeroCard;

        var allButNoHeroes = all.Where(c => c is not HeroCard).ToArray();

        var count = allButNoHeroes.Length;
        var halfCards1 = allButNoHeroes.Take(count / 2).ToArray();
        var halfCards2 = allButNoHeroes.Skip(count / 2).ToArray();

        var deck1 = new FieldDeck(halfCards1);
        var deck2 = new FieldDeck(halfCards2);

        return ((hero1, deck1), (hero2, deck2));
    }
}

public record StartBotGameCommand(string PlayerId) : ICommand<Result>;

public class StartBotGameValidator : UserRequestValidator<StartBotGameCommand>
{
    //public StartBotGameValidator(IAccessorAsync<ClaimsPrincipal> userAccessor) : base(userAccessor) { }
}
