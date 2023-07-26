using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static MapData;

public interface IMapObject
{
    Transform[] Children { get; }
    void Touch();
    void UnTouch();
    EntryType Type { get; }
    bool CanAttach { get; }
}

public interface IAttachableMapObject : IMapObject
{
    void Attach();
    void UnAttach();
}
public partial class Map : MonoBehaviour
{
    public GameObject GroundPrefab;
    public GameObject WallPrefab;
    public Transform TargetRoot;
    public MapData Data;
    public Transform ObstacleRoot;
    private int[,] m_Occupations;
    private Stack<int> freeEntryIndex = new Stack<int>();

    public List<GameObject> targetGOs = new List<GameObject>();

    private int finalTotalCount;
    public int FinalTotalCount => finalTotalCount;

    private DateTimeOffset startTime;

    private int m_Step = 0;
    public int step
    {
        get => m_Step;
        set
        {
            m_Step = value;
            if (gameMainUI != null)
            {
                gameMainUI.txtStep.text = $"步数: {step}";
            }
        }
    }
    private GameMainUI gameMainUI;

    private void Awake()
    {
        Generate();
        FillData();

        startTime = DateTimeOffset.Now;
        gameMainUI = FindObjectOfType<GameMainUI>();
        if (gameMainUI != null)
        {
            gameMainUI.startTime = startTime;
        }
        step = 0;
    }

    private void FillData()
    {
        foreach(var mObject in TargetRoot.GetComponentsInChildren<IMapObject>())
        {
            AddMapObject(mObject);
        }
    }

    public void AddMapObject<T>(T mObject) where T : IMapObject
    {
        var id = targetGOs.Count;
        targetGOs.Add((mObject as MonoBehaviour).gameObject);
        if (mObject.Type == EntryType.Final)
        {
            finalTotalCount += mObject.Children.Length;
        }
        foreach (var block in mObject.Children)
        {
            var tilePos = GetTilePosition(block.position);
            Data.Entries.Add(new MapData.EntryData()
            {
                Id = id,
                Position = tilePos,
                Type = mObject.Type
            });
            m_Occupations[tilePos.x, tilePos.y] = Data.Entries.Count;
        }
    }

    public IEnumerable<MapData.EntryData> RemoveMapObject<T>(T mObject) where T : IMapObject
    {
        foreach (var block in mObject.Children)
        {
            var subTilePos = GetTilePosition(block.transform.position);
            if (Clear(subTilePos, out var entryData))
            {
                yield return entryData;
            }
            else
            {
                Debug.LogError($"{subTilePos} Clear error");
            }
        }
    }

    public void Generate()
    {
        var min = new Vector3(-Data.Size.x / 2, 0, -Data.Size.y / 2);
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        var Size = Data.Size;
        m_Occupations = new int[Size.x, Size.y];
        for (var i = 0; i < Data.Entries.Count; i++)
        {
            var entry = Data.Entries[i];
            if (entry.Type != EntryType.Wall && entry.Type != EntryType.Hole) continue;
            var pos = entry.Position;
            m_Occupations[pos.x, pos.y] = i + 1;
        }

        for (var i = -1; i <= Size.x; i++)
        {
            for (var j = -1; j <= Size.y; j++)
            {
                var offset = new Vector3(i, 0, j);
                if (i == -1 || j == -1 || i == Size.x || j == Size.y)
                {
                    var wallIns = Instantiate(WallPrefab, transform, false);
                    wallIns.transform.localPosition = min + offset;
                    if (!Application.isPlaying)
                    {
                        wallIns.hideFlags = HideFlags.HideAndDontSave;
                    }
                    continue;
                }
                var entryIndex = m_Occupations[i, j] - 1;
                MapData.EntryData? entryData = entryIndex >= 0 ? Data.Entries[entryIndex] : null;
                if (entryData != null)
                {
                    var type = entryData.Value.Type;
                    if (type == MapData.EntryType.Hole) continue;
                    if (type == MapData.EntryType.Wall)
                    {
                        var wallIns = Instantiate(WallPrefab, transform, false);
                        wallIns.transform.localPosition = min + offset;
                        if (!Application.isPlaying)
                        {
                            wallIns.hideFlags = HideFlags.HideAndDontSave;
                        }
                    }
                }
                var ins = Instantiate(GroundPrefab, transform, false);
                ins.transform.localPosition = min + offset + new Vector3(0, -1f);
                ins.name = "Gen";
                if (!Application.isPlaying)
                {
                    ins.hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }
    }

    public Vector2Int GetTilePosition(Vector3 position)
    {
        var min = new Vector3(-Data.Size.x / 2, 0, -Data.Size.y / 2);
        position -= min;
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
    }

    public bool Overlap(Vector2Int tilePos, out MapData.EntryData entry)
    {
        var x = tilePos.x;
        var y = tilePos.y;
        if (x >= Data.Size.x || y >= Data.Size.y || x < 0 || y < 0)
        {
            entry = new EntryData()
            {
                Position = tilePos,
                Type = EntryType.Wall,
                Id = 0,
            };
            return true;
        }
        var entryIndex = m_Occupations[x, y] - 1;
        if (entryIndex < 0)
        {
            entry = default;
            return false;
        }
        entry = Data.Entries[entryIndex];
        return true;
    }

    public bool Clear(Vector2Int tilePos, out MapData.EntryData entry)
    {
        var x = tilePos.x;
        var y = tilePos.y;
        if (x >= Data.Size.x || y >= Data.Size.y || x < 0 || y < 0)
        {
            entry = default;
            return false;
        }

        var entryIndex = m_Occupations[x, y] - 1;
        if (entryIndex < 0)
        {
            entry = default;
            return false;
        }
        m_Occupations[x, y] = 0;
        entry = Data.Entries[entryIndex];
        Data.Entries[entryIndex] = default;
        freeEntryIndex.Push(entryIndex);
        return true;
    }

    internal Vector3 GetPosition(Vector2Int moveOffset)
    {
        var min = new Vector3(-Data.Size.x / 2, 0, -Data.Size.y / 2);
        return min + new Vector3(moveOffset.x, 0, moveOffset.y);
    }

    internal void OnFinal()
    {
        Debug.LogError("Final!");
        AudioSystem.instance.succeed();
        StartCoroutine(delayOpenFinishPanel(true));
    }

    internal void OnDrop()
    {
        Debug.LogError("Drop!");
        AudioSystem.instance.playerDrop();
        StartCoroutine(delayOpenFinishPanel(false));
    }

    IEnumerator delayOpenFinishPanel(bool isSucceed)
    {
        var timeSpan = DateTimeOffset.Now - startTime;
        yield return new WaitForSecondsRealtime(isSucceed ? 1f : 2f);
        var panel = FindObjectOfType<FinishPannel>(true);
        if (panel != null)
        {
            panel.Init(isSucceed, timeSpan, step);
        }
    }
}