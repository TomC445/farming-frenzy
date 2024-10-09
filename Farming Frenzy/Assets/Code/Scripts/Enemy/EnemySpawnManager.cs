using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    #region Editor Fields
    [Header("Enemies")]
    [SerializeField] private GameObject _enemyPrefab;
    [Header("Spawn Position")]
    [SerializeField] private Transform _spawnPositions;
    #endregion

    #region Properties

    #endregion

    #region Singleton
    public static EnemySpawnManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Methods

    public void SpawnEnemies(int enemyNumber)
    {
        var randomIndex = Random.Range(0, _spawnPositions.childCount);
        var spawnPosition = _spawnPositions.GetChild(randomIndex);
        for (int i = 0; i < enemyNumber; i++)
        {
            Instantiate(_enemyPrefab, spawnPosition.position, Quaternion.identity);
        }
    }
    #endregion
}
