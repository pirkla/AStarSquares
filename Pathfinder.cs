using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace AStarSquares
{
    public class PathFinder: IPathFinder
    {
        private const int STRAIGHT_COST = 10;
        private const int DIAGONAL_COST = 14;

        public List<Tuple<INavNode, int>> FindPath(INavNode start, INavNode end, IEnumerable<INavNode> allNodes, int maxHorizontal, int maxVertical) {
            Dictionary<Vector3Int, NavCostNode> allCosts = allNodes.Distinct().ToDictionary( navNode => navNode.Anchor, navNode => new NavCostNode(int.MaxValue, 0, navNode, null));
            List<NavCostNode> openList = new List<NavCostNode> { new NavCostNode(0,GetDistance(start.Anchor, end.Anchor), start, null) };
            List<NavCostNode> closedList = new List<NavCostNode>();

            while (openList.Count > 0) {
                NavCostNode currentNode = openList.Min();
                if (currentNode.NavNode == end) {
                    return CalculatePath(currentNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (NavCostNode neighborNode in GetNeighbors(currentNode, maxHorizontal, maxVertical, allCosts))
                {
                    if (closedList.Contains(neighborNode)) continue;

                    int tentativeGCost = currentNode.GCost + GetDistance(currentNode.NavNode.Anchor, neighborNode.NavNode.Anchor) + neighborNode.NavNode.MovePenalty;
                    if (tentativeGCost < neighborNode.GCost) {
                        neighborNode.FromCostNode = currentNode;
                        neighborNode.GCost = tentativeGCost;
                        neighborNode.HCost = GetDistance(neighborNode.NavNode.Anchor, end.Anchor);
                        if (!openList.Contains(neighborNode)) {
                            openList.Add(neighborNode);
                        }
                    }
                }
            }
            return null;
        }

        private List<NavCostNode> GetNeighbors(NavCostNode targetNode, int radius, int maxVert, Dictionary<Vector3Int, NavCostNode> allNodes) {
            List<NavCostNode> nodes = new List<NavCostNode>();
            nodes.AddRange(targetNode.LinkedLocs.Where( loc => allNodes.ContainsKey(loc)).Select( loc => allNodes[loc]));
            for (int z = -radius; z <= radius; z++) {
                for (int x = -radius; x <= radius; x++) {
                    nodes.AddRange(GetNodes(new Vector3Int(x + targetNode.NavNode.Anchor.x, targetNode.NavNode.Anchor.y, z + targetNode.NavNode.Anchor.z), allNodes, maxVert));
                }
            }
            return nodes;
        }

        private List<Tuple<INavNode, int>> CalculatePath(NavCostNode endCostNode) {
            List<Tuple<INavNode, int>> path = new List<Tuple<INavNode, int>>();
            path.Add(new Tuple<INavNode, int>(endCostNode.NavNode, endCostNode.GCost));
            NavCostNode currentCostNode = endCostNode;
            while(currentCostNode.FromCostNode != null) {
                path.Add(new Tuple<INavNode, int>(currentCostNode.FromCostNode.NavNode, currentCostNode.FromCostNode.GCost));
                currentCostNode = currentCostNode.FromCostNode;
            }
            path.Reverse();
            return path;
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

        private IEnumerable<NavCostNode> GetNodes(Vector3Int position, Dictionary<Vector3Int,NavCostNode> navCostNodes, int maxVert) {
            return navCostNodes.Where(node => node.Key.x == position.x)
                .Where(node => node.Key.z == position.z)
                .Where(node => Mathf.Abs(node.Key.y - position.y) <= maxVert)
                .Select(kvp => kvp.Value);
        }

        private class NavCostNode: IComparable
        {
            public NavCostNode(int gCost, int hCost, INavNode navNode, NavCostNode fromCostNode) {
                GCost = gCost;
                HCost = hCost;
                LinkedLocs = navNode.LinkedNavNodes.Select(linkedNode => linkedNode.Anchor);
                NavNode = navNode;
                FromCostNode = fromCostNode;
            }
            public int CompareTo(object other) {
                if (other == null) return 1;
                return this.FCost.CompareTo(((NavCostNode)other).FCost);
            }
            public NavCostNode FromCostNode;
            public IEnumerable<Vector3Int> LinkedLocs;
            public INavNode NavNode;
            public int GCost;
            public int HCost;
            public int FCost => GCost + HCost;
        }
    }
}