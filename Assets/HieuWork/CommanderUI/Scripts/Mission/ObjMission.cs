using UnityEngine;

[System.Serializable]
public class ObjMission
{
    public string id;
    public string name;
    public bool isBot;
    public PlayerRole role;

    public ObjMission(string id, string name, bool isBot, PlayerRole role)
    {
        this.id = id;
        this.name = name;
        this.isBot = isBot;
        this.role = role;
    }
}
