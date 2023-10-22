using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;

public class WFC : MonoBehaviour
{
    public int dimensions;
    public Tile[] tileObjs;
    public List<Cell> grid;
    public Cell cellObj;
    public Tile[] props;

    int iterations = 0;

    void Awake()
    {
        grid = new List<Cell>();
        InitializeGrid();
    }

    void InitializeGrid()
    {
        for (int y = 0; y< dimensions; y++)
        {
            for (int x = 0; x< dimensions; x++)
            {
                Cell newCell = Instantiate(cellObj, new Vector2(x, y), Quaternion.identity);
                newCell.CreateCell(false, tileObjs);
                grid.Add(newCell);
            }
        }

        StartCoroutine(CheckEntropy());
    }

    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>(grid);
        //cells that arent collapsed
        tempGrid.RemoveAll(x => x.isCollapsed);
        //sorting to get cell with least options
        tempGrid.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; });
        tempGrid.RemoveAll(x => x.tileOptions.Length != tempGrid[0].tileOptions.Length);

        /*int optionCount = tempGrid[0].tileOptions.Length;
        int stopIndex = default;

        for(int i = 1; i < tempGrid.Count; i++)
        {
            if(tempGrid[i].tileOptions.Length > optionCount)
            {
                stopIndex = i;
                break;
            }
        }

        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }
*/
        yield return new WaitForSeconds(0.001f);
        CollapsedCell(tempGrid);
    }

    void CollapsedCell(List<Cell> tempGrid)
    {
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);

        Cell cellToCollapse = tempGrid[randIndex];

        cellToCollapse.isCollapsed = true;
        Tile tileToPlace = tileObjs[0];
        if (cellToCollapse.tileOptions.Length != 0)
        {
            tileToPlace = cellToCollapse.tileOptions[UnityEngine.Random.Range(0, cellToCollapse.tileOptions.Length)];
        }
        else
        {
            Debug.Log("No option Error at " + iterations);
        }
            
        cellToCollapse.tileOptions = new Tile[] { tileToPlace };

        Tile foundTile = cellToCollapse.tileOptions[0];
        Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity);

        if (foundTile.itemIsPlacable && UnityEngine.Random.Range(0,100) < 15)
        {
            Instantiate(props[UnityEngine.Random.Range(0, 2)], cellToCollapse.transform.position, Quaternion.identity);
        }

        UpdateMap();
    }
    
    void UpdateMap()
    {
        List<Cell> newGenerationCell = new List<Cell>(grid);

        for (int y = 0; y < dimensions; y++)
        {
            for(int x = 0; x< dimensions; x++)
            {
                int index = x + y * dimensions;
                //no change if cell is collapsed
                if(grid[index].isCollapsed)
                {
                    Debug.Log("called");
                    newGenerationCell[index] = grid[index];
                }
                else
                {
                    List<Tile> options = new List<Tile>();
                    foreach(Tile t in tileObjs)
                    {
                        options.Add(t);
                    }

                    //look above
                    if (y > 0)
                    {
                        Cell up = grid[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach(Tile t in up.tileOptions)
                        {
                            int validOption = Array.FindIndex(tileObjs, obj => obj == t);
                            Tile[] valid = tileObjs[validOption].upNeighbor;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidOptions(options, validOptions);
                    }

                    //look right
                    if (x < dimensions - 1)
                    {
                        Cell right = grid[x + 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile t in right.tileOptions)
                        {
                            int validOption = Array.FindIndex(tileObjs, obj => obj == t);
                            Tile[] valid = tileObjs[validOption].leftNeighbor;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidOptions(options, validOptions);
                    }

                    //look down
                    if (y < dimensions - 1)
                    {
                        Cell down = grid[x + (y + 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile t in down.tileOptions)
                        {
                            int validOption = Array.FindIndex(tileObjs, obj => obj == t);
                            Tile[] valid = tileObjs[validOption].downNeighbor;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidOptions(options, validOptions);
                    }

                    //look left
                    if (x > 0)
                    {
                        Cell left = grid[x - 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile t in left.tileOptions)
                        {
                            int validOption = Array.FindIndex(tileObjs, obj => obj == t);
                            Tile[] valid = tileObjs[validOption].rightNeighbor;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidOptions(options, validOptions);
                    }

                    Tile[] newTileList = new Tile[options.Count];

                    for (int i = 0; i<options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        grid = newGenerationCell;
        iterations++;

        if(iterations < dimensions * dimensions)
        {
            StartCoroutine(CheckEntropy());
        }
    }

    //check if tile can be placed at that position
    //remove if it cannot be placed in that position
    void CheckValidOptions(List<Tile> optionList, List<Tile> validOption)
    {
        for (int i = optionList.Count-1; i >= 0 ; i--)
        {
            Tile option = optionList[i];
            if(!validOption.Contains(option))
            {
                optionList.RemoveAt(i);
            }    
        }
    }
}
