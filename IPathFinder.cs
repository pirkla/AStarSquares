using System.Collections.Generic;
using System;

namespace AStarSquares
{
    public interface IPathFinder {
        NavPath FindPath(INavNode start, INavNode end, IEnumerable<INavNode> allNodes, int maxDistance, int maxUp, int maxDown);
    }
}
