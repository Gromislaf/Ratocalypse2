using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratocalypse
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject background;

        [Header("Plecak — 20 slotów (przypisz w kolejności)")]
        [SerializeField] private InventorySlotUI[] bagSlots;

        [Header("Ekwipunek — sloty na sylwetce")]
        [SerializeField] private InventorySlotUI slotMainHand;
        [SerializeField] private InventorySlotUI slotOffHand;
        [SerializeField] private InventorySlotUI slotHelmet;
        [SerializeField] private InventorySlotUI slotChest;
        [SerializeField] private InventorySlotUI slotLegs;
        [SerializeField] private InventorySlotUI slotBoots;

        private void OnEnable()
        {
            EventBus.Subscribe<OnInventoryChanged>(OnInventoryChanged);
            EventBus.Subscribe<OnItemEquipped>(OnInventoryChanged);
            EventBus.Subscribe<OnItemUnequipped>(OnInventoryChanged);
            EventBus.Subscribe<OnSkillTreeToggled>(OnSkillTreeToggled);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnInventoryChanged>(OnInventoryChanged);
            EventBus.Unsubscribe<OnItemEquipped>(OnInventoryChanged);
            EventBus.Unsubscribe<OnItemUnequipped>(OnInventoryChanged);
            EventBus.Unsubscribe<OnSkillTreeToggled>(OnSkillTreeToggled);
        }

        private void Start()
        {
            if (inventory == null)
                Debug.LogError("[InventoryUI] InventorySystem nie przypisany!", this);

            panel.SetActive(false);
            if (background != null) background.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
                Toggle();
        }

        private void Toggle()
        {
            bool isOpen = !panel.activeSelf;
            panel.SetActive(isOpen);
            if (background != null) background.SetActive(isOpen);
            EventBus.Publish(new OnInventoryToggled { isOpen = isOpen });
            if (isOpen)
                Refresh();
        }

        private void OnInventoryChanged(OnInventoryChanged _) => Refresh();
        private void OnInventoryChanged(OnItemEquipped _)     => Refresh();
        private void OnInventoryChanged(OnItemUnequipped _)   => Refresh();

        private void OnSkillTreeToggled(OnSkillTreeToggled e)
        {
            if (e.isOpen && panel.activeSelf)
            {
                panel.SetActive(false);
                if (background != null) background.SetActive(false);
                EventBus.Publish(new OnInventoryToggled { isOpen = false });
            }
        }


        private void Refresh()
        {
            if (!panel.activeSelf) return;

            // Plecak
            for (int i = 0; i < bagSlots.Length; i++)
            {
                int index = i;
                ItemData item = index < inventory.Bag.Count ? inventory.Bag[index] : null;
                bagSlots[i].Setup(item, () => OnBagSlotClicked(index));
            }

            // Ekwipunek
            RefreshEquipSlot(slotMainHand, EquipmentSlot.MainHand);
            RefreshEquipSlot(slotOffHand,  EquipmentSlot.OffHand);
            RefreshEquipSlot(slotHelmet,   EquipmentSlot.Helmet);
            RefreshEquipSlot(slotChest,    EquipmentSlot.Chest);
            RefreshEquipSlot(slotLegs,     EquipmentSlot.Legs);
            RefreshEquipSlot(slotBoots,    EquipmentSlot.Boots);
        }

        private void RefreshEquipSlot(InventorySlotUI slotUI, EquipmentSlot slot)
        {
            if (slotUI == null) return;
            slotUI.Setup(inventory.GetEquipped(slot), () => inventory.Unequip(slot));
        }

        private void OnBagSlotClicked(int index)
        {
            if (index >= inventory.Bag.Count) return;
            ItemData item = inventory.Bag[index];

            if (item.itemType == ItemType.Consumable)
            {
                inventory.UseConsumable(item);
                return;
            }

            EquipmentSlot? target = GetDefaultSlot(item);
            if (target.HasValue)
                inventory.Equip(item, target.Value);
        }

        private static EquipmentSlot? GetDefaultSlot(ItemData item)
        {
            return item.itemType switch
            {
                ItemType.Weapon     => EquipmentSlot.MainHand,
                ItemType.Shield     => EquipmentSlot.OffHand,
                ItemType.Helmet     => EquipmentSlot.Helmet,
                ItemType.Chest      => EquipmentSlot.Chest,
                ItemType.Legs       => EquipmentSlot.Legs,
                ItemType.Boots      => EquipmentSlot.Boots,
                _                   => null
            };
        }
    }
}
