using System.Collections;
using UnityEngine;

/// <summary>
/// CanvasGroup�� unscaled time���� ���̵� ��/�ƿ�.
/// Time.timeScale=0(����) ���¿����� ����.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class FadeCanvasGroup : MonoBehaviour
{
    private CanvasGroup cg;
    Coroutine routine;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetInstant(float alpha, bool interactable)
    {
        if (routine != null) StopCoroutine(routine);
        cg.alpha = Mathf.Clamp01(alpha);
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
    }

    public void FadeInUnscaled(float targetAlpha = 1f, float duration = 0.35f, bool interactableAtEnd = true)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeTo(targetAlpha, duration, interactableAtEnd));
    }

    public void FadeOutUnscaled(float targetAlpha = 0f, float duration = 0.35f, bool interactableAtEnd = false)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeTo(targetAlpha, duration, interactableAtEnd));
    }

    private IEnumerator FadeTo(float targetAlpha, float duration, bool interactableAtEnd)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // ���� �߿��� ����
            float k = Mathf.Clamp01(t / duration);
            cg.alpha = Mathf.Lerp(start, targetAlpha, k);
            yield return null;
        }
        cg.alpha = Mathf.Clamp01(targetAlpha);
        cg.interactable = interactableAtEnd;
        cg.blocksRaycasts = interactableAtEnd;
        routine = null;
    }
}
