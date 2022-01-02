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
            NativeHashMap<Vector3Int, NavCostNode> allCosts = new NativeHashMap<Vector3Int, NavCostNode>(allNodes.Count(), Allocator.Temp);
            foreach (INavNode node in allNodes)
            {
                allCosts.TryAdd(node.Anchor, new NavCostNode(){
                    GCost = int.MaxValue,
                    NavNode = node,
                    Index = node.Anchor
                });
            }

            NativeList<Vector3Int> openList = new NativeList<Vector3Int>(Allocator.Temp);
            NativeList<Vector3> closedList = new NativeList<Vector3>(Allocator.Temp);


            NavCostNode startNode = allCosts[start.Anchor];
            startNode.GCost = 0;
            startNode.HCost = GetDistance(start.Anchor, end.Anchor);
            openList.Add(start.Anchor);


            while (openList.Length > 0) {
                NavCostNode currentCostNode =  lowestFCostNode(openList, allCosts);
                if (currentCostNode.NavNode == end) {
                    return CalculatePath(currentCostNode, allCosts);
                }

                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentCostNode.Index) {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedList.Add(currentCostNode.Index);

                foreach (NavNodeLink navNodeLink in currentCostNode.NavNode.NavNodeLinks)
                {
                    Debug.Log("checking links");
                    if (!allCosts.TryGetValue(navNodeLink.LinkedNavNode.Anchor, out NavCostNode linkedCostNode)) continue;
                    if (closedList.Contains(linkedCostNode.Index)) continue;

                    Debug.Log("checking links 2");

                    int tentativeGCost = currentCostNode.GCost + navNodeLink.Distance + linkedCostNode.NavNode.MovePenalty;
                    if (tentativeGCost < linkedCostNode.GCost) {
                        Debug.Log("checking links 3");
                        linkedCostNode.FromIndex = currentCostNode.Index;
                        linkedCostNode.FromLink = navNodeLink;
                        linkedCostNode.GCost = tentativeGCost;
                        linkedCostNode.HCost = GetDistance(linkedCostNode.NavNode.Anchor, end.Anchor);
                        if(!openList.Contains(linkedCostNode.Index)) {
                            openList.Add(linkedCostNode.Index);
                        }
                    }
                }
            }
            openList.Dispose();
            closedList.Dispose();
            allCosts.Dispose();

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
        private NavPath CalculatePath(NavCostNode endCostNode, NativeHashMap<Vector3Int, NavCostNode> allCostNodes) {
            List<NavPath.PathNode> path = new List<NavPath.PathNode>();

            NavCostNode currentCostNode = endCostNode;
            while(currentCostNode.FromIndex != null) {    
                path.Add(new NavPath.PathNode(currentCostNode.FromLink, currentCostNode.GCost));
                currentCostNode = allCostNodes[(Vector3Int)currentCostNode.FromIndex];
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