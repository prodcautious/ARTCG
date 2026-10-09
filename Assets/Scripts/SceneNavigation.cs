using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneNavigation : MonoBehaviour
{
    private const string PackButtonName = "OpenPacksIconButton";

    private void Start()
    {
        RefreshPackButtonState();
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene("Main");
    }

    public void OpenPackOpening()
    {
        if (CardInventory.Instance != null &&
            CardInventory.Instance.HasCollectedAllCards)
        {
            return;
        }

        SceneManager.LoadScene("PackOpening");
    }

    public void OpenCardDictionary()
    {
        SceneManager.LoadScene("CardDictionary");
    }

    public void OpenTradeScene()
    {
        SceneManager.LoadScene("TradeScene");
    }

    private void RefreshPackButtonState()
    {
        GameObject packButtonObject = GameObject.Find(PackButtonName);

        if (packButtonObject == null)
        {
            return;
        }

        Button packButton = packButtonObject.GetComponent<Button>();

        if (packButton != null)
        {
            packButton.interactable = CardInventory.Instance == null ||
                !CardInventory.Instance.HasCollectedAllCards;
        }
    }
}
