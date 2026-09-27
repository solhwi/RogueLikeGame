using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RogueLike.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        LevelUp,
        GameOver,
        LevelComplete
    }

    public class GameManager : Singleton<GameManager>
    {
        [Header("Lives")]
        [SerializeField] private int startingLives = 3;

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnLivesChanged;

        public GameState State { get; private set; } = GameState.Playing;
        public int Score { get; private set; }
        public int Lives { get; private set; }

        private Vector3? checkpointPosition;

        protected override void Awake()
        {
            base.Awake();
            Lives = startingLives;
        }

        private void Update()
        {
            if (State == GameState.GameOver)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        public void SetCheckpoint(Vector3 position)
        {
            checkpointPosition = position;
        }

        public bool TryGetCheckpoint(out Vector3 position)
        {
            position = checkpointPosition ?? Vector3.zero;
            return checkpointPosition.HasValue;
        }

        public void LoseLife()
        {
            Lives = Mathf.Max(0, Lives - 1);
            OnLivesChanged?.Invoke(Lives);

            if (Lives <= 0)
            {
                SetState(GameState.GameOver);
            }
        }

        public void SetState(GameState newState)
        {
            if (State == newState)
            {
                return;
            }

            State = newState;
            Time.timeScale = (newState == GameState.Paused || newState == GameState.LevelUp) ? 0f : 1f;
            OnStateChanged?.Invoke(newState);
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
            else if (State == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        public void EnterLevelUpSelection()
        {
            if (State != GameState.Playing)
            {
                return;
            }
            SetState(GameState.LevelUp);
        }

        public void ExitLevelUpSelection()
        {
            if (State != GameState.LevelUp)
            {
                return;
            }
            SetState(GameState.Playing);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            Lives = startingLives;
            Score = 0;
            checkpointPosition = null;
            SetState(GameState.Playing);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
