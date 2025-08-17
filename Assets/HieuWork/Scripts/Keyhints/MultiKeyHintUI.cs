using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class KeyHintEntry
{
    public string actionName;
    public TMP_Text hintText;
}

public class MultiKeyHintUI : MonoBehaviour
{
    [Header("Player Input")]
    public PlayerInput playerInput;
    public string actionMapName = "Player";
    public string targetBindingGroup = "Keyboard&Mouse";

    [Header("Key Hints")]
    public List<KeyHintEntry> keyHints = new List<KeyHintEntry>();

    public static GameObject interactTarget;

    public static bool isHoldingItem;

    void Awake()
    {
        UpdateAllKeyHints();
    }

    public void UpdateAllKeyHints()
    {
        if (playerInput == null)
            return;

        var actionMap = playerInput.actions.FindActionMap(actionMapName);
        if (actionMap == null)
            return;

        foreach (var entry in keyHints)
        {
            if (entry.hintText == null)
                continue;

            var action = actionMap.FindAction(entry.actionName);
            if (action == null)
            {
                entry.hintText.text = "?";
                continue;
            }

            // Điều kiện đặc biệt cho Interact
            if (entry.actionName == "Interact" && interactTarget == null)
            {
                entry.hintText.gameObject.SetActive(false); // ẩn hoặc để trống nếu không có target
                continue;
            }

            if (entry.actionName == "Sprint" || entry.actionName == "Move")
            {
                entry.hintText.text =
                    "Hold [" + GetBindingString(action) + "] to " + entry.actionName;
                continue;
            }
            if (entry.actionName == "Drop" && isHoldingItem == false)
            {
                entry.hintText.gameObject.SetActive(false); // ẩn hoặc để trống nếu không có target
                continue;
            }

            entry.hintText.gameObject.SetActive(true);
            entry.hintText.text = "Press [" + GetBindingString(action) + "] to " + entry.actionName;
        }
    }

    private string GetBindingString(InputAction action)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (binding.isComposite)
            {
                int partIndex = i + 1;
                while (
                    partIndex < action.bindings.Count
                    && action.bindings[partIndex].isPartOfComposite
                )
                {
                    var partBinding = action.bindings[partIndex];
                    if (
                        !string.IsNullOrEmpty(partBinding.groups)
                        && partBinding.groups.Contains(targetBindingGroup)
                    )
                    {
                        string keyName = InputControlPath.ToHumanReadableString(
                            partBinding.effectivePath,
                            InputControlPath.HumanReadableStringOptions.OmitDevice
                        );

                        if (sb.Length > 0)
                            sb.Append(" / ");
                        sb.Append(keyName.ToUpper());
                    }
                    partIndex++;
                }
                break;
            }
            else
            {
                if (
                    !string.IsNullOrEmpty(binding.groups)
                    && binding.groups.Contains(targetBindingGroup)
                )
                {
                    string keyName = InputControlPath.ToHumanReadableString(
                        binding.effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice
                    );

                    sb.Append(keyName.ToUpper());
                    break;
                }
            }
        }

        return sb.Length > 0 ? sb.ToString() : "?";
    }
}
