using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace AStarSquares
{
    public class NavNode : MonoBehaviour, IPointerClickHandler, INavNode
    {
        public bool IsWalkable = true;

        public int MovePenalty { get; set; } = 0;

        public IList<NavNodeLink> NavNodeLinks { get; set; } = new List<NavNodeLink>();

        public Vector3 Anchor => transform.TransformPoint(new Vector3(0,1,0));

        private ClickTest[] targets;

        #if UNITY_EDITOR
        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(Anchor + Vector3.up * .1f, 0.1f);
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


