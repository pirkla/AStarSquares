using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace AStarSquares {
    public struct NavCostNode: IComparable
    {
        public int CompareTo(object other) {
            if (other == null) return 1;
            return this.FCost.CompareTo(((NavCostNode)other).FCost);
        }
        public Vector3Int? FromIndex;
        public INavNode NavNode;
        public NavNodeLink FromLink;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
        public Vector3Int Index;
    }
}
