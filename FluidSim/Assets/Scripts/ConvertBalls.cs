using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Converts the balls representing the collisions to a list of data
/// </summary>
public class ConvertBalls : MonoBehaviour
{
    // base data
    public Transform ballsParent;
    public string fileName;

    /// <summary>
    /// Saves the ball data.
    /// </summary>
    public void SaveBallData()
    {
        // create a new list of balls
        Balls balls = new();

        // add all of the balls to the list
        var childCount = ballsParent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            balls.balls.Add(ballsParent.GetChild(i).position);
        }

        // save the ball data to file
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

/// <summary>
/// Holds a list of balls (because JSON serilization)
/// </summary>
public class Balls
{
    public List<Vector3> balls = new();
}