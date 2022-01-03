using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using AStarSquares;


public class ClickTest : MonoBehaviour
{

	[SerializeField] private NavGrid navGrid;
	[SerializeField] private INavNode selectedNode;
	[SerializeField] private INavNode lastNode;
	private IPathFinder pathFinder = new PathFinder();
	[SerializeField] private NavPath path;
	[SerializeField] private IList<INavNode> availableNodes = new List<INavNode>();

	[SerializeField] private NavActor selectedActor;


	[SerializeField] private IList<NavPath> currentPaths;

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

		if (path != null && path.PathNodes.Count > 0) {
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

	public void NodeSelected(GameObject go) {
		if (go.TryGetComponent<INavNode>( out INavNode node)) {
			NodeSelected(node);
		}
	}


	public void ActorSelected(GameObject go) {
		if (go.TryGetComponent<NavActor>(out NavActor actor)) {
			ActorSelected(actor);
		}
	}

	public void ActorSelected(NavActor actor) {
		selectedActor = actor;
	}
	public void NodeSelected(INavNode node) {
		foreach (INavNode availableNode in availableNodes)
		{			
			ExecuteEvents.Execute<ITargetable>(availableNode.gameObject, null, (x,y)=> x.OnDetarget());
		}
		// ExecuteEvents.ExecuteHierarchy<ISelectable>(node.gameObject, null, (x,y)=>x.OnSelect());
		if ( !selectedActor ) {
			return;
		}


		if (selectedActor.CurrentNode != null) {
			if (currentPaths != null) {
				NavPath movePath = currentPaths.FirstOrDefault( path => path.PathNodes.LastOrDefault().NavLink?.LinkedNavNode == node);
				if (movePath != null) {
					// StartCoroutine(selectedActor.TravelPath(movePath));
					StartCoroutine(MoveActor(movePath, selectedActor));
					return;
				}
			}


			currentPaths = selectedActor.GetAvailablePaths(navGrid);
			foreach (NavPath path in currentPaths) {
				if (path.PathNodes.Count < 1) continue;
				ExecuteEvents.ExecuteHierarchy<ITargetable>(path.PathNodes.Last().NavLink.LinkedNavNode.gameObject, null, (x,y)=> x.OnTarget());
			}

			// availableNodes = navGrid.GetLinkedNodes(selectedActor.CurrentNode, 2);
			// foreach (INavNode availableNode in availableNodes)
			// {			
			// 	ExecuteEvents.ExecuteHierarchy<ITargetable>(availableNode.gameObject, null, (x,y)=> x.OnTarget());
			// }
			// path = pathFinder.FindPath(selectedActor.CurrentNode, node, availableNodes, 1, 0);
			// StartCoroutine(selectedActor.TravelPath(path));
		} else {
			selectedActor.CurrentNode = node;
			selectedActor.transform.position = node.Anchor + Vector3.up * .4f;
		}
	}


	public IEnumerator MoveActor(NavPath path, NavActor actor) {
		yield return actor.TravelPath(path);
		foreach (NavPath oldPath in currentPaths) {
			if (oldPath.PathNodes.Count < 1) continue;
			ExecuteEvents.ExecuteHierarchy<ITargetable>(oldPath.PathNodes.Last().NavLink.LinkedNavNode.gameObject, null, (x,y)=> x.OnDetarget());
		}
		currentPaths = actor.GetAvailablePaths(navGrid);
		foreach (NavPath newPath in currentPaths) {
			if (newPath.PathNodes.Count < 1) continue;
			ExecuteEvents.ExecuteHierarchy<ITargetable>(newPath.PathNodes.Last().NavLink.LinkedNavNode.gameObject, null, (x,y)=> x.OnTarget());
		}
		
		yield return null;
	}
}
