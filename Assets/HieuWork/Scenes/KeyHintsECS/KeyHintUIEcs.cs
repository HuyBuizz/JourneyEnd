using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class KeyHintUIEcs : MonoBehaviour
{
    [Header("References")]
    public KeyActionMapping keyActionMapping;
    public TextMeshProUGUI hintText;
    public KeyHintContext context; // << thêm

    [Header("UI")]
    [TextArea]
    public string header = "Key Hints";

    private void OnEnable()
    {
        if (context == null) context = KeyHintContext.Instance;
        if (context != null) context.OnFlagsChanged += RefreshKeyHints;
        RefreshKeyHints();
    }

    private void OnDisable()
    {
        if (context != null) context.OnFlagsChanged -= RefreshKeyHints;
    }

    [ContextMenu("Refresh Key Hints")]
    public void RefreshKeyHints()
    {
        if (keyActionMapping == null || hintText == null)
        {
            Debug.LogError("❌ KeyActionMapping hoặc Text UI chưa được gán!");
            return;
        }

        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(header))
            sb.AppendLine(header);

        var list = keyActionMapping.KeyActions;
        if (list != null)
        {
            foreach (var ka in list)
            {
                if (!ShouldShow(ka)) continue; // << lọc theo điều kiện
                string keyLabel = FormatKeyLabel(ka.KeyCode);
                sb.AppendLine($"{keyLabel}: {ka.Action}");
            }
        }

        // Nếu không có mục nào thỏa điều kiện, có thể hiển thị fallback
        // if (sb.ToString().Trim() == header.Trim()) sb.AppendLine("—");

        hintText.text = sb.ToString();
    }

    public void UpdateKeyHints() => RefreshKeyHints();

    private bool ShouldShow(KeyAction ka)
    {
        switch (ka.Condition.Mode)
        {
            case ConditionMode.Always:
                return true;
            case ConditionMode.RequireFlag:
                return context != null && context.Has(ka.Condition.Flag);
            case ConditionMode.RequireNotFlag:
                return context == null || !context.Has(ka.Condition.Flag);
            default:
                return true;
        }
    }

    private static string FormatKeyLabel(Key key)
    {
        switch (key)
        {
            case Key.Space: return "Space";
            case Key.LeftShift:
            case Key.RightShift: return "Shift";
            case Key.LeftCtrl:
            case Key.RightCtrl: return "Ctrl";
            case Key.LeftAlt:
            case Key.RightAlt: return "Alt";
            case Key.UpArrow: return "↑";
            case Key.DownArrow: return "↓";
            case Key.LeftArrow: return "←";
            case Key.RightArrow: return "→";
            default: return key.ToString();
        }
    }
}
