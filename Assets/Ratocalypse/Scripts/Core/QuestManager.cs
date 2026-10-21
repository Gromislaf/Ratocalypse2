using System.Collections.Generic;
using UnityEngine;

public enum QuestState { None, Active, ItemFound, Completed }

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public QuestData  ActiveQuest      { get; private set; }
    public QuestState State            { get; private set; }
    public Vector3    SearchAreaCenter { get; private set; }

    public IReadOnlyCollection<string> CompletedQuests => _completedQuests;
    private readonly HashSet<string> _completedQuests = new();

    private InventorySystem inventory;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        inventory = FindFirstObjectByType<InventorySystem>();
    }

    private void OnEnable()  => EventBus.Subscribe<OnInventoryChanged>(OnInventoryChanged);
    private void OnDisable() => EventBus.Unsubscribe<OnInventoryChanged>(OnInventoryChanged);

    public void AcceptQuest(QuestData quest, Vector3 searchAreaCenter)
    {
        if (State != QuestState.None) return;

        ActiveQuest = quest;
        SearchAreaCenter = searchAreaCenter;
        State = QuestState.Active;

        EventBus.Publish(new OnQuestAccepted
        {
            Quest            = quest,
            SearchAreaCenter = searchAreaCenter,
            SearchAreaRadius = quest.searchAreaRadius
        });
    }

    public void CompleteQuest()
    {
        if (State != QuestState.ItemFound || ActiveQuest == null) return;

        string completedId = ActiveQuest.questId;
        float  xpReward    = ActiveQuest.xpReward;
        inventory?.RemoveFromBag(ActiveQuest.targetItem);

        _completedQuests.Add(completedId);
        EventBus.Publish(new OnQuestCompleted { questId = completedId, xpReward = xpReward });

        State       = QuestState.Completed;
        ActiveQuest = null;
    }

    public void RestoreFromSave(List<string> completedIds, QuestData activeQuest, QuestState restoredState, Vector3 searchAreaCenter)
    {
        _completedQuests.Clear();
        foreach (var id in completedIds)
            _completedQuests.Add(id);

        ActiveQuest      = activeQuest;
        State            = restoredState;
        SearchAreaCenter = searchAreaCenter;

        if ((restoredState == QuestState.Active || restoredState == QuestState.ItemFound) && activeQuest != null)
        {
            EventBus.Publish(new OnQuestAccepted
            {
                Quest            = activeQuest,
                SearchAreaCenter = searchAreaCenter,
                SearchAreaRadius = activeQuest.searchAreaRadius
            });
        }
    }

    private void OnInventoryChanged(OnInventoryChanged _)
    {
        if (State != QuestState.Active || ActiveQuest == null) return;
        if (inventory == null) inventory = FindFirstObjectByType<InventorySystem>();
        if (inventory == null) return;

        if (inventory.HasItemInBag(ActiveQuest.targetItem))
        {
            State = QuestState.ItemFound;
            EventBus.Publish(new OnQuestItemFound { Quest = ActiveQuest });
        }
    }
}
