using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD: pasek XP (poziomy fill) + tekst poziomu + krotka notyfikacja "LEVEL UP!".
/// Subskrybuje OnPlayerXPGained i OnPlayerLevelUp przez EventBus.
/// </summary>
public class XPBarUI : MonoBehaviour
{
    [Header("XP Bar")]
    [SerializeField] private Image xpFill;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text xpText;

    [Header("Level Up")]
    [SerializeField] private GameObject levelUpNotification;
    [SerializeField] private float levelUpDisplayTime = 2f;

    [Header("Dane")]
    [SerializeField] private PlayerStats stats;

    private void Start()
    {
        if (levelUpNotification != null)
            levelUpNotification.SetActive(false);

        if (stats != null)
            Refresh(stats.currentXP, stats.XPRequiredForNextLevel, stats.currentLevel);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnPlayerXPGained>(OnXPGained);
        EventBus.Subscribe<OnPlayerLevelUp>(OnLevelUp);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnPlayerXPGained>(OnXPGained);
        EventBus.Unsubscribe<OnPlayerLevelUp>(OnLevelUp);
    }

    private void OnXPGained(OnPlayerXPGained e)
    {
        Refresh(e.totalXP, e.xpToNextLevel, stats != null ? stats.currentLevel : 0);
    }

    private void OnLevelUp(OnPlayerLevelUp e)
    {
        if (levelText != null)
            levelText.text = $"LVL {e.newLevel}";

        if (levelUpNotification != null)
            StartCoroutine(ShowLevelUpNotification());
    }

    private void Refresh(float currentXP, float xpToNext, int level)
    {
        if (xpFill != null)
            xpFill.fillAmount = xpToNext > 0f ? currentXP / xpToNext : 0f;

        if (levelText != null)
            levelText.text = $"LVL {level}";

        if (xpText != null)
            xpText.text = $"{Mathf.FloorToInt(currentXP)} / {Mathf.FloorToInt(xpToNext)}";
    }

    private IEnumerator ShowLevelUpNotification()
    {
        levelUpNotification.SetActive(true);
        yield return new WaitForSeconds(levelUpDisplayTime);
        levelUpNotification.SetActive(false);
    }
}
