using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Jeden node w drzewku umiejętności.
// Prefab: Image (tło) + Button + TMP "Nazwa" + TMP "Opis".
[RequireComponent(typeof(Button))]
public class SkillNodeUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Kolory stanów")]
    [SerializeField] private Color colorUnlocked   = new Color(0.95f, 0.78f, 0.15f); // złoty
    [SerializeField] private Color colorAvailable  = new Color(0.85f, 0.85f, 0.85f); // jasny szary
    [SerializeField] private Color colorUnavailable = new Color(0.20f, 0.20f, 0.20f); // ciemny

    private SkillDefinition _skill;
    private SkillTreeManager _manager;
    private SkillTreeUI _treeUI;

    public void Initialize(in SkillDefinition skill, SkillTreeManager manager, SkillTreeUI treeUI)
    {
        _skill = skill;
        _manager = manager;
        _treeUI = treeUI;

        nameText.text = skill.displayName;
        descriptionText.text = skill.description;

        GetComponent<Button>().onClick.AddListener(OnClick);
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked  = _manager.IsUnlocked(_skill.id);
        bool available = _manager.CanUnlock(_skill);

        background.color = unlocked ? colorUnlocked
                         : available ? colorAvailable
                         : colorUnavailable;
    }

    private void OnClick()
    {
        if (_manager.TryUnlock(_skill.id))
            _treeUI.RefreshAll();
    }
}
