using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    //public override void OnInspectorGUI()
    //{
    //    base.OnInspectorGUI();

    //    var map = (Map)target;

    //    if (GUILayout.Button("生成地面"))
    //        map.Generate();
    //}
}
