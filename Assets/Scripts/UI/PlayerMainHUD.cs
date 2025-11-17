using UnityEngine;
using UnityEngine.UI;
using Util;
using Player;
using TMPro;
using Event;

namespace UI
{
    public class PlayerMainHUD : MonoBehaviour
    {
        [Header("- UI -")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider experienceSlider;
        [SerializeField] private TextMeshProUGUI currentLevel;
        [SerializeField] private TextMeshProUGUI totalExp;
        [SerializeField] private TextMeshProUGUI totalTime;

        [Header("- Dependencies -")]
        [SerializeField] private PlayerController playerController;
        private PlayerStats playerStats;

        private float timeElapsed = 0f;
        private bool isGameRunning = false;

        #region 
        private void Awake()
        {
            playerStats = playerController.GetStats();

            //Game
            EventBus.Subscribe<OnGameStart>(OnGameStart);
            EventBus.Subscribe<OnGameEnd>(OnGameEnd);

            //Player
            EventBus.Subscribe<OnPlayerHit>(_ => RefreshStats());
            EventBus.Subscribe<OnPlayerDeath>(_ => RefreshStats());
            EventBus.Subscribe<OnPlayerAcquireExp>(_ => RefreshStats());
            EventBus.Subscribe<OnPlayerLevelUp>(_ => RefreshStats());
        }

        private void Start()
        {
            RefreshStats();
        }

        private void Update()
        {
            if (isGameRunning)
            {
                timeElapsed += Time.deltaTime;
                UpdateTotalTimeUI(timeElapsed);
            }
        }

        private void OnDestroy()
        {
            //Game
            EventBus.Unsubscribe<OnGameStart>(OnGameStart);
            EventBus.Unsubscribe<OnGameEnd>(OnGameEnd);

            //Player
            EventBus.Unsubscribe<OnPlayerHit>(_ => RefreshStats());
            EventBus.Unsubscribe<OnPlayerDeath>(_ => RefreshStats());
            EventBus.Subscribe<OnPlayerAcquireExp>(_ => RefreshStats());
            EventBus.Subscribe<OnPlayerLevelUp>(_ => RefreshStats());
        }
        #endregion


        public void OnGameStart(OnGameStart onGameStart)
        {
            isGameRunning = true;
            timeElapsed = 0f;

            canvasGroup.SetGroupState(true);
        }

        public void OnGameEnd(OnGameEnd onGameEnd)
        {
            isGameRunning = false;

            canvasGroup.SetGroupState(true);
        }

        private void RefreshStats()
        {
            currentLevel.text = $"Lv. {playerStats.CurrentLevel}";
            totalExp.text = playerStats.TotalExperience.ToString();

            healthSlider.maxValue = playerStats.MaxHealth;
            healthSlider.value = playerStats.CurrentHealth;
            experienceSlider.value = playerStats.GetCurrentLevelProgress();
        }

        private void UpdateTotalTimeUI(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            totalTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}