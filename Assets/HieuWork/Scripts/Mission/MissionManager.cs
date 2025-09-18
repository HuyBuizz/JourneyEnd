using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    // Nhiệm vụ
    public List<Mission> rootMissions = new List<Mission>();
    public List<Mission> allMissions = new List<Mission>();
    public Mission selectedMission;

    private readonly Dictionary<string, Mission> _map = new Dictionary<string, Mission>();
    public Dictionary<PlayerRole, List<Mission>> roleMissions = new Dictionary<PlayerRole, List<Mission>>();

    // Events
    public event Action OnMissionStepChanged;
    public event Action OnSelectedMissionChanged;
    public Action OnMissionListChanged;
    public event Action<Mission, MissionStep> OnMissionProgress;

    // Vai trò người chơi hiện tại
    public PlayerRole currentPlayerRole;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        currentPlayerRole = PlayerManager.Instance.GetRole();
        InitializeRoleMissions();
        InitializeMissions();

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnRoleChanged += OnPlayerRoleChanged;
        }
    }

    private void OnDestroy()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnRoleChanged -= OnPlayerRoleChanged;
        }
    }

    // ===================== INIT =====================

    private void InitializeMissions()
    {
        rootMissions.Clear();
        allMissions.Clear();
        _map.Clear();

        // Gán nhiệm vụ mặc định cho local player
        AssignDefaultMissionsToPlayer("local", currentPlayerRole);

        // Chọn mission mặc định
        if (allMissions.Count > 0)
            SelectMission(allMissions[0]);

        OnMissionListChanged?.Invoke();
        Debug.Log($"[MissionManager] Initialized missions for role {currentPlayerRole}");
    }

    private void OnPlayerRoleChanged(PlayerRole newRole)
    {
        currentPlayerRole = newRole;

        // Reset toàn bộ nhiệm vụ khi đổi role
        rootMissions.Clear();
        allMissions.Clear();
        _map.Clear();

        AssignDefaultMissionsToPlayer("local", newRole);

        if (allMissions.Count > 0)
            SelectMission(allMissions[0]);
        else
            selectedMission = null;

        OnMissionListChanged?.Invoke();
        OnSelectedMissionChanged?.Invoke();
        Debug.Log($"[MissionManager] Role changed → reset missions for {newRole}");
    }

    private void RebuildFlatIndex()
    {
        allMissions.Clear();
        _map.Clear();
        foreach (var root in rootMissions)
        {
            AddMissionToIndex(root);
        }
    }

    private void AddMissionToIndex(Mission mission)
    {
        if (mission == null) return;
        allMissions.Add(mission);
        _map[mission.id] = mission;
        foreach (var child in mission.children)
        {
            AddMissionToIndex(child);
        }
    }

    // ===================== PROGRESS =====================

    public void TryProgressStep(MissionStepType actionType)
    {
        bool progressed = false;
        for (int i = 0; i < allMissions.Count; i++)
        {
            var m = allMissions[i];
            if (m.status != MissionStatus.Active) continue;

            var step = m.GetCurrentStep();
            if (step != null && !step.isCompleted && step.type == actionType)
            {
                m.CompleteCurrentStep();
                progressed = true;
                HandleAfterProgress(m, step);
            }
        }
        if (progressed)
        {
            OnMissionStepChanged?.Invoke();
            OnSelectedMissionChanged?.Invoke();
            OnMissionListChanged?.Invoke();
        }
    }

    public void ReportRescueDelta(int delta)
    {
        if (delta == 0) return;
        bool progressed = false;
        foreach (var m in allMissions)
        {
            if (m.status != MissionStatus.Active) continue;
            var step = m.GetCurrentStep();
            if (step == null || step.type != MissionStepType.RescuePeople) continue;

            if (step.peopleToRescue > 0)
            {
                step.rescuedCount += delta;
                step.rescuedCount = Mathf.Max(0, step.rescuedCount);
                if (step.rescuedCount >= step.peopleToRescue)
                {
                    m.CompleteCurrentStep();
                    progressed = true;
                    HandleAfterProgress(m, step);
                }
            }
        }
        if (progressed)
        {
            OnMissionStepChanged?.Invoke();
            OnSelectedMissionChanged?.Invoke();
            OnMissionListChanged?.Invoke();
        }
    }

    public void ReportExtinguishDelta(float delta)
    {
        if (Mathf.Approximately(delta, 0f)) return;
        bool progressed = false;
        foreach (var m in allMissions)
        {
            if (m.status != MissionStatus.Active) continue;
            var step = m.GetCurrentStep();
            if (step == null || step.type != MissionStepType.ExtinguishFire) continue;

            if (step.fireAmountToExtinguish > 0f)
            {
                step.extinguishedAmount += Mathf.Max(0f, delta);
                if (step.extinguishedAmount >= step.fireAmountToExtinguish)
                {
                    m.CompleteCurrentStep();
                    progressed = true;
                    HandleAfterProgress(m, step);
                }
            }
        }
        if (progressed)
        {
            OnMissionStepChanged?.Invoke();
            OnSelectedMissionChanged?.Invoke();
            OnMissionListChanged?.Invoke();
        }
    }

    private void HandleAfterProgress(Mission m, MissionStep stepJustCompleted)
    {
        OnMissionProgress?.Invoke(m, stepJustCompleted);

        if (!string.IsNullOrEmpty(m.parentId))
            GetMissionById(m.parentId)?.TryMarkCompleted();

        OnMissionListChanged?.Invoke();
    }

    // ===================== SELECTION =====================

    public void SelectMission(Mission mission)
    {
        if (mission == null) return;
        if (mission.status != MissionStatus.Active && mission.status != MissionStatus.Completed) return;

        selectedMission = mission;
        OnSelectedMissionChanged?.Invoke();
        Debug.Log($"Selected mission: {mission.title}");
    }

    public MissionStep GetCurrentStep() => selectedMission?.GetCurrentStep();

    public Mission GetMissionById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _map.TryGetValue(id, out var m) ? m : null;
    }

    public List<Mission> GetActiveMissions() => allMissions.FindAll(m => m.status == MissionStatus.Active);

    public List<Mission> GetCompletedMissions() => allMissions.FindAll(m => m.status == MissionStatus.Completed);

    public List<Mission> GetMissionsForRole(PlayerRole role)
    {
        if (role == PlayerRole.Commander)
            return new List<Mission>(allMissions);

        return allMissions.FindAll(m =>
            m.assignedRole == role ||
            m.status == MissionStatus.Completed);
    }

    public List<Mission> GetMissionsForCurrentPlayer()
    {
        var role = PlayerManager.Instance.GetRole();
        return GetMissionsForRole(role);
    }

    // ===================== ASSIGN =====================

    public void AssignMissionToPlayer(string playerId, Mission mission)
    {
        if (mission == null) return;

        allMissions.Add(mission);
        _map[mission.id] = mission;
        mission.status = MissionStatus.Active;

        Debug.Log($"[MissionManager] Assigned mission {mission.title} to player {playerId} (role: {mission.assignedRole})");
        OnMissionListChanged?.Invoke();
    }

    public void AssignDefaultMissionsToPlayer(string playerId, PlayerRole role)
    {
        if (!roleMissions.ContainsKey(role)) return;

        foreach (var mission in roleMissions[role])
        {
            Mission clonedMission = CloneMission(mission);
            AssignMissionToPlayer(playerId, clonedMission);
        }
    }

    private Mission CloneMission(Mission mission)
    {
        return new Mission
        {
            id = mission.id,
            title = mission.title,
            description = mission.description,
            type = mission.type,
            status = MissionStatus.Active,
            steps = new List<MissionStep>(mission.steps),
            children = new List<Mission>(mission.children),
            assignedRole = mission.assignedRole
        };
    }

    private void InitializeRoleMissions()
    {
        roleMissions.Clear();

        // ENGINEER
        roleMissions[PlayerRole.Engineer] = new List<Mission>
        {
            new Mission
            {
                id = "eng_main",
                title = "Sửa chữa hệ thống",
                description = "Khảo sát và sửa chữa hệ thống ống dẫn",
                type = MissionType.Main,
                status = MissionStatus.Locked,
                assignedRole = PlayerRole.Engineer,
                steps = new List<MissionStep>
                {
                    new MissionStep
                    {
                        id = "inspect_area",
                        description = "Đi đến khu vực hỏng hóc",
                        type = MissionStepType.ReachLocation,
                        targetLocation = GameObject.Find("EngineerInspectPoint")?.transform
                    },
                    new MissionStep
                    {
                        id = "check_pipe",
                        description = "Kiểm tra ống dẫn",
                        type = MissionStepType.InteractObject,
                        targetObject = GameObject.Find("BrokenPipe")
                    },
                    new MissionStep
                    {
                        id = "repair_pipe",
                        description = "Sửa chữa ống dẫn",
                        type = MissionStepType.InteractObject,
                        targetObject = GameObject.Find("PipeFixStation")
                    }
                }
            }
        };

        // MEDIC
        roleMissions[PlayerRole.Medic] = new List<Mission>
        {
            new Mission
            {
                id = "medic_main",
                title = "Giải cứu nạn nhân",
                description = "Tìm và cứu hộ nạn nhân trong vùng nguy hiểm",
                type = MissionType.Main,
                status = MissionStatus.Locked,
                assignedRole = PlayerRole.Medic,
                steps = new List<MissionStep>
                {
                    new MissionStep
                    {
                        id = "find_victim",
                        description = "Tìm nạn nhân trong khu vực cháy",
                        type = MissionStepType.ReachLocation,
                        targetLocation = GameObject.Find("VictimPoint")?.transform
                    },
                    new MissionStep
                    {
                        id = "heal_victim",
                        description = "Sơ cứu cho nạn nhân",
                        type = MissionStepType.InteractObject,
                        targetObject = GameObject.Find("Victim")
                    },
                    new MissionStep
                    {
                        id = "rescue_victim",
                        description = "Đưa nạn nhân đến SafeZone",
                        type = MissionStepType.RescuePeople,
                        targetLocation = GameObject.Find("SafeZone")?.transform,
                        peopleToRescue = 1
                    }
                }
            }
        };

        // FIREFIGHTER
        roleMissions[PlayerRole.Firefighter] = new List<Mission>
        {
            new Mission
            {
                id = "ff_main",
                title = "Dập tắt đám cháy chính",
                description = "Khống chế và dập tắt đám cháy lớn",
                type = MissionType.Main,
                status = MissionStatus.Locked,
                assignedRole = PlayerRole.Firefighter,
                steps = new List<MissionStep>
                {
                    new MissionStep
                    {
                        id = "reach_fire",
                        description = "Tiếp cận khu vực cháy",
                        type = MissionStepType.ReachLocation,
                        targetLocation = GameObject.Find("FireZone")?.transform
                    },
                    new MissionStep
                    {
                        id = "connect_hose",
                        description = "Nối vòi nước vào trụ cứu hỏa",
                        type = MissionStepType.InteractObject,
                        targetObject = GameObject.Find("WaterHydrant")
                    },
                    new MissionStep
                    {
                        id = "extinguish_fire",
                        description = "Dập tắt ngọn lửa chính",
                        type = MissionStepType.ExtinguishFire,
                        targetLocation = GameObject.Find("FireZone")?.transform,
                        fireAmountToExtinguish = 100f
                    }
                }
            }
        };
    }
}
