// KeyHintContext.cs
using System.Collections.Generic;
using UnityEngine;
using System;

public class KeyHintContext : MonoBehaviour
{
    public static KeyHintContext Instance { get; private set; }

    private readonly HashSet<string> _flags = new HashSet<string>();
    public event Action OnFlagsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Has(string flag) => !string.IsNullOrEmpty(flag) && _flags.Contains(flag);

    public void SetFlag(string flag, bool value)
    {
        if (string.IsNullOrEmpty(flag)) return;
        bool changed = false;

        if (value)
            changed = _flags.Add(flag);
        else
            changed = _flags.Remove(flag);

        if (changed) OnFlagsChanged?.Invoke();
    }

    // tiện debug trong Editor
    public IEnumerable<string> AllFlags() => _flags;
}
