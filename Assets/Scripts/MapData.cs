using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct MapData
{
    [Serializable]
    public enum EntryType
    {
        None, Hole, Wall, Switch, Button, Door, Target, Nontarget, Floor, Final, Mine
    }

    [Serializable]
    public struct EntryData
    {
        public Vector2Int Position;
        public EntryType Type;
        public int Id;
    }

    [Serializable]
    public struct SwitchInfo
    {

    }

    [Serializable]
    public struct ButtonInfo
    {

    }

    [Serializable]
    public struct DoorInfo
    {
        int AngleScale;
    }

    [Serializable]
    public struct TargetInfo
    {

    }

    public Vector2Int Size;
    public List<EntryData> Entries;
    public SwitchInfo[] SwitchInfos;
    public ButtonInfo[] ButtonInfos;
    public DoorInfo[] DoorInfos;
}