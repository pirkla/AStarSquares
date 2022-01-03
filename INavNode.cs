using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace AStarSquares
{
    public interface INavNode {
        Vector3Int Anchor { get; }
        int MovePenalty { get; }
        IList<NavNodeLink> NavNodeLinks { get; set; }
        NavActor OccupyingActor { get; set;}

        NavActor PassthroughActor { get; set;}

        GameObject gameObject { get; }
    }
}
