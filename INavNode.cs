using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace AStarSquares
{
    public interface INavNode {
        Vector3 Anchor { get; }
        int MovePenalty { get; }
        IList<NavNodeLink> NavNodeLinks { get; set; }

    }
}
