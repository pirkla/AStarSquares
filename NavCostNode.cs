using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine;


namespace AStarSquares {
    public struct NavCostNode
    {
        public int3 FromIndex;
        public int Distance;
        public int GCost;
        public int HCost;
        public int FCost;
        public int3 Index;

        public NavCostNodeLink[] CostNodeLinks;
    }

    public struct NavCostNodeLink {
        public int Distance;
        public int3 LinkedIndex;
    }
}
