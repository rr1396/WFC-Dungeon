using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool isCollapsed;
    public Tile[] tileOptions;


    //constructors
    public void CreateCell(bool collapsed, Tile[] tiles)
    {
        isCollapsed = collapsed;
        tileOptions = tiles;
    }

    public void RecreateCell(Tile[] tiles)
    {
        tileOptions = tiles;
    }
}
