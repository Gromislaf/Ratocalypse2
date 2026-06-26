using UnityEngine;
using UnityEngine.InputSystem;

public class test_HP : MonoBehaviour
{
    [SerializeField] private PlayerHealthComponent health;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private ItemData testItem;
    [SerializeField] private EnemyAI testEnemy;

    private void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame) health.TakeDamage(20f);
        if (Keyboard.current.jKey.wasPressedThisFrame) health.Heal(20f);
        if (Keyboard.current.kKey.wasPressedThisFrame) health.UseStamina(25f);
        if (Keyboard.current.lKey.wasPressedThisFrame) inventory.AddToBag(testItem);
        if (Keyboard.current.eKey.wasPressedThisFrame)
            ((IDamageable)testEnemy).TakeDamage(20f, false, Vector3.zero);
    }
}