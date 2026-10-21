using UnityEngine;

public enum QuestState { None, Active, ItemFound, Completed }

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public QuestData ActiveQuest  { get; private set; }
    public QuestState State       { get; private set; }
    public Vector3 SearchAreaCenter { get; private set; }

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

        inventory?.RemoveFromBag(ActiveQuest.targetItem);

        EventBus.Publish(new OnQuestCompleted
        {
            questId   = ActiveQuest.questId,
            xpReward  = ActiveQuest.xpReward
        });

        State       = QuestState.Completed;
        ActiveQuest = null;
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
