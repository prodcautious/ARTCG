using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PackOpener : MonoBehaviour
{
    public CardDisplay cardPrefab;
    public Transform cardContainer;
    public GameObject cardRevealOverlay;

    private bool cardIsOpen;

    public bool CardIsOpen => cardIsOpen;

    public void OpenPack()
    {
        if (cardIsOpen)
        {
            return;
        }

        if (CardInventory.Instance == null)
        {
            Debug.LogError(
                "No CardInventory exists. Start the game from the Main scene."
            );
            return;
        }

        CardData selectedCard =
            CardInventory.Instance.GetRandomUnownedCard();

        if (selectedCard == null)
        {
            Debug.Log("Every card has already been collected!");
            return;
        }

        bool cardWasAdded =
            CardInventory.Instance.AddCard(selectedCard);

        if (!cardWasAdded)
        {
            Debug.LogWarning(
                $"{selectedCard.cardName} is already in the inventory."
            );
            return;
        }

        ClearPulledCards();

        CardDisplay newCard =
            Instantiate(cardPrefab, cardContainer, false);

        RectTransform cardRect =
            newCard.GetComponent<RectTransform>();

        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localRotation = Quaternion.identity;
        cardRect.localScale = Vector3.one * 2.5f;

        newCard.SetCard(selectedCard);
        AddCardDepthEffect(newCard);

        cardIsOpen = true;

        if (cardRevealOverlay != null)
        {
            cardRevealOverlay.SetActive(true);
        }

    }

    public void BackFromCard()
    {
        ClearPulledCards();
        cardIsOpen = false;

        if (cardRevealOverlay != null)
        {
            cardRevealOverlay.SetActive(false);
        }

        SceneManager.LoadScene("Main");
    }

    private void ClearPulledCards()
    {
        for (int i = cardContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(cardContainer.GetChild(i).gameObject);
        }
    }

    private void AddCardDepthEffect(CardDisplay card)
    {
        if (card.backgroundImage != null)
        {
            Shadow shadow =
                card.backgroundImage.GetComponent<Shadow>();

            if (shadow == null)
            {
                shadow =
                    card.backgroundImage.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            shadow.effectDistance = new Vector2(3f, -4f);
            shadow.useGraphicAlpha = true;
            card.backgroundImage.raycastTarget = true;
        }

        if (card.GetComponent<CardTiltEffect>() == null)
        {
            card.gameObject.AddComponent<CardTiltEffect>();
        }
    }
}
