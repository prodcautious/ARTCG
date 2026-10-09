using UnityEngine;
using UnityEngine.UI;

public class CardDictionaryDisplay : MonoBehaviour
{
    public CardDisplay cardPrefab;
    public Transform content;
    public GameObject detailOverlay;
    public CardDisplay detailCard;

    private void Start()
    {
        if (CardInventory.Instance == null)
        {
            Debug.LogError("No CardInventory exists. Start the game from the Main scene.");
            return;
        }

        if (detailOverlay != null)
        {
            detailOverlay.SetActive(false);
        }

        foreach (CardData card in CardInventory.Instance.OwnedCards)
        {
            CardDisplay display =
                Instantiate(cardPrefab, content, false);

            display.SetCard(card);

            Button button = display.GetComponent<Button>();

            if (button == null)
            {
                button = display.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = display.backgroundImage;
            button.onClick.AddListener(() => ShowCard(card));
        }
    }

    public void ShowCard(CardData card)
    {
        if (detailOverlay == null || detailCard == null)
        {
            return;
        }

        detailCard.SetCard(card);
        detailOverlay.SetActive(true);
    }

    public void CloseCard()
    {
        if (detailOverlay != null)
        {
            detailOverlay.SetActive(false);
        }
    }
}
