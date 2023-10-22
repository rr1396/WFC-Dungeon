using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Tile[] upNeighbor;
    public Tile[] downNeighbor;
    public Tile[] leftNeighbor;
    public Tile[] rightNeighbor;
    public bool itemIsPlacable;
}
