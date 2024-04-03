using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConvertBalls))]
public class ConvertBallsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ConvertBalls script = (ConvertBalls)target;
        if (GUILayout.Button("Open File"))
        {
            script.SaveBallData();
        }
    }
}