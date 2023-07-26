using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Linq;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.SceneManagement;

public class Volumn
{
    public List<MapData.EntryData> entries = new List<MapData.EntryData>();
    private List<GameObject> attackment = new List<GameObject>();
}

public class Movement : MonoBehaviour
{
    Transform cubes;
    bool isMoving;
    bool isRotating;
    bool isAttaching;
    bool isDropped;
    bool isFinal;
    bool isFinish => isDropped || isFinal;
    bool isBuzy => isMoving || isRotating || isAttaching;
    float moveDuration = 0.2f;
    public InputAction MoveAction;
    public InputAction TouchAction;
    public InputAction RotateAction;
    public InputAction ReloadAction;

    private Map map;
    private Volumn volumn = new Volumn();
    private int tileRotate;
    private Vector2Int tilePosition;

    // Start is called before the first frame update
    void Start()
    {
        if (map == null)
        {
            map = FindObjectOfType<Map>();
        }
        cubes = transform;
        tilePosition = map.GetTilePosition(transform.position);
    }

    private void OnEnable()
    {
        MoveAction.Enable();
        TouchAction.Enable();
        RotateAction.Enable();
        ReloadAction.Enable();
    }

    private void OnDisable()
    {
        MoveAction.Disable();
        TouchAction.Disable();
        RotateAction.Disable();
        ReloadAction.Disable();
    }

    // Update is called once per frame
    private Vector2Int previousMoveOffset;
    void Update()
    {
        //float h = Input.GetAxisRaw("Horizontal");
        //float v = Input.GetAxisRaw("Vertical");
        if (isFinish) return;
        var vec = MoveAction.ReadValue<Vector2>();
        var h = vec.x;
        var v = vec.y;

        if (ReloadAction.WasPressedThisFrame())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (!isBuzy)
        {
            //var targetPos = cubes.position;
            var moveOffset = Vector2Int.zero;

            if (h > 0)
            {
                moveOffset.x++;
            }
            else if (h < 0)
            {
                moveOffset.x--;
            }
            else if (v > 0)
            {
                moveOffset.y++;
            }
            else if (v < 0)
            {
                moveOffset.y--;
            }

            if (moveOffset != Vector2Int.zero)
            {
                previousMoveOffset = moveOffset;
                Move(moveOffset);
            }
        }
        if (!isBuzy && TouchAction.WasPressedThisFrame())
        {
            if (touchTargets.Count != 0)
            {
                var touchTargetsCached = touchTargets.ToArray();
                foreach (var targetId in touchTargetsCached)
                {
                    var obj = map.targetGOs[targetId];
                    if (!obj.TryGetComponent<IAttachableMapObject>(out var mObject) || !mObject.CanAttach) continue;
                    mObject.UnTouch();
                    mObject.Attach();
                    foreach(var l in map.RemoveMapObject(mObject))
                    {
                        var entry = l;
                        entry.Position -= tilePosition;
                        entry.Position = entry.Position.Rotate(-tileRotate);
                        volumn.entries.Add(entry);
                    }
                    obj.transform.SetParent(transform, true);
                    touchTargets.Remove(targetId);
                }
                map.step++;
                AudioSystem.instance.playerTouch();
            }
            else
            {
                var hasUnAttached = new HashSet<int>();
                for(var i = volumn.entries.Count - 1; i >= 0; i--)
                {
                    var entry = volumn.entries[i];
                    if (entry.Type == MapData.EntryType.Target) continue;
                    if (hasUnAttached.Add(entry.Id))
                    {
                        var obj = map.targetGOs[entry.Id];
                        obj.transform.SetParent(map.TargetRoot, true);
                        if (obj.TryGetComponent<IAttachableMapObject>(out var mObj))
                        {
                            mObj.UnTouch();
                            mObj.UnAttach();
                            map.AddMapObject(mObj);
                        }
                    }
                    volumn.entries.RemoveAt(i);
                }

                if (hasUnAttached.Count != 0)
                {
                    map.step++;
                    AudioSystem.instance.playerUntouch();
                }

                var overlapResult = OverlapResult.Create();
                ProcessOverlap(Vector2Int.zero, ref overlapResult, new MapData.EntryData()
                {
                    Position = Vector2Int.zero,
                    Type = MapData.EntryType.Mine
                }, tileRotate);
                for (var i = 0; i < volumn.entries.Count; i++)
                {
                    ProcessOverlap(Vector2Int.zero, ref overlapResult, volumn.entries[i], tileRotate);
                    if (!overlapResult.canMove) break;
                }

                if (overlapResult.holeCount >= volumn.entries.Count + 1)
                {
                    map.OnDrop();
                    isDropped = true;
                }

                if (isDropped)
                {
                    cubes.DOMoveY(-500f, 5f).SetEase(Ease.InQuad);
                }
            }
        }

        if (!isRotating && RotateAction.WasPressedThisFrame())
        {
            var newRotate = tileRotate + (int)RotateAction.ReadValue<float>();

            var overlapResult = OverlapResult.Create();
            ProcessOverlap(Vector2Int.zero, ref overlapResult, new MapData.EntryData()
            {
                Position = Vector2Int.zero,
                Type = MapData.EntryType.Mine
            }, newRotate);
            for (var i = 0; i < volumn.entries.Count; i++)
            {
                ProcessOverlap(Vector2Int.zero, ref overlapResult, volumn.entries[i], newRotate);
                if (!overlapResult.canMove) break;
            }

            if (overlapResult.holeCount >= volumn.entries.Count + 1)
            {
                map.OnDrop();
                isDropped = true;
            }

            if (overlapResult.finalCount >= map.FinalTotalCount)
            {
                map.OnFinal();
                isFinal = true;
            }

            if (overlapResult.canMove)
            {
                tileRotate = newRotate;
                isRotating = true;
                transform.DOJump(transform.position, 5, 1, 0.5f);
                var seq = DOTween.Sequence();
                seq.Insert(.2f, transform.DORotate(new Vector3(0, tileRotate * 90, 0), 0.2f));
                if (!isDropped)
                {
                    seq.OnComplete(() =>
                     {
                         isRotating = false;
                         detectTouch();
                     });
                }
                else
                {
                    seq.Append(cubes.DOMoveY(-500f, 5f).SetEase(Ease.InQuad));
                }

                map.step++;
                AudioSystem.instance.playerRotate();
            }
        }
    }

    private void Move(Vector2Int moveOffset)
    {
        var overlapResult = OverlapResult.Create();
        ProcessOverlap(moveOffset, ref overlapResult, new MapData.EntryData() 
        {
            Position = Vector2Int.zero,
            Type = MapData.EntryType.Mine
        }, tileRotate);
        if (!overlapResult.canMove) return;
        foreach (var volumnEntry in volumn.entries)
        {
            ProcessOverlap(moveOffset, ref overlapResult, volumnEntry, tileRotate);
            if (!overlapResult.canMove) return;
        }

        if (overlapResult.holeCount >= volumn.entries.Count + 1)
        {
            map.OnDrop();
            isDropped = true;
        }

        if (overlapResult.finalCount >= map.FinalTotalCount)
        {
            map.OnFinal();
            isFinal = true;
        }

        if (overlapResult.canMove)
        {
            isMoving = true;
            Sequence scaleSequence = DOTween.Sequence();
            scaleSequence.Append(cubes.DOScaleY(0.7f, 0.1f));
            scaleSequence.Append(cubes.DOScaleY(1f, 0.1f));

            var moveSeq = DOTween.Sequence();
            tilePosition += moveOffset;
            moveSeq.Append(cubes.DOMove(map.GetPosition(tilePosition), moveDuration));
            map.step++;
            AudioSystem.instance.playerMove();

            if (!isDropped)
            {
                moveSeq.OnComplete(() =>
                {
                    isMoving = false;
                    detectTouch();
                });
            }
            else
            {
                moveSeq.Append(cubes.DOMoveY(-500f, 5f).SetEase(Ease.InQuad));
            }
        }
    }

    public struct OverlapResult
    {
        public int holeCount;
        public int finalCount;
        public bool canMove;

        public static OverlapResult Create()
        {
            var ret = default(OverlapResult);
            ret.canMove = true;
            ret.holeCount = 0;
            return ret;
        }
    }

    private void ProcessOverlap(Vector2Int moveOffset, ref OverlapResult result, MapData.EntryData entryData, int rotate)
    {
        var offset = entryData.Position;
        var localTilePos = tilePosition;
        localTilePos += moveOffset;
        localTilePos += offset.Rotate(rotate);

        if (map.Overlap(localTilePos, out var entry))
        {
            switch (entry.Type)
            {
                case MapData.EntryType.None:
                case MapData.EntryType.Floor:
                case MapData.EntryType.Switch:
                case MapData.EntryType.Button:
                    break;
                case MapData.EntryType.Hole:
                    result.holeCount++;
                    break;
                case MapData.EntryType.Final:
                    if (/*entryData.Type == MapData.EntryType.Mine || */entryData.Type == MapData.EntryType.Target)
                        result.finalCount++;
                    break;
                default:
                    result.canMove &= false;
                    break;
            }
        }
    }

    HashSet<Vector2Int> tempHashSetVector2Int = new HashSet<Vector2Int>();
    HashSet<int> touchTargets = new HashSet<int>();
    HashSet<int> previousTouchTarget = new HashSet<int>();
    private void detectTouch()
    {
        tempHashSetVector2Int.Clear();
        var ret = tempHashSetVector2Int;
        ret.Add(new Vector2Int(0, -1));
        ret.Add(new Vector2Int(0, 1));
        ret.Add(new Vector2Int(1, 0));
        ret.Add(new Vector2Int(-1, 0));

        foreach (var e in volumn.entries)
        {
            if (e.Type != MapData.EntryType.Target) continue; 
            ret.Add(e.Position + new Vector2Int(0, -1));
            ret.Add(e.Position + new Vector2Int(0, 1));
            ret.Add(e.Position + new Vector2Int(1, 0));
            ret.Add(e.Position + new Vector2Int(-1, 0));
        }

        foreach (var e in volumn.entries)
        {
            ret.Remove(e.Position);
        }


        (touchTargets, previousTouchTarget) = (previousTouchTarget, touchTargets);

        touchTargets.Clear();
        foreach (var pos in tempHashSetVector2Int)
        {
            if (map.Overlap(tilePosition + pos.Rotate(tileRotate), out var entry))
            {
                if (entry.Type != MapData.EntryType.Target
                    && entry.Type != MapData.EntryType.Nontarget) continue;
                var targetId = entry.Id;
                if (map.targetGOs[targetId].TryGetComponent<IMapObject>(out var mObject))
                {
                    touchTargets.Add(targetId);
                }
            }
        }

        foreach (var e in previousTouchTarget)
        {
            if (!touchTargets.Contains(e))
            {
                if (map.targetGOs[e].TryGetComponent<IMapObject>(out var mObject))
                {
                    mObject.UnTouch();
                }
            }
        }

        foreach (var e in touchTargets)
        {
            if (!previousTouchTarget.Contains(e))
            {
                if (map.targetGOs[e].TryGetComponent<IMapObject>(out var mObject))
                {
                    mObject.Touch();
                }
            }
        }
    }
}
