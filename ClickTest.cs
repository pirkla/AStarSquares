using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using AStarSquares;


public class ClickTest : MonoBehaviour, INodeTarget
{

	[SerializeField] private NavGrid navGrid;
	[SerializeField] private INavNode selectedNode;
	[SerializeField] private INavNode lastNode;
	private IPathFinder pathFinder = new PathFinder();
	[SerializeField] private NavPath path = new NavPath();
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

		if (path.PathNodes != null && path.PathNodes.Count > 0) {
			Gizmos.color = Color.magenta;
			// Gizmos.DrawLine(lastNode.Anchor, path.PathNodes[0].NavLink.LinkedNavNode.Anchor);
			for (int i = 0; i < path.PathNodes.Count - 1; i++) {
				Gizmos.DrawLine(path.PathNodes[i].NavLink.LinkedNavNode.Anchor + Vector3.up * .1f, path.PathNodes[i+1].NavLink.LinkedNavNode.Anchor + Vector3.up * .1f);
			}
		}

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
			path = pathFinder.FindPath(actor.CurrentNode, node, availableNodes, 1, 0);
			StartCoroutine(actor.TravelPath(path));
		} else {
			actor.CurrentNode = node;
			actor.transform.position = node.Anchor + Vector3.up * .4f;
		}
	}
}
