using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

namespace AStarSquares
{
    public struct NavPath {
        public PathNode[] PathNodes;
    }
    public struct PathNode {
            public int3 LinkedLocation;
            public int Distance;
            public int Cost;
        }
}