using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;

    [SerializeField] private Tile aliveTile;
    [SerializeField] private Tile deadTile;

    [SerializeField] private float updateInterval = 0.05f;

    [SerializeField] private Pattern pattern;

    private HashSet<Vector3Int> aliveCells;
    private HashSet<Vector3Int> cellsToCheck;

    public int population;
    public int iterations;
    public float time;


    private void Awake()
    {
        aliveCells = new HashSet<Vector3Int>();
        cellsToCheck = new HashSet<Vector3Int>();
    }

    private void Start()
    {
        SetPattern();
    }

    private void OnEnable()
    {
        StartCoroutine(Simulate());
    }




    private void SetPattern()
    {
        Clear();

        Vector2Int center = pattern.GetCenter();

        for (int i = 0; i < pattern.cells.Length; i++) 
        {
            Vector3Int cell = (Vector3Int)(pattern.cells[i] - center);
            currentState.SetTile(cell, aliveTile);
            aliveCells.Add(cell);
        }
        population = aliveCells.Count;
    }

    private void UpdateState()
    {
        cellsToCheck.Clear();
        foreach(var cell in aliveCells)
        {
            for(int i = -1; i <= 1;  i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    cellsToCheck.Add(cell + new Vector3Int(i, j));
                }
            }
        }

        foreach(var cell in cellsToCheck)
        {
            int neighborCount = CountNeighbors(cell);
            bool isAlive = IsAlive(cell); 

            if(!isAlive && neighborCount == 3)
            {
                nextState.SetTile(cell, aliveTile);
                aliveCells.Add(cell);
            }
            else if(isAlive && (neighborCount < 2 || neighborCount > 3))
            {
                nextState.SetTile(cell, null);
                aliveCells.Remove(cell);
            }
            else
            {
                nextState.SetTile(cell, currentState.GetTile(cell));
            }
        }


        Tilemap old = currentState;
        currentState = nextState;
        nextState = old;
        nextState.ClearAllTiles();
    }

    private int CountNeighbors(Vector3Int cell)
    {
        int count = 0;
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;

                Vector3Int neighbor = cell + new Vector3Int(i, j);
                if(IsAlive(neighbor))
                {
                    count++;
                }
            }
        }
        return count;
    }

    private bool IsAlive(Vector3Int cell)
    {
        return currentState.HasTile(cell);
    }

    private IEnumerator Simulate()
    {
        var interval = new WaitForSeconds(updateInterval);
        yield return interval;

        while(enabled)
        {
            UpdateState();

            population = aliveCells.Count;
            iterations++;
            time += updateInterval;

            yield return interval;
        }
    }

    private void Clear()
    {
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();

        aliveCells.Clear();
        cellsToCheck.Clear();

        population = 0;
        iterations = 0;
        time = 0.0f;
    }
}

