using UnityEngine;

public class Node : MonoBehaviour{
    public Node left, up, right, down, parent;
    public Vector2Int pos;
    public bool isWalkable;
    public int fCost, gCost, hCost;

    public GameObject obstacle, pathGreen;

    public void UpdateNode(){
        obstacle.SetActive(!isWalkable);
        GetComponent<Collider>().enabled = isWalkable;
    }
}
