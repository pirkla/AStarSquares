
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;


namespace AStarSquares
{
    
    public class SelectionSquare : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ITargetable {
        SpriteRenderer sprite;
        Animator animator;

        public IList<UnityAction> ClickListeners { get; set; } = new List<UnityAction>();
        public IList<UnityAction> PointerEnterListeners { get; set; } = new List<UnityAction>();
        public IList<UnityAction> PointerExitListeners { get; set; } = new List<UnityAction>();

        private void Start() {
            sprite = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
        }

        public void OnTarget(Color color, bool shouldAnimate = false) {
            sprite.enabled = true;
            sprite.color = color;
            animator.SetBool("shouldAnimate", shouldAnimate);
        }

        public void OnDetarget() {
            sprite.enabled = false;
            animator.SetBool("shouldAnimate", false);
        }

        public void OnPointerClick(PointerEventData eventData) {
            RunClickListeners();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            RunPointerEnterListeners();
        }

        public void OnPointerExit(PointerEventData eventData) {
            RunPointerExitListeners();
        }

        public void RunClickListeners() {
            foreach (UnityAction action in ClickListeners.ToList())
            {
                action.Invoke();
            }
        }

        public void RunPointerEnterListeners() {
            foreach (UnityAction action in PointerEnterListeners.ToList())
            {
                action.Invoke();
            }
        }

        public void RunPointerExitListeners() {
            foreach (UnityAction action in PointerExitListeners.ToList())
            {
                action.Invoke();
            }
        }


        public void AddClickListener(UnityAction call) {
            ClickListeners.Add(call);
        }
        public void RemoveClickListener(UnityAction call) {
            ClickListeners.Remove(call);
        }

        public void ClearClickListeners() {
            ClickListeners.Clear();
        }
    }

    public interface ITargetable: IEventSystemHandler {
        IList<UnityAction> ClickListeners { get; set; }
        IList<UnityAction> PointerEnterListeners { get; set; }
        IList<UnityAction> PointerExitListeners { get; set; }
        
        // void RemoveClickListener(UnityAction call);
        // void AddClickListener(UnityAction call);
        // void ClearClickListeners();
        void OnTarget(Color color, bool shouldAnimate = false);
        void OnDetarget();

        // void RunClickListeners();
    }
}