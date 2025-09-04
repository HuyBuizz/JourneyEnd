using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;


    // Optional helper for building from scene
    public MissionChain currentMissionChain;


    // Cây nhiệm vụ root (mỗi level có thể có nhiều main root)
    public List<Mission> rootMissions = new List<Mission>();


    // Danh sách phẳng để tương thích UI hiện tại
    public List<Mission> allMissions = new List<Mission>();


    // Mission đang được chọn để hiển thị HUD
    public Mission selectedMission;


    // Tra cứu nhanh theo id
    private readonly Dictionary<string, Mission> _map = new Dictionary<string, Mission>();


    // Events (giữ tương thích + thêm payload nếu cần)
    public event Action OnMissionStepChanged;
    public event Action OnSelectedMissionChanged;
    public event Action OnMissionListChanged;
    public event Action<Mission, MissionStep> OnMissionProgress; // mới (payload)


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional — giữ tiến độ qua scene
        // DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        InitializeMissions();
        OnMissionStepChanged?.Invoke();
    }


    private void InitializeMissions()
    {
        // 1) Tải template
        currentMissionChain = LoadMissionChainTemplate();


        // 2) Tạo main từ template
        var main = CreateMainMissionFromChain(currentMissionChain);


        // 3) Thêm side con *nằm trong* main
        CreateChildSideMissions(main);


        // 4) Đăng ký root, flatten & index
        rootMissions.Clear();
        if (main != null) rootMissions.Add(main);


        RebuildFlatIndex();


        // 5) Select mission mặc định
        if (allMissions.Count > 0)
            SelectMission(allMissions[0]);


        OnMissionListChanged?.Invoke();
    }
    private MissionChain LoadMissionChainTemplate()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        var chain = new MissionChain { levelName = sceneName };


        if (sceneName == "Level")
        {
            chain.steps.Add(new MissionStep
            {
                id = "talk_npc",
                description = "Nói chuyện với NPC để nhận nhiệm vụ",
                type = MissionStepType.TalkToNPC,
                targetNPC = GameObject.Find("NPC_Captain"),
            });


            chain.steps.Add(new MissionStep
            {
                id = "go_to_location",
                description = "Đi đến khu vực hiện trường (FireZone)",
                type = MissionStepType.ReachLocation,
                targetLocation = GameObject.Find("FireZone")?.transform,
            });


            chain.steps.Add(new MissionStep
            {
                id = "extinguish_fire",
                description = "Dập tắt đám cháy",
                type = MissionStepType.ExtinguishFire,
                fireAmountToExtinguish = 100f
            });


            chain.steps.Add(new MissionStep
            {
                id = "rescue_people",
                description = "Cứu người trong tòa nhà",
                type = MissionStepType.RescuePeople,
                peopleToRescue = 3
            });
        }


        return chain;
    }
    private Mission CreateMainMissionFromChain(MissionChain chain)
    {
        if (chain == null || chain.steps.Count == 0) return null;


        var mainMission = new Mission
        {
            id = "main_mission_" + chain.levelName,
            title = "Nhiệm vụ chính - " + chain.levelName,
            description = "Hoàn thành các bước & side bắt buộc",
            type = MissionType.Main,
            status = MissionStatus.Active,
            steps = new List<MissionStep>(),
            currentStepIndex = 0
        };


        // Deep copy steps từ chain
        for (int i = 0; i < chain.steps.Count; i++)
            mainMission.steps.Add(chain.steps[i].Clone());


        return mainMission;
    }
    private void CreateChildSideMissions(Mission main)
    {
        if (main == null) return;
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "Level") return;


        var sideCollect = new Mission
        {
            id = "side_collect_info",
            title = "Thu thập thông tin",
            description = "Tìm hiểu nguyên nhân cháy nổ",
            type = MissionType.Side,
            status = MissionStatus.Locked,
            isRequiredChild = false, // không bắt buộc
            unlockAfterParentStep = 1, // mở sau khi hoàn tất step index 0 của main
            steps = new List<MissionStep>
            {
                new MissionStep
                {
                    id = "investigate_area",
                    description = "Khảo sát hiện trường",
                    type = MissionStepType.ReachLocation,
                    targetLocation = GameObject.Find("InvestigationPoint")?.transform
                }
            }
        };
        var sideMedical = new Mission
        {
            id = "side_medical_aid",
            title = "Hỗ trợ y tế",
            description = "Sơ cứu những người bị thương",
            type = MissionType.Side,
            status = MissionStatus.Locked,
            isRequiredChild = true, // bắt buộc để main được Completed
            unlockAfterParentStep = 2, // mở sau step index 1 của main
            steps = new List<MissionStep>
{
new MissionStep
{
id = "treat_victims",
description = "Điều trị cho nạn nhân",
type = MissionStepType.InteractObject,
targetObject = GameObject.Find("MedicalKit")
}
}
        };


        sideCollect.parentId = main.id;
        sideMedical.parentId = main.id;
        main.children.Add(sideCollect);
        main.children.Add(sideMedical);
    }
    private void RebuildFlatIndex()
    {
        allMissions.Clear();
        _map.Clear();
        for (int i = 0; i < rootMissions.Count; i++)
        {
            Flatten(rootMissions[i], allMissions);
        }
        for (int i = 0; i < allMissions.Count; i++)
        {
            _map[allMissions[i].id] = allMissions[i];
        }
    }


    private void Flatten(Mission root, List<Mission> acc)
    {
        if (root == null) return;
        acc.Add(root);
        for (int i = 0; i < root.children.Count; i++)
            Flatten(root.children[i], acc);
    }


    // ===================== Progress APIs =====================


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
        for (int i = 0; i < allMissions.Count; i++)
        {
            var m = allMissions[i];
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
        for (int i = 0; i < allMissions.Count; i++)
        {
            var m = allMissions[i];
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
        // Mở khóa con nếu m là parent (main)
        if (string.IsNullOrEmpty(m.parentId))
        {
            CheckAndUnlockChildren(m);
        }


        // Truyền payload
        OnMissionProgress?.Invoke(m, stepJustCompleted);


        // Nếu là child Completed → thử đánh dấu cha nếu đủ điều kiện
        if (!string.IsNullOrEmpty(m.parentId))
        {
            var parent = GetMissionById(m.parentId);
            if (parent != null)
            {
                parent.TryMarkCompleted();
            }
        }
    }


    private void CheckAndUnlockChildren(Mission parent)
    {
        if (parent == null) return;
        for (int i = 0; i < parent.children.Count; i++)
        {
            var child = parent.children[i];
            if (child.status != MissionStatus.Locked) continue;


            bool condition = (child.unlockAfterParentStep < 0 && parent.status == MissionStatus.Active)
            || (parent.currentStepIndex >= child.unlockAfterParentStep);


            if (condition)
            {
                child.status = MissionStatus.Active;
                Debug.Log($"🔓 Unlocked side: {child.title} (parent: {parent.title})");
            }
        }
    }
    // ===================== Selection & Queries =====================


    public void SelectMission(Mission mission)
    {
        if (mission == null) return;
        if (mission.status != MissionStatus.Active && mission.status != MissionStatus.Completed) return;


        selectedMission = mission;
        OnSelectedMissionChanged?.Invoke();
        Debug.Log($"📋 Selected mission: {mission.title}");
    }


    public MissionStep GetCurrentStep() => selectedMission?.GetCurrentStep();


    public Transform GetCurrentTarget()
    {
        var step = GetCurrentStep();
        if (step == null) return null;


        switch (step.type)
        {
            case MissionStepType.TalkToNPC:
                return step.targetNPC ? step.targetNPC.transform : null;
            case MissionStepType.ReachLocation:
                return step.targetLocation;
            case MissionStepType.InteractObject:
                return step.targetObject ? step.targetObject.transform : null;
            case MissionStepType.ExtinguishFire:
            case MissionStepType.RescuePeople:
                // Nếu có anchor khu vực, trả về để đặt waypoint
                return step.targetLocation;
            default:
                return null;
        }
    }


    public Mission GetMissionById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _map.TryGetValue(id, out var m) ? m : null;
    }


    public List<Mission> GetActiveMissions()
    {
        var list = new List<Mission>();
        for (int i = 0; i < allMissions.Count; i++)
            if (allMissions[i].status == MissionStatus.Active) list.Add(allMissions[i]);
        return list;
    }


    public List<Mission> GetCompletedMissions()
    {
        var list = new List<Mission>();
        for (int i = 0; i < allMissions.Count; i++)
            if (allMissions[i].status == MissionStatus.Completed) list.Add(allMissions[i]);
        return list;
    }


    // Hỗ trợ chuyển đổi (giữ tương thích UI cũ)
    public MissionState ConvertToMissionState(MissionStatus status)
    {
        switch (status)
        {
            case MissionStatus.Locked: return MissionState.Locked;
            case MissionStatus.Active: return MissionState.Active;
            case MissionStatus.Completed: return MissionState.Completed;
            case MissionStatus.Failed: return MissionState.Failed;
            default: return MissionState.Locked;
        }
    }
}