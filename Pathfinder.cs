using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;


namespace AStarSquares
{
    public class PathFinder: IPathFinder
    {
        private const int STRAIGHT_COST = 10;
        private const int DIAGONAL_COST = 14;


        public NavPath FindPath(int3 start, int3 end, IEnumerable<INavNode> allNodes, int maxHorizontal, int maxVertical) {
            NativeHashMap<int3, NavCostNode> allCosts = new NativeHashMap<int3, NavCostNode>(allNodes.Count(), Allocator.Temp);
            foreach (INavNode node in allNodes)
            {
                allCosts.TryAdd(node.Anchor.asInt3(), new NavCostNode(){
                    GCost = int.MaxValue,
                    Index = new int3(node.Anchor.x, node.Anchor.y, node.Anchor.z),
                    CostNodeLinks = node.NavNodeLinks.Select( nodeLink => new NavCostNodeLink() {
                        Distance = nodeLink.Distance,
                        LinkedIndex = nodeLink.LinkedNavNode.Anchor.asInt3()
                    }).ToArray()
                    // CostNodeLinks = node.NavNodeLinks.Select( node => new NavCostNodeLink(){
                    //     node.LinkedNavNode.Anchor.ToArray()
                    // }))
                });
            }

            NativeList<int3> openList = new NativeList<int3>(Allocator.Temp);
            NativeList<int3> closedList = new NativeList<int3>(Allocator.Temp);


            NavCostNode startNode = allCosts[start];
            startNode.GCost = 0;
            startNode.HCost = GetDistance(start, end);
            openList.Add(start);


            while (openList.Length > 0) {
                NavCostNode currentCostNode =  lowestFCostNode(openList, allCosts);
                if (currentCostNode.Index.Equals(end)) {
                    return CalculatePath(currentCostNode, allCosts);
                }

                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i].Equals(currentCostNode.Index)) {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedList.Add(currentCostNode.Index);

                foreach (NavCostNodeLink navNodeLink in currentCostNode.CostNodeLinks)
                {
                    if (!allCosts.TryGetValue(navNodeLink.LinkedIndex, out NavCostNode linkedCostNode)) continue;
                    if (closedList.Contains(linkedCostNode.Index)) continue;

                    
                    int tentativeGCost = currentCostNode.GCost + navNodeLink.Distance;
                    if (tentativeGCost < linkedCostNode.GCost) {
                        linkedCostNode.FromIndex = currentCostNode.Index;
                        linkedCostNode.GCost = tentativeGCost;
                        linkedCostNode.HCost = GetDistance(linkedCostNode.Index, end);
                        linkedCostNode.Distance = navNodeLink.Distance;
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

        private NavCostNode lowestFCostNode(NativeList<int3> openList, NativeHashMap<int3, NavCostNode> allCostNodes) {
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
        private NavPath CalculatePath(NavCostNode endCostNode, NativeHashMap<int3, NavCostNode> allCostNodes) {
            List<PathNode> path = new List<PathNode>();

            NavCostNode currentCostNode = endCostNode;
            while(!currentCostNode.FromIndex.Equals(new int3(0,0,0))) {    
                path.Add(new PathNode() {
                    LinkedLocation = (int3)currentCostNode.FromIndex,
                    Cost = currentCostNode.GCost,
                    Distance = currentCostNode.Distance
                });
                currentCostNode = allCostNodes[(int3)currentCostNode.FromIndex];
            }
            path.Reverse();
            return new NavPath(){
                PathNodes = path.ToArray()
            };
        }


        private int GetDistance(int3 from, int3 to)
        {
            int distX = Mathf.Abs(from.x - to.x);
            int distY = Mathf.Abs(from.y - to.y);
            int distZ = Mathf.Abs(from.z - to.z);
            if (distX > distZ)
            {
                return DIAGONAL_COST * distZ + STRAIGHT_COST * (distX - distZ) + STRAIGHT_COST * distY;
            }

            return DIAGONAL_COST * distX + STRAIGHT_COST * (distZ - distX) + STRAIGHT_COST * distY;
        }
    }
}