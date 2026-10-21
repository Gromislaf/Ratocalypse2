using UnityEngine;

// Punkt zapisu gry — umieść w scenie z Collider (isTrigger = true) na layerze "Interactable".
// Gracz klika → PlayerController wywołuje Interact() → SaveSystem.Save().
// EventBus.OnGameSaved trafia do UI (np. tekst "Zapisano!").
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
        SaveSystem.Instance?.Save(savePointId);
    }
}
