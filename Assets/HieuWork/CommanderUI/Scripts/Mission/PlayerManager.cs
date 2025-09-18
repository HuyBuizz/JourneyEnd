using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    public static PlayerManager Instance;
    public event Action<PlayerRole> OnRoleChanged;

    [Header("Players")]
    private List<ObjMission> allPlayers = new List<ObjMission>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        allPlayers.Add(new ObjMission("local", "You", false, PlayerRole.Commander));

        allPlayers.Add(new ObjMission("bot1", "Bot Alpha", true, PlayerRole.None));
        allPlayers.Add(new ObjMission("bot2", "Bot Bravo", true, PlayerRole.None));
        allPlayers.Add(new ObjMission("bot3", "Bot Charlie", true, PlayerRole.None));
    }

    public List<ObjMission> GetAllPlayers()
    {
        return allPlayers;
    }

    public void AssignRole(string playerId, PlayerRole newRole)
    {
        ObjMission p = allPlayers.Find(x => x.id == playerId);
        if (p != null)
        {
            p.role = newRole;
            Debug.Log($"[PlayerManager] {p.name} assigned role {newRole}");

            // Nếu là local player → trigger OnRoleChanged
            if (!p.isBot && p.id == "local")
            {
                OnRoleChanged?.Invoke(newRole);
            }
        }
    }

    public PlayerRole GetRole()
    {
        // Luôn trả role của local player
        var local = allPlayers.Find(x => x.id == "local");
        return local != null ? local.role : PlayerRole.None;
    }
}
