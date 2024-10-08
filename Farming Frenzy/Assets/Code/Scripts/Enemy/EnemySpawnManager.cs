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

    #region Debug
    public bool _spawnEnemies;
    #endregion

    #region Methods
    private void Update()
    {
        if(_spawnEnemies)
        {
            _spawnEnemies = false;
            SpawnEnemies();
        }
    }

    public void SpawnEnemies()
    {
        var randomIndex = Random.Range(0, _spawnPositions.childCount);
        var spawnPosition = _spawnPositions.GetChild(randomIndex);
        var enemyNumber = Random.Range(0, 4);
        for (int i = 0; i < enemyNumber; i++)
        {
            Instantiate(_enemyPrefab, spawnPosition.position, Quaternion.identity);
        }
    }
    #endregion
}
