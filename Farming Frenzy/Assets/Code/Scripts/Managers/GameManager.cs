using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
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
        _quotaText.text = string.Format("0/{0}", _quota);
    }

    #region Methods
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_isPaused) { PauseGame(); }
            else { ResumeGame(); }
        }

        if (_isTimerRunning)
        {
            _time += Time.deltaTime;
            UpdateTimerDisplay();
            UpdateDate();
        }
    }

    private void UpdateTimerDisplay()
    {
        float minutes = Mathf.FloorToInt(_time / 60);
        float seconds = Mathf.FloorToInt(_time % 60);
        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateDate()
    {
        if (_time >= (_dayCount + 1) * _dayTime)
        {
            _dayNightAnimator.SetTrigger("NightTime");
            _dayCount++;
            if(_dayCount % _enemySpawnFrequency == 0)
            {
                EnemySpawnManager.Instance.SpawnEnemies(Random.Range(_enemyDifficulty, _enemyDifficulty + 2));
            }
            if (_dayCount % 2 == 0)
            {
                _weekCount++;
                CheckGameOver();
            }
        }

        var _clockFaceAngle = (Mathf.FloorToInt(_time) / _dayTime) * 360;
        _clockHand.transform.eulerAngles = new Vector3(0, 0, -_clockFaceAngle);
        _dayText.text = string.Format("{0}", _days[_dayCount%7]);
        _weekText.text = string.Format("Week:{0:0}",_weekCount+1);
    }

    public void PauseGame()
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
        if(PlayerController.Instance.Money < amount)
        {
            return;
        }
        AudioManager.Instance.PlaySFX("kaching");
        _currentQuotaPayment += amount;
        PlayerController.Instance.Purchase(amount);
        _quotaText.text = string.Format("{0}/{1}", _currentQuotaPayment, _quota);
    }

    public void CheckGameOver()
    {
        if (_currentQuotaPayment < _quota)
        {
            Time.timeScale = 0f;
            AudioManager.Instance.ToggleMusic();
            AudioManager.Instance.PlaySFX("gameOver");
            _gameOverMenu.SetActive(true);
        }
        else
        {
            _quota *= _quotaIncreaseRate;
            _currentQuotaPayment = 0;
            _quotaText.text = string.Format("{0}/{1}", _currentQuotaPayment, _quota);
        }
    }
    #endregion
}
