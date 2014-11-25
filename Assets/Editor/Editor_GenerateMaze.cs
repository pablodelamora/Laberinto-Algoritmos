using UnityEngine;
using UnityEditor;

public class Editor_GenerateMaze : EditorWindow
{
	[MenuItem ("Edit/Generate Maze %m")]
	private static void GenerateMaze()
	{
		bool scriptfound = false;

		if (Selection.activeTransform == null)
		{
			Debug.LogWarning("First select an object(s) with 'MazeGeneration.cs' script attached to it!");
		}
		else
		{
			foreach (GameObject obj in Selection.gameObjects)
			{
				Maze mazeScript;
				mazeScript = obj.GetComponent<Maze>();
				if (mazeScript != null)
				{
					mazeScript.GenerateMaze();
					scriptfound = true;
				}
			}
			if (!scriptfound)
			{
				Debug.LogWarning("No 'MazeGeneration.cs' script is attached to the selected objects!");
			}
		}
	}

	[MenuItem ("Edit/Refresh Maze %#m")]
	private static void RefreshMaze()
	{
		bool scriptfound = false;
		
		if (Selection.activeTransform == null)
		{
			Debug.LogWarning("First select an object(s) with 'MazeGeneration.cs' script attached to it!");
		}
		else
		{
			foreach (GameObject obj in Selection.gameObjects)
			{
				Maze mazeScript;
				mazeScript = obj.GetComponent<Maze>();
				if (mazeScript != null)
				{
					mazeScript.DeleteMaze(true);
					mazeScript.GenerateObjects();
					scriptfound = true;
				}
			}
			if (!scriptfound)
			{
				Debug.LogWarning("No 'MazeGeneration.cs' script is attached to the selected objects!");
			}
		}
	}

	[MenuItem ("Edit/Remove Maze %#r")]
	private static void RemoveMaze()
	{
		bool scriptfound = false;
		
		if (Selection.activeTransform == null)
		{
			Debug.LogWarning("First select an object(s) with 'MazeGeneration.cs' script attached to it!");
		}
		else
		{
			foreach (GameObject obj in Selection.gameObjects)
			{
				Maze mazeScript;
				mazeScript = obj.GetComponent<Maze>();
				if (mazeScript != null)
				{
					mazeScript.DeleteMaze(true);
					scriptfound = true;
				}
			}
			if (!scriptfound)
			{
				Debug.LogWarning("No 'MazeGeneration.cs' script is attached to the selected objects!");
			}
		}
	}
}
