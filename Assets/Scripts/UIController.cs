using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject HUD_Group;      // Playing에서만 보임
    public GameObject PausePanel;     // Paused에서 보임
    public GameObject GameOverPanel;  // GameOver에서 보임

    // (+α) 선택: GameOver 페이드 인용
    public FadeCanvasGroup gameOverFader; // GameOverPanel에 CanvasGroup(+FadeCanvasGroup) 연결

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            //GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
        else
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                //gameManager.OnReady += HandleReady;
            }
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    void Start()
    {
        // 시작 상태 반영
        if (GameManager.Instance != null)
            HandleStateChanged(GameManager.Instance.CurrentState);
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        bool playing = (state == GameManager.GameState.Playing);
        bool paused = (state == GameManager.GameState.Paused);
        bool over = (state == GameManager.GameState.GameOver);

        if (HUD_Group) HUD_Group.SetActive(playing);
        if (PausePanel) PausePanel.SetActive(paused);

        if (GameOverPanel)
        {
            // 기본 토글
            GameOverPanel.SetActive(over);

            // (+α) 페이드 인: 정지 상태에서도 보여야 하므로 unscaled time 사용
            if (over && gameOverFader != null)
                gameOverFader.FadeInUnscaled(targetAlpha: 1f, duration: 0.35f, interactableAtEnd: true);
        }

        // 커서 제어: UI 패널이 있을 때만 보이게
        bool showCursor = paused || over;
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // ── Buttons ────────────────────────────────────────────────
    public void Btn_Resume() => GameManager.Instance?.Resume();
    public void Btn_Restart() => GameManager.Instance?.RestartLevel();

    public void HandleReady()
    {
        GameManager.Instance.OnStateChanged += HandleStateChanged;
    }
}
