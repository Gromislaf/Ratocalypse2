using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ratocalypse
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;

        private Action onClickCallback;

        public void Setup(ItemData item, Action onClick)
        {
            onClickCallback = onClick;
            bool hasItem = item != null && item.icon != null;
            iconImage.sprite = hasItem ? item.icon : null;
            iconImage.enabled = hasItem;
        }

        public void OnClick()
        {
            onClickCallback?.Invoke();
        }
    }
}
