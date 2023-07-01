﻿using Corelibs.Basic.Collections;
using Trinica.Entities.Gameplay.Cards;
using Trinica.Entities.Shared;

namespace Trinica.Entities.Gameplay;

public class SlotGroup
{
    public int MaxSlots { get; private set; }

    public List<ItemCard> ItemCards { get; private set; }
    public List<SkillCard> SkillCards { get; private set; }

    public List<CardId> OrderedCards { get; private set; }

    public bool AddCard(ICard card)
    {
        if (OrderedCards.Count >= MaxSlots)
            return false;

        OrderedCards ??= new();
        if (card is ItemCard itemCard)
        {
            ItemCards ??= new();
            ItemCards.Add(itemCard);
            OrderedCards.Add(itemCard.Id);
        }
        else
        if (card is SkillCard skillCard)
        {
            SkillCards ??= new();
            SkillCards.Add(skillCard);
            OrderedCards.Add(skillCard.Id);
        }

        return true;
    }

    public bool RemoveCard(ICard card)
    {
        if (OrderedCards.IsNullOrEmpty())
            return false;

        OrderedCards ??= new();
        if (card is ItemCard itemCard)
        {
            ItemCards.Remove(itemCard);
            OrderedCards = OrderedCards.Where(c => c.ToString() == itemCard.Id.ToString()).ToList();
        }
        else
        if (card is SkillCard skillCard)
        {
            SkillCards.Remove(skillCard);
            OrderedCards = OrderedCards.Where(c => c.ToString() == skillCard.Id.ToString()).ToList();
        }

        return true;
    }
}

public interface ISlotPolicy
{
    bool CanAdd(CardId source, CardId target);
}