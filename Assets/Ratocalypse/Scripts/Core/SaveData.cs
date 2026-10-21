using System.Collections.Generic;

public class SaveData
{
    public int version = 1;

    // Gracz
    public float hp;
    public float stamina;
    public int   level;
    public float xp;
    public int   skillPoints;

    // Ekwipunek — nazwy itemów (klucze do itemRegistry w SaveSystem)
    public List<string>              bag      = new();
    public Dictionary<string,string> equipped = new();

    // Drzewko umiejętności
    public List<string> unlockedSkills = new();

    // Questy
    public string       activeQuestId;
    public int          questState;         // QuestState enum cast do int
    public float        searchAreaX, searchAreaY, searchAreaZ;
    public List<string> completedQuestIds = new();

    // Pozycja gracza
    public float posX, posY, posZ;

    // ID punktu zapisu (do logów / respawnu)
    public string lastSavePointId;
}
