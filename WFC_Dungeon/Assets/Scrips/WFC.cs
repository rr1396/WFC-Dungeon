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
    public int dimensionX;
    public int dimensionY;
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
        for (int y = 0; y< dimensionY; y++)
        {
            for (int x = 0; x< dimensionX; x++)
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

        for (int y = 0; y < dimensionY; y++)
        {
            for(int x = 0; x< dimensionX; x++)
            {
                int index = x + y * dimensionX;
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
                        Cell up = grid[x + (y - 1) * dimensionX];
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
                    if (x < dimensionX - 1)
                    {
                        Cell right = grid[x + 1 + y * dimensionX];
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
                    if (y < dimensionY - 1)
                    {
                        Cell down = grid[x + (y + 1) * dimensionX];
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
                        Cell left = grid[x - 1 + y * dimensionX];
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

        if(iterations < dimensionX * dimensionY)
        {
            StartCoroutine(CheckEntropy());
        }
        else
        {
            Debug.Log("Map Generated");
            GenerateEdges(grid);
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

    void GenerateEdges(List<Cell> grid)
    {
        List<Cell> upEdgeTiles = new List<Cell>();
        List<Cell> downEdgeTiles = new List<Cell>();
        List<Cell> leftEdgeTiles = new List<Cell>();
        List<Cell> rightEdgeTiles = new List<Cell>();

        //up and down Cells
        for(int i=0; i<dimensionX; i++)
        {
            Cell newCell = Instantiate(cellObj, new Vector2(i, -1), Quaternion.identity);
            Tile[] tempTiles = new Tile[] { grid[i].tileOptions[0].downEdge };
            newCell.CreateCell(true, tempTiles);
            downEdgeTiles.Add(newCell);

            Instantiate(downEdgeTiles[i].tileOptions[0], downEdgeTiles[i].transform.position, Quaternion.identity);


            Cell newUpCell = Instantiate(cellObj, new Vector2(i, dimensionY), Quaternion.identity);
            tempTiles = new Tile[] { grid[i + dimensionX * (dimensionY-1)].tileOptions[0].upEdge };
            newUpCell.CreateCell(true, tempTiles);
            upEdgeTiles.Add(newUpCell);

            Instantiate(upEdgeTiles[i].tileOptions[0], upEdgeTiles[i].transform.position, Quaternion.identity);
        }

        //left and right
        for(int i=0; i< dimensionY; i++)
        {
            Cell newCell = Instantiate(cellObj, new Vector2(-1, i), Quaternion.identity);
            Tile[] tempTiles = new Tile[] { grid[i * dimensionX].tileOptions[0].leftEdge };
            newCell.CreateCell(true, tempTiles);
            leftEdgeTiles.Add(newCell);

            Cell newRightCell = Instantiate(cellObj, new Vector2(dimensionX, i), Quaternion.identity);
            tempTiles = new Tile[] { grid[dimensionX * (i+1) - 1].tileOptions[0].rightEdge };
            newRightCell.CreateCell(true, tempTiles);
            rightEdgeTiles.Add(newRightCell);
        }

        //corners
        Cell BLCell = Instantiate(cellObj, new Vector2(-1, -1), Quaternion.identity);
        Tile[] BLTiles = new Tile[] { downEdgeTiles[0].tileOptions[0].leftEdge };
        BLCell.CreateCell(true, BLTiles);
        downEdgeTiles.Add(BLCell);

        Cell BRCell = Instantiate(cellObj, new Vector2(dimensionX, -1), Quaternion.identity);
        Tile[] BRTiles = new Tile[] { downEdgeTiles[dimensionX-1].tileOptions[0].rightEdge };
        BRCell.CreateCell(true, BRTiles);
        downEdgeTiles.Add(BRCell);

        Cell TLCell = Instantiate(cellObj, new Vector2(-1, dimensionY), Quaternion.identity);
        Tile[] TLTiles = new Tile[] { upEdgeTiles[0].tileOptions[0].leftEdge };
        TLCell.CreateCell(true, TLTiles);
        upEdgeTiles.Add(TLCell);
        
        Cell TRCell = Instantiate(cellObj, new Vector2(dimensionX, dimensionY), Quaternion.identity);
        Tile[] TRTiles = new Tile[] { upEdgeTiles[dimensionX-1].tileOptions[0].rightEdge };
        TRCell.CreateCell(true, TRTiles);
        upEdgeTiles.Add(TRCell);

        for(int i=0; i< downEdgeTiles.Count; i++)
        {
            Instantiate(downEdgeTiles[i].tileOptions[0], downEdgeTiles[i].transform.position, Quaternion.identity);

            Instantiate(upEdgeTiles[i].tileOptions[0], upEdgeTiles[i].transform.position, Quaternion.identity);
        }
        for (int i = 0; i < leftEdgeTiles.Count; i++)
        {
            Instantiate(leftEdgeTiles[i].tileOptions[0], leftEdgeTiles[i].transform.position, Quaternion.identity);

            Instantiate(rightEdgeTiles[i].tileOptions[0], rightEdgeTiles[i].transform.position, Quaternion.identity);
        }

        grid.AddRange(downEdgeTiles);
        grid.AddRange(upEdgeTiles);
        grid.AddRange(leftEdgeTiles);
        grid.AddRange(rightEdgeTiles);
    }
}
