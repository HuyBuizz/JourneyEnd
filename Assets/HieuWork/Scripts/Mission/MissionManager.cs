using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;
    public MissionChain currentMissionChain;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentMissionChain = LoadMissionForCurrentLevel();
    }

    private MissionChain LoadMissionForCurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        var chain = new MissionChain { levelName = sceneName };

        if (sceneName == "Level1")
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
                targetLocation = GameObject.Find("FireZone").transform
            });
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
        }
    }

    public MissionStep GetCurrentStep() => currentMissionChain?.GetCurrentStep();
}
