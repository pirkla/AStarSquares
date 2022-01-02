using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStarSquares;
using System.Linq;


namespace AStarSquares
{
    public class NavActor : MonoBehaviour
    {
        public INavNode CurrentNode;

        [SerializeField] private AnimationCurve jumpCurve = new AnimationCurve(new Keyframe(0, 0,0,5), new Keyframe(.5f, 1), new Keyframe(1, 0,-5,0));

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public IEnumerator TravelPath(NavPath path) {
            foreach (PathNode pathNode in path.PathNodes)
            {
                if (pathNode.Distance > 14) {
                    yield return JumpToPoint(pathNode.Location.asVector3() + Vector3.up * .4f, 1,1);
                }
                yield return MoveToPoint(pathNode.Location.asVector3() + Vector3.up * .4f,1);
                // CurrentNode = pathNode.NavLink.LinkedNavNode;
            }
            // CurrentNode.OccupyingActor = this;
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
    }

}
