using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;


namespace AStarSquares
{
    public class NavNode : MonoBehaviour, IPointerClickHandler, INavNode
    {
        [SerializeField] GameEvent onClicked;


        public bool IsWalkable = true;

        public int MovePenalty => movePenalty;
        [SerializeField]
        private int movePenalty = 0;

        public IList<NavNodeLink> NavNodeLinks { get; set; } = new List<NavNodeLink>();

        public Vector3Int Anchor => Vector3Int.RoundToInt(transform.TransformPoint(new Vector3(0, 1, 0)));

        public NavActor OccupyingActor { get; set; }
        public NavActor PassthroughActor { get; set; }


        private IEnumerable<GameObject> targets;

        #if UNITY_EDITOR
        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(Anchor, 0.1f);
        }
        #endif


        private void Start() {
            targets = FindObjectsOfType<ClickTest>().Select( x => x.gameObject );   
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            onClicked.Invoke(gameObject);
        }
    }
}


