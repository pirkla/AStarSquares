using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Mathematics;

namespace AStarSquares
{
    public class PathFinder: IPathFinder
    {

        public NavPath FindPath(INavNode start, INavNode end, IEnumerable<INavNode> allNodes, int maxDistance, int maxUp, int maxDown) {
            List<NavCostNode> allCosts = allNodes.Select( navNode => new NavCostNode(int.MaxValue, 0, navNode, null, null)).ToList();
            List<NavCostNode> openList = new List<NavCostNode> { new NavCostNode(0,NavExtensions.GetDistance(start.Anchor, end.Anchor), start, null, null) };
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
                    if (navNodeLink.Vertical > maxUp || navNodeLink.Vertical < maxDown || navNodeLink.Distance > maxDistance) continue;
                    NavCostNode linkedCostNode = allCosts.FirstOrDefault(cost => cost.NavNode == navNodeLink.LinkedNavNode);
                    if (linkedCostNode == null) continue;
                    if (closedList.Contains(linkedCostNode)) continue;

                    int jumpPenalty = NavExtensions.GetFlatDistance(currentCostNode.NavNode.Anchor, linkedCostNode.NavNode.Anchor) > 14 ? 5 : 0;
                    int localCost = navNodeLink.Distance + linkedCostNode.NavNode.MovePenalty + jumpPenalty;

                    int tentativeGCost = currentCostNode.GCost + localCost;
                    if (tentativeGCost < linkedCostNode.GCost) {
                        linkedCostNode.FromCostNode = currentCostNode;
                        linkedCostNode.FromLink = navNodeLink;
                        linkedCostNode.GCost = tentativeGCost;
                        linkedCostNode.HCost = NavExtensions.GetDistance(linkedCostNode.NavNode.Anchor, end.Anchor);
                        linkedCostNode.LocalCost = localCost;
                        if (!openList.Contains(linkedCostNode)) {
                            openList.Add(linkedCostNode);
                        }
                    }
                }
            }
            return new NavPath(new List<PathNode>());
        }

        private NavPath CalculatePath(NavCostNode endCostNode) {
            List<PathNode> path = new List<PathNode>();

            NavCostNode currentCostNode = endCostNode;
            while(currentCostNode.FromCostNode != null) {
                path.Add(new PathNode(currentCostNode.FromLink, currentCostNode.LocalCost));
                currentCostNode = currentCostNode.FromCostNode;
            }
            path.Reverse();
            return new NavPath(path);
        }
    }
}