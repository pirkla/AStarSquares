
using UnityEngine;
using UnityEngine.EventSystems;


namespace AStarSquares
{
    
    public class SelectionSquare : MonoBehaviour, ISelectable, ITargetable {
        SpriteRenderer sprite;
        private void Start() {
            sprite = GetComponentInChildren<SpriteRenderer>();
        }
        public void OnSelect() {
            sprite.enabled = true;
            sprite.color = Color.green;
        }
        public void OnDeselect() {
            sprite.enabled = false;
        }

        public void OnTarget() {
            sprite.enabled = true;
            sprite.color = Color.white;
        }

        public void OnDetarget() {
            sprite.enabled = false;
        }
    }

    public interface ISelectable: IEventSystemHandler {
        void OnSelect();
        void OnDeselect();
    }
    public interface ITargetable: IEventSystemHandler {
        void OnTarget();
        void OnDetarget();
    }
}