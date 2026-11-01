using UnityEngine;

// Punkt zapisu gry — umieść w scenie z Collider (isTrigger = true) na layerze "Interactable".
// SavePoint.Interact() → publikuje OnCheckpointReached → wywołuje SaveSystem.Save()
// → SaveSystem publikuje OnGameSaved (dla UI).
[RequireComponent(typeof(Collider))]
public class SavePoint : MonoBehaviour, IInteractable
{
    [SerializeField] private string savePointId = "savepoint_01";

    public bool CanInteract => true;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public void Interact()
    {
        EventBus.Publish(new OnCheckpointReached { checkpointId = savePointId });
        if (SaveSystem.Instance != null) SaveSystem.Instance.Save(savePointId);
    }
}
