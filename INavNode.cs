using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AStarSquares
{
    public interface INavNode {
        Vector3Int Anchor { get; }
        int MovePenalty { get; }
        IEnumerable<INavNode> LinkedNavNodes { get; }

    }
}
