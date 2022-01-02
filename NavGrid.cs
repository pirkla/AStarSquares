using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace AStarSquares
{
    public class NavGrid : MonoBehaviour
    {
        private const int STRAIGHT_COST = 10;
        private const int DIAGONAL_COST = 14;
        
        List<INavNode> AllNodes { get; set; } = new List<INavNode>();

        private void Start() {
            foreach (INavNode node in GetComponentsInChildren<INavNode>())
            {
                NodeAdded(node);
            }
        }

        public void NodeAdded(INavNode node) {
            GetNeighborNodes(node).ToList().ForEach( neighborNode => {
                int distance = GetDistance(node.Anchor, neighborNode.Anchor);
                int vertical = node.Anchor.y - neighborNode.Anchor.y;
                if (!node.NavNodeLinks.Any( node => node.LinkedNavNode == neighborNode)) {
                    node.NavNodeLinks.Add(new NavNodeLink(neighborNode, distance, -vertical));
                }
                if (!neighborNode.NavNodeLinks.Any( neighborNode => neighborNode.LinkedNavNode == node)) {
                    neighborNode.NavNodeLinks.Add(new NavNodeLink(node, distance, vertical));
                }
            });
            AllNodes.Add(node);
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



        // public void AsLinkList() {
        //     NativeMultiHashMap<int3, NavCostNodeLink> allLinks = new NativeMultiHashMap<int3, NavCostNodeLink>(allNodes.Count(), Allocator.Temp);
        //     NativeHashMap<int3, NavCostNode> allCosts = new NativeHashMap<int3, NavCostNode>(allNodes.Count(), Allocator.Temp);
        //     foreach (INavNode node in allNodes)
        //     {
        //         foreach (NavNodeLink link in node.NavNodeLinks)
        //         {
        //             allLinks.Add(node.Anchor.asInt3(), new NavCostNodeLink(){
        //                 Distance = link.Distance,
        //                 LinkedIndex = link.LinkedNavNode.Anchor.asInt3()
        //             });
        //         }

        //         allCosts.TryAdd(node.Anchor.asInt3(), new NavCostNode(){
        //             GCost = int.MaxValue,
        //             Index = node.Anchor.asInt3(),
        //             Linked = false
        //         });
        //     }
        // }

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

        private List<INavNode> GetNodesInRadius(Vector3Int position, int radius) {
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
                    if ((x * x) + (z * z) <= radius * radius) {
                        IEnumerable<INavNode> newNodes = GetNodes(new Vector2Int(x + position.x,z + position.z));
                        newNodes.ToList().ForEach( jumpCheckNode => {
                            int jumpCheckX = x;
                            int jumpCheckY = jumpCheckNode.Anchor.y + 1;
                            int jumpCheckZ = z;
                            bool foundNode = false;
                            while ( jumpCheckX > 0 || jumpCheckZ > 0 ) {
                                jumpCheckX = MoveTowards(jumpCheckX, 0, 1);
                                jumpCheckY = MoveTowards(jumpCheckY, position.y + 1, 1);
                                jumpCheckZ = MoveTowards(jumpCheckZ, 0, 1);
                                Debug.Log(new Vector3Int(jumpCheckX + position.x,jumpCheckY,jumpCheckZ + position.z));
                                if (AllNodes.Any( node => node.Anchor == new Vector3Int(jumpCheckX + position.x,jumpCheckY,jumpCheckZ + position.z))) {
                                    foundNode = true;
                                }
                            }
                            if (!foundNode) {
                                nodes.Add(jumpCheckNode);
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

        private int GetDistance(Vector3Int from, Vector3Int to)
        {
            Vector3Int dist = from - to;
            int distX = Mathf.Abs(dist.x);
            int distY = Mathf.Abs(dist.y);
            int distZ = Mathf.Abs(dist.z); 
            if (distX > distZ)
            {
                return DIAGONAL_COST * distZ + STRAIGHT_COST * (distX - distZ) + STRAIGHT_COST * distY;
            }

            return DIAGONAL_COST * distX + STRAIGHT_COST * (distZ - distX) + STRAIGHT_COST * distY;
        }

        private int MoveTowards(int current, int target, int delta) {
            if (Math.Abs(target-current) <= delta) {
                return target;
            }
            return (current + Math.Sign(target-current) * delta);
        }
    }

}
