using System.Collections.Generic;
using UnityEngine;

public static class APathfinding{
    public static List<Node> FindPath(Node startNode, Node targetNode)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        
        while (openSet.Count > 0){
            Node currentNode = FindBestNodeInOpenSet(openSet, closedSet);
 
            if (currentNode == targetNode) return RetracePath(startNode, targetNode);

            CheckAndAddNeighbours(openSet, closedSet, currentNode, targetNode);
        }

        return null;
    }
    

    private static Node FindBestNodeInOpenSet(List<Node> openSet, HashSet<Node> closedSet){
        Node current = openSet[0];
        for (int i = 1; i < openSet.Count; i++)
        {
            if (openSet[i].fCost < current.fCost || openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost)
            {
                current = openSet[i];
            }
        }
        
        openSet.Remove(current);
        closedSet.Add(current);
        
        return current;
    }
    
    private static void CheckAndAddNeighbours(List<Node> openSet, HashSet<Node> closedSet, Node currentNode, Node targetNode){
        List<Node> neighbours = GetNeigborOfNode(currentNode);
        foreach (Node neighbour in neighbours) {
            if (!neighbour.isWalkable || closedSet.Contains(neighbour)) continue;
 
            int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
            if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
            {
                neighbour.gCost = newMovementCostToNeighbour;
                neighbour.hCost = GetDistance(neighbour, targetNode);
                neighbour.fCost = neighbour.gCost + neighbour.hCost;
                neighbour.parent = currentNode;
 
                if (!openSet.Contains(neighbour))
                    openSet.Add(neighbour);
            }
        }
    }
    
    private static int GetDistance(Node nodeObjectA, Node nodeObjectB)
    {
        int dstX = Mathf.Abs(nodeObjectA.pos.x- nodeObjectB.pos.x);
        int dstY = Mathf.Abs(nodeObjectA.pos.y - nodeObjectB.pos.y);
 
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private static List<Node> RetracePath(Node start, Node target)
    {
        List<Node> path = new List<Node>();
        Node current = target;
        
        while(current != start) {
            current.pathGreen.SetActive(true);
            path.Add(current);
            current = current.parent;
        }
        path.Add(current);
        
        path.Reverse();
        return path;
    }

    private static List<Node> GetNeigborOfNode(Node node){
        List<Node> children = new List<Node>();
        if (node.left != null){
            children.Add(node.left);
            if (node.up != null){
                children.Add(node.left.up);
                children.Add(node.up);
            }
            if (node.down != null){
                children.Add(node.left.down);
                children.Add(node.down);
            }
        }
        if (node.right != null){
            children.Add(node.right);
            if(node.up != null) children.Add(node.right.up);
            if(node.down != null) children.Add(node.right.down);
        }

        return children;
    }
    
}
