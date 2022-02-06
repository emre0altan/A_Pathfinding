using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class NodeGrid : MonoBehaviour{
    public Node nodePrefab;
    public Vector2Int gridSize;
    [Range(0f,1f)]
    public float obstaclePossibility;
    public NativeList<NodeJob> NodeJobGrid;
    
    private List<List<Node>> _grid;
    

    private void Awake(){
        SetupGrid();
        ShuffleGrid();
    }

    private void SetupGrid(){
        NodeJobGrid = new NativeList<NodeJob>(Allocator.Persistent);
        _grid = new List<List<Node>>();
        for (int i = 0; i < gridSize.x; i++){
            
            _grid.Add(new List<Node>());
            for (int j = 0; j < gridSize.y; j++){
                
                Node node = Instantiate(nodePrefab, transform);
                node.transform.position = new Vector3(i-gridSize.x * 0.5f +0.5f, 0, gridSize.y * 0.5f -0.5f - j);
                node.pos = new Vector2Int(i, j);
                
                NodeJob nodeJob = new NodeJob(Vector2Int.zero, node.pos, node.isWalkable, 0, 0, 0);
                NodeJobGrid.Add(nodeJob);

                if (i > 0){
                    node.left = _grid[i-1][j];
                    _grid[i-1][j].right = node;
                }
                
                if (j > 0){
                    node.up = _grid[i][j-1];
                    _grid[i][j-1].down = node;
                }
                
                _grid[i].Add(node);
            }
        }
    }

    public void ShuffleGrid(){
        for (int i = 0; i < gridSize.x; i++){
            for (int j = 0; j < gridSize.y; j++){
                _grid[i][j].isWalkable = Random.Range(0f, 1f) > obstaclePossibility || IsCorner(i, j, gridSize);
                NodeJobGrid[gridSize.x * i + j] = new NodeJob(Vector2Int.zero, _grid[i][j].pos, _grid[i][j].isWalkable, 0, 0, 0);
                _grid[i][j].UpdateNode();
            }
        }
    }

    public Node GetNode(int i, int j) => _grid[i][j];
    
    private bool IsCorner(int i, int j, Vector2Int gridSize){
        return (i == 0 || i == gridSize.x -1) && (j == 0 || j == gridSize.y - 1);
    }

    public void ShowPath(NodeJob[] path){
        for (int i = 0; i < path.Length; i++){
            _grid[path[i].pos.x][path[i].pos.y].pathGreen.SetActive(true);
        }
    }
    
    public void ShowPath(List<Node> path){
        for (int i = 0; i < path.Count; i++){
            path[i].pathGreen.SetActive(true);
        }
    }

    public void ClearPath(){
        for (int i = 0; i < gridSize.x; i++){
            for (int j = 0; j < gridSize.y; j++){
                _grid[i][j].pathGreen.SetActive(false);
            }
        }
    }
    
    private void OnDestroy()
    {
        NodeJobGrid.Dispose();
    }
}
