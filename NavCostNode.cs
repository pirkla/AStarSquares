using System.Collections.Generic;
using System;

namespace AStarSquares {
    public class NavCostNode: IComparable
    {
        public NavCostNode(int gCost, int hCost, INavNode navNode, NavCostNode fromCostNode, NavNodeLink fromLink) {
            GCost = gCost;
            HCost = hCost;
            NavNodeLinks = navNode.NavNodeLinks;
            NavNode = navNode;
            FromCostNode = fromCostNode;
            FromLink = fromLink;
        }
        public int CompareTo(object other) {
            if (other == null) return 1;
            return this.FCost.CompareTo(((NavCostNode)other).FCost);
        }
        public NavCostNode FromCostNode;
        public IEnumerable<NavNodeLink> NavNodeLinks;
        public INavNode NavNode;
        public NavNodeLink FromLink;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
    }
}
