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
        
        public IDictionary<Vector3, INavNode> AllNodes { get; private set;} = new Dictionary<Vector3, INavNode>();

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
            AllNodes.Add(node.Anchor, node);
        }

        public void NodeRemoved(INavNode node) {
            node.NavNodeLinks.ToList().ForEach( otherLink => {
                otherLink.LinkedNavNode.NavNodeLinks.ToList().RemoveAll( link => link.LinkedNavNode.Equals(node));
            });
            AllNodes.Remove(node.Anchor);
        }

        public void NodesRemoved(Vector2 position) {
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

            for (int y = -radius; y <= radius; y++) {
                for (int x = -radius; x <= radius; x++) {
                    if ((x * x) + (y * y) <= radius * radius) {
                        nodes.AddRange(GetNodes(new Vector2(x + position.x,y + position.z)));
                    }
                }
            }
            return nodes;
        }

        public IEnumerable<INavNode> GetNeighborNodes(INavNode node) {
            List<INavNode> nodes = new List<INavNode>();

            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
                    nodes.AddRange(GetNodes(new Vector2(x + node.Anchor.x,y + node.Anchor.z)));
                }
            }
            return nodes;
        }


        public IEnumerable<INavNode> GetNodes(Vector2 position) {
            return AllNodes.Where(node => node.Key.x == position.x).Where(node => node.Key.z == position.y).Select(kvp => kvp.Value);
        }

        public IEnumerable<INavNode> GetNodes(Vector3 position) {
            return GetNodes(new Vector2(position.x, position.z));
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
