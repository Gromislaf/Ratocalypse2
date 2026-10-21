using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    private InventorySystem inventory;

    private void Awake()
    {
        inventory = FindFirstObjectByType<InventorySystem>();
    }

    public bool CanInteract => itemData != null;

    public void Interact()
    {
        if (inventory == null || itemData == null) return;
        if (inventory.AddToBag(itemData))
            Destroy(gameObject);
    }
}
