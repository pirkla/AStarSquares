using System.Collections.Generic;
using System.Linq;

namespace AStarSquares
{
    public class NavPath {

        public NavPath(IList<PathNode> pathNodes) {
            PathNodes = pathNodes;
            TotalCost = pathNodes.Aggregate(0, (total, next) => total += next.Cost );
        }
        public int TotalCost;
        public IList<PathNode> PathNodes;


    }
    public struct PathNode {
        public PathNode(NavNodeLink navLink, int cost) {
            NavLink = navLink;
            Cost = cost;
        }
        public NavNodeLink NavLink;
        public int Cost;
    }
    
}