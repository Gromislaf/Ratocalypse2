using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

// Singleton MonoBehaviour. Umieść na GameManager GO lub dedykowanym GO w scenie.
// Przypisz w Inspektorze: stats, inventory, skillTree, playerTransform, itemRegistry, questRegistry.
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    [SerializeField] private PlayerStats      stats;
    [SerializeField] private InventorySystem  inventory;
    [SerializeField] private SkillTreeManager skillTree;
    [SerializeField] private Transform        playerTransform;
    [SerializeField] private ItemData[]       itemRegistry;
    [SerializeField] private QuestData[]      questRegistry;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool HasSave() => File.Exists(SavePath);

    public void Save(string savePointId)
    {
        var quest = QuestManager.Instance;

        var data = new SaveData
        {
            version        = 1,
            hp             = stats.currentHp,
            stamina        = stats.currentStamina,
            level          = stats.currentLevel,
            xp             = stats.currentXP,
            skillPoints    = stats.availableSkillPoints,
            bag            = inventory.GetBagNames(),
            equipped       = inventory.GetEquippedNames(),
            unlockedSkills = new List<string>(skillTree.UnlockedSkills),
            activeQuestId  = quest?.ActiveQuest?.questId,
            questState     = (int)(quest?.State ?? QuestState.None),
            searchAreaX    = quest?.SearchAreaCenter.x ?? 0f,
            searchAreaY    = quest?.SearchAreaCenter.y ?? 0f,
            searchAreaZ    = quest?.SearchAreaCenter.z ?? 0f,
            completedQuestIds = quest != null
                ? new List<string>(quest.CompletedQuests)
                : new List<string>(),
            posX            = playerTransform.position.x,
            posY            = playerTransform.position.y,
            posZ            = playerTransform.position.z,
            lastSavePointId = savePointId
        };

        File.WriteAllText(SavePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        EventBus.Publish(new OnGameSaved { checkpointId = savePointId });
    }

    public void Load()
    {
        if (!HasSave()) return;

        var data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(SavePath));
        if (data == null) return;

        // 1. Statystyki gracza (przed przeliczeniem bonusów)
        stats.currentLevel         = data.level;
        stats.currentXP            = data.xp;
        stats.availableSkillPoints = data.skillPoints;
        stats.currentHp            = data.hp;
        stats.currentStamina       = data.stamina;

        // 2. Skille — przelicza skillBonuses
        skillTree.RestoreFromSave(data.unlockedSkills ?? new List<string>());

        // 3. Ekwipunek — przelicza bonusy ekwipunku
        inventory.RestoreFromSave(
            data.bag      ?? new List<string>(),
            data.equipped ?? new Dictionary<string, string>(),
            itemRegistry);

        // 4. Stan questów
        QuestData activeQuest = null;
        if (!string.IsNullOrEmpty(data.activeQuestId))
        {
            foreach (var q in questRegistry)
            {
                if (q.questId == data.activeQuestId) { activeQuest = q; break; }
            }
        }
        QuestManager.Instance?.RestoreFromSave(
            data.completedQuestIds ?? new List<string>(),
            activeQuest,
            (QuestState)data.questState,
            new Vector3(data.searchAreaX, data.searchAreaY, data.searchAreaZ));

        // 5. Pozycja gracza
        var savedPos = new Vector3(data.posX, data.posY, data.posZ);
        var agent    = playerTransform.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.Warp(savedPos);
        else
            playerTransform.position = savedPos;
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}
