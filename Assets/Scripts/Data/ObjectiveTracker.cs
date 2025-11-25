using UnityEngine;
using System.Collections.Generic;

public class ObjectiveTracker : MonoBehaviour
{
    private Dictionary<string, bool> objectiveFlags;

    private void Awake()
    {
        objectiveFlags = new Dictionary<string, bool>();
        SetFlag("MissionObjectiveCollected", true);
    }

    public void SetFlag(string flag, bool value)
    {
        objectiveFlags.Add(flag, value);
    }

    public bool GetFlag(string flag)
    {
        return objectiveFlags.GetValueOrDefault(flag, defaultValue: false);
    }
}