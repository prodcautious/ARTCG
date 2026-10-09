using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CardTiltEffect : MonoBehaviour,
    IPointerMoveHandler,
    IPointerExitHandler,
    IPointerUpHandler
{
    [Header("Tilt")]
    [SerializeField] private float maxTilt = 8f;
    [SerializeField] private float smoothSpeed = 12f;

    [Header("Depth")]
    [SerializeField] private Shadow cardShadow;
    [SerializeField] private Vector2 restingShadow = new Vector2(3f, -4f);
    [SerializeField] private float shadowMovement = 0.35f;

    private RectTransform cardRect;
    private Vector2 targetTilt;
    private Vector2 targetShadow;

    private void Awake()
    {
        cardRect = (RectTransform)transform;
        targetShadow = restingShadow;

        if (cardShadow == null)
        {
            cardShadow = GetComponentInChildren<Shadow>();
        }
    }

    private void OnEnable()
    {
        targetTilt = Vector2.zero;
        targetShadow = restingShadow;
        transform.localRotation = Quaternion.identity;

        if (cardShadow != null)
        {
            cardShadow.effectDistance = restingShadow;
        }
    }

    private void Update()
    {
        Quaternion targetRotation = Quaternion.Euler(targetTilt.x, targetTilt.y, 0f);
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            smoothSpeed * Time.unscaledDeltaTime);

        if (cardShadow != null)
        {
            cardShadow.effectDistance = Vector2.Lerp(
                cardShadow.effectDistance,
                targetShadow,
                smoothSpeed * Time.unscaledDeltaTime);
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (cardRect == null)
        {
            cardRect = (RectTransform)transform;
        }

        Vector2 localPoint;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cardRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint))
        {
            return;
        }

        Vector2 halfSize = cardRect.rect.size * 0.5f;
        float horizontal = Mathf.Clamp(localPoint.x / halfSize.x, -1f, 1f);
        float vertical = Mathf.Clamp(localPoint.y / halfSize.y, -1f, 1f);

        targetTilt = new Vector2(vertical * maxTilt, -horizontal * maxTilt);
        targetShadow = restingShadow + new Vector2(
            horizontal * maxTilt * shadowMovement,
            vertical * maxTilt * shadowMovement);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetTilt();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetTilt();
    }

    private void OnDisable()
    {
        ResetTilt();
        transform.localRotation = Quaternion.identity;
    }

    private void ResetTilt()
    {
        targetTilt = Vector2.zero;
        targetShadow = restingShadow;
    }
}
