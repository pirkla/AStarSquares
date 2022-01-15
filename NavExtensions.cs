using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AStarSquares
{
    public static class NavExtensions {
        private const int STRAIGHT_COST = 10;
        private const int DIAGONAL_COST = 14;

        public static int GetDistance(Vector3Int a, Vector3Int b)
        {
            Vector3Int dist = a - b;
            int distX = Mathf.Abs(dist.x);
            int distY = Mathf.Abs(dist.y);
            int distZ = Mathf.Abs(dist.z); 
            if (distX > distZ)
            {
                return DIAGONAL_COST * distZ + STRAIGHT_COST * (distX - distZ) + STRAIGHT_COST * distY;
            }

            return DIAGONAL_COST * distX + STRAIGHT_COST * (distZ - distX) + STRAIGHT_COST * distY;
        }

        public static int GetFlatDistance(Vector3Int a, Vector3Int b)
        {
            Vector3Int dist = a - b;
            int distX = Mathf.Abs(dist.x);
            int distZ = Mathf.Abs(dist.z); 
            if (distX > distZ)
            {
                return DIAGONAL_COST * distZ + STRAIGHT_COST * (distX - distZ);
            }

            return DIAGONAL_COST * distX + STRAIGHT_COST * (distZ - distX);
        }

        public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve,float smoothness){
            List<Vector3> points;
            List<Vector3> curvedPoints;
            int pointsLength = 0;
            int curvedLength = 0;
            
            if(smoothness < 1.0f) smoothness = 1.0f;
            
            pointsLength = arrayToCurve.Length;
            
            curvedLength = (pointsLength*Mathf.RoundToInt(smoothness))-1;
            curvedPoints = new List<Vector3>(curvedLength);
            
            float t = 0.0f;
            for(int pointInTimeOnCurve = 0;pointInTimeOnCurve < curvedLength+1;pointInTimeOnCurve++){
                t = Mathf.InverseLerp(0,curvedLength,pointInTimeOnCurve);
                
                points = new List<Vector3>(arrayToCurve);
                
                for(int j = pointsLength-1; j > 0; j--){
                    for (int i = 0; i < j; i++){
                        points[i] = (1-t)*points[i] + t*points[i+1];
                    }
                }
                
                curvedPoints.Add(points[0]);
            }
            
            return(curvedPoints.ToArray());
        }

        public static Vector3Int[] NeighborVectors => new Vector3Int[] {
            Vector3Int.forward,
            Vector3Int.back,
            Vector3Int.left,
            Vector3Int.right
        };
        public static Vector3[] subDivideLine(IList<Vector3> line) {
            List<Vector3> subdividedLine = new List<Vector3>();
            subdividedLine.Add(line.First());
            for (int i = 0; i < line.Count-1; i++)
            {
                subdividedLine.Add(line[i]);
                subdividedLine.Add((line[i] + line[i+1])/2);
            }
            subdividedLine.Add(line.Last());
            return subdividedLine.ToArray();
        }

        //https://www.habrador.com/tutorials/interpolation/1-catmull-rom-splines/
        public static Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            //The cubic polynomial: a + b * t + c * t^2 + d * t^3
            Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

            return pos;
        }

        public static Vector3[] GetCatmullRomSpline(Vector3[] line)
        {
            List<Vector3> spline = new List<Vector3>();
            spline.Add(line.First());
            for (int i = 1; i < line.Length-2; i++)
            {
                //The 4 points we need to form a spline between p1 and p2
                Vector3 p0 = line[i-1];
                Vector3 p1 = line[i];
                Vector3 p2 = line[i+1];
                Vector3 p3 = line[i+2];

                //The start position of the line
                Vector3 lastPos = p1;

                //The spline's resolution
                //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
                float resolution = 0.2f;

                //How many times should we loop?
                int loops = Mathf.FloorToInt(1f / resolution);

                for (int l = 1; l <= loops; l++)
                {
                    //Which t position are we at?
                    float t = l * resolution;

                    //Find the coordinate between the end points with a Catmull-Rom spline
                    Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

                    //Draw this line segment
                    // Gizmos.DrawLine(lastPos, newPos);
                    spline.Add(newPos);

                    //Save this pos so we can draw the next line segment
                    lastPos = newPos;
                }
            }
            spline.Add(line.Last());
            return spline.ToArray();
        }

        public static Vector3[] GetCatmullRomSplineLoop(Vector3[] points)
        {
            List<Vector3> spline = new List<Vector3>();
            spline.Add(points.First());

            for (int i = 0; i < points.Length; i++)
            {
                //The 4 points we need to form a spline between p1 and p2
                Vector3 p0 = points[ClampListPos(points, i - 1)];
                Vector3 p1 = points[i];
                Vector3 p2 = points[ClampListPos(points, i + 1)];
                Vector3 p3 = points[ClampListPos(points, i + 2)];

                //The start position of the line
                Vector3 lastPos = p1;

                //The spline's resolution
                //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
                float resolution = 0.2f;

                //How many times should we loop?
                int loops = Mathf.FloorToInt(1f / resolution);

                for (int l = 1; l <= loops; l++)
                {
                    //Which t position are we at?
                    float t = l * resolution;

                    //Find the coordinate between the end points with a Catmull-Rom spline
                    Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

                    spline.Add(newPos);

                    //Save this pos so we can draw the next line segment
                    lastPos = newPos;
                }
            }
            return spline.ToArray();
        }

    	//Clamp the list positions to allow looping
        static int ClampListPos(Vector3[] points, int pos)
        {
            if (pos < 0)
            {
                pos = points.Length - 1;
            }

            if (pos > points.Length)
            {
                pos = 1;
            }
            else if (pos > points.Length - 1)
            {
                pos = 0;
            }

            return pos;
        }
        public static Vector3[] FindBorderPoints(IEnumerable<Vector3Int> vectorGroup) {
            List<Vector3Int> foundVectors = new List<Vector3Int>();
            HashSet<Vector3> borderPoints = new HashSet<Vector3>();

            Vector3Int start = vectorGroup.First( it => !vectorGroup.Contains(it + Vector3Int.right));
            Vector3Int nextVector = start + Vector3Int.right;
            Vector3Int lastVector = start + Vector3Int.right + Vector3Int.forward;
            Vector3Int startVector = nextVector;

            int i = 0;

            do {
                i++;

                Vector3Int forward = Vector3Int.RoundToInt(Vector3.Normalize(nextVector - lastVector));
                Vector3Int right = new Vector3Int(forward.z, forward.y, -forward.x);

                if (!vectorGroup.Contains(nextVector + right)) {
                    borderPoints.Add(nextVector - (Vector3)forward * .5f + (Vector3)right * .5f + Vector3.up * .1f);
                    lastVector = nextVector;
                    nextVector += right;
                    continue;
                } else if (!vectorGroup.Contains(nextVector + forward)) {
                    borderPoints.Add(nextVector - (Vector3)forward * .5f + (Vector3)right * .5f+ Vector3.up * .1f);
                    lastVector = nextVector;
                    nextVector += forward;
                    continue;
                } else if (!vectorGroup.Contains(nextVector - right)) {
                    borderPoints.Add(nextVector - (Vector3)forward * .5f + (Vector3)right * .5f+ Vector3.up * .1f);
                    borderPoints.Add(nextVector + (Vector3)forward * .5f + (Vector3)right * .5f+ Vector3.up * .1f);
                    borderPoints.Add(nextVector + (Vector3)forward * .5f - (Vector3)right * .5f+ Vector3.up * .1f);
                    lastVector = nextVector;
                    nextVector -= right;
                    continue;
                } else {
                    borderPoints.Add(nextVector - (Vector3)forward * .5f + (Vector3)right * .5f+ Vector3.up * .1f);
                    lastVector = nextVector;
                    nextVector -= forward;
                }
            } while (i<300 && nextVector != startVector);
            return borderPoints.ToArray();
        }
        public static HashSet<Vector3Int> FindSameYNeighbors(IEnumerable<Vector3Int> allAnchors, Vector3Int target) {
            HashSet<Vector3Int> immediateNeighbors = new HashSet<Vector3Int>();
            foreach (Vector3Int neighborVector in NavExtensions.NeighborVectors)
            {
                foreach (Vector3Int possibleNeighbor in allAnchors)
                {
                    if (possibleNeighbor == target + neighborVector) {
                        immediateNeighbors.Add(possibleNeighbor);
                    }
                }
            }
            return immediateNeighbors;
        }

        public static List<List<Vector3Int>> FindGroupedVectors(IEnumerable<Vector3Int> allAnchors) {
            List<List<Vector3Int>> vector3IntLists = new List<List<Vector3Int>>();

            List<Vector3Int> availableAnchors = allAnchors.ToList();

            foreach (Vector3Int anchor in allAnchors)
            {
                if (!availableAnchors.Contains(anchor)) continue;
                List<Vector3Int> currentGroup = new List<Vector3Int>();
                
                HashSet<Vector3Int> nextAnchors = FindSameYNeighbors(availableAnchors, anchor);
                nextAnchors.Add(anchor);

                while (nextAnchors.Count > 0) {
                    foreach (Vector3Int nextAnchor in nextAnchors.ToList())
                    {
                        availableAnchors.Remove(nextAnchor);
                        currentGroup.Add(nextAnchor);
                        nextAnchors.Remove(nextAnchor);
                        foreach (var item in FindSameYNeighbors(availableAnchors, nextAnchor))
                        {
                            nextAnchors.Add(item);
                            availableAnchors.Remove(item);
                        }
                    }
                }
                vector3IntLists.Add(currentGroup);
            }
            return vector3IntLists;
        }
    }
}