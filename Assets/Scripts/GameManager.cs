using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ���� Singleton(���� 1��) ��������������������������������������������������������������������������
    public static GameManager Instance { get; private set; }

    // ���� Game State (����Ʈ ����) ����������������������������������������������������������������
    public enum GameState { Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    // ���� ������ ������(UI ��)�� �˸�
    public event Action<GameState> OnStateChanged;
    public event Action OnReady;

    void Awake()
    {
        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}
        Instance = this;
        OnReady?.Invoke();
        // �ʿ��ϸ� DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetState(GameState.Playing); // ���� ���� ����
    }

    void Update()
    {
        // Day2���� ���� ESC ��� ����
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (CurrentState == GameState.Playing) Pause();
            else if (CurrentState == GameState.Paused) Resume();
        }
    }

    // ���� Public Controls ����������������������������������������������������������������������������������
    public void Pause() => SetState(GameState.Paused);
    public void Resume() => SetState(GameState.Playing);

    // �÷��̾ �׾��� �� �� ���� ȣ��
    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return; // �ߺ� ����
        SetState(GameState.GameOver);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // ���ε� �� ����(�߿�)
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    // ���� Core: ���� ���� ���� ó��(�ð�/�̺�Ʈ) ������������������������������������
    private void SetState(GameState newState)
    {
        CurrentState = newState;

        // ���º� timeScale
        if (newState == GameState.Paused || newState == GameState.GameOver)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

        OnStateChanged?.Invoke(newState);
    }
}
