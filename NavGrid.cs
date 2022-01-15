using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace AStarSquares
{
    public class NavGrid : MonoBehaviour
    {
        public List<INavNode> AllNodes { get; set; } = new List<INavNode>();


        #if UNITY_EDITOR
        [SerializeField] private AnimationCurve displayCurve = new AnimationCurve(new Keyframe(0, 0,0,5), new Keyframe(.5f, .5f), new Keyframe(1, 0,-5,0));
        
        private void OnDrawGizmos() {
            foreach (INavNode node in AllNodes) 
            {
                foreach (NavNodeLink link in node.NavNodeLinks)
                {
                    Gizmos.color = Color.blue;
                    Vector3 lastPosition = node.Anchor;
                    float lastOffset = 0;
                    for (float i = .1f; i <= .9f; i+= .1f)
                    {
                        Vector3 nextPosition = Vector3.Lerp(node.Anchor, link.LinkedNavNode.Anchor, i);
                        float offset = displayCurve.Evaluate(i);
                        Gizmos.DrawLine(lastPosition + Vector3.up * lastOffset, nextPosition + Vector3.up * offset);
                        lastOffset = offset;
                        lastPosition = nextPosition;
                    }
                }
            }
        }
        #endif

        private void Start() {
            AllNodes.AddRange(GetComponentsInChildren<INavNode>());
            StartCoroutine(SetNodeLinksBatched(AllNodes,20));
        }

        IEnumerator SetNodeLinksBatched(List<INavNode> navNodes, int maxBatch) {
            for (int i = 0; i < navNodes.Count; i+=maxBatch)
            {
                int range = i + maxBatch > navNodes.Count ? navNodes.Count-i : maxBatch;
                foreach (INavNode node in navNodes.GetRange(i,range))
                {
                    SetNodeLinks(node);
                }
                yield return null;
            }
        }

        private void SetNodeLinks(INavNode node) {
            GetJumpableNodes(node.Anchor, 2).ToList().ForEach( neighborNode => {
                int distance = NavExtensions.GetDistance(node.Anchor, neighborNode.Anchor);
                int vertical = node.Anchor.y - neighborNode.Anchor.y;
                if (!node.NavNodeLinks.Any( node => node.LinkedNavNode == neighborNode)) {
                    node.NavNodeLinks.Add(new NavNodeLink(neighborNode, distance, -vertical));
                }
                if (!neighborNode.NavNodeLinks.Any( neighborNode => neighborNode.LinkedNavNode == node)) {
                    neighborNode.NavNodeLinks.Add(new NavNodeLink(node, distance, vertical));
                }
            });
        }

        public void NodeRemoved(INavNode node) {
            node.NavNodeLinks.ToList().ForEach( otherLink => {
                otherLink.LinkedNavNode.NavNodeLinks.ToList().RemoveAll( link => link.LinkedNavNode.Equals(node));
            });
            AllNodes.Remove(node);
        }

        public void NodesRemoved(Vector2Int position) {
            foreach (INavNode node in GetNodes(position))
            {
                NodeRemoved(node);
            }
        }

        public IList<INavNode> GetLinkedNodes(INavNode targetNode, int iterations) {
            List<INavNode> returnNodes = new List<INavNode>();
            if (targetNode == null) { return returnNodes; }

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

        public List<INavNode> GetNodesInRadius(Vector3Int position, int radius) {
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

        private List<INavNode> GetJumpableNodes(Vector3Int position, int radius) {
            List<INavNode> nodes = new List<INavNode>();

            for (int z = -radius; z <= radius; z++) {
                for (int x = -radius; x <= radius; x++) {
                    if ((z != 0 || x != 0) && (x * x) + (z * z) <= radius * radius) {
                        IEnumerable<INavNode> newNodes = GetNodes(new Vector2Int(x + position.x,z + position.z));
                        newNodes.ToList().ForEach( possibleNode => {
                            Vector3Int toPoint;
                            Vector3Int fromPoint;
                            int sign;
                            if (possibleNode.Anchor.y > position.y) {
                                toPoint = position;
                                fromPoint = possibleNode.Anchor;
                                sign = 1;
                            } else {
                                toPoint = possibleNode.Anchor;
                                fromPoint = position;
                                sign = -1;
                            }

                            int jumpCheckX = x * sign;
                            int jumpCheckY = fromPoint.y + 1;
                            int jumpCheckZ = z * sign;
                            bool foundBlock = false;

                            while ( jumpCheckX != 0 || jumpCheckZ != 0 || jumpCheckY != toPoint.y + 1) {
                                jumpCheckX = MoveTowards(jumpCheckX, 0, 1);
                                jumpCheckZ = MoveTowards(jumpCheckZ, 0, 1);

                                Vector3Int jumpCheckPos = new Vector3Int(toPoint.x + jumpCheckX, jumpCheckY, toPoint.z + jumpCheckZ);
                                if (AllNodes.Any(node => node.Anchor == jumpCheckPos)){
                                    foundBlock = true;
                                    break;
                                }
                                jumpCheckY = MoveTowards(jumpCheckY, toPoint.y + 1, 1);
                            }
                            if (!foundBlock) {
                                nodes.Add(possibleNode);
                            }
                        });
                    }
                }
            }
            return nodes;
        }

        public IEnumerable<INavNode> GetNeighborNodes(INavNode node) {
            List<INavNode> nodes = new List<INavNode>();

            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
                    nodes.AddRange(GetNodes(new Vector2Int(x + node.Anchor.x,y + node.Anchor.z)));
                }
            }
            return nodes;
        }


        private IEnumerable<INavNode> GetNodes(Vector2Int position) {
            return AllNodes.Where(node => node.Anchor.x == position.x).Where(node => node.Anchor.z == position.y);
        }

        public IEnumerable<INavNode> GetNodes(Vector3Int position) {
            return AllNodes.Where( it => it.Anchor == position);
        }

        private int MoveTowards(int current, int target, int delta) {
            if (Math.Abs(target-current) <= delta) {
                return target;
            }
            return (current + Math.Sign(target-current) * delta);
        }
    }

}
