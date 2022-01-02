using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Unity.Collections;


namespace AStarSquares
{
    public class PathFinder: IPathFinder
    {
        private const int STRAIGHT_COST = 10;
        private const int DIAGONAL_COST = 14;


        public NavPath FindPath(INavNode start, INavNode end, IEnumerable<INavNode> allNodes, int maxHorizontal, int maxVertical) {
            NativeHashMap<Vector3Int, NavCostNode> allCosts = new NativeHashMap<Vector3Int, NavCostNode>();
            allNodes.ToList().ForEach( node => {
                allCosts.Add(node.Anchor, new NavCostNode(){
                    GCost = int.MaxValue,
                    NavNode = node,
                });
            });
            NativeList<Vector3Int> openList = new NativeList<Vector3Int>();
            NavCostNode startNode = allCosts[start.Anchor];
            startNode.GCost = 0;
            startNode.HCost = GetDistance(start.Anchor, end.Anchor);
            openList.Add(start.Anchor);
            //  allNodes.Select( navNode => new NavCostNode() {
            //     GCost = int.MaxValue,
            //     NavNode = navNode,
            //     Index = navNode.Anchor
            // }).ToList();


            // NavCostNode startNode = allCosts.Find( navCostNode => navCostNode.NavNode == start);
            // startNode.GCost = 0;
            // startNode.HCost = GetDistance(start.Anchor, end.Anchor);
            // openList.Add(startNode.Index);
            
            //  {
            //     new NavCostNode() {
            //         Position = new int3(start.Anchor.x, start.Anchor.y, start.Anchor.z),
            //         GCost = 0,
            //         HCost = GetDistance(start.Anchor, end.Anchor),
            //         NavNode = start
            //     }
            // };

            // List<NavCostNode> allCosts = allNodes.Select( navNode => new NavCostNode(int.MaxValue, 0, navNode, null, null)).ToList();
            // List<NavCostNode> openList = new List<NavCostNode> { new NavCostNode(0,GetDistance(start.Anchor, end.Anchor), start, null, null) };
            NativeList<Vector3> closedList = new NativeList<Vector3>();

            while (openList.Count() > 0) {
                NavCostNode currentCostNode =  lowestFCostNode(openList, allCosts);
                if (currentCostNode.NavNode == end) {
                    return CalculatePath(currentCostNode, allCosts);
                }

                openList.Remove(currentCostNode);
                closedList.Add(currentCostNode);

                foreach (NavNodeLink navNodeLink in currentCostNode.NavNode.NavNodeLinks)
                {
                    Debug.Log("checking links");
                    NavCostNode linkedCostNode = allCosts.FirstOrDefault(cost => cost.NavNode == navNodeLink.LinkedNavNode);
                    if (linkedCostNode.Equals(default(NavCostNode))) continue;
                    if (closedList.Any(costNode => costNode.NavNode == linkedCostNode.NavNode)) continue;

                    Debug.Log("checking links 2");

                    int tentativeGCost = currentCostNode.GCost + navNodeLink.Distance + linkedCostNode.NavNode.MovePenalty;
                    if (tentativeGCost < linkedCostNode.GCost) {
                        Debug.Log("checking links 3");
                        linkedCostNode.FromNavNode = currentCostNode.NavNode;
                        linkedCostNode.FromLink = navNodeLink;
                        linkedCostNode.GCost = tentativeGCost;
                        linkedCostNode.HCost = GetDistance(linkedCostNode.NavNode.Anchor, end.Anchor);
                        if (!openList.Any(costNode => costNode.NavNode == linkedCostNode.NavNode)) {
                            openList.Add(linkedCostNode);
                        }
                    }
                }
            }
            return new NavPath();
        }

        private NavCostNode lowestFCostNode(NativeList<Vector3Int> openList, NativeHashMap<Vector3Int, NavCostNode> allCostNodes) {
            NavCostNode lowestFCostNode = new NavCostNode(){ 
                HCost = int.MaxValue,
                GCost = int.MaxValue
            };
            openList.ToList().ForEach( index => {
                if (allCostNodes[index].FCost < lowestFCostNode.FCost) {
                    lowestFCostNode = allCostNodes[index];
                }
            });
            return lowestFCostNode;
        }
        private NavPath CalculatePath(NavCostNode endCostNode, IEnumerable<NavCostNode> allCostNodes) {
            List<NavPath.PathNode> path = new List<NavPath.PathNode>();

            NavCostNode currentCostNode = endCostNode;
            while(currentCostNode.FromNavNode != null) {    
                path.Add(new NavPath.PathNode(currentCostNode.FromLink, currentCostNode.GCost));
                currentCostNode = allCostNodes.ToList().Find(navCostNode => navCostNode.NavNode == currentCostNode.FromNavNode);
            }
            Debug.Log("count" + path.Count);
            path.Reverse();
            return new NavPath(path);
        }


        private int GetDistance(Vector3Int from, Vector3Int to)
        {
            Vector3Int dist = from - to;
            int distX = Mathf.Abs(dist.x);
            int distY = Mathf.Abs(dist.y);
            int distZ = Mathf.Abs(dist.z);
            if (distX > distZ)
            {
                return DIAGONAL_COST * distZ + STRAIGHT_COST * (distX - distZ) + STRAIGHT_COST * distY;
            }

            return DIAGONAL_COST * distX + STRAIGHT_COST * (distZ - distX) + STRAIGHT_COST * distY;
        }
    }
}