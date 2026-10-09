using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PackSliceController : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private RectTransform topFlap;
    [SerializeField] private Image sliceProgress;
    [SerializeField] private PackOpener packOpener;

    [Header("Slice Gesture")]
    [SerializeField] private float sliceZoneHeight = 120f;
    [SerializeField, Range(0.5f, 1f)]
    private float requiredCoverage = 0.72f;

    [Header("Animation")]
    [SerializeField] private float returnDuration = 0.22f;
    [SerializeField] private float openDuration = 0.28f;

    private RectTransform packRect;
    private Vector2 flapClosedPosition;
    private Quaternion flapClosedRotation;
    private Coroutine activeTween;
    private bool isSlicing;
    private bool hasOpened;
    private float minimumSliceX;
    private float maximumSliceX;
    private float startSliceX;
    private float lastSliceX;

    private void Awake()
    {
        packRect = (RectTransform)transform;

        if (topFlap != null)
        {
            flapClosedPosition = topFlap.anchoredPosition;
            flapClosedRotation = topFlap.localRotation;
        }

        SetSliceProgress(0f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (hasOpened || !TryGetLocalPoint(eventData, out Vector2 localPoint))
        {
            return;
        }

        float sliceLineY = packRect.rect.yMax - sliceZoneHeight;

        if (Mathf.Abs(localPoint.y - sliceLineY) > sliceZoneHeight)
        {
            return;
        }

        if (activeTween != null)
        {
            StopCoroutine(activeTween);
            activeTween = null;
        }

        isSlicing = true;
        minimumSliceX = localPoint.x;
        maximumSliceX = localPoint.x;
        startSliceX = localPoint.x;
        lastSliceX = localPoint.x;
        SetSliceProgress(0f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isSlicing || hasOpened ||
            !TryGetLocalPoint(eventData, out Vector2 localPoint))
        {
            return;
        }

        float sliceLineY = packRect.rect.yMax - sliceZoneHeight;

        if (Mathf.Abs(localPoint.y - sliceLineY) > sliceZoneHeight * 1.25f)
        {
            return;
        }

        float clampedX = Mathf.Clamp(
            localPoint.x,
            packRect.rect.xMin,
            packRect.rect.xMax);

        minimumSliceX = Mathf.Min(minimumSliceX, clampedX);
        maximumSliceX = Mathf.Max(maximumSliceX, clampedX);
        lastSliceX = clampedX;

        float coveredDistance = maximumSliceX - minimumSliceX;
        float requiredDistance = packRect.rect.width * requiredCoverage;
        float progress = Mathf.Clamp01(coveredDistance / requiredDistance);

        SetSliceProgress(progress);
        PreviewFlapMovement(progress, clampedX - startSliceX);

        if (progress >= 1f)
        {
            CompleteSlice();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isSlicing || hasOpened)
        {
            return;
        }

        isSlicing = false;
        StartReturnTween();
    }

    private bool TryGetLocalPoint(
        PointerEventData eventData,
        out Vector2 localPoint)
    {
        if (packRect == null)
        {
            packRect = (RectTransform)transform;
        }

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            packRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);
    }

    private void PreviewFlapMovement(float progress, float horizontalTravel)
    {
        if (topFlap == null)
        {
            return;
        }

        float direction = Mathf.Approximately(horizontalTravel, 0f)
            ? 1f
            : Mathf.Sign(horizontalTravel);

        topFlap.anchoredPosition =
            flapClosedPosition + new Vector2(
                direction * progress * 32f,
                progress * 10f);

        topFlap.localRotation =
            flapClosedRotation * Quaternion.Euler(
                0f,
                0f,
                -direction * progress * 8f);
    }

    private void CompleteSlice()
    {
        isSlicing = false;
        hasOpened = true;
        AudioManager.Instance.PlayPackRip();

        if (activeTween != null)
        {
            StopCoroutine(activeTween);
        }

        activeTween = StartCoroutine(OpenTween());
    }

    private IEnumerator OpenTween()
    {
        Vector2 startPosition = topFlap != null
            ? topFlap.anchoredPosition
            : Vector2.zero;
        Quaternion startRotation = topFlap != null
            ? topFlap.localRotation
            : Quaternion.identity;
        float direction = Mathf.Approximately(lastSliceX - startSliceX, 0f)
            ? 1f
            : Mathf.Sign(lastSliceX - startSliceX);
        Vector2 endPosition = flapClosedPosition + new Vector2(
            direction * packRect.rect.width * 0.7f,
            180f);
        Quaternion endRotation = flapClosedRotation *
            Quaternion.Euler(0f, 0f, -direction * 28f);

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = EaseOutCubic(
                Mathf.Clamp01(elapsed / openDuration));

            if (topFlap != null)
            {
                topFlap.anchoredPosition = Vector2.LerpUnclamped(
                    startPosition,
                    endPosition,
                    amount);
                topFlap.localRotation = Quaternion.SlerpUnclamped(
                    startRotation,
                    endRotation,
                    amount);
            }

            yield return null;
        }

        packOpener.OpenPack();

        if (packOpener.CardIsOpen)
        {
            gameObject.SetActive(false);
        }
        else
        {
            hasOpened = false;
            ResetVisualsImmediately();
        }
    }

    private void StartReturnTween()
    {
        if (activeTween != null)
        {
            StopCoroutine(activeTween);
        }

        activeTween = StartCoroutine(ReturnTween());
    }

    private IEnumerator ReturnTween()
    {
        Vector2 startPosition = topFlap != null
            ? topFlap.anchoredPosition
            : Vector2.zero;
        Quaternion startRotation = topFlap != null
            ? topFlap.localRotation
            : Quaternion.identity;
        float startProgress = sliceProgress != null
            ? sliceProgress.fillAmount
            : 0f;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = EaseOutCubic(
                Mathf.Clamp01(elapsed / returnDuration));

            if (topFlap != null)
            {
                topFlap.anchoredPosition = Vector2.Lerp(
                    startPosition,
                    flapClosedPosition,
                    amount);
                topFlap.localRotation = Quaternion.Slerp(
                    startRotation,
                    flapClosedRotation,
                    amount);
            }

            SetSliceProgress(Mathf.Lerp(startProgress, 0f, amount));
            yield return null;
        }

        ResetVisualsImmediately();
        activeTween = null;
    }

    private void SetSliceProgress(float amount)
    {
        if (sliceProgress != null)
        {
            sliceProgress.fillAmount = amount;
        }
    }

    private void ResetVisualsImmediately()
    {
        if (topFlap != null)
        {
            topFlap.anchoredPosition = flapClosedPosition;
            topFlap.localRotation = flapClosedRotation;
        }

        SetSliceProgress(0f);
    }

    private static float EaseOutCubic(float amount)
    {
        float inverse = 1f - amount;
        return 1f - inverse * inverse * inverse;
    }
}
