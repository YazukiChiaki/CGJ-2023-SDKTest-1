using DG.DOTweenEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Networking.PlayerConnection;
using UnityEditor.Overlays;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using static Map;

public class SceneEditorWindow : EditorWindow
{
    public Map map;
    public SerializedProperty mapPty;
    public SerializedObject serializedObj;

    [MenuItem("SceneEditor/Window")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SceneEditorWindow));
    }

    private MapData mapData;
    private Vector2Int mapSize = new Vector2Int(20, 20);
    private MapData.EntryData[,] cubeInfos;
    private Stack stateStack = new Stack();

    
    public void InitMap()
    {
        map = FindObjectOfType<Map>();
        mapData = map.Data;
        mapSize = mapData.Size;
        cubeInfos = new MapData.EntryData[mapSize.x, mapSize.y];
        Array.ForEach(mapData.Entries.ToArray(), (cubeInfo) =>
        {
            cubeInfos[cubeInfo.Position.x, cubeInfo.Position.y] = cubeInfo;
        });
        for (var i = 0; i < cubeInfos.GetLength(0); i++)
        {
            for (var j = 0; j < cubeInfos.GetLength(1); j++)
            {
                if (cubeInfos[i, j].Type != MapData.EntryType.None)
                    continue;
                cubeInfos[i, j].Type = MapData.EntryType.Floor;
                cubeInfos[i, j].Position = new Vector2Int(i, j);
            }
        }
        UpdateSecene();
    }

    private void Awake()
    {
        InitMap();
    }


    private void PlayModeChange(PlayModeStateChange obj)
    {
        if (obj != PlayModeStateChange.EnteredEditMode)
            return;
        InitMap();
    }

    private void OnEnable()
    {
        serializedObj = new SerializedObject(this);
        mapPty = serializedObj.FindProperty("map");
        EditorApplication.playModeStateChanged += PlayModeChange;
    }

    void OnGUI()
    {
        EditorGUILayout.PropertyField(mapPty, true);
        mapSize = EditorGUILayout.Vector2IntField("地图尺寸", mapSize);
        if (GUILayout.Button("初始化/重新生成"))
        {
            InitMapData();
            return;
        }
        var xStr = Selection.transforms.Length <= 0 ? "none" : Selection.transforms[0].position.x.ToString();
        var yStr = Selection.transforms.Length <= 0 ? "none" : Selection.transforms[0].position.z.ToString();
        GUILayout.TextField($"选中物体的坐标:  x({xStr}), y({yStr})");
        // Delete 
        if (GUILayout.Button("删除选中"))
        {
            Array.ForEach(Selection.transforms, trans =>
            {
                ref MapData.EntryData cubeInfo = ref cubeInfos[(int)trans.position.x + (mapSize.x / 2), (int)trans.position.z + (mapSize.y / 2)];
                switch (trans.tag)
                {
                    case "ground":
                        cubeInfo.Type = MapData.EntryType.Hole;
                        break;
                    default:
                        cubeInfo.Type = MapData.EntryType.Floor;
                        break;
                }
            });
            UpdateSecene();
            return;
        }
        if (GUILayout.Button("在选中位置生成墙"))
        {
            Array.ForEach(Selection.transforms, trans =>
            {
                cubeInfos[(int)trans.position.x + (mapSize.x / 2), (int)trans.position.z + (mapSize.y / 2)].Type = MapData.EntryType.Wall;
            });
            UpdateSecene();
            return;
        }
        if (GUILayout.Button("选中处填充地板"))
        {
            Array.ForEach(Selection.transforms, trans =>
            {
                var x = (int)trans.position.x + (mapSize.x / 2); var y = (int)trans.position.z + (mapSize.y / 2);
                //ref MapData.EntryData cubeInfo = ref cubeInfos[x, y];
                int[,] offsets = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
                for(var i = 0; i < offsets.GetLength(0); i++) 
                {
                    var offsetX = x + offsets[i, 0];
                    var offsetY = y + offsets[i, 1];
                    if (offsetX < 0 || offsetX >= mapSize.x || offsetY < 0 || offsetY >= mapSize.y)
                        continue;
                    ref MapData.EntryData offsetCube = ref cubeInfos[offsetX, offsetY];
                    if(offsetCube.Type == MapData.EntryType.Hole)
                    {
                        offsetCube.Type = MapData.EntryType.Floor;
                    }
                }
            });
            UpdateSecene();
            return;
        }
        if (GUILayout.Button("回退"))
        {
            Cancle();
            return;
        }
        EditorUtility.SetDirty(this);
    }

    private void InitMapData()
    {
        mapData = new MapData();
        mapData.Size = mapSize;
        cubeInfos = new MapData.EntryData[mapSize.x, mapSize.y];
        for (var i = 0; i < mapSize.x; i++)
        {
            for (var n = 0; n < mapSize.y; n++)
            {
                var entryData = new MapData.EntryData();
                entryData.Position = new Vector2Int(i, n);
                entryData.Type = MapData.EntryType.Floor;
                cubeInfos[i, n] = entryData;
            }
        }
        UpdateSecene();
    }

    private void UpdateSecene(bool cancle = false)
    {
        mapData.Entries = new List<MapData.EntryData>(mapSize.x * mapSize.y);
        for (var i = 0; i < mapSize.x; i++)
        {
            for (var n = 0; n < mapSize.y; n++)
            {
                var entryData = cubeInfos[i, n];
                if (entryData.Type == MapData.EntryType.None || entryData.Type == MapData.EntryType.Floor) continue;
                mapData.Entries.Add(entryData);
            }
        }
        map.Data = mapData;
        map.Generate();
        EditorUtility.SetDirty(this);
        if (cancle)
            return;
        if(stateStack.Count >= 25)
        {
            stateStack.Pop();
        }
        stateStack.Push(mapData);
    }

    private void Cancle()
    {
        if (stateStack.Count <= 0)
            return;
        mapData = stateStack.Count == 1 ? (MapData)stateStack.Peek() : (MapData)stateStack.Pop();
        mapSize = mapData.Size;
        cubeInfos = new MapData.EntryData[mapSize.x, mapSize.y];
        int index = 0;
        Array.ForEach(mapData.Entries.ToArray(), (cubeInfo) =>
        {
            var x = index / mapSize.y;
            var y = (index % mapSize.y);
            index++;
            cubeInfo.Position.x = x;
            cubeInfo.Position.y = y;
            cubeInfos[x, y] = cubeInfo;
        });
        UpdateSecene(true);
    }

    private void OnDestroy()
    {
        EditorApplication.playModeStateChanged -= PlayModeChange;
    }
}
