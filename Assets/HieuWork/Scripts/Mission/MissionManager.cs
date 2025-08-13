using System; // Thêm dòng này
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;
    public MissionChain currentMissionChain;

    // Thêm sự kiện
    public event Action OnMissionStepChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        currentMissionChain = LoadMissionForCurrentLevel();
        OnMissionStepChanged?.Invoke(); // Gọi khi load mission mới
    }

    private MissionChain LoadMissionForCurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        var chain = new MissionChain { levelName = sceneName };

        if (sceneName == "Level")
        {
            chain.steps.Add(
                new MissionStep
                {
                    id = "talk_npc",
                    description = "Nói chuyện với NPC để nhận nhiệm vụ",
                    type = MissionStepType.TalkToNPC,
                    targetNPC = GameObject.Find("NPC_Captain"),
                }
            );

            chain.steps.Add(
                new MissionStep
                {
                    id = "go_to_location",
                    description = "Đi đến khu vực hiện trường (FireZone)",
                    type = MissionStepType.ReachLocation,
                    targetLocation = GameObject.Find("FireZone").transform,
                }
            );
        }

        return chain;
    }

    public void TryProgressStep(MissionStepType actionType)
    {
        var step = currentMissionChain.GetCurrentStep();
        if (step != null && !step.isCompleted && step.type == actionType)
        {
            step.CompleteStep();
            currentMissionChain.CompleteCurrentStep();
            OnMissionStepChanged?.Invoke(); // Gọi khi bước nhiệm vụ thay đổi
        }
    }

    public MissionStep GetCurrentStep() => currentMissionChain?.GetCurrentStep();

    public Transform GetCurrentTarget()
    {
        var step = GetCurrentStep();
        if (step == null)
            return null;

        switch (step.type)
        {
            case MissionStepType.TalkToNPC:
                return step.targetNPC != null ? step.targetNPC.transform : null;
            case MissionStepType.ReachLocation:
                return step.targetLocation;
            // Thêm các loại mission khác nếu có
            default:
                return null;
        }
    }
}
