using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// "ConvertBallsEditor" will add a button to the UI off "ConvertBalls" that will turn the balls into save data
/// </summary>
[CustomEditor(typeof(ConvertBalls))]
public class ConvertBallsEditor : Editor
{
    // add the Button to the inspector
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