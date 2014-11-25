using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Attach this script to the Plane object(s)

public class Maze : MonoBehaviour
{
	public bool fitSize = true;							
	[Range(5, 1000)] public int xSize = 48;				 
	[Range(5, 1000)] public int ySize = 48;				
	[Range(50, 950)] public int mapDensity = 600;		
	public bool showDebugMessages = false;				
	public GameObject visibleWallObject;				
	public bool visibleWallsAtDiagonals = false;		
	public bool mergeVisibleWalls = true;				
	public GameObject invisibleWallObject;				
	public bool mergeInvisibleWalls = true;				
		
	public GameObject floorObject;						
	public bool mergeFloors = true;											
	public GameObject pathObject;			
	public List<GameObject> objectsToSpawn;				
	[Range(0, 100)] public float objectsQuantity = 2;	
	public bool spawnEvenly = false;					

	string prefabNamePrefix = "#Maze#";					
	int[,] map;											
	int height;
	int width;
	int objectsCreated;
	int visibleWalls;
	int invisibleWalls;
	int floors;
	int floorCount;
	int countFrom = 100000;
	List<GameObject> spawnedObjects = new List<GameObject>();


	GameObject ball1;
	GameObject ball2;
	Vector3 ball1Size;
	Vector3 ball2Size;
	Camera cameraTop;
	Camera camera3D;
	float cameraTopX;
	float camera3DX;
	bool showTexts = false;
	GUIStyle textStyle = new GUIStyle();
	int currentMapDensity;
	bool updateStats = false;
	int freeFieldsCount = 0;
	int distance = -1;
	int maxDistanceBall1 = 0;
	int maxDistanceBall2 = 0;
	int blueBalls = 0;
	int cyanBalls = 0;
	int greenBalls = 0;
	int yellowBalls = 0;
	int smallWhiteBalls = 0;
	int smallPurpleBalls = 0;



	// Field = Map coordinates (0, 0 is the left bottom corner)
	public struct Field
	{
		public int x;
		public int y;
	}

	// Create Map and place White and Purple Ball (can be deleted).
	void Start ()
	{
		cameraTop = GameObject.Find("Camera Top").GetComponent<Camera>();
		camera3D = GameObject.Find("Camera 3D").GetComponent<Camera>();
		cameraTopX = cameraTop.transform.localPosition.x;
		camera3DX = camera3D.transform.localPosition.x;
		ball1 = GameObject.Find("Sphere 1");
		ball2 = GameObject.Find("Sphere 2");
		ball1Size = ball1.transform.lossyScale;
		ball2Size = ball2.transform.lossyScale;
		textStyle.alignment = TextAnchor.MiddleLeft;
		GenerateMaze();
		currentMapDensity = mapDensity;
		updateStats = true;

		// <Place White and Purple Ball>
		List<Field> freeFields = new List<Field>();
		freeFields = GetFreeFields();
		if (freeFields.Count >= 2)
		{
			int index = Random.Range(0, freeFields.Count);
			ball1.transform.localPosition = GetFieldLocalPos(freeFields[index]) + new Vector3(0, 0.1f, 0);
			freeFields.RemoveAt(index);
			index = Random.Range(0, freeFields.Count);
			ball2.transform.localPosition = GetFieldLocalPos(freeFields[index]) + new Vector3(0, 0.1f, 0);
		}
		else
		{
			ball1.transform.localPosition = GetFieldLocalPos(new Field() {x = 0, y = 0});
			ball2.transform.localPosition = GetFieldLocalPos(new Field() {x = xSize + 1, y = ySize + 1});
		}
		spawnedObjects.Add(ball1);
		spawnedObjects.Add(ball2);
		// </Place White and Purple Ball>

		cameraTop.transform.localPosition = new Vector3(0, cameraTop.transform.localPosition.y, cameraTop.transform.localPosition.z);
		//camera3D.transform.localPosition = new Vector3 (0, camera3D.transform.localPosition.y, camera3D.transform.localPosition.z);
	}

	void Update ()
	{



		// Crear laberinto
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			GenerateMaze();
			currentMapDensity = mapDensity;
			updateStats = true;
			ball1.transform.parent = null;
			ball2.transform.parent = null;
			ball1.transform.localScale = ball1Size;
			ball2.transform.localScale = ball2Size;
			ball1.transform.parent = transform;
			ball2.transform.parent = transform;

			// Poner las bolitas blanca y rosa
			List<Field> freeFields = new List<Field>();
			freeFields = GetFreeFields();
			if (freeFields.Count >= 2)
			{
				int index = Random.Range(0, freeFields.Count);
				ball1.transform.localPosition = GetFieldLocalPos(freeFields[index]) + new Vector3(0, 0.1f, 0);
				freeFields.RemoveAt(index);
				index = Random.Range(0, freeFields.Count);
				ball2.transform.localPosition = GetFieldLocalPos(freeFields[index]) + new Vector3(0, 0.1f, 0);
			}
			else
			{
				ball1.transform.localPosition = GetFieldLocalPos(new Field() {x = 0, y = 0});
				ball2.transform.localPosition = GetFieldLocalPos(new Field() {x = xSize + 1, y = ySize + 1});
			}
			spawnedObjects.Add(ball1);
			spawnedObjects.Add(ball2);
			// 
		}
		// </New Maze>

		// <Change Next Map Density>
		if (Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			if (mapDensity <= 900)
			{
				mapDensity += 50;
			}
		}
		if (Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			if (mapDensity >= 100)
			{
				mapDensity -= 50;
			}
		}
		// </Change Next Map Density>

		// <White Ball movement>
		Field targetField = new Field() {x = 0, y = 0};
		Field originalField = new Field() {x = 0, y = 0};
		bool move = false;
		bool moved = false;
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			originalField = GetField(ball1);
			targetField = originalField;
			if (targetField.x > 0)
			{
				targetField.x--;
				move = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			originalField = GetField(ball1);
			targetField = originalField;
			if (targetField.x < xSize + 1)
			{
				targetField.x++;
				move = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			originalField = GetField(ball1);
			targetField = originalField;
			if (targetField.y > 0)
			{
				targetField.y--;
				move = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			originalField = GetField(ball1);
			targetField = originalField;
			if (targetField.y < ySize + 1)
			{
				targetField.y++;
				move = true;
			}
		}
		if (move)
		{
			if (Input.GetKey(KeyCode.RightShift))
			{
				ball1.transform.localPosition = GetFieldLocalPos(targetField) + new Vector3(0, 0.1f, 0);
				moved = true;
				updateStats = true;
			}
			else
			{
				if (map[targetField.x, targetField.y] > 0)
				{
					bool blocked = false;
					foreach (GameObject obj in spawnedObjects)
					{
						if (GetField(obj).x == targetField.x && GetField(obj).y == targetField.y)
						{
							blocked = true;
							break;
						}
					}
					if (!blocked)
					{
						ball1.transform.localPosition = GetFieldLocalPos(targetField) + new Vector3(0, 0.1f, 0);
						moved = true;
						updateStats = true;
					}
				}
			}
		}
		if (moved && Input.GetKey(KeyCode.RightControl))
		{
			// <Drop Small White Ball>
			if (map[originalField.x, originalField.y] > 0)
			{
				bool blocked = false;
				foreach (GameObject obj in spawnedObjects)
				{
					if (GetField(obj).x == originalField.x && GetField(obj).y == originalField.y)
					{
						blocked = true;
						break;
					}
				}
				if (!blocked)
				{
					CreateObject(ball1, originalField, 0.4f);
				}
			}
			// </Drop Small White Ball>
		}
		// </White Ball movement>

		// <Purple Ball movement>
		targetField = new Field() {x = 0, y = 0};
		originalField = new Field() {x = 0, y = 0};
		move = false;
		moved = false;
		if (Input.GetKeyDown(KeyCode.A))
		{
			originalField = GetField(ball2);
			targetField = originalField;
			if (targetField.x > 0)
			{
				targetField.x--;
				move = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.D))
		{
			originalField = GetField(ball2);
			targetField = originalField;
			if (targetField.x < xSize + 1)
			{
				targetField.x++;
				move = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			originalField = GetField(ball2);
			targetField = originalField;
			if (targetField.y > 0)
			{
				targetField.y--;
				move = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.W))
		{
			originalField = GetField(ball2);
			targetField = originalField;
			if (targetField.y < ySize + 1)
			{
				targetField.y++;
				move = true;
			}
		}
		if (move)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				ball2.transform.localPosition = GetFieldLocalPos(targetField) + new Vector3(0, 0.1f, 0);
				moved = true;
				updateStats = true;
			}
			else
			{
				if (map[targetField.x, targetField.y] > 0)
				{
					bool blocked = false;
					foreach (GameObject obj in spawnedObjects)
					{
						if (GetField(obj).x == targetField.x && GetField(obj).y == targetField.y)
						{
							blocked = true;
							break;
						}
					}
					if (!blocked)
					{
						ball2.transform.localPosition = GetFieldLocalPos(targetField) + new Vector3(0, 0.1f, 0);
						moved = true;
						updateStats = true;
					}
				}
			}
		}
		if (moved && Input.GetKey(KeyCode.LeftControl))
		{
			// <Drop Small Purple Ball>
			if (map[originalField.x, originalField.y] > 0)
			{
				bool blocked = false;
				foreach (GameObject obj in spawnedObjects)
				{
					if (GetField(obj).x == originalField.x && GetField(obj).y == originalField.y)
					{
						blocked = true;
						break;
					}
				}
				if (!blocked)
				{
					CreateObject(ball2, originalField, 0.5f);
				}
			}
			// </Drop Small Purple Ball>
		}
		// </Purple Ball movement>

		// <Generate Path>
		if (Input.GetKeyDown(KeyCode.P))
		{
			DeletePath();
			CreatePath(ball1, ball2, false, true);
		}
		// </Generate Path>

		// <Spawn 10 Yellow Balls>
		if (Input.GetKeyDown(KeyCode.R))
		{
//			SpawnObject((GameObject)Resources.Load("Yellow Ball"), 10);
			updateStats = true;
		}

	}



	
	public void GenerateMaze()
	{
		bool mapGenerated = false;
		int middleX;
		int middleY;

		if (fitSize)
		{
			xSize = Mathf.FloorToInt(transform.lossyScale.x * 10) - 2;
			ySize = Mathf.FloorToInt(transform.lossyScale.z * 10) - 2;
		}
		
		// Clear all the objects from the previous generation
		DeleteMaze(false);

		if (xSize < 5 || ySize < 5)
		{
			if (fitSize)
			{
				Debug.LogWarning("The plane object '" + transform.name + "' is too small to create a Maze on it!");
			}
			else
			{
				Debug.LogWarning(transform.name + ": No se puede crear!");
			}
		}
		else
		{
			if (xSize * ySize >= 10000)
			{
				Debug.LogWarning(transform.name + ": Too big mazes can take too much time to be created!");
			}

			// <Generate map>
			floorCount = 0;

			// <Map size at least 1>
			do
			{
				map = new int[xSize + 2, ySize + 2];
				middleX = Random.Range(xSize / 3, 2 * xSize / 3) + 1;
				middleY = Random.Range(ySize / 3, 2 * ySize / 3) + 1;
				map[middleX, middleY] = 1;
				map[middleX, middleY - 1] = 2;
				map[middleX + 1, middleY] = 3;
				map[middleX, middleY + 1] = 4;
				map[middleX - 1, middleY] = 5;
				for (int i = 1; i <= xSize; i++)
				{
					for (int j = 1; j <= ySize; j++)
					{
						if (map[i, j] == 0 && map[i, j - 1] == 0 && map[i + 1, j] == 0 && map[i, j + 1] == 0 && map[i - 1, j] == 0)
						{
							int r = Random.Range(0, 1000 - mapDensity);
							if (r == 0)
							{
								map[i, j] = 1;
								map[i, j - 1] = 2;
								map[i + 1, j] = 3;
								map[i, j + 1] = 4;
								map[i - 1, j] = 5;
							}
							if (r == 1)
							{
								map[i, j] = 1;
								map[i + 1, j] = 3;
								map[i, j + 1] = 4;
								map[i - 1, j] = 5;
							}
							if (r == 2)
							{
								map[i, j] = 1;
								map[i, j - 1] = 2;
								map[i, j + 1] = 4;
								map[i - 1, j] = 5;
							}
							if (r == 3)
							{
								map[i, j] = 1;
								map[i, j - 1] = 2;
								map[i + 1, j] = 3;
								map[i - 1, j] = 5;
							}
							if (r == 4)
							{
								map[i, j] = 1;
								map[i, j - 1] = 2;
								map[i + 1, j] = 3;
								map[i, j + 1] = 4;
							}
							if (r == 5)
							{
								map[i, j] = 1;
								map[i, j - 1] = 2;
								map[i + 1, j] = 3;
							}
							if (r == 6)
							{
								map[i, j] = 1;
								map[i, j - 1] = 2;
								map[i, j + 1] = 4;
							}
							if (r == 7)
							{
								map[i, j] = 1;
								map[i, j - 1] = 2;
								map[i - 1, j] = 5;
							}
							if (r == 8)
							{
								map[i, j] = 1;
								map[i + 1, j] = 3;
								map[i, j + 1] = 4;
							}
							if (r == 9)
							{
								map[i, j] = 1;
								map[i + 1, j] = 3;
								map[i - 1, j] = 5;
							}
							if (r == 10)
							{
								map[i, j] = 1;
								map[i, j + 1] = 4;
								map[i - 1, j] = 5;
							}
							if (r == 11)
							{
								map[i, j] = 1;
								map[i, j - 1] = 2;
							}
							if (r == 12)
							{
								map[i, j] = 1;
								map[i + 1, j] = 3;
							}
							if (r == 13)
							{
								map[i, j] = 1;
								map[i, j + 1] = 4;
							}
							if (r == 14)
							{
								map[i, j] = 1;
								map[i - 1, j] = 5;
							}
						}
						if (i == 1)
						{
							map[i - 1, j] = 0;
						}
						if (j == 1)
						{
							map[i, j - 1] = 0;
						}
						if (i == xSize)
						{
							map[i + 1, j] = 0;
						}
						if (j == ySize)
						{
							map[i, j + 1] = 0;
						}
					}
				}
				for (int k = 1; k <= xSize; k++)
				{
					for (int l = 1; l <= ySize; l++)
					{
						if (map[k, l] == 2)
						{
							for (int j = 2; j <= ySize / 2; j++)
							{
								for (int i = -j; i <= j; i++)
								{
									if (map[k, l - 1] == 0 && l - j > -1 && k + i > -1 && k + i < xSize && (map[k + i, l - j] == 3 || map[k + i, l - j] == 4 || map[k + i, l - j] == 5))
									{
										map[k, l] = 10;
										map[k + i, l - j] = 10;
										map[k, l - 1] = 10;
										int m = 0;
										int n = 0;
										int o = 0;
										do
										{
											int r = Random.Range(0, 2);
											if (r == 0 && n - 1 > -j)
											{
												n--;
												if (map[k + m, l - 1 + n] == 0)
												{
													map[k + m, l - 1 + n] = 10;
												}
											}
											if (r == 1 && m < i)
											{
												m++;
												if (map[k + m, l - 1 + n] == 0)
												{
													map[k + m, l - 1 + n] = 10;
												}
											}
											if (r == 1 && m > i)
											{
												m--;
												if (map[k + m, l - 1 + n] == 0)
												{
													map[k + m, l - 1 + n] = 10;
												}
											}
											if ((n == -j && m == i - 1) || (n == -j && m == i + 1) || (n == -j + 1 && m == i))
											{
												o = 1;
											}
										}
										while (o != 1);
									}
									if (map[k, l - 1] == 0 && map[k, l] != 10)
									{
										map[k, l] = 0;
									}
									if (map[k, l] == 0 && map[k, l + 1] == 1 && map[k + 1, l + 1] == 0 && map[k, l + 2] == 0 && map[k - 1, l + 1] == 0)
									{
										map[k, l + 1] = 0;
									}
								}
							}
						}
						if (map[k, l] == 3)
						{
							for (int i = 2; i <= xSize / 2; i++)
							{
								for (int j = -i; j <= i; j++)
								{
									if (map[k + 1, l] == 0 && k + i < xSize && l + j > -1 && l + j < ySize && (map[k + i, l + j] == 2 || map[k + i, l + j] == 4 || map[k + i, l + j] == 5))
									{
										map[k, l] = 10;
										map[k + i, l + j] = 10;
										map[k + 1, l] = 10;
										int m = 0;
										int n = 0;
										int o = 0;
										do
										{
											int r = Random.Range(0, 2);
											if (r == 0 && m + 1 < i)
											{
												m++;
												if (map[k + 1 + m, l + n] == 0)
												{
													map[k + 1 + m, l + n] = 10;
												}
											}
											if (r == 1 && n < j)
											{
												n++;
												if (map[k + 1 + m, l + n] == 0)
												{
													map[k + 1 + m, l + n] = 10;
												}
											}
											if (r == 1 && n > j)
											{
												n--;
												if (map[k + 1 + m, l + n] == 0)
												{
													map[k + 1 + m, l + n] = 10;
												}
											}
											if ((m == i && n == j - 1) || (m == i && n == j + 1) || (m == i - 1 && n == j))
											{
												o = 1;
											}
										}
										while (o != 1);
									}
									if (map[k + 1, l] == 0 && map[k, l] != 10)
									{
										map[k, l] = 0;
									}
									if (map[k, l] == 0 && map[k - 1, l] == 1 && map[k - 1, l + 1] == 0 && map[k - 2, l] == 0 && map[k - 1, l - 1] == 0)
									{
										map[k - 1, l] = 0;
									}
								}
							}
						}
						if (map[k, l] == 4)
						{
							for (int j = 2; j <= ySize / 2; j++)
							{
								for (int i = -j; i <= j; i++)
								{
									if (map[k, l + 1] == 0 && l + j < ySize && k + i > -1 && k + i < xSize && (map[k + i, l + j] == 2 || map[k + i, l + j] == 3 || map[k + i, l + j] == 5))
									{
										map[k, l] = 10;
										map[k + i, l + j] = 10;
										map[k, l + 1] = 10;
										int m = 0;
										int n = 0;
										int o = 0;
										do
										{
											int r = Random.Range(0, 2);
											if (r == 0 && n + 1 < j)
											{
												n++;
												if (map[k + m, l + 1 + n] == 0)
												{
													map[k + m, l + 1 + n] = 10;
												}
											}
											if (r == 1 && m < i)
											{
												m++;
												if (map[k + m, l + 1 + n] == 0)
												{
													map[k + m, l + 1 + n] = 10;
												}
											}
											if (r == 1 && m > i)
											{
												m--;
												if (map[k + m, l + 1 + n] == 0)
												{
													map[k + m, l + 1 + n] = 10;
												}
											}
											if ((n == j && m == i - 1) || (n == j && m == i + 1) || (n == j - 1 && m == i))
											{
												o = 1;
											}
										}
										while (o != 1);
									}
									if ((map[k, l + 1] == 0 && map[k, l] != 10))
									{
										map[k, l] = 0;
									}
									if (map[k, l] == 0 && map[k, l - 1] == 1 && map[k + 1, l - 1] == 0 && map[k, l - 2] == 0 && map[k - 1, l - 1] == 0)
									{
										map[k, l - 1] = 0;
									}
								}
							}
						}
						if (map[k, l] == 5)
						{
							for (int i = 2; i <= xSize / 2; i++)
							{
								for (int j = -i; j <= i; j++)
								{
									if (map[k - 1, l] == 0 && k - i > -1 && l + j > -1 && l + j < ySize && (map[k - i, l + j] == 2 || map[k - i, l + j] == 3 || map[k - i, l + j] == 4))
									{
										map[k, l] = 10;
										map[k - i, l + j] = 10;
										map[k - 1, l] = 10;
										int m = 0;
										int n = 0;
										int o = 0;
										do
										{
											int r = Random.Range(0, 2);
											if (r == 0 && m - 1 > -i)
											{
												m--;
												if (map[k - 1 + m, l + n] == 0)
												{
													map[k - 1 + m, l + n] = 10;
												}
											}
											if (r == 1 && n < j)
											{
												n++;
												if (map[k - 1 + m, l + n] == 0)
												{
													map[k - 1 + m, l + n] = 10;
												}
											}
											if (r == 1 && n > j)
											{
												n--;
												if (map[k - 1 + m, l + n] == 0)
												{
													map[k - 1 + m, l + n] = 10;
												}
											}
											if ((m == -i && n == j - 1) || (m == -i && n == j + 1) || (m == -i + 1 && n == j))
											{
												o = 1;
											}
										}
										while (o != 1);
									}
									if (map[k - 1, l] == 0 && map[k, l] != 10)
									{
										map[k, l] = 0;
									}
									if (map[k, l] == 0 && map[k + 1, l] == 1 && map[k + 1, l + 1] == 0 && map[k + 2, l] == 0 && map[k + 1, l - 1] == 0)
									{
										map[k + 1, l] = 0;
									}
								}
							}
						}
					}
				}
				mapGenerated = false;
				for (int i = 1; i <= xSize; i++)
				{
					for (int j = 1; j <= ySize; j++)
					{
						if (!mapGenerated && map[i, j] != 0)
						{
							mapGenerated = true;
							break;
						}
					}
					if (mapGenerated)
					{
						break;
					}
				}
			}
			while (!mapGenerated);
			// 
			
			if (map[middleX, middleY] == 0)
			{
				do
				{
					middleX = Random.Range(1, xSize + 1);
					middleY = Random.Range(1, ySize + 1);
				}
				while (map[middleX, middleY] == 0);
			}
			map[middleX, middleY] = 100;
			if (map[middleX, middleY - 1] != 0)
			{
				map[middleX, middleY - 1] = 101;
			}
			if (map[middleX + 1, middleY] != 0)
			{
				map[middleX + 1, middleY] = 101;
			}
			if (map[middleX, middleY + 1] != 0)
			{
				map[middleX, middleY + 1] = 101;
			}
			if (map[middleX - 1, middleY] != 0)
			{
				map[middleX - 1, middleY] = 101;
			}
			int s = 101;
			int t;
			do
			{
				s++;
				t = 1;
				for (int i = 1; i <= xSize; i++)
				{
					for (int j = 1; j <= ySize; j++)
					{
						if (map[i, j] == s - 1)
						{
							t = 0;
							if (map[i, j - 1] > 0 && map[i, j - 1] < 100)
							{
								map[i, j - 1] = s;
							}
							if (map[i + 1, j] > 0 && map[i + 1, j] < 100)
							{
								map[i + 1, j] = s;
							}
							if (map[i, j + 1] > 0 && map[i, j + 1] < 100)
							{
								map[i, j + 1] = s;
							}
							if (map[i - 1, j] > 0 && map[i - 1, j] < 100)
							{
								map[i - 1, j] = s;
							}
						}
					}
				}
			}
			while (t != 1);
			
			//
			for (int i = 1; i <= xSize; i++)
			{
				for (int j = 1; j <= ySize; j++)
				{
					if (map[i, j] > 0 && map[i, j] < 100)
					{
						map[i, j] = 0;
					}
					if (map[i, j] != 0)
					{
						floorCount++;
					}
				}
			}
			// 

			if (showDebugMessages)
			{
				Debug.Log("   Se creo el mapa " );
			}
			//

			GenerateObjects();
		}
	}

	// 
	public void GenerateObjects()
	{
		RefreshValues();

		//
		if (visibleWallObject == null && invisibleWallObject == null && floorObject == null)
		{
			Debug.LogWarning(transform.name + ": 'Visible Wall Object', 'Invisible Wall Object' and 'Floor Object' are null! No objects were created! Attach object(s) you want to create first.");
		}
		else
		{
			objectsCreated = 0;
			visibleWalls = 0;
			invisibleWalls = 0;
			floors = 0;
			
			// <Separate Visible and Invisible Walls>
			if (visibleWallObject != null || invisibleWallObject != null)
			{
				if ((visibleWallObject != invisibleWallObject) || (visibleWallObject == invisibleWallObject && mergeVisibleWalls != mergeInvisibleWalls))
				{
					for (int i = 1; i <= xSize; i++)
					{
						for (int j = 1; j <= ySize; j++)
						{
							if (map[i, j] > 0)
							{
								if (map[i - 1, j] == 0)
								{
									map[i - 1, j] = -1;
								}
								if (map[i + 1, j] == 0)
								{
									map[i + 1, j] = -1;
								}
								if (map[i, j - 1] == 0)
								{
									map[i, j - 1] = -1;
								}
								if (map[i, j + 1] == 0)
								{
									map[i, j + 1] = -1;
								}
								if (visibleWallsAtDiagonals)
								{
									if (map[i - 1, j - 1] == 0)
									{
										map[i - 1, j - 1] = -1;
									}
									if (map[i - 1, j + 1] == 0)
									{
										map[i - 1, j + 1] = -1;
									}
									if (map[i + 1, j - 1] == 0)
									{
										map[i + 1, j - 1] = -1;
									}
									if (map[i + 1, j + 1] == 0)
									{
										map[i + 1, j + 1] = -1;
									}
								}
							}
						}
					}
				}
			}
			// </Separate Visible and Invisible Walls>
			
			// <Set all floors to ID 1>
			if (floorObject != null)
			{
				for (int i = 1; i <= xSize; i++)
				{
					for (int j = 1; j <= ySize; j++)
					{
						if (map[i, j] > 0)
						{
							map[i, j] = 1;
						}
					}
				}
			}
			// </Set all floors to ID 1>
			
			for (int i = 0; i <= xSize + 1; i++)
			{
				for (int j = 0; j <= ySize + 1; j++)
				{
					if (map[i, j] >= -1)
					{
						// <Create Visible Walls>
						if (map[i, j] == -1 && visibleWallObject != null)
						{
							CreateTerrainObject(visibleWallObject, -1, i, j, mergeVisibleWalls);
							if (mergeVisibleWalls)
							{
								j += height - 1;
							}
						}
						// </Create Visible Walls>
						
						// <Create Invisible Walls>
						if (map[i, j] == 0 && invisibleWallObject != null)
						{
							CreateTerrainObject(invisibleWallObject, 0, i, j, mergeInvisibleWalls);
							if (mergeInvisibleWalls)
							{
								j += height - 1;
							}
						}
						// </Create Invisible Walls>
						
						// <Create Floors>
						if (map[i, j] == 1 && floorObject != null)
						{
							CreateTerrainObject(floorObject, 1, i, j, mergeFloors);
							if (mergeFloors)
							{
								j += height - 1;
							}
						}
						// </Create Floors>
					}
				}
			}
			if (showDebugMessages)
			{
				Debug.Log("   " + objectsCreated + " terrain objects created (" + visibleWalls + " visible walls, " + invisibleWalls + " invisible walls, " + floors + " floors).");
			}
		}
		// </Create Walls and Floors>
		
//		SpawnObjects();
	}

	void CreateTerrainObject(GameObject obj, int objectID, int i, int j, bool merge)
	{
		height = 1;
		width = 1;
		GameObject createdObject;
		createdObject = (GameObject)Instantiate(obj);
		createdObject.transform.parent = transform;
		createdObject.name = prefabNamePrefix + obj.name;
		createdObject.transform.localPosition = new Vector3((i - (xSize + 1) * 0.5f) / transform.lossyScale.x, obj.transform.position.y, (j - (ySize + 1) * 0.5f) / transform.lossyScale.z);
		objectsCreated++;
		if (objectID == -1)
		{
			visibleWalls++;
		}
		if (objectID == 0)
		{
			invisibleWalls++;
		}
		if (objectID == 1)
		{
			floors++;
		}
		// <Merge Objects>
		if (merge)
		{
			GetMergeSize(i, j, objectID);
		}
		if (height > 1 || width > 1)
		{
			createdObject.transform.localPosition += new Vector3((width - 1) * 0.5f / transform.lossyScale.x, 0, (height - 1) * 0.5f / transform.lossyScale.z);
			createdObject.transform.localScale = new Vector3(createdObject.transform.localScale.x * width, createdObject.transform.localScale.y, createdObject.transform.localScale.z * height);

			/*
			// <Fix tiling>
			if (fixTiling)
			{
				createdObject.renderer.material.mainTextureScale = new Vector2(createdObject.renderer.material.mainTextureScale.x * width, createdObject.renderer.material.mainTextureScale.y * height);
//?? Cannot be fixed correctly (only for rotated objects?)
			}
			// </Fix tiling>
			*/
		}
		// </Merge Objects>
	}

	void GetMergeSize(int i, int j, int objectID)
	{
		for (int k = j; k <= ySize + 1; k++)
		{
			if (map[i, k] == objectID && k > j)
			{
				height++;
			}
			if (map[i, k] != objectID || k == ySize + 1)
			{
				if (i <= xSize)
				{
					for (int l = i + 1; l <= xSize + 1; l++)
					{
						bool blocked = false;
						for (int jj = j; jj < j + height; jj++)
						{
							if (map[l, jj] != objectID)
							{
								blocked = true;
								break;
							}
						}
						if (!blocked)
						{
							width++;
							for (int jj = j; jj < j + height; jj++)
							{
								if (map[l, jj] > 0)
								{
									map[l, jj] = 2;
								}
								else
								{
									map[l, jj] = -2;
								}
							}
						}
						else
						{
							break;
						}
					}
				}
				break;
			}
		}
	}
	

	// Create one object of concrete scale at specific spot (center of the field).
	// Object will be stacked if another object is already there.
	public void CreateObject(GameObject obj, Field field, Vector3 scale)
	{
		if (obj != null)
		{
			GameObject createdObject;
			createdObject = (GameObject)Instantiate(obj);
			createdObject.transform.localScale = scale;
			createdObject.transform.parent = transform;
			createdObject.name = prefabNamePrefix + obj.name;
			createdObject.transform.localPosition = new Vector3((field.x - (xSize + 1) * 0.5f) / transform.lossyScale.x, obj.transform.position.y, (field.y - (ySize + 1) * 0.5f) / transform.lossyScale.z);
			if (obj != pathObject)
			{
				spawnedObjects.Add(createdObject);
			}
		}
	}

	// Create one object of concrete scale multiple at specific spot (center of the field).
	// Object will be stacked if another object is already there.
	public void CreateObject(GameObject obj, Field field, float scaleMultiple)
	{
		CreateObject(obj, field, obj.transform.lossyScale * scaleMultiple);
	}

	// Create one object at specific spot (center of the field).
	// Object will be stacked if another object is already there.
	public void CreateObject(GameObject obj, Field field)
	{
		CreateObject(obj, field, obj.transform.lossyScale);
	}

	GameObject GetNextObject(List<GameObject> objectList, int currentIndex, bool nextObject)
	{
		GameObject nextObj;
		if (!nextObject)
		{
			nextObj = objectList[Random.Range(0, objectList.Count)];
		}
		else
		{
			if (currentIndex == objectList.Count - 1)
			{
				nextObj = objectList[0];
			}
			else
			{
				nextObj = objectList[currentIndex + 1];
			}
		}
		return nextObj;
	}

	List<Field> GetFreeFields()
	{
		List<Field> freeFields = new List<Field>();

		RefreshValues();
		if (spawnedObjects.Count > 0)
		{
			foreach (GameObject obj in spawnedObjects)
			{
				Field currentField = GetField(obj);
				if (map[currentField.x, currentField.y] == 1)
				{
					map[currentField.x, currentField.y] = countFrom;
				}
			}
		}
		for (int i = 1; i <= xSize; i++)
		{
			for (int j = 1; j <= ySize; j++)
			{
				if (map[i, j] == 1)
				{
					freeFields.Add(new Field() {x = i, y = j});
				}
			}
		}
		return freeFields;
	}

	// Return random floor field.
	public Field RandomField()
	{
		Field field;
		do
		{
			field.x = Random.Range(1, xSize + 1);
			field.y = Random.Range(1, ySize + 1);
		}
		while (map[field.x, field.y] <= 0);
		return field;
	}

	public void DeleteMaze(bool showName)
	{
		int objectsRemoved = 0;
		spawnedObjects.Clear();

		foreach (Transform child in transform.GetComponentsInChildren<Transform>())
		{
			if (child != null && child.name.Length >= prefabNamePrefix.Length && child.name.Substring(0, prefabNamePrefix.Length) == prefabNamePrefix)
			{
				DestroyImmediate(child.gameObject);
				objectsRemoved++;
			}
		}
		if (showDebugMessages)
		{
			if (showName)
			{
				Debug.Log(transform.name + ": " + objectsRemoved + " objects removed.");
			}
			else
			{
				Debug.Log("   " + objectsRemoved + " objects removed.");
			}
		}
	}

	// Distance (in fields) between the two fields.
	public int Distance(Field fieldA, Field fieldB)
	{
		if (map == null)
		{
			Debug.LogWarning(transform.name + ": Cannot calculate distance (map has not been generated yet)!");
			return -1;
		}
		if (fieldA.x < 1 || fieldA.x > xSize || fieldA.y < 1 || fieldA.y > ySize)
		{
			Debug.LogWarning(transform.name + ": Cannot calculate distance (point A is out of the map)!");
			return -1;
		}
		if (fieldB.x < 1 || fieldB.x > xSize || fieldB.y < 1 || fieldB.y > ySize)
		{
			Debug.LogWarning(transform.name + ": Cannot calculate distance (point B is out of the map)!");
			return -1;
		}
		if (map[fieldA.x, fieldA.y] <= 0)
		{
			Debug.LogWarning(transform.name + ": Cannot calculate distance (point A is inside the wall)!");
			return -1;
		}
		if (map[fieldB.x, fieldB.y] <= 0)
		{
			Debug.LogWarning(transform.name + ": Cannot calculate distance (point B is inside the wall)!");
			return -1;
		}
		if (fieldA.x == fieldB.x && fieldA.y == fieldB.y)
		{
			return 0;
		}
		RefreshValues();
		List<Field> currentFields = new List<Field>();
		List<Field> nextFields = new List<Field>();
		int m = countFrom;
		bool end = false;
		int distance;
		map[fieldA.x, fieldA.y] = m;
		currentFields.Add(new Field() {x = fieldA.x, y = fieldA.y});
		do
		{
			m++;
			nextFields.Clear();
			foreach (Field field in currentFields)
			{
				if (map[field.x, field.y - 1] == 1)
				{
					map[field.x, field.y - 1] = m;
					if (field.x == fieldB.x && field.y - 1 == fieldB.y)
					{
						end = true;
					}
					else
					{
						nextFields.Add(new Field() {x = field.x, y = field.y - 1});
					}
				}
				if (map[field.x, field.y + 1] == 1)
				{
					map[field.x, field.y + 1] = m;
					if (field.x == fieldB.x && field.y + 1 == fieldB.y)
					{
						end = true;
					}
					else
					{
						nextFields.Add(new Field() {x = field.x, y = field.y + 1});
					}
				}
				if (map[field.x - 1, field.y] == 1)
				{
					map[field.x - 1, field.y] = m;
					if (field.x - 1 == fieldB.x && field.y == fieldB.y)
					{
						end = true;
					}
					else
					{
						nextFields.Add(new Field() {x = field.x - 1, y = field.y});
					}
				}
				if (map[field.x + 1, field.y] == 1)
				{
					map[field.x + 1, field.y] = m;
					if (field.x + 1 == fieldB.x && field.y == fieldB.y)
					{
						end = true;
					}
					else
					{
						nextFields.Add(new Field() {x = field.x + 1, y = field.y});
					}
				}
			}
			currentFields = new List<Field>(nextFields);
		}
		while (!end);
		distance = m - map[fieldA.x, fieldA.y];
		return distance;
	}

	// Distance (in fields) between the GameObject and the field.
	public int Distance(GameObject obj, Field field)
	{
		return Distance(GetField(obj), field);
	}

	// Distance (in fields) between the two GameObjects.
	public int Distance(GameObject objA, GameObject objB)
	{
		return Distance(GetField(objA), GetField(objB));
	}

	// Maximum possible distance (in fields) you can go from the field (-1 returned for invalid entry).
	public int MaxDistance(Field fieldFrom)
	{
		if (map == null)
		{
			Debug.LogWarning(transform.name + ": Cannot calculate distance (map has not been generated yet)!");
			return -1;
		}
		if (fieldFrom.x < 1 || fieldFrom.x > xSize || fieldFrom.y < 1 || fieldFrom.y > ySize)
		{
			Debug.LogWarning(transform.name + ": Cannot calculate distance (point is out of the map)!");
			return -1;
		}
		if (map[fieldFrom.x, fieldFrom.y] <= 0)
		{
			Debug.LogWarning(transform.name + ": Cannot calculate distance (point is inside the wall)!");
			return -1;
		}
		RefreshValues();
		List<Field> currentFields = new List<Field>();
		List<Field> nextFields = new List<Field>();
		int m = countFrom;
		int distance;
		map[fieldFrom.x, fieldFrom.y] = m;
		currentFields.Add(new Field() {x = fieldFrom.x, y = fieldFrom.y});
		do
		{
			m++;
			nextFields.Clear();
			foreach (Field field in currentFields)
			{
				if (map[field.x, field.y - 1] == 1)
				{
					map[field.x, field.y - 1] = m;
					nextFields.Add(new Field() {x = field.x, y = field.y - 1});
				}
				if (map[field.x, field.y + 1] == 1)
				{
					map[field.x, field.y + 1] = m;
					nextFields.Add(new Field() {x = field.x, y = field.y + 1});
				}
				if (map[field.x - 1, field.y] == 1)
				{
					map[field.x - 1, field.y] = m;
					nextFields.Add(new Field() {x = field.x - 1, y = field.y});
				}
				if (map[field.x + 1, field.y] == 1)
				{
					map[field.x + 1, field.y] = m;
					nextFields.Add(new Field() {x = field.x + 1, y = field.y});
				}
			}
			currentFields = new List<Field>(nextFields);
		}
		while (nextFields.Count > 0);
		distance = m - map[fieldFrom.x, fieldFrom.y] - 1;
		return distance;
	}

	// Maximum possible distance (in fields) you can go from the GameObject (-1 returned for invalid entry).
	public int MaxDistance(GameObject objFrom)
	{
		return MaxDistance(GetField(objFrom));
	}

	// Crea el camino para poder conseguir el camino mas corto
	public List<Field> CreatePath(Field fieldA, Field fieldB, bool simplify, bool generatePath)
	{
		List<Field> path = new List<Field>();
		List<Field> possibleDirections = new List<Field>();
		Field currentField = fieldB;
		bool skipField = true;

		if (Distance(fieldA, fieldB) <= 0)
		{
			Debug.LogWarning(string.Format("{0}: Cannot create path between [{1}, {2}] and [{3}, {4}].", transform.name, fieldA.x, fieldA.y, fieldB.x, fieldB.y));
			return null;
		}
		else
		{
			int currentValue = map[fieldB.x, fieldB.y] - 1;
			path.Add(currentField);
			while (currentValue > map[fieldA.x, fieldA.y])
			{
				possibleDirections.Clear();
				if (map[currentField.x, currentField.y - 1] == currentValue)
				{
					possibleDirections.Add(new Field() {x = currentField.x, y = currentField.y - 1});
				}
				if (map[currentField.x, currentField.y + 1] == currentValue)
				{
					possibleDirections.Add(new Field() {x = currentField.x, y = currentField.y + 1});
				}
				if (map[currentField.x - 1, currentField.y] == currentValue)
				{
					possibleDirections.Add(new Field() {x = currentField.x - 1, y = currentField.y});
				}
				if (map[currentField.x + 1, currentField.y] == currentValue)
				{
					possibleDirections.Add(new Field() {x = currentField.x + 1, y = currentField.y});
				}
				currentField = possibleDirections[Random.Range(0, possibleDirections.Count)];
				if (simplify)
				{
					if (skipField)
					{
						skipField = false;
					}
					else
					{
						path.Add(currentField);
						skipField = true;
					}
				}
				else
				{
					path.Add(currentField);
				}
				currentValue--;
			}
		}
		path.Reverse();
		if (generatePath)
		{
			if (pathObject == null)
			{
				Debug.LogWarning(transform.name + ": Cannot create path objects ('generatePath' is true but 'Path Object' is null)!");
			}
			else
			{
				foreach (Field field in path)
				{
					CreateObject(pathObject, field);
				}
			}
		}
		return path;
	}

	// Generate random but minimum path between the field and the GameObject. The path (List of fields) is returned.
	// If "simplify" is true: the path contains only each second field (enough for movement and in most cases better - for diagonal movement).
	// If "generatePath" is true: the path will be filled with the "pathObject".
	public List<Field> CreatePath(Field field, GameObject obj, bool simplify, bool generatePath)
	{
		return CreatePath(field, GetField(obj), simplify, generatePath);
	}

	// Generate random but minimum path between the GameObject and the field. The path (List of fields) is returned.
	// If "simplify" is true: the path contains only each second field (enough for movement and in most cases better - for diagonal movement).
	// If "generatePath" is true: the path will be filled with the "pathObject".
	public List<Field> CreatePath(GameObject obj, Field field, bool simplify, bool generatePath)
	{
		return CreatePath(GetField(obj), field, simplify, generatePath);
	}

	// Generate random but minimum path between the two GameObjects. The path (List of fields) is returned.
	// If "simplify" is true: the path contains only each second field (enough for movement and in most cases better - for diagonal movement).
	// If "generatePath" is true: the path will be filled with the "pathObject".
	public List<Field> CreatePath(GameObject objA, GameObject objB, bool simplify, bool generatePath)
	{
		return CreatePath(GetField(objA), GetField(objB), simplify, generatePath);
	}

	// Delete the old path (generated by "CreatePath" function).
	public void DeletePath()
	{
		if (pathObject != null)
		{
			foreach (Transform child in transform.GetComponentsInChildren<Transform>())
			{
				if (child != null && child.name == prefabNamePrefix + pathObject.name)
				{
					DestroyImmediate(child.gameObject);
				}
			}
		}
	}

	// Convert GameObject current position to the field.
	public Field GetField(GameObject obj)
	{
		Field field;
		Vector3 objCoords = transform.InverseTransformPoint(obj.transform.position);
		field.x = Mathf.RoundToInt(objCoords.x * transform.lossyScale.x + (xSize + 1) * 0.5f);
		field.y = Mathf.RoundToInt(objCoords.z * transform.lossyScale.z + (ySize + 1) * 0.5f);
		if (field.x < 0 || field.x > xSize + 1 || field.y < 0 || field.y > ySize + 1)
		{
			Debug.LogWarning(string.Format("{0}: Object is out of the map (field [{1}, {2}])! Returning field [0, 0].", obj.name, field.x, field.y));
			field.x = 0;
			field.y = 0;
		}
		return field;
	}

	// Get local position of the center of the field.
	public Vector3 GetFieldLocalPos(Field field)
	{
		Vector3 localPosition;
		localPosition = new Vector3((field.x - (xSize + 1) * 0.5f) / transform.lossyScale.x, 0, (field.y - (ySize + 1) * 0.5f) / transform.lossyScale.z);
		return localPosition;
	}

	void RefreshValues()
	{
		for (int i = 0; i <= xSize + 1; i++)
		{
			for (int j = 0; j <= ySize + 1; j++)
			{
				if (map[i, j] > 0)
				{
					map[i, j] = 1;
				}
				else
				{
					map[i, j] = 0;
				}
			}
		}
	}
}
