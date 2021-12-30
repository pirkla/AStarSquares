using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AStarSquares
{
    public class NavGrid : MonoBehaviour
    {
        private const int STRAIGHT_COST = 10;
        private const int DIAGONAL_COST = 14;
        
        public IDictionary<Vector3Int, List<INavNode>> AllNodes { get; private set;} = new Dictionary<Vector3Int, List<INavNode>>();

        private void Start() {
            foreach (INavNode node in GetComponentsInChildren<INavNode>())
            {
                NodeAdded(node);
            }
        }

        public void NodeAdded(INavNode node) {
            GetNeighborNodes(node).ToList().ForEach( neighborNode => {
                float distance = GetDistance(node.Anchor, neighborNode.Anchor);
                float vertical = node.Anchor.y - neighborNode.Anchor.y;
                if (!node.NavNodeLinks.Any( node => node.LinkedNavNode == neighborNode)) {
                    node.NavNodeLinks.Add(new NavNodeLink(neighborNode, distance, -vertical));
                }
                if (!neighborNode.NavNodeLinks.Any( neighborNode => neighborNode.LinkedNavNode == node)) {
                    neighborNode.NavNodeLinks.Add(new NavNodeLink(node, distance, vertical));
                }
            });
            Vector3Int roundedLoc = Vector3Int.RoundToInt(node.Anchor);
            AllNodes.TryGetValue(roundedLoc, out List<INavNode> dictNodes);
            List<INavNode> newNodes = dictNodes ?? new List<INavNode>();
            newNodes.Add(node);
            AllNodes.Add(Vector3Int.RoundToInt(node.Anchor), newNodes);
        }

        public void NodeRemoved(INavNode node) {
            node.NavNodeLinks.ToList().ForEach( otherLink => {
                otherLink.LinkedNavNode.NavNodeLinks.ToList().RemoveAll( link => link.LinkedNavNode.Equals(node));
            });
            AllNodes.Remove(Vector3Int.RoundToInt(node.Anchor));
        }

        public void NodesRemoved(Vector2Int position) {
            foreach (INavNode node in GetNodes(position))
            {
                NodeRemoved(node);
            }
        }

        public IList<INavNode> GetLinkedNodes(INavNode targetNode, int iterations) {
            List<INavNode> returnNodes = new List<INavNode>();
            IList<NavNodeLink> currentLinks = targetNode.NavNodeLinks;
            for (int i = 0; i < iterations; i++)
            {
                List<NavNodeLink> newLinks = new List<NavNodeLink>();
                currentLinks.ToList().ForEach( link => {
                    if (!returnNodes.Contains(link.LinkedNavNode)) {
                        returnNodes.Add(link.LinkedNavNode);
                        newLinks.AddRange(link.LinkedNavNode.NavNodeLinks);
                    }
                });
                currentLinks = newLinks;
            }
            return returnNodes;
        }

        public List<INavNode> GetNodesInRadius(Vector3 position, int radius) {
            List<INavNode> nodes = new List<INavNode>();
            Vector3Int roundedPosition = Vector3Int.RoundToInt(position);
            for (int y = -radius; y <= radius; y++) {
                for (int x = -radius; x <= radius; x++) {
                    if ((x * x) + (y * y) <= radius * radius) {
                        nodes.AddRange(GetNodes(new Vector2Int(x + roundedPosition.x,y + roundedPosition.z)));
                    }
                }
            }
            return nodes;
        }

        public IEnumerable<INavNode> GetNeighborNodes(INavNode node) {
            List<INavNode> nodes = new List<INavNode>();
            Vector3Int roundedAnchor = Vector3Int.RoundToInt(node.Anchor);
            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
                    nodes.AddRange(GetNodes(new Vector2Int(x + roundedAnchor.x,y + roundedAnchor.z)));
                }
            }
            return nodes;
        }


        public IEnumerable<INavNode> GetNodes(Vector2Int position) {
            Debug.Log(AllNodes.Where(entry => entry.Key.x == position.x).Where(entry => entry.Key.z == position.y).SelectMany(entry => entry.Value).Count());
            return AllNodes.Where(node => node.Key.x == position.x).Where(node => node.Key.z == position.y).SelectMany(kvp => kvp.Value);
        }

        public IEnumerable<INavNode> GetNodes(Vector3Int position) {
            return GetNodes(new Vector2Int(position.x, position.z));
        }

        private float GetDistance(Vector3 from, Vector3 to)
        {
            Vector3 dist = from - to;
            float distX = Mathf.Abs(dist.x);
            float distY = Mathf.Abs(dist.y);
            float distZ = Mathf.Abs(dist.z);
            if (distX > distZ)
            {
                return DIAGONAL_COST * distZ + STRAIGHT_COST * (distX - distZ) + STRAIGHT_COST * distY;
            }

            return DIAGONAL_COST * distX + STRAIGHT_COST * (distZ - distX) + STRAIGHT_COST * distY;
        }
    }

}
