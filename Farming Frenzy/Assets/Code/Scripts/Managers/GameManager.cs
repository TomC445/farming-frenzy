using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Editor Fields
    [Header("Menus")]
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _dateText;
    [SerializeField] private Image _clockHand;
    [Header("Game Options")]
    [SerializeField] private int _quota;
    [SerializeField] private float _timerRate;
    [SerializeField] private float _dayTime; [Tooltip("Daytime in Seconds")]
    #endregion

    #region Properties
    private bool _isPaused = false;
    private bool _isTimerRunning = false;
    private float _time;
    private int _dayCount;
    private int _weekCount;
    public int Quota => _quota;
    public bool IsTimerRunning => _isTimerRunning;
    private string[] _days = {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};
    #endregion

    private void Start()
    {
        _isTimerRunning = true;
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
            _dayCount++;
            if (_dayCount % 7 == 0)
            {
                _weekCount++;
            }
        }

        var _clockFaceAngle = (Mathf.FloorToInt(_time) / _dayTime) * 360;
        _clockHand.transform.eulerAngles = new Vector3(0, 0, -_clockFaceAngle);
        _dateText.text = string.Format("{0}\nWeek:{1:0}", _days[_dayCount], _weekCount+1);
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
    #endregion
}
