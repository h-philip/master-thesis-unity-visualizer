using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using JetBrains.Annotations;
using SimpleFileBrowser;
using System.Diagnostics;
using UnityEngine.UIElements;

public class FilesLoader : MonoBehaviour
{
    public string DefaultDirectory;
    public GameObject ObstaclePrefab;
    public float ObstacleRadius;
    public GameObject WaypointPrefab;
    public float WaypointRadius;
    public GameObject GoalSpacePrefab;
    public TimeController TimeControllerObject;
    public LineRenderer MainPath;
    public LineRenderer ExtraPath;

    private readonly List<GameObject> obstacles = new();
    private readonly List<Vector3> path = new();
    private readonly List<float> times = new();
    private readonly List<GameObject> route = new();
    private readonly List<GameObject> goalSpaces = new();

    void Update()
    {

    }
    
    private void LoadObstacles(string file)
    {
        foreach (GameObject go in obstacles)
        {
            Destroy(go);
        }
        obstacles.Clear();

        StreamReader reader = new(file);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            float[] values = Array.ConvertAll(line.Split(" "), float.Parse);
            Vector3 position = new(values[0], values[2], values[1]);
            GameObject obstacle = Instantiate(ObstaclePrefab, position, Quaternion.identity);
            obstacle.transform.localScale = 2 * ObstacleRadius * Vector3.one;
            obstacles.Add(obstacle);
        }
    }

    private void LoadPath(string file)
    {
        path.Clear();

        StreamReader reader = new(file);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            float[] values = Array.ConvertAll(line.Split(" "), float.Parse);
            Vector3 position = new(values[0], values[2], values[1]);
            path.Add(position);
        }

		MainPath.positionCount = path.Count;
		MainPath.SetPositions(path.ToArray());

        TimeControllerObject.ResetPath(path);
    }

    private void AddPath(string file)
    {
        List<Vector3> list = new();

        StreamReader reader = new(file);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            float[] values = Array.ConvertAll(line.Split(" "), float.Parse);
            Vector3 position = new(values[0], values[2], values[1]);
            list.Add(position);
        }

		ExtraPath.positionCount = list.Count;
		ExtraPath.SetPositions(list.ToArray());
    }

    private void LoadTimes(string file)
    {
        times.Clear();

        StreamReader reader = new(file);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            float[] values = Array.ConvertAll(line.Split(" "), float.Parse);
            times.Add(values[0]);
        }

        TimeControllerObject.ResetTimes(times);
    }

    private void LoadRoute(string file)
    {
        route.Clear();

        StreamReader reader = new(file);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            float[] values = Array.ConvertAll(line.Split(" ")[0..3], float.Parse);
            Vector3 position = new(values[0], values[2], values[1]);
            GameObject waypoint = Instantiate(WaypointPrefab, position, Quaternion.identity);
            waypoint.transform.localScale = 2 * WaypointRadius * Vector3.one;
            route.Add(waypoint);
        }
    }

    private void LoadGoalSpaces(string file)
    {
        goalSpaces.Clear();

        StreamReader reader = new(file);
        float minX, minY, minZ, maxX, maxY, maxZ;
        minX = minY = minZ = float.MaxValue;
        maxX = maxY = maxZ = float.MinValue;
        bool wasBlankLine = false;
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();

            if (line.Length == 0)
            {
                if (wasBlankLine && minX < float.MaxValue)
                {
                    AddGoalSpace(minX, minY, minZ, maxX, maxY, maxZ);
                    minX = minY = minZ = float.MaxValue;
                    maxX = maxY = maxZ = float.MinValue;
                }
                wasBlankLine = true;
                continue;
            }
            wasBlankLine = false;

            float[] values = Array.ConvertAll(line.Split(" "), float.Parse);
            if (values[0] < minX)
                minX = values[0];
            if (values[2] < minY)
                minY = values[2];
            if (values[1] < minZ)
                minZ = values[1];
            if (values[0] > maxX)
                maxX = values[0];
            if (values[2] > maxY)
                maxY = values[2];
            if (values[1] > maxZ)
                maxZ = values[1];
        }
        if (minX < float.MaxValue)
            AddGoalSpace(minX, minY, minZ, maxX, maxY, maxZ);
    }

    private void AddGoalSpace(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
        Vector3 size = new(maxX - minX, maxY - minY, maxZ - minZ);
        Vector3 center = new((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2);
        GameObject goalSpace = Instantiate(GoalSpacePrefab, center, Quaternion.identity);
        goalSpace.transform.localScale = size;
        goalSpaces.Add(goalSpace);
    }

    public void LoadFiles()
	{
		FileBrowser.ShowLoadDialog((paths) =>
		{
			string path = paths[0];
			if (string.IsNullOrEmpty(path))
				return;
			string file = Path.Join(path, "obstacles.dat");
			if (File.Exists(file))
			{
				LoadObstacles(file);
			}
			file = Path.Join(path, "path.dat");
			if (File.Exists(file))
			{
				LoadPath(file);
			}
            else
            {
                file = Path.Join(path, "path_disturbance_not_applied.dat");
				if (File.Exists(file))
				{
					LoadPath(file);
                    file = Path.Join(path, "path_disturbance_applied.dat");
					if (File.Exists(file))
					{
						AddPath(file);
					}
				}
                else
				{
					file = Path.Join(path, "path_disturbance_applied.dat");
					if (File.Exists(file))
					{
						LoadPath(file);
					}
				}
                
			}
			file = Path.Join(path, "acceleration_x.dat");
			if (File.Exists(file))
			{
				LoadTimes(file);
			}
			file = Path.Join(path, "route.dat");
			if (File.Exists(file))
			{
				LoadRoute(file);
			}
			file = Path.Join(path, "goal_space.dat");
			if (File.Exists(file))
			{
				LoadGoalSpaces(file);
			}
		},
		null,
		FileBrowser.PickMode.Folders, false, DefaultDirectory, null, "Folder to load files from", "Select");
	}

	public void LoadPath()
	{
		FileBrowser.ShowLoadDialog((paths) =>
		{
			string path = paths[0];

			if (string.IsNullOrEmpty(path))
				return;
			string file = Path.Join(path, "path.dat");
			if (File.Exists(file))
			{
				AddPath(file);
			}
		},
		null,
		FileBrowser.PickMode.Folders, false, DefaultDirectory, null, "Folder to load files from", "Select");
	}
}
