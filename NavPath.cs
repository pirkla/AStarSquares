using System.Collections.Generic;
using System.Linq;

namespace AStarSquares
{
    public struct NavPath {

        public NavPath(IList<PathNode> pathNodes) {
            PathNodes = pathNodes;
            TotalCost = pathNodes.Aggregate(0, (total, next) => total += next.Cost );
        }
        public int TotalCost;
        public IList<PathNode> PathNodes;

        public struct PathNode {
            public PathNode(INavNode navNode, int cost) {
                NavNode = navNode;
                Cost = cost;
            }
            public INavNode NavNode;
            public int Cost;
        }
    }
    
}