using UnityEngine;
using System.Collections.Generic;

public class ObjectiveTracker : MonoBehaviour
{
    private Dictionary<string, bool> objectiveFlags;

    private void Awake()
    {
        objectiveFlags = new Dictionary<string, bool>();
    }

    public void SetFlag(string flag, bool value)
    {
        objectiveFlags.TryAdd(flag, value);
    }

    public bool GetFlag(string flag)
    {
        return objectiveFlags.GetValueOrDefault(flag, defaultValue: false);
    }
}