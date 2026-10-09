using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DefaultObserverEventHandler))]
public class TradeCardTarget : MonoBehaviour
{
    [Tooltip("Must match the cardId in CardInventory, for example: earthworm")]
    [SerializeField] private string cardId;

    [Tooltip("Card presentation used by the pack-opening reveal.")]
    [SerializeField] private CardDisplay cardPrefab;

    [Tooltip("Seconds the scan result is shown on screen.")]
    [SerializeField] private float messageDuration = 3f;

    private DefaultObserverEventHandler eventHandler;
    private static GameObject activeRevealCanvas;
    private string message;
    private float hideMessageAt;

    private void Awake()
    {
        eventHandler = GetComponent<DefaultObserverEventHandler>();
        eventHandler.OnTargetFound.AddListener(UnlockCard);
    }

    private void OnDestroy()
    {
        if (eventHandler != null)
        {
            eventHandler.OnTargetFound.RemoveListener(UnlockCard);
        }
    }

    private void UnlockCard()
    {
        CardInventory inventory = CardInventory.Instance;

        if (inventory == null)
        {
            ShowMessage("Start from the Main scene before scanning.");
            return;
        }

        CardData card = inventory.GetCard(cardId);

        if (card == null)
        {
            ShowMessage($"No card uses the id '{cardId}'.");
            return;
        }

        if (inventory.AddCard(card))
        {
            ShowCardReveal(card);
        }
        else
        {
            ShowMessage($"You already own {card.cardName}.");
        }
    }

    private void ShowCardReveal(CardData card)
    {
        if (cardPrefab == null)
        {
            ShowMessage($"Received {card.cardName}!");
            return;
        }

        if (activeRevealCanvas != null)
        {
            Destroy(activeRevealCanvas);
        }

        activeRevealCanvas = new GameObject(
            "TradeCardRevealCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas revealCanvas = activeRevealCanvas.GetComponent<Canvas>();
        revealCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        revealCanvas.sortingOrder = 100;

        CanvasScaler canvasScaler =
            activeRevealCanvas.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080f, 1920f);
        canvasScaler.matchWidthOrHeight = 0f;
        canvasScaler.referencePixelsPerUnit = 16f;

        GameObject dismissArea = new GameObject(
            "DismissArea",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));

        RectTransform overlayRect =
            dismissArea.GetComponent<RectTransform>();
        overlayRect.SetParent(activeRevealCanvas.transform, false);
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = dismissArea.GetComponent<Image>();
        overlayImage.color = Color.clear;
        overlayImage.raycastTarget = true;

        Button dismissButton = dismissArea.GetComponent<Button>();
        dismissButton.transition = Selectable.Transition.None;
        dismissButton.onClick.AddListener(DismissReveal);

        CardDisplay revealedCard =
            Instantiate(cardPrefab, overlayRect, false);
        RectTransform cardRect = revealedCard.GetComponent<RectTransform>();
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localRotation = Quaternion.identity;
        cardRect.localScale = Vector3.one * 2.5f;

        revealedCard.SetCard(card);
        AddCardDepthEffect(revealedCard);
    }

    private void DismissReveal()
    {
        if (activeRevealCanvas != null)
        {
            Destroy(activeRevealCanvas);
            activeRevealCanvas = null;
        }
    }

    private static void AddCardDepthEffect(CardDisplay card)
    {
        if (card.backgroundImage != null)
        {
            Shadow shadow = card.backgroundImage.GetComponent<Shadow>();

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

    private void ShowMessage(string text)
    {
        message = text;
        hideMessageAt = Time.unscaledTime + messageDuration;
        Debug.Log($"Trade scan: {text}");
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(message) || Time.unscaledTime >= hideMessageAt)
        {
            return;
        }

        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(Screen.height * 0.025f),
            wordWrap = true
        };

        float width = Screen.width * 0.8f;
        float height = Screen.height * 0.09f;
        Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.08f, width, height);
        GUI.Box(rect, message, style);
    }
}
