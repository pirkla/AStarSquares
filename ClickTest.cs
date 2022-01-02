using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using AStarSquares;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

public class ClickTest : MonoBehaviour, INodeTarget
{

	[SerializeField] private NavGrid navGrid;
	[SerializeField] private INavNode selectedNode;
	[SerializeField] private INavNode lastNode;
	// private IPathFinder pathFinder = new PathFinder();
	// [SerializeField] private NativeList<PathNode> path;
	[SerializeField] private IList<INavNode> availableNodes = new List<INavNode>();

	[SerializeField] private NavActor actor;

    #if UNITY_EDITOR
    private void OnDrawGizmos() {
		if (selectedNode != null)
		{
			Gizmos.color = Color.blue;
			Ray xRay = new Ray(selectedNode.Anchor, Vector3.back);
			Ray zRay = new Ray(selectedNode.Anchor, Vector3.right);
			Gizmos.DrawRay(xRay);
			Gizmos.DrawRay(zRay);
		}

		// if (path.Count() > 0) {
		// 	Gizmos.color = Color.magenta;
		// 	// Gizmos.DrawLine(lastNode.Anchor, path.PathNodes[0].NavLink.LinkedNavNode.Anchor);
		// 	for (int i = 0; i < path.Count() - 1; i++) {
		// 		Gizmos.DrawLine(path[i].Location.asVector3() + Vector3.up * .1f, path[i+1].Location.asVector3() + Vector3.up * .1f);
		// 	}
		// }

		if (availableNodes != null) {
			Gizmos.color = Color.green;
			availableNodes.ToList().ForEach( node => {
				Gizmos.DrawWireSphere(node.Anchor, .2f);
			});
		}
    }
    #endif


	void Start ()
	{
		navGrid = GetComponent<NavGrid>();
	}

	private void Update() {
		
	}

	public void NodeSelected(INavNode node) {
		if (actor.CurrentNode != null) {
			availableNodes = navGrid.GetLinkedNodes(actor.CurrentNode, 4);

            NativeMultiHashMap<int3, NavCostNodeLink> allLinks = new NativeMultiHashMap<int3, NavCostNodeLink>(availableNodes.Count(), Allocator.TempJob);
            NativeHashMap<int3, NavCostNode> allCosts = new NativeHashMap<int3, NavCostNode>(availableNodes.Count(), Allocator.TempJob);
            foreach (INavNode costNode in availableNodes)
            {
                foreach (NavNodeLink link in costNode.NavNodeLinks)
                {
                    allLinks.Add(costNode.Anchor.asInt3(), new NavCostNodeLink(){
                        Distance = link.Distance,
                        LinkedIndex = link.LinkedNavNode.Anchor.asInt3()
                    });
                }

                allCosts.TryAdd(costNode.Anchor.asInt3(), new NavCostNode(){
                    GCost = int.MaxValue,
                    Index = costNode.Anchor.asInt3(),
                    Linked = false
                });
            }

			NativeList<PathNode> resultList = new NativeList<PathNode>(Allocator.TempJob);
			FindPathJob job = new FindPathJob() {
				Start = actor.CurrentNode.Anchor.asInt3(),
				End = node.Anchor.asInt3(),
				allCosts = allCosts,
				allLinks = allLinks,
				ResultPath = resultList
			};
			job.Run();
			
			// path = job.ResultPath;

			// resultList.Dispose();
			// path = pathFinder.FindPath(actor.CurrentNode.Anchor.asInt3(), node.Anchor.asInt3(), availableNodes, 1, 0);
			List<PathNode> localPath = new List<PathNode>();
			localPath.AddRange(job.ResultPath);
			StartCoroutine(actor.TravelPath(localPath));
			resultList.Dispose();
			allLinks.Dispose();
			allCosts.Dispose();
			actor.CurrentNode = node;
		} else {
			actor.CurrentNode = node;
			actor.transform.position = node.Anchor + Vector3.up * .4f;
		}
	}
}
