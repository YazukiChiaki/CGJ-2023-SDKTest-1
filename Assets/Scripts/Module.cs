using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapData;

public class Module : MonoBehaviour, IAttachableMapObject
{
    public Transform[] Blocks;
    public Transform[] Children => Blocks;
    public SpriteRenderer Hint;
    public MapData.EntryType EntryType;
    public EntryType Type => EntryType;
    public Material TouchedMat;
    public Material UnTouchedMat;
    [SerializeField]
    private bool canAttach;

    public bool CanAttach => canAttach;

    // Start is called before the first frame update
    void Start()
    {
        UnAttach();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Touch()
    {
        if (Hint != null)
        {
            Hint.color = Color.red;
        }


    }

    public void UnTouch()
    {
        if (Hint != null)
        {
            Hint.color = Color.white;
        }
    }

    public void Attach()
    {
        if (Hint != null)
        {
            Hint.color = Color.green;
        }

        if (TouchedMat != null)
        {
            foreach (var b in Blocks)
            {
                var r = b.GetComponent<Renderer>();
                if (r != null)
                {
                    r.material = TouchedMat;
                }
            }
        }
    }
    
    public void UnAttach()
    {
        if (Hint != null)
        {
            Hint.color = Color.white;
        }

        if (UnTouchedMat != null)
        {
            foreach (var b in Blocks)
            {
                var r = b.GetComponent<Renderer>();
                if (r != null)
                {
                    r.material = UnTouchedMat;
                }
            }
        }
    }
}
