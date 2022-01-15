using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStarSquares;
using System.Linq;
using UnityEngine.EventSystems;


namespace AStarSquares
{
    public class NavActor : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] GameEvent onClicked;

        [SerializeField] public int MovementPoints = 50;
        public INavNode CurrentNode;

        private PathFinder pathFinder = new PathFinder();

        private Vector3Int anchor => Vector3Int.RoundToInt(transform.TransformPoint(new Vector3(.5f, 0, .5f)));

        [SerializeField] private AnimationCurve jumpCurve = new AnimationCurve(new Keyframe(0, 0,0,5), new Keyframe(.5f, 1), new Keyframe(1, 0,-5,0));

        public void GuessCurrentNode() {
            NavGrid grid = FindObjectOfType<NavGrid>();
            CurrentNode = grid.GetNodes(anchor).FirstOrDefault();
        }

        public IEnumerator TravelPath(NavPath path, float speed) {
            foreach (PathNode pathNode in path.PathNodes)
            {
                if (pathNode.NavLink.Distance > 14) {
                    yield return JumpToPoint(pathNode.NavLink.LinkedNavNode.Anchor + Vector3.up * .4f, 1,3);
                }
                yield return MoveToPoint(pathNode.NavLink.LinkedNavNode.Anchor + Vector3.up * .4f,speed);
                CurrentNode = pathNode.NavLink.LinkedNavNode;
            }
            CurrentNode.OccupyingActor = this;
            yield return null;
        }

        public IEnumerator MoveToPoint (Vector3 target, float speed){
            Vector3 nextPosition = transform.position;
            Vector3 currentPosition = transform.position;

            while (currentPosition != target)
            {
                currentPosition = nextPosition;
                nextPosition = Vector3.MoveTowards(currentPosition, target, speed * Time.deltaTime);
                transform.position = nextPosition;
                yield return new WaitForEndOfFrame();
            }
            transform.position = target;
            yield return null;
        }



        public IEnumerator JumpToPoint(Vector3 target, float jumpHeight, float speed) {
            Vector2 startXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 targetXZ = new Vector2(target.x, target.z);
            Vector2 currentXZ = startXZ;
            Vector2 nextXZ = startXZ;
            float totalXZDistance = Vector2.Distance(startXZ, targetXZ);
            float startY = transform.position.y;
            float targetY = target.y;

            float forceEnd = 0;

            while (currentXZ != targetXZ || forceEnd > 4) {
                forceEnd += Time.deltaTime;
                currentXZ = nextXZ;
                nextXZ = Vector2.MoveTowards(currentXZ, targetXZ, speed * Time.deltaTime);
                float nextXZDistToTarget = Vector3.Distance(nextXZ, targetXZ);
                float nextXZNormalized = nextXZDistToTarget/totalXZDistance;
                float nextY = Mathf.Lerp(targetY, startY, nextXZNormalized);
                float curveMod = jumpCurve.Evaluate(nextXZNormalized);
                transform.position = new Vector3(nextXZ.x, nextY + curveMod * jumpHeight, nextXZ.y);
                yield return new WaitForEndOfFrame();
            }
            transform.position = target;
            yield return null;
        }
        public void OnPointerClick(PointerEventData pointerEventData)
        {
            onClicked.Invoke(gameObject);
        }
        public IList<NavPath> GetAvailablePaths(NavGrid grid) {
            List<NavPath> paths = new List<NavPath>();
            IList<INavNode> possibleNodes = grid.GetLinkedNodes(CurrentNode, MovementPoints/10);
            possibleNodes.ToList().ForEach( node => {
                NavPath newPath = pathFinder.FindPath(CurrentNode, node, possibleNodes, 40,1,-3);
                if (newPath.TotalCost < MovementPoints) {
                    paths.Add(newPath);
                }
            });
            return paths;
        }

        public IList<NavPath> GetAvailableRunPaths(NavGrid grid) {
            List<NavPath> paths = new List<NavPath>();
            IList<INavNode> possibleNodes = grid.GetLinkedNodes(CurrentNode, MovementPoints * 2/10);
            possibleNodes.ToList().ForEach( node => {
                NavPath newPath = pathFinder.FindPath(CurrentNode, node, possibleNodes, 40,1,-3);
                if (newPath.TotalCost < MovementPoints*2) {
                    paths.Add(newPath);
                }
            });
            return paths;
        }
    }



}
