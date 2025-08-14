using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject HUD_Group;      // Playing������ ����
    public GameObject PausePanel;     // Paused���� ����
    public GameObject GameOverPanel;  // GameOver���� ����

    // (+��) ����: GameOver ���̵� �ο�
    public FadeCanvasGroup gameOverFader; // GameOverPanel�� CanvasGroup(+FadeCanvasGroup) ����

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;

        // ���� ���� �ݿ�
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
            // �⺻ ���
            GameOverPanel.SetActive(over);

            // (+��) ���̵� ��: ���� ���¿����� ������ �ϹǷ� unscaled time ���
            if (over && gameOverFader != null)
                gameOverFader.FadeInUnscaled(targetAlpha: 1f, duration: 0.35f, interactableAtEnd: true);
        }

        // Ŀ�� ����: UI �г��� ���� ���� ���̰�
        bool showCursor = paused || over;
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // ���� Buttons ������������������������������������������������������������������������������������������������
    public void Btn_Resume() => GameManager.Instance?.Resume();
    public void Btn_Restart() => GameManager.Instance?.RestartLevel();
}
