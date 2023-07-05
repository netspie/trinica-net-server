﻿using Trinica.Entities.HeroCards;
using Trinica.Entities.Shared;

namespace Trinica.Entities.Gameplay.Cards;

public class HeroCard : ICard, ICardWithSlots, ICardWithStats, ICombatCard, ICardWithItems
{
    public HeroCardId Id { get; private set; }
    public StatisticPointGroup Statistics { get; private set; }
    public SlotGroup Slots { get; private set; }

    public List<IEffect> Effects { get; private set; } = new();

    public HeroCard(
       HeroCardId id,
       StatisticPointGroup statistics)
    {
        Id = id;
        Statistics = statistics;
    }

    public List<ItemCard> ItemCards => Slots.ItemCards;

    CardId ICard.Id => Id;
    CardId ICombatCard.Id => Id;

    public bool DoesPowerDamage(int skillIndex)
    {
        var skillCard = Slots.SkillCards[skillIndex];
        return skillCard.DoesDamage;
    }

    public IEffect[] GetEffects(int skillIndex)
    {
        var skillCard = Slots.SkillCards[skillIndex];
        return skillCard.Effects;
    }
}
