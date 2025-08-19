using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ── Singleton(씬에 1개) ─────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Game State (라이트 버전) ────────────────────────────────
    public enum GameState { Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    // 상태 변경을 구독자(UI 등)에 알림
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
        // 필요하면 DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetState(GameState.Playing); // 시작 상태 보장
    }

    void Update()
    {
        // Day2에서 만든 ESC 토글 유지
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (CurrentState == GameState.Playing) Pause();
            else if (CurrentState == GameState.Paused) Resume();
        }
    }

    // ── Public Controls ─────────────────────────────────────────
    public void Pause() => SetState(GameState.Paused);
    public void Resume() => SetState(GameState.Playing);

    // 플레이어가 죽었을 때 한 번만 호출
    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return; // 중복 방지
        SetState(GameState.GameOver);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // 리로드 전 복구(중요)
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    // ── Core: 상태 변경 공통 처리(시간/이벤트) ──────────────────
    private void SetState(GameState newState)
    {
        CurrentState = newState;

        // 상태별 timeScale
        if (newState == GameState.Paused || newState == GameState.GameOver)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

        OnStateChanged?.Invoke(newState);
    }
}
