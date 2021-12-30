using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AStarSquares
{
    public class NavNode : MonoBehaviour, IPointerClickHandler, INavNode
    {
        public bool IsWalkable = true;

        public int MovePenalty { get; set; } = 0;

        public IEnumerable<INavNode> LinkedNavNodes { get; } = new List<INavNode>();

        public Vector3Int Anchor => Vector3Int.RoundToInt(transform.TransformPoint(new Vector3(-0.5f, 1, 0.5f)));


        private ClickTest[] targets;

        #if UNITY_EDITOR
        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(Anchor, 0.1f);
        }
        #endif


        private void Start() {
            targets = FindObjectsOfType<ClickTest>();   
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            foreach (ClickTest target in targets)
            {
                ExecuteEvents.Execute<INodeTarget>(target.gameObject, null, (x,y)=>x.NodeSelected(this));
            }
        }
    }
}
