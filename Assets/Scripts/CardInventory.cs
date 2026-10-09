using System.Collections.Generic;
using UnityEngine;

public class CardInventory : MonoBehaviour
{
    public static CardInventory Instance { get; private set; }

    public List<CardData> allCards;

    private readonly List<CardData> ownedCards = new();

    public IReadOnlyList<CardData> OwnedCards => ownedCards;

    public bool HasCollectedAllCards
    {
        get
        {
            if (allCards == null || allCards.Count == 0)
            {
                return false;
            }

            bool hasAtLeastOneCard = false;

            foreach (CardData card in allCards)
            {
                if (card == null)
                {
                    continue;
                }

                hasAtLeastOneCard = true;

                if (!OwnsCard(card.cardId))
                {
                    return false;
                }
            }

            return hasAtLeastOneCard;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool OwnsCard(string cardId)
    {
        return ownedCards.Exists(card => card.cardId == cardId);
    }

    public bool AddCard(CardData card)
    {
        if (card == null || OwnsCard(card.cardId))
        {
            return false;
        }

        ownedCards.Add(card);
        AudioManager.Instance.PlayCardReceived();
        return true;
    }

    public CardData GetCard(string cardId)
    {
        if (string.IsNullOrWhiteSpace(cardId) || allCards == null)
        {
            return null;
        }

        return allCards.Find(card => card != null && card.cardId == cardId);
    }

    public bool AddCard(string cardId)
    {
        return AddCard(GetCard(cardId));
    }

    public CardData GetRandomUnownedCard()
    {
        List<CardData> unownedCards =
            allCards.FindAll(card => !OwnsCard(card.cardId));

        if (unownedCards.Count == 0)
        {
            return null;
        }

        return unownedCards[Random.Range(0, unownedCards.Count)];
    }
}
