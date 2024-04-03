using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConvertBalls : MonoBehaviour
{
    public Transform ballsParent;
    public string fileName;

    public void SaveBallData()
    {
        Balls balls = new();

        var childCount = ballsParent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            balls.balls.Add(ballsParent.GetChild(i).position);
        }

        string json = JsonUtility.ToJson(balls);
        string path = Application.persistentDataPath + "/" + fileName;
        FileStream fileStream = new FileStream(path, FileMode.Create);
        using (StreamWriter writer = new StreamWriter(fileStream))
        {
            writer.Write(json);
        }

        print("Successfully Saved: " + childCount + " Points");
    }
}
public class Balls
{
    public List<Vector3> balls = new();
}