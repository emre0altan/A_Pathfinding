using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

using random = Unity.Mathematics.Random;

[Serializable]
public struct NodeJob: IEquatable<NodeJob>{
    public Vector2Int parent;
    public Vector2Int pos;
    public bool isWalkable;
    public int fCost, gCost, hCost;

    public NodeJob(Vector2Int parent, Vector2Int pos, bool isWalkable, int fCost, int gCost, int hCost){
        this.parent = parent;
        this.pos = pos;
        this.isWalkable = isWalkable;
        this.fCost = fCost;
        this.gCost = gCost;
        this.hCost = hCost;
    }

    public bool Equals(NodeJob other){
        return pos == other.pos;
    }

    public void Print(){
        Debug.Log("NODE-" + pos + " : PARENT:" + parent + " FCOST:" + fCost + " GCOST:" + gCost + " HCOST:" + hCost);    
    }
    
    public override int GetHashCode()
    {
        return 1; 
    }
}

public class APathfindingJob : MonoBehaviour
{
    private PathfindingJob _pathfindingJob;
    private JobHandle _pathfindingJobHandle;
    
    NativeList<NodeJob> _openSet;
    NativeList<NodeJob> _path;
    NativeHashSet<NodeJob> _closedSet;

    public void StartJob(NativeList<NodeJob> nodeJobGrid, Vector2Int gridSize, Vector2Int startPos, Vector2Int endPos){
        _openSet = new NativeList<NodeJob>(Allocator.TempJob);
        _path = new NativeList<NodeJob>(Allocator.TempJob);
        _closedSet = new NativeHashSet<NodeJob>(100, Allocator.TempJob);
        
        _pathfindingJob = new PathfindingJob()
        {
            grid = nodeJobGrid,
            startNode = nodeJobGrid[gridSize.x * startPos.x + startPos.y],
            endNode = nodeJobGrid[gridSize.x * endPos.x + endPos.y],
            size = gridSize,
            openSet = _openSet,
            path = _path,
            closedSet = _closedSet,
        };

        _pathfindingJobHandle = _pathfindingJob.Schedule();
    }

    public NodeJob[] CompleteJob(){
        _pathfindingJobHandle.Complete();
        _openSet.Dispose();
        _closedSet.Dispose();
        
        NodeJob[] normalPath = _path.ToArray();
        _path.Dispose();
        return normalPath;
    }

    [BurstCompile]
    struct PathfindingJob : IJob
    {
        public NativeList<NodeJob> grid;
        public NodeJob startNode;
        public NodeJob endNode;
        
        public Vector2Int size;
        
        public NativeList<NodeJob> openSet;
        public NativeList<NodeJob> path;
        public NativeHashSet<NodeJob> closedSet;

        public void Execute (){
            openSet.Add(startNode);
            while (openSet.Length > 0){

                NodeJob currentNode = FindBestNodeInOpenSet();
                
                if (currentNode.pos.Equals(endNode.pos)){
                    RetracePath();
                    return;
                }
                CheckAndAddNeighbours(currentNode);
            }
        }

        private NodeJob FindBestNodeInOpenSet(){
            NodeJob currentNode = openSet[0];
            int currentOpenSetIndex = 0;
            for (int x = 1; x < openSet.Length; x++){
                if((openSet[x].fCost < currentNode.fCost ||
                    openSet[x].fCost == currentNode.fCost && openSet[x].hCost < currentNode.hCost)){
                    currentNode = openSet[x];
                    currentOpenSetIndex = x;
                }
            }
            openSet.RemoveAt(currentOpenSetIndex);
            closedSet.Add(currentNode);
            return currentNode;
        }
        
        private void RetracePath()
        {
            NodeJob current = grid[size.x * endNode.pos.x + endNode.pos.y];
            while(!current.pos.Equals(startNode.pos) && !path.Contains(current)){
                path.Add(current);
                current = grid[size.x * current.parent.x + current.parent.y];
            }
            path.Add(current);
        }

        private void CheckAndAddNeighbours(NodeJob currentNode){
            NativeList<NodeJob> neighbours = GetNeigbourOfNode(currentNode);
            foreach (NodeJob neighbour in neighbours){
                if (!neighbour.isWalkable || closedSet.Contains(neighbour)){
                    continue;
                }
                int newMovCostToNeigh = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovCostToNeigh < neighbour.gCost || !openSet.Contains(neighbour)){
                    NodeJob editedNeighbor = new NodeJob(
                        currentNode.pos, neighbour.pos, true,
                        newMovCostToNeigh + GetDistance(neighbour, endNode),
                        newMovCostToNeigh, GetDistance(neighbour, endNode)
                    );
                    grid.ElementAt(size.x * neighbour.pos.x + neighbour.pos.y) = editedNeighbor;
                        
                    if (!openSet.Contains(editedNeighbor)){
                        openSet.Add(editedNeighbor);
                    }
                }
            }
        }
        
        private static int GetDistance(NodeJob nodeA, NodeJob nodeB)
        {
            int dstX = Mathf.Abs(nodeA.pos.x - nodeB.pos.x);
            int dstY = Mathf.Abs(nodeA.pos.y - nodeB.pos.y);
 
            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
        
        private NativeList<NodeJob> GetNeigbourOfNode(NodeJob node){
            NativeList<NodeJob> children = new NativeList<NodeJob>(Allocator.Temp);
            if (node.pos.x > 0){
                children.Add(grid[size.x * (node.pos.x-1) + node.pos.y]);
                if (node.pos.y > 0) children.Add(grid[size.x * (node.pos.x-1) + node.pos.y - 1]);
                if (node.pos.y < size.y - 1) children.Add(grid[size.x * (node.pos.x-1) + node.pos.y + 1]);
            }

            if (node.pos.x < size.x - 1){
                children.Add(grid[size.x * (node.pos.x + 1) + node.pos.y]);
                if (node.pos.y > 0) children.Add(grid[size.x * (node.pos.x + 1) + node.pos.y - 1]);
                if (node.pos.y < size.y - 1) children.Add(grid[size.x * (node.pos.x + 1) + node.pos.y + 1]);
            }

            if (node.pos.y > 0) children.Add(grid[size.x * node.pos.x + node.pos.y - 1]);
            if (node.pos.y < size.y - 1) children.Add(grid[size.x * node.pos.x + node.pos.y + 1]);

            return children;
        }
    }
}
