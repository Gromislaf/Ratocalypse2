using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Panel drzewka umiejętności. Toggle klawiszem C.
// W Inspektorze przypisz: panel, skillPointsText, trzy kolumny, prefab node'a, manager, stats.
public class SkillTreeUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI skillPointsText;

    [Header("Kolumny (rodzice node'ów)")]
    [SerializeField] private Transform warriorColumn;
    [SerializeField] private Transform hunterColumn;
    [SerializeField] private Transform survivorColumn;

    [Header("Referencje")]
    [SerializeField] private SkillNodeUI skillNodePrefab;
    [SerializeField] private SkillTreeManager skillTreeManager;
    [SerializeField] private PlayerStats stats;

    private SkillNodeUI[] _nodes;
    private bool _isOpen;

    private void OnEnable()
    {
        EventBus.Subscribe<OnInventoryToggled>(OnInventoryToggled);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnInventoryToggled>(OnInventoryToggled);
    }

    private void Start()
    {
        BuildNodes();
        panel.SetActive(false);
        _isOpen = false;
    }

    private void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        _isOpen = !_isOpen;
        panel.SetActive(_isOpen);
        EventBus.Publish(new OnSkillTreeToggled { isOpen = _isOpen });
        if (_isOpen) RefreshAll();
    }

    private void OnInventoryToggled(OnInventoryToggled e)
    {
        if (e.isOpen && _isOpen)
        {
            _isOpen = false;
            panel.SetActive(false);
            EventBus.Publish(new OnSkillTreeToggled { isOpen = false });
        }
    }

    public void RefreshAll()
    {
        skillPointsText.text = $"Punkty umiejętności: {stats.availableSkillPoints}";
        foreach (var node in _nodes)
            node.Refresh();
    }

    private void BuildNodes()
    {
        var allSkills = SkillDatabase.All;
        _nodes = new SkillNodeUI[allSkills.Length];

        for (int i = 0; i < allSkills.Length; i++)
        {
            ref readonly var skill = ref allSkills[i];
            Transform column = skill.branch switch
            {
                SkillBranch.Warrior  => warriorColumn,
                SkillBranch.Hunter   => hunterColumn,
                SkillBranch.Survivor => survivorColumn,
                _ => warriorColumn
            };

            var node = Instantiate(skillNodePrefab, column);
            node.Initialize(skill, skillTreeManager, this);
            _nodes[i] = node;
        }
    }
}
