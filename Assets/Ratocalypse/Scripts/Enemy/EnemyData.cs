using UnityEngine;

[CreateAssetMenu(menuName = "Ratpocalypse/Enemy Data", fileName = "NewEnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Identyfikacja")]
    public EnemyType enemyType = EnemyType.CommonRat;
    public string displayName = "Szczur";

    [Header("Życie")]
    public float maxHp = 50f;

    [Header("Ruch")]
    public float moveSpeed = 3.5f;
    public float patrolSpeed = 1.5f;
    [Tooltip("Promień losowania punktu patrolowego wokół punktu startowego")]
    public float patrolRadius = 4f;

    [Header("Wykrycie")]
    [Tooltip("Promień w którym wróg zauważa gracza")]
    public float detectRange = 8f;
    [Tooltip("Promień poza którym wróg rezygnuje z pogoni")]
    public float losePlayerRange = 14f;
    [Tooltip("Czas postoju w stanie Alert przed pogonią (sekundy)")]
    public float alertDelay = 0.6f;

    [Header("Walka")]
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    [Tooltip("Minimalny czas między kolejnymi atakami")]
    public float attackCooldown = 1.5f;
    [Tooltip("Czas ogłuszenia po otrzymaniu stuna")]
    public float stunDuration = 2f;

    [Header("Nagrody")]
    public float xpReward = 25f;
}
