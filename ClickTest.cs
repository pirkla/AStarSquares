using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using AStarSquares;
using TMPro;
using UnityEngine.UI;


public class ClickTest : MonoBehaviour
{
	[SerializeField] private LayoutGroup layoutTarget;
	[SerializeField] private GameObject buttonPrefab;
	[SerializeField] private NavGrid navGrid;
	[SerializeField] private INavNode selectedNode;
	[SerializeField] private INavNode lastNode;
	private IPathFinder pathFinder = new PathFinder();
	[SerializeField] private NavPath path;
	[SerializeField] private IList<INavNode> availableNodes = new List<INavNode>();

	[SerializeField] private NavActor selectedActor;


	[SerializeField] private IEnumerable<NavPath> currentWalkPaths = new List<NavPath>();
	[SerializeField] private IEnumerable<NavPath> currentRunPaths = new List<NavPath>();

	[SerializeField] private IList<INavNode> targetedNodes = new List<INavNode>();


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
		// selectedActor = actor;
		// updatePaths(selectedActor.GetAvailablePaths(navGrid), selectedActor.GetAvailableRunPaths(navGrid));
		
		// foreach (Ability ability in actor.GetComponent<Character>().abilities)
		// {
		// 	Button button = Instantiate(buttonPrefab, layoutTarget.transform).GetComponent<Button>();
		// 	button.GetComponent<Image>().sprite = ability.Icon;
		// 	button.GetComponentInChildren<TextMeshProUGUI>().text = ability.Text;
		// 	if (ability is IHasPaths targetAbility) {
		// 		button.onClick.RemoveAllListeners();
		// 		button.onClick.AddListener(delegate { 
		// 			HighlighPathOnClick(targetAbility, actor, navGrid);
		// 			});
		// 	}
		// 	if (ability is IHasTargets ability2) {
		// 		button.onClick.RemoveAllListeners();
		// 		button.onClick.AddListener( delegate {
		// 			HighlightTargetsOnClick(ability2.GetAvailableTargets(actor.gameObject, navGrid), ability.TargetColor);
		// 		});
		// 	}
		// 	// button.
		// }


	}

	public void HighlightTargetsOnClick(IEnumerable<GameObject> targets, Color color) {
		foreach (GameObject target in targets)
		{
			ExecuteEvents.ExecuteHierarchy<ITargetable>(target, null, (x,y)=> x.OnTarget(color));
		}
	}
	// public void HighlighPathOnClick(IHasPaths pathObject, NavActor actor, NavGrid grid) {
	// 	IEnumerable<NavPath> paths = pathObject.GetAvailablePaths(actor, grid);
	// 	IEnumerable<INavNode> nodesToHighlight = paths.Select( it => it.PathNodes.LastOrDefault().NavLink?.LinkedNavNode).Where( it => it != null);
	// 	targetNodes(nodesToHighlight, Color.white);
	// }

	public void NodeSelected(INavNode node) {
		// foreach (INavNode availableNode in availableNodes)
		// {			
		// 	ExecuteEvents.Execute<ITargetable>(availableNode.gameObject, null, (x,y)=> x.OnDetarget());
		// }

		// if ( !selectedActor ) { 
		// 	return;
		// }


		// if (selectedActor.CurrentNode != null) {
		// 	if (currentWalkPaths != null) {
		// 		NavPath walkPath = currentWalkPaths.FirstOrDefault( path => path.PathNodes.LastOrDefault().NavLink?.LinkedNavNode == node);
		// 		if (walkPath != null) {
		// 			StartCoroutine(MoveActor(walkPath, selectedActor, 1));
		// 			return;
		// 		}
		// 		NavPath runPath = currentRunPaths.FirstOrDefault( path => path.PathNodes.LastOrDefault().NavLink?.LinkedNavNode == node);
		// 		if (runPath != null) {
		// 			StartCoroutine(MoveActor(runPath, selectedActor,3));
		// 			return;
		// 		}
		// 	}
		// 	updatePaths(selectedActor.GetAvailablePaths(navGrid), selectedActor.GetAvailableRunPaths(navGrid));


		// } else {
		// 	selectedActor.CurrentNode = node;
		// 	selectedActor.transform.position = node.Anchor + Vector3.up * .4f;
		// }
	}

	private void updateWalkPaths(IEnumerable<NavPath> newPaths) {
		currentWalkPaths = newPaths;
	}

	private void updateRunPaths(IEnumerable<NavPath> newPaths) {
		currentRunPaths = newPaths;
	}
	
	private void updatePaths(IEnumerable<NavPath> walkPaths, IEnumerable<NavPath> runPaths) {
		detargetAllNodes();
		updateWalkPaths(walkPaths);
		updateRunPaths(runPaths);
		highlightPaths(currentWalkPaths, currentRunPaths);
	}

	private void highlightPaths(IEnumerable<NavPath> walkPaths, IEnumerable<NavPath> runPaths) {

		IEnumerable<INavNode> walkNodesToHighlight = walkPaths.Select( it => it.PathNodes.LastOrDefault().NavLink?.LinkedNavNode).Where( it => it != null);
		IEnumerable<INavNode> runNodesToHighlight = runPaths.Select( it => it.PathNodes.LastOrDefault().NavLink?.LinkedNavNode).Except(walkNodesToHighlight).Where( it => it != null);
		targetNodes(walkNodesToHighlight, Color.white);
		targetNodes(runNodesToHighlight, Color.yellow);
	}

	private void targetNodes(IEnumerable<INavNode> nodes, Color color) {
		foreach (INavNode node in nodes)
		{
			ExecuteEvents.ExecuteHierarchy<ITargetable>(node.gameObject, null, (x,y)=> x.OnTarget(color));
			targetedNodes.Add(node);
		}
	}

	private void detargetAllNodes() {
		foreach (INavNode node in targetedNodes)
		{
			ExecuteEvents.ExecuteHierarchy<ITargetable>(node.gameObject, null, (x,y)=> x.OnDetarget());
		}
		targetedNodes.Clear();

	}



	public IEnumerator MoveActor(NavPath path, NavActor actor, float speed) {
		yield return actor.TravelPath(path, speed);
		updatePaths(actor.GetAvailablePaths(navGrid), actor.GetAvailableRunPaths(navGrid));
		selectedActor = actor;
		yield return null;
	}
}
