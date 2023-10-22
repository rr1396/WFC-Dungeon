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
    public Tile upEdge;
    public Tile downEdge;
    public Tile leftEdge;
    public Tile rightEdge;
}
