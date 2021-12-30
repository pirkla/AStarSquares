using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AStarSquares
{
    public class NavGrid : MonoBehaviour
    {
        public Dictionary<Vector3Int, INavNode> AllNodes { get; private set;}

        private void Start() {
            AllNodes = GetComponentsInChildren<INavNode>().ToDictionary(node => node.Anchor);
        }

        public void NodeAdded(INavNode node) {
            AllNodes.Add(node.Anchor, node);
        }

        public void NodeRemoved(INavNode node) {
            AllNodes.Remove(node.Anchor);
        }

        public void NodesRemoved(Vector2Int position) {
            foreach (INavNode node in GetNodes(position))
            {
                NodeRemoved(node);
            }
        }

        public IEnumerable<INavNode> GetNodesInRadius(Vector3Int position, int radius) {
            List<INavNode> nodes = new List<INavNode>();

            for (int y = -radius; y <= radius; y++) {
                for (int x = -radius; x <= radius; x++) {
                    if ((x * x) + (y * y) <= radius * radius) {
                        nodes.AddRange(GetNodes(new Vector2Int(x + position.x,y + position.z)));
                    }
                }
            }
            return nodes;
        }


        public IEnumerable<INavNode> GetNodes(Vector2Int position) {
            return AllNodes.Where(node => node.Key.x == position.x).Where(node => node.Key.z == position.y).Select(kvp => kvp.Value);
        }

        public IEnumerable<INavNode> GetNodes(Vector3Int position) {
            return GetNodes(new Vector2Int(position.x, position.z));
        }
    }

}
