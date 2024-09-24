using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Editor Fields
    [Header("Menus")]
    [SerializeField] private GameObject _pauseMenu;
    #endregion

    #region Properties
    private bool _isPaused = false;
    #endregion

    #region Methods
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_isPaused) { PauseGame(); }
            else { ResumeGame(); }
        }
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
