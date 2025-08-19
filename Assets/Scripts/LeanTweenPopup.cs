using UnityEngine;
using UnityEngine.UI; // ��ư �����(����)

public class LeanTweenPopup : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] RectTransform target;      // Ʈ���� RectTransform (���� �� ��ũ��Ʈ�� ���� ������Ʈ)
    [SerializeField] CanvasGroup canvasGroup;   // ���̵�/�Է� ����(������ �ڵ� �߰�)

    [Header("Animation")]
    [SerializeField] float duration = 0.25f;
    [SerializeField] Vector3 hiddenScale = new Vector3(0.85f, 0.85f, 1f);
    [SerializeField] Vector3 shownScale = Vector3.one;
    [SerializeField] bool useUnscaledTime = true; // �Ͻ����� �߿��� �����ϰ�

    [Header("Start State")]
    [SerializeField] bool startHidden = true;   // ���� �� ���� ���·� ���� ����

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

        // �Է� ���
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        // ���۰� ����
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

        // ������ ���� Ŭ�� ����
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

        // target�� �ɸ� �ܿ� Ʈ���� ������ �����ϰ� �� �� �� ĵ��
        LeanTween.cancel(gameObject);
    }
}
