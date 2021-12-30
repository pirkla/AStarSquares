using System.Collections.Generic;
using System;

namespace AStarSquares
{
    public interface IPathFinder {
        List<Tuple<INavNode, int>> FindPath(INavNode start, INavNode end, IEnumerable<INavNode> allNodes, int maxHorizontal, int maxVertical);
    }
}
