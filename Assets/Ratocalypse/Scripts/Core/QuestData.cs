using UnityEngine;

[CreateAssetMenu(menuName = "Ratocalypse/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questId;
    public string questName;
    [TextArea(2, 4)] public string description;
    public ItemData targetItem;
    public float xpReward = 200f;
    public float searchAreaRadius = 15f;
}
