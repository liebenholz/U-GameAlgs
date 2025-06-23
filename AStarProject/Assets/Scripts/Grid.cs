using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

	public Transform player;
	public LayerMask unwalkableMask;
	public Vector2 numberOfGrids;
	public float nodeSize;
	Node[,] grid;

	float nodeHalfSize;
	int gridSizeX, gridSizeY;

	void Awake()
	{
		nodeHalfSize = nodeSize * 0.5f;
		gridSizeX = Mathf.RoundToInt (numberOfGrids.x / nodeSize);
		gridSizeY = Mathf.RoundToInt (numberOfGrids.y / nodeSize);

		CreateGrid ();	
	}

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 bottomLeft = transform.position - Vector3.right * numberOfGrids.x / 2 - Vector3.forward * numberOfGrids.y / 2;

		for (int x = 0; x < gridSizeX; x++) 
		{
			for (int y = 0; y < gridSizeY; y++) 
			{
				Vector3 nodePosition = bottomLeft + Vector3.right * (x * nodeSize + nodeHalfSize) + Vector3.forward * (y * nodeSize + nodeHalfSize);
				bool walkable = !(Physics.CheckSphere (nodePosition, nodeHalfSize, unwalkableMask));
				grid [x, y] = new Node (walkable, nodePosition, x , y);
			}
		}
	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node> ();
		for (int x = -1; x <= 1; ++x) 
		{
			for (int y = -1; y <= 1; ++y) 
			{
				if (x == 0 && y == 0) 
				{
					continue;
				}

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) 
				{
					neighbours.Add(grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}
		
	public Node GetNodeFromPosition(Vector3 position) {
		float percentX = (position.x + numberOfGrids.x / 2) / numberOfGrids.x;
		float percentY = (position.z + numberOfGrids.y / 2) / numberOfGrids.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt ((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt ((gridSizeY - 1) * percentY);
		return grid [x, y];
	}

	public List<Node> path;

	void OnDrawGizmos()
	{
		Gizmos.DrawWireCube (transform.position, new Vector3 (numberOfGrids.x, 1, numberOfGrids.y));

		if (grid != null) {

            Node playernode = GetNodeFromPosition(player.position);

            foreach (Node n in grid) 
			{
				Gizmos.color = (n.isWalkable) ? Color.white : Color.red;

                if (playernode == n)
                {
                    Gizmos.color = Color.black;
                }
                else
                {
                    if (path != null && path.Contains(n))
                        Gizmos.color = Color.black;
				}

                Gizmos.DrawCube (n.position, Vector3.one * (nodeSize - 0.1f));
			}
		}
	}

}
