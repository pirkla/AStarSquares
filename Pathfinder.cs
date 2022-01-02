using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;


namespace AStarSquares
{
    // public class PathFinder
    // {


    public struct FindPathJob: IJob {
        private const int STRAIGHT_COST = 10;
        private const int DIAGONAL_COST = 14;

        public int3 Start;
        public int3 End;
        public NativeMultiHashMap<int3, NavCostNodeLink> allLinks;
        public  NativeHashMap<int3, NavCostNode> allCosts;
        public NativeList<PathNode> ResultPath;
        public void Execute() {
            ResultPath = FindPath(Start, End, allLinks, allCosts);
        }
        public NativeList<PathNode> FindPath(int3 start, int3 end, NativeMultiHashMap<int3, NavCostNodeLink> allLinks, NativeHashMap<int3, NavCostNode> allCosts) {
            // NativeMultiHashMap<int3, NavCostNodeLink> allLinks = new NativeMultiHashMap<int3, NavCostNodeLink>(allNodes.Count(), Allocator.Temp);
            // NativeHashMap<int3, NavCostNode> allCosts = new NativeHashMap<int3, NavCostNode>(allNodes.Count(), Allocator.Temp);
            // foreach (INavNode node in allNodes)
            // {
            //     foreach (NavNodeLink link in node.NavNodeLinks)
            //     {
            //         allLinks.Add(node.Anchor.asInt3(), new NavCostNodeLink(){
            //             Distance = link.Distance,
            //             LinkedIndex = link.LinkedNavNode.Anchor.asInt3()
            //         });
            //     }

            //     allCosts.TryAdd(node.Anchor.asInt3(), new NavCostNode(){
            //         GCost = int.MaxValue,
            //         Index = node.Anchor.asInt3(),
            //         Linked = false
            //     });
            // }

            NativeList<int3> openList = new NativeList<int3>(Allocator.Temp);
            NativeList<int3> closedList = new NativeList<int3>(Allocator.Temp);


            NavCostNode startNode = allCosts[start];
            startNode.GCost = 0;
            startNode.HCost = GetDistance(start, end);
            allCosts[start] = startNode;
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

                foreach (NavCostNodeLink navNodeLink in allLinks.GetValuesForKey(currentCostNode.Index))
                {
                    if (!allCosts.TryGetValue(navNodeLink.LinkedIndex, out NavCostNode linkedCostNode)) continue;
                    if (closedList.Contains(linkedCostNode.Index)) continue;

                    
                    int tentativeGCost = currentCostNode.GCost + navNodeLink.Distance;
                    if (tentativeGCost < linkedCostNode.GCost) {
                        linkedCostNode.FromIndex = currentCostNode.Index;
                        linkedCostNode.Linked = true;
                        linkedCostNode.GCost = tentativeGCost;
                        linkedCostNode.HCost = GetDistance(linkedCostNode.Index, end);
                        linkedCostNode.Distance = navNodeLink.Distance;
                        if(!openList.Contains(linkedCostNode.Index)) {
                            openList.Add(linkedCostNode.Index);
                            allCosts[linkedCostNode.Index] = linkedCostNode;
                        }
                    }
                }
            }
            openList.Dispose();
            closedList.Dispose();
            allCosts.Dispose();
            allLinks.Dispose();

            return new NativeList<PathNode>();
        }

        private NavCostNode lowestFCostNode(NativeList<int3> openList, NativeHashMap<int3, NavCostNode> allCostNodes) {
            NavCostNode lowestFCostNode = allCostNodes[openList[0]];
            foreach (int3 nodeIndex in openList)
            {
                if (allCostNodes[nodeIndex].FCost < lowestFCostNode.FCost) {
                    lowestFCostNode = allCostNodes[nodeIndex];
                }
            }
            return lowestFCostNode;
        }
        private NativeList<PathNode> CalculatePath(NavCostNode endCostNode, NativeHashMap<int3, NavCostNode> allCostNodes) {
            NativeList<PathNode> path = new NativeList<PathNode>(allCostNodes.Count(), Allocator.Temp);
            NavCostNode currentCostNode = endCostNode;
            while(currentCostNode.Linked) {    
                path.Add(new PathNode() {
                    Location = currentCostNode.Index,
                    Cost = currentCostNode.GCost,
                    Distance = currentCostNode.Distance
                });
                currentCostNode = allCostNodes[currentCostNode.FromIndex];
            }
            path.Reverse();
            return path;
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


        // }


    }

    public struct NavCostNode
    {
        public int3 FromIndex;
        public bool Linked;
        public int Distance;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
        public int3 Index;
    }

    public struct NavCostNodeLink {
        public int Distance;
        public int3 LinkedIndex;
    }
}