using UnityEngine;
using UnityEngine.UI;

namespace Ratocalypse
{
    public class PlayerHUD : MonoBehaviour
    {
        [Header("HP — wypełnienie w centrum BGUI")]
        [SerializeField] private Image hpFillImage;

        [Header("Stamina — pasek poniżej BGUI")]
        [SerializeField] private Image staminaFillImage;

        [Header("Źródło danych (PlayerStats asset)")]
        [SerializeField] private PlayerStats playerStats;

        private void OnEnable()
        {
            EventBus.Subscribe<OnPlayerDamaged>(OnDamaged);
            EventBus.Subscribe<OnPlayerHealed>(OnHealed);
            EventBus.Subscribe<OnPlayerStaminaChanged>(OnStaminaChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnPlayerDamaged>(OnDamaged);
            EventBus.Unsubscribe<OnPlayerHealed>(OnHealed);
            EventBus.Unsubscribe<OnPlayerStaminaChanged>(OnStaminaChanged);
        }

        private void Start()
        {
            if (playerStats == null)
            {
                Debug.LogError("[PlayerHUD] PlayerStats nie przypisany!", this);
                return;
            }
            SetHP(playerStats.currentHp, playerStats.MaxHp);
            SetStamina(playerStats.currentStamina, playerStats.MaxStamina);
        }

        private void OnDamaged(OnPlayerDamaged e) => SetHP(e.currentHp, e.maxHp);
        private void OnHealed(OnPlayerHealed e)   => SetHP(e.currentHp, e.maxHp);
        private void OnStaminaChanged(OnPlayerStaminaChanged e) => SetStamina(e.current, e.max);

        private void SetHP(float current, float max)
        {
            if (hpFillImage != null)
                hpFillImage.fillAmount = max > 0f ? current / max : 0f;
        }

        private void SetStamina(float current, float max)
        {
            if (staminaFillImage != null)
                staminaFillImage.fillAmount = max > 0f ? current / max : 0f;
        }
    }
}
