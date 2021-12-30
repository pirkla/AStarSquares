using System.Collections.Generic;
using System;

namespace AStarSquares
{
    public interface IPathFinder {
        NavPath FindPath(INavNode start, INavNode end, IEnumerable<INavNode> allNodes, int maxHorizontal, int maxVertical);
    }
}
