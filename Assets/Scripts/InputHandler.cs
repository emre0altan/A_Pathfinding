using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class InputHandler : MonoBehaviour{
    public APathfindingJob aPathfindingJob;
    public NodeGrid nodeGrid;
    
    private Node _startNode, _endNode;
    private Ray _ray;
    private RaycastHit _hit;
    private Camera _camera;

    private void Start(){
        _camera = Camera.main;
    }
    
    private void Update(){
        if (Input.GetKeyDown(KeyCode.S)){
            nodeGrid.ShuffleGrid();
            StartCoroutine(PathfindRoutine());
        }
        
        if (Input.GetMouseButtonDown(0)){
            if (_startNode != null && _endNode != null){
                _startNode = null;
                _endNode = null;
                nodeGrid.ClearPath();
            }
            else{
                _ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(_ray, out _hit, 100f)){
                    if (_startNode == null){
                        _startNode = _hit.transform.GetComponent<Node>();
                    }
                    else if (_endNode == null){
                        _endNode = _hit.transform.GetComponent<Node>();
                        StartCoroutine("JobPathfind");
                    }
                }
            }
        }
    }

    IEnumerator JobPathfind(){
        StopCoroutine("JobPathfind");
        StopCoroutine("PathfindRoutine");
        
        JobPathfinding(_startNode, _endNode);
        yield return new WaitForSeconds(0.5f);
        nodeGrid.ClearPath();
    }
    
    IEnumerator PathfindRoutine(){
        StopCoroutine("JobPathfind");
        StopCoroutine("PathfindRoutine");
        
        NormalPathfinding(nodeGrid.GetNode(0,0),nodeGrid.GetNode(nodeGrid.gridSize.x-1, nodeGrid.gridSize.y-1));
        yield return new WaitForSeconds(0.5f);
        nodeGrid.ClearPath();
        NormalPathfinding(nodeGrid.GetNode(0,nodeGrid.gridSize.y-1),nodeGrid.GetNode(nodeGrid.gridSize.x-1, 0));
        yield return new WaitForSeconds(0.5f);
        nodeGrid.ClearPath();
        JobPathfinding(nodeGrid.GetNode(0,0),nodeGrid.GetNode(nodeGrid.gridSize.x-1, nodeGrid.gridSize.y-1));
        yield return new WaitForSeconds(0.5f);
        nodeGrid.ClearPath();
        JobPathfinding(nodeGrid.GetNode(0,nodeGrid.gridSize.y-1),nodeGrid.GetNode(nodeGrid.gridSize.x-1, 0));
        yield return new WaitForSeconds(0.5f);
        nodeGrid.ClearPath();
    }

    public void NormalPathfinding(Node from, Node to){
        float timer = Time.realtimeSinceStartup;
        List<Node> path = APathfinding.FindPath(from, to);
        float interval = Time.realtimeSinceStartup - timer;
        Debug.Log("Normal:" + interval * 1000);
        if(path != null) nodeGrid.ShowPath(path);
    }
    
    public void JobPathfinding(Node from, Node to){
        float timer = Time.realtimeSinceStartup;
        aPathfindingJob.StartJob(nodeGrid.NodeJobGrid, nodeGrid.gridSize, from.pos, to.pos);
        NodeJob[] path = aPathfindingJob.CompleteJob();
        float interval = Time.realtimeSinceStartup - timer;
        Debug.Log("With Job:" + interval * 1000);
        if(path != null) nodeGrid.ShowPath(path);
    }
}
