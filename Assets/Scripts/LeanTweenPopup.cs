using UnityEngine;
using UnityEngine.UI; // 버튼 연결용(선택)

public class LeanTweenPopup : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] RectTransform target;      // 트윈할 RectTransform (보통 이 스크립트가 붙은 오브젝트)
    [SerializeField] CanvasGroup canvasGroup;   // 페이드/입력 제어(없으면 자동 추가)

    [Header("Animation")]
    [SerializeField] float duration = 0.25f;
    [SerializeField] Vector3 hiddenScale = new Vector3(0.85f, 0.85f, 1f);
    [SerializeField] Vector3 shownScale = Vector3.one;
    [SerializeField] bool useUnscaledTime = true; // 일시정지 중에도 동작하게

    [Header("Start State")]
    [SerializeField] bool startHidden = true;   // 시작 시 닫힌 상태로 둘지 여부

    bool isOpen;
    LTDescr scaleTween;
    LTDescr fadeTween;

    void Reset()
    {
        target = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Awake()
    {
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (startHidden == true)
        {
            PrepareHidden();
            gameObject.SetActive(false);
        }
        else
        {
            PrepareShown();
        }
    }

    void PrepareHidden()
    {
        target.localScale = hiddenScale;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        isOpen = false;
    }

    void PrepareShown()
    {
        target.localScale = shownScale;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        isOpen = true;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        KillTweens();

        // 입력 허용
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        // 시작값 고정
        target.localScale = hiddenScale;
        canvasGroup.alpha = 0f;

        scaleTween = LeanTween.scale(target, shownScale, duration)
            .setEaseOutBack()
            .setIgnoreTimeScale(useUnscaledTime);

        fadeTween = LeanTween.value(gameObject, 0f, 1f, duration)
            .setOnUpdate(a => canvasGroup.alpha = a)
            .setIgnoreTimeScale(useUnscaledTime)
            .setOnComplete(() => isOpen = true);
    }

    public void Close()
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        KillTweens();

        // 닫히는 동안 클릭 방지
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        scaleTween = LeanTween.scale(target, hiddenScale, duration)
            .setEaseInBack()
            .setIgnoreTimeScale(useUnscaledTime);

        fadeTween = LeanTween.value(gameObject, 1f, 0f, duration)
            .setOnUpdate(a => canvasGroup.alpha = a)
            .setIgnoreTimeScale(useUnscaledTime)
            .setOnComplete(() =>
            {
                isOpen = false;
                gameObject.SetActive(false);
            });
    }

    public void Toggle()
    {
        if (isOpen == true)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    void KillTweens()
    {
        if (scaleTween != null)
        { 
            LeanTween.cancel(scaleTween.id);
            scaleTween = null;
        }
        
        if (fadeTween != null)
        {
            LeanTween.cancel(fadeTween.id);
            fadeTween = null;
        }

        // target에 걸린 잔여 트윈이 있으면 안전하게 한 번 더 캔슬
        LeanTween.cancel(gameObject);
    }
}
