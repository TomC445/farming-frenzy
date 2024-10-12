using Code.Scripts.Plants;
using Code.Scripts.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Autoharvester : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private int _harvestSeconds;
    [SerializeField] private int _harvestRadius = 2;
    #endregion

    #region Properties
    private Animator _animator;
    #endregion

    #region Methods
    private void Start()
    {
        _animator = GetComponent<Animator>();
        StartCoroutine(HarvestCoroutine());
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingOrder = 10000 - Mathf.CeilToInt(gameObject.transform.position.y);
        }
    }
    private void HarvestSurroundingPlants()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, _harvestRadius);
        foreach (Collider2D collider in hitColliders)
        {
            if(collider.CompareTag("Plant"))
            {
                var plant = collider.gameObject.GetComponent<Plant>();
                plant.Harvest();
            }
        }
    }

    private IEnumerator HarvestCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_harvestSeconds);
            _animator.SetTrigger("Harvest");
            yield return new WaitForSeconds(0.2f);
            HarvestSurroundingPlants();
        }
    }

    private void OnMouseDown()
    {
        
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (PlayerController.Instance.CurrentlyActiveCursor == PlayerController.CursorState.Shovel)
        {
            Destroy(gameObject);
        }
    }
    #endregion
}
