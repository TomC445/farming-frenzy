using System;
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
        private bool _isPaused = false;
        private bool _isTimerRunning = false;
        private float _time;
        private int _dayCount;
        private int _weekCount;
        private int _currentQuotaPayment;
        public int Quota => _quota;
        public bool IsTimerRunning => _isTimerRunning;
        private string[] _days = {"Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"};
        #endregion

        private void Start()
        {
            _isTimerRunning = true;
            _quotaText.text = $"0/{_quota}";
        }

        #region Methods
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!_isPaused) { PauseGame(); }
                else { ResumeGame(); }
            }

            if (!_isTimerRunning) return;
            _time += Time.deltaTime;
            UpdateTimerDisplay();
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
            if (_time >= (_dayCount + 1) * _dayTime)
            {
                _dayNightAnimator.SetTrigger(NightTime);
                _dayCount++;
                if(_dayCount % _enemySpawnFrequency == 0)
                {
                    var numEnemies = Random.Range(_enemyDifficulty, _enemyDifficulty + 2) * Math.Max(1, _dayCount / 7);
                    EnemySpawnManager.Instance.SpawnEnemies(numEnemies);
                }
                if (_dayCount % 7 == 0)
                {
                    _weekCount++;
                    CheckGameOver();
                }
            }

            var clockFaceAngle = (Mathf.FloorToInt(_time) / _dayTime) * 360;
            _clockHand.transform.eulerAngles = new Vector3(0, 0, -clockFaceAngle);
            _dayText.text = $"{_days[_dayCount % 7]}";
            _weekText.text = $"Week:{_weekCount + 1:0}";
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
            if (!PlayerController.Instance.TryPurchase(amount)) return;

            AudioManager.Instance.PlaySFX("kaching");
            _currentQuotaPayment += amount;
            _quotaText.text = $"{_currentQuotaPayment}/{_quota}";
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
                    AudioManager.Instance.PlaySFX("gameOver");
                    _gameOverMenu.SetActive(true);
                    return;
                }
            }

            // Setup next quota
            _quota *= _quotaIncreaseRate;
            _currentQuotaPayment = 0;
            _quotaText.text = $"{_currentQuotaPayment}/{_quota}";
        }
        #endregion
    }
}
