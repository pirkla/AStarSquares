using UnityEngine;
using UnityEngine.Events;
using AStarSquares;

namespace AStarSquares
{
    public class GameEventListener : MonoBehaviour
    {
        public GameEvent Event;
        public ObjectSelectedEvent Response;

        private void OnEnable()
        { Event.Register(this); }

        private void OnDisable()
        { Event.Deregister(this); }

        public void OnEventRaised(GameObject go)
        { 
            Response.Invoke(go); 
        }
    }

    [System.Serializable]
    public class ObjectSelectedEvent : UnityEvent<GameObject> {

    }
}
