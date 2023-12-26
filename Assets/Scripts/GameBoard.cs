using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameBoard : MonoBehaviour
{
    // [SerializeField] makes this field show up on the editor
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;
    [SerializeField] private Tile aliveTile;
    [SerializeField] private Tile deadTile;
    [SerializeField] private Pattern pattern;
    [SerializeField] private float updateInterval = 0.05f;
    

    // HashSet only checks cells that are alive and the cells you need to check instead of all the total cells. Saving memory and making this not run slow.
    private HashSet<Vector3Int> aliveCells;
    private HashSet<Vector3Int> cellsToCheck;
    

    public int population { get; private set; }
    public int iterations { get; private set; }
    public float time { get; private set; }

    private void Awake()
    {
        aliveCells = new HashSet<Vector3Int>();
        cellsToCheck = new HashSet<Vector3Int>();
    }

    private void Start()
    {
        SetPattern(pattern);
    }

    private void SetPattern(Pattern pattern)
    {
        Clear();

        Vector2Int center = pattern.GetCenter();

        for(int i = 0; i < pattern.cells.Length; i++)
        {
            Vector3Int cell = (Vector3Int)(pattern.cells[i] - center);
            currentState.SetTile(cell, aliveTile);
            aliveCells.Add(cell);
        }
        population = aliveCells.Count;

    }

    private void Clear()
    {
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
        
        population = 0;
        iterations = 0;
        time = 0f;
        

    }

    private void OnEnable()
    {
        // Coroutine suspend execution of the function for x amount of seconds. Need to yield
        StartCoroutine(Simulate());
    }

    private IEnumerator Simulate()
    {
        var interval = new WaitForSeconds(updateInterval);
        yield return interval;

        while (enabled)
        {
            UpdateState();

        
            population = aliveCells.Count;
            iterations++;
            time += updateInterval;
        
            yield return interval;
        }
    }

    private void UpdateState()
    {
        
        cellsToCheck.Clear();

        foreach (Vector3Int cell in aliveCells)
        {
            /*
                Manually unroll the loop. Obviously more effort.
                                o is your cell

                                    xxx
                                    xox
                                    xxx

                cellsToCheck.Add(cell);
                cellsToCheck.Add(cell + new Vector3Int(-1, 1)); left / up
                cellsToCheck.Add(cell + new Vector3Int(-1, 0)); left
                cellsToCheck.Add(cell + new Vector3Int(-1, -1)); left / down
                cellsToCheck.Add(cell + new Vector3Int(0, -1)); down
                cellsToCheck.Add(cell + new Vector3Int(0, 1)); up
                cellsToCheck.Add(cell + new Vector3Int(1, 1)); right / up
                cellsToCheck.Add(cell + new Vector3Int(1, 0)); right
                cellsToCheck.Add(cell + new Vector3Int(1, -1)); right / down
            */
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    cellsToCheck.Add(cell + new Vector3Int(x, y, 0));
                }
            }
        }
        // transitioning cells to the next state
        foreach(Vector3Int cell in cellsToCheck)
        {
            int neighbors = CountNeighbors(cell);
            bool alive = IsAlive(cell);

            if(!alive && neighbors == 3)
            {
                // becomes alive
                nextState.SetTile(cell, aliveTile);
                aliveCells.Add(cell);

            }
            else if(alive && (neighbors < 2 || neighbors > 3))
            {
                // becomes dead
                nextState.SetTile(cell, deadTile);
                aliveCells.Remove(cell);

            }
            else
            {
                // stays the same. no change to aliveCells
                nextState.SetTile(cell, currentState.GetTile(cell));
                
            }

        }
        Tilemap temp = currentState;
        currentState = nextState;
        nextState = temp;
        nextState.ClearAllTiles();
        
    }

        private int CountNeighbors(Vector3Int cell)
        {

            // same as UpdateState();
            int count = 0;
            for(int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector3Int neighbor = cell + new Vector3Int(x, y, 0);
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }
                    else if(IsAlive(neighbor))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    

    private bool IsAlive(Vector3Int cell)
    {
        return currentState.GetTile(cell) == aliveTile;
    }

}
