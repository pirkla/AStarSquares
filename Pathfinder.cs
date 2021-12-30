using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace AStarSquares
{
    public class PathFinder: IPathFinder
    {
        private const float STRAIGHT_COST = 10;
        private const float DIAGONAL_COST = 14;


        public NavPath FindPath(INavNode start, INavNode end, IEnumerable<INavNode> allNodes, int maxHorizontal, int maxVertical) {
            List<NavCostNode> allCosts = allNodes.Select( navNode => new NavCostNode(int.MaxValue, 0, navNode, null)).ToList();
            List<NavCostNode> openList = new List<NavCostNode> { new NavCostNode(0,GetDistance(start.Anchor, end.Anchor), start, null) };
            List<NavCostNode> closedList = new List<NavCostNode>();

            while (openList.Count > 0) {
                NavCostNode currentCostNode = openList.Min();
                if (currentCostNode.NavNode == end) {
                    return CalculatePath(currentCostNode);
                }

                openList.Remove(currentCostNode);
                closedList.Add(currentCostNode);

                foreach (NavNodeLink navNodeLink in currentCostNode.NavNodeLinks)
                {
                    NavCostNode linkedCostNode = allCosts.FirstOrDefault( cost => cost.NavNode == navNodeLink.LinkedNavNode);
                    if (linkedCostNode == null) continue;
                    if (closedList.Contains(linkedCostNode)) continue;

                    float tentativeGCost = currentCostNode.GCost + navNodeLink.Distance + linkedCostNode.NavNode.MovePenalty;
                    if (tentativeGCost < linkedCostNode.GCost) {
                        linkedCostNode.FromCostNode = currentCostNode;
                        linkedCostNode.GCost = tentativeGCost;
                        linkedCostNode.HCost = GetDistance(linkedCostNode.NavNode.Anchor, end.Anchor);
                        if (!openList.Contains(linkedCostNode)) {
                            openList.Add(linkedCostNode);
                        }
                    }
                }
            }
            return new NavPath();
        }

        private NavPath CalculatePath(NavCostNode endCostNode) {
            List<NavPath.PathNode> path = new List<NavPath.PathNode>();

            path.Add(new NavPath.PathNode(endCostNode.NavNode, endCostNode.GCost));
            NavCostNode currentCostNode = endCostNode;
            while(currentCostNode.FromCostNode != null) {
                path.Add(new NavPath.PathNode(currentCostNode.FromCostNode.NavNode, currentCostNode.FromCostNode.GCost));
                currentCostNode = currentCostNode.FromCostNode;
            }
            path.Reverse();
            return new NavPath(path);
        }


        private float GetDistance(Vector3 from, Vector3 to)
        {
            Vector3 dist = from - to;
            float distX = Mathf.Abs(dist.x);
            float distY = Mathf.Abs(dist.y);
            float distZ = Mathf.Abs(dist.z);
            if (distX > distZ)
            {
                return DIAGONAL_COST * distZ + STRAIGHT_COST * (distX - distZ) + STRAIGHT_COST * distY;
            }

            return DIAGONAL_COST * distX + STRAIGHT_COST * (distZ - distX) + STRAIGHT_COST * distY;
        }

        private class NavCostNode: IComparable
        {
            public NavCostNode(float gCost, float hCost, INavNode navNode, NavCostNode fromCostNode) {
                GCost = gCost;
                HCost = hCost;
                NavNodeLinks = navNode.NavNodeLinks;
                NavNode = navNode;
                FromCostNode = fromCostNode;
            }
            public int CompareTo(object other) {
                if (other == null) return 1;
                return this.FCost.CompareTo(((NavCostNode)other).FCost);
            }
            public NavCostNode FromCostNode;
            public IEnumerable<NavNodeLink> NavNodeLinks;
            public INavNode NavNode;
            public float GCost;
            public float HCost;
            public float FCost => GCost + HCost;
        }
    }
}