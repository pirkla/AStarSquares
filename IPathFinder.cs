using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Mathematics;

namespace AStarSquares
{
    public interface IPathFinder {
        NavPath FindPath(int3 start, int3 end, IEnumerable<INavNode> allNodes, int maxHorizontal, int maxVertical);
    }
}
