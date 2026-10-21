using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private QuestData questToGive;
    [SerializeField] private Transform searchAreaCenter;

    private QuestDialogUI dialogUI;

    private void Awake()
    {
        dialogUI = FindFirstObjectByType<QuestDialogUI>(FindObjectsInactive.Include);
    }

    public bool CanInteract => true;

    public void Interact()
    {
        if (dialogUI == null) return;

        var qm = QuestManager.Instance;
        if (qm == null) return;

        switch (qm.State)
        {
            case QuestState.None when questToGive != null:
                dialogUI.ShowQuestOffer(questToGive, OnAccept);
                break;

            case QuestState.Active:
                dialogUI.ShowMessage("Wróć gdy znajdziesz " + questToGive.targetItem.itemName + ".");
                break;

            case QuestState.ItemFound:
                dialogUI.ShowQuestComplete(questToGive, OnComplete);
                break;

            case QuestState.Completed:
                dialogUI.ShowMessage("Dzięki za pomoc!");
                break;
        }
    }

    private void OnAccept()
    {
        Vector3 center = searchAreaCenter != null ? searchAreaCenter.position : transform.position;
        QuestManager.Instance.AcceptQuest(questToGive, center);
    }

    private void OnComplete()
    {
        QuestManager.Instance.CompleteQuest();
    }
}
