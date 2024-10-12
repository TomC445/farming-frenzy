using System;
using System.Linq;
using Code.Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Code.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        private static readonly int NightTime = Animator.StringToHash("NightTime");

        #region Editor Fields
        [Header("Menus")]
        [SerializeField] private GameObject _pauseMenu;
        [SerializeField] private GameObject _gameOverMenu;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private TextMeshProUGUI _weekText;
        [SerializeField] private TextMeshProUGUI _quotaText;
        [SerializeField] private Image _clockHand;
        [SerializeField] private Animator _dayNightAnimator;
        [Header("Game Options")]
        [SerializeField] private int _quota;
        [SerializeField] private int _quotaIncreaseRate;
        [SerializeField] private float _timerRate;
        [SerializeField] private float _dayTime; [Tooltip("Daytime in Seconds")]
        [SerializeField] private int _enemyDifficulty;
        [SerializeField] private int _enemySpawnFrequency;
        #endregion

        #region Properties
        private bool _isPaused;
        private float _time;
        private int _dayCount;
        private int _weekCount;
        private int _currentQuotaPayment;
        private bool _animationStarted;
        private bool IsTimerRunning { get; set; }

        public int _goats;
        private string[] _days = {"Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"};
        #endregion

        private void Start()
        {
            AudioManager.Instance.SetInitialMusicVolume();
            IsTimerRunning = true;
            _quotaText.text = $"{_quota}G";
            _goats = 0;
        }

        #region Methods
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!_isPaused) { PauseGame(); }
                else { ResumeGame(); }
            }

            if (!IsTimerRunning) return;
            _time += Time.deltaTime;
            UpdateTimerDisplay();
            UpdateGoatCount();
            UpdateDate();
        }

        private void UpdateTimerDisplay()
        {
            float minutes = Mathf.FloorToInt(_time / 60);
            float seconds = Mathf.FloorToInt(_time % 60);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void UpdateDate()
        {
            var nextDayTime = (_dayCount + 1) * _dayTime;

            // Start animation 3s before
            if (!_animationStarted && _time >= nextDayTime - 3)
            {
                _animationStarted = true;
                _dayNightAnimator.SetTrigger(NightTime);
            }

            if (_time >= nextDayTime)
            {
                _dayCount++;
                AudioManager.Instance.PlaySFX("rooster");
                _animationStarted = false;

                // Spawn only once every $period days
                var rightDayForSpawn = _dayCount % _enemySpawnFrequency == 0;

                // TODO this could only be in easy-med mode ?
                // Only spawn on Thursday and Saturday in Week 1 to reduce load on player
                var gracePeriod = _dayCount <= 7;
                var reducedSpawnDay = gracePeriod && _dayCount is not (3 or 5);

                if(rightDayForSpawn && !reducedSpawnDay)
                {
                    var week = Mathf.CeilToInt(_dayCount / 7.0f);
                    var numEnemies = Random.Range(_enemyDifficulty, _enemyDifficulty + 1) * Mathf.RoundToInt((float) Math.Pow(week, 2));
                    //AudioManager.Instance.IncreaseGoats(numEnemies);
                    EnemySpawnManager.Instance.SpawnEnemies(numEnemies);
                }

                if (_dayCount % 7 == 0)
                {
                    _weekCount++;
                    CheckGameOver();
                }
            }

            var clockFaceAngle = Mathf.FloorToInt(_time) / _dayTime * 360;
            _clockHand.transform.eulerAngles = new Vector3(0, 0, -clockFaceAngle);
            _dayText.text = $"{_days[_dayCount % 7]}";
            _weekText.text = $"Week:{_weekCount + 1:0}";
        }

        private void UpdateGoatCount() {
            var allGameObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            // Count how many of them have the name "Enemy(Clone)"
            var enemyCount = allGameObjects.Count(obj => obj.name.StartsWith("Enemy"));
            _goats = enemyCount;
        }

        private void PauseGame()
        {
            _pauseMenu.gameObject.SetActive(true);
            Time.timeScale = 0f;
            _isPaused = true;
        }

        public void ResumeGame()
        {
            _pauseMenu.gameObject.SetActive(false);
            Time.timeScale = 1f;
            _isPaused = false;
        }

        public void PayQuota(int amount)
        {
            if (_currentQuotaPayment >= _quota) return;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                var maxCanBuy = Math.Min(_quota - _currentQuotaPayment, PlayerController.Instance.Money);
                print($"yes keydown, max can buy is {maxCanBuy}");
                amount = maxCanBuy;
            }

            if (amount == 0 || !PlayerController.Instance.TryPurchase(amount)) return;

            AudioManager.Instance.PlaySFX("kaching");
            _currentQuotaPayment += amount;
            _quotaText.text = $"{_quota-_currentQuotaPayment}G";
        }

        private void CheckGameOver()
        {
            var diff = _quota - _currentQuotaPayment;

            // Player hasn't paid enough
            if (diff > 0)
            {
                if (PlayerController.Instance.Money >= diff)
                {
                    // Just pay for them and carry on
                    PlayerController.Instance.Purchase(diff);
                }
                else
                {
                    // Not enough to pay - game over!
                    Time.timeScale = 0f;
                    AudioManager.Instance.ToggleMusic();
                    AudioManager.Instance.ToggleSFX();
                    AudioManager.Instance.PlaySFX("gameOver");
                    _gameOverMenu.SetActive(true);
                    return;
                }
            }

            // Setup next quota
            _quota *= _quotaIncreaseRate;
            _currentQuotaPayment = 0;
            _quotaText.text = $"{_quota-_currentQuotaPayment}G";
        }
        #endregion
    }
}
