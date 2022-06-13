using System.Collections.Generic;
using UnityEngine;

public class ColliderHelper : MonoBehaviour
{
    public static Vector2[] GetBoxColliderVertices2D(BoxCollider2D collider)
    {
        Mesh colliderMesh = collider.CreateMesh(true, true);
        Vector2[] vertices = new Vector2[colliderMesh.vertexCount];
        for (int i = 0; i < colliderMesh.vertexCount; i++)
        {
            if (i % 2 == 0)
            {
                vertices[i / 2] = new Vector2(colliderMesh.vertices[i].x, colliderMesh.vertices[i].y);
            }
            else
            {
                vertices[(colliderMesh.vertexCount + i) / 2] = new Vector2(colliderMesh.vertices[i].x, colliderMesh.vertices[i].y);
            }
        }
        return vertices;
    }

    public static Vector2[] GetCapsuleColliderVertices2D(CapsuleCollider2D collider)
    {
        if (
            collider.transform.rotation.eulerAngles.x != 0 ||
            collider.transform.rotation.eulerAngles.y != 0 ||
            collider.transform.rotation.eulerAngles.z != 0
        )
        {
            Debug.LogWarning("Capsule colliders cannot be rotated when getting vertices.", collider);
            return null;
        }
        Mesh colliderMesh = collider.CreateMesh(false, false);
        Vector2[] vertices = new Vector2[colliderMesh.vertexCount];
        for (int i = 0; i < colliderMesh.vertexCount; i++)
        {
            // apply transformations
            Vector2 vertex = colliderMesh.vertices[i];
            if (i % 2 == 0)
            {
                vertices[i / 2] = vertex;
            }
            else
            {
                vertices[colliderMesh.vertexCount - (i / 2) - 1] = vertex;
            }
        }
        return vertices;
    }

    public static Vector2[] GetCircleColliderVertices2D(CircleCollider2D collider)
    {
        Mesh colliderMesh = collider.CreateMesh(true, true);
        Vector2[] vertices = new Vector2[colliderMesh.vertexCount];
        for (int i = 0; i < colliderMesh.vertexCount; i++)
        {
            // apply transformations
            Vector2 vertex = colliderMesh.vertices[i] - collider.transform.position;

            if (collider.transform.lossyScale.x < 0 && collider.transform.lossyScale.y < 0)
            {
                vertex *= new Vector2(
                    collider.transform.lossyScale.x,
                    collider.transform.lossyScale.y
                );
            }
            else if (Mathf.Abs(collider.transform.lossyScale.x) < Mathf.Abs(collider.transform.lossyScale.y))
            {
                vertex *= new Vector2(
                    collider.transform.lossyScale.x / collider.transform.lossyScale.y,
                    1
                );
            }
            else
            {
                vertex *= new Vector2(
                    1,
                    collider.transform.lossyScale.y / collider.transform.lossyScale.x
                );
            }

            vertex = (collider.transform.rotation * vertex) + collider.transform.position;
            if (i % 2 == 0)
            {
                vertices[i / 2] = vertex;
            }
            else
            {
                vertices[colliderMesh.vertexCount - (i / 2) - 1] = vertex;
            }
        }
        return vertices;
    }

    public static Vector2[] GetPolygonColliderVertices2D(PolygonCollider2D collider)
    {
        Vector2[] vertices = new Vector2[collider.points.Length];
        for (int i = 0; i < collider.points.Length; i++)
        {
            // apply transformations
            vertices[i] = (collider.transform.rotation * (collider.points[i] * collider.transform.lossyScale)) + collider.transform.position;
        }
        return vertices;
    }

    public static Vector2[] GetColliderVertices2D(Collider2D collider)
    {
        if (collider as BoxCollider2D)
        {
            return GetBoxColliderVertices2D(collider as BoxCollider2D);
        }
        else if (collider as CapsuleCollider2D)
        {
            return GetCapsuleColliderVertices2D(collider as CapsuleCollider2D);
        }
        else if (collider as CircleCollider2D)
        {
            return GetCircleColliderVertices2D(collider as CircleCollider2D);
        }
        else if (collider as PolygonCollider2D)
        {
            return GetPolygonColliderVertices2D(collider as PolygonCollider2D);
        }

        Debug.LogWarning("Unsupported collider type.", collider);
        return null;
    }

    public struct CastResult2D
    {
        public bool success;
        public Vector2 ray;
        public Vector2 hitPoint;
        public Collider2D hitCollider;

        public CastResult2D(bool _success = false, Vector2 _ray = new Vector2(), Vector2 _hitPoint = new Vector2(), Collider2D _hitCollider = null)
        {
            success = _success;
            ray = _ray;
            hitPoint = _hitPoint;
            hitCollider = _hitCollider;
        }
    }

    public static CastResult2D LinesIntersect2D(Vector2 line1point1, Vector2 line1point2, Vector2 line2point1, Vector2 line2point2)
    {
        // Using the classic y=mx+b, we can set the equation for each line equal (m1x+b1=m2x+b2).
        // Then with a bit of algebra, the point which the lines cross can be found.

        CastResult2D result = new CastResult2D();
        float line1xDiff = line1point1.x - line1point2.x;
        float line2xDiff = line2point1.x - line2point2.x;
        float line1slope = (line1point1.y - line1point2.y) / line1xDiff;
        float line2slope = (line2point1.y - line2point2.y) / line2xDiff;
        float resultX = 0;
        float resultY = 0;

        // Vertical lines need to be dealt with separately because their slopes are undefined.
        if (Mathf.Approximately(line1xDiff, 0))
        {
            if (!Mathf.Approximately(line2xDiff, 0))
            {
                resultX = line1point1.x;
                resultY = (line2slope * line1point1.x) + line2point1.y - (line2slope * line2point1.x);
            }
            else
            {
                return result;
            }
        }
        else if (Mathf.Approximately(line2xDiff, 0))
        {
            resultX = line2point1.x;
            resultY = (line1slope * line2point1.x) + line1point1.y - (line1slope * line1point1.x);
        }
        else
        {
            // If the lines are parallel, their slopes will be equal and the lines will not cross.
            float slopeDiff = line1slope - line2slope;
            if (!Mathf.Approximately(slopeDiff, 0))
            {
                float b1 = line1point1.y - (line1slope * line1point1.x);
                resultX = (line2point1.y - (line2slope * line2point1.x) - b1) / slopeDiff;
                resultY = (line1slope * resultX) + b1;
            }
            else
            {
                return result;
            }
        }

        // After finding the intersection point, it needs to be tested to see if it lies between both sets of endpoints.
        float line1minX = Mathf.Min(line1point1.x, line1point2.x);
        float line1maxX = Mathf.Max(line1point1.x, line1point2.x);
        float line2minX = Mathf.Min(line2point1.x, line2point2.x);
        float line2maxX = Mathf.Max(line2point1.x, line2point2.x);
        float line1minY = Mathf.Min(line1point1.y, line1point2.y);
        float line1maxY = Mathf.Max(line1point1.y, line1point2.y);
        float line2minY = Mathf.Min(line2point1.y, line2point2.y);
        float line2maxY = Mathf.Max(line2point1.y, line2point2.y);
        if (
            (line1minX < resultX || Mathf.Approximately(line1minX, resultX)) &&
            (line1maxX > resultX || Mathf.Approximately(line1maxX, resultX)) &&
            (line2minX < resultX || Mathf.Approximately(line2minX, resultX)) &&
            (line2maxX > resultX || Mathf.Approximately(line2maxX, resultX)) &&
            (line1minY < resultY || Mathf.Approximately(line1minY, resultY)) &&
            (line1maxY > resultY || Mathf.Approximately(line1maxY, resultY)) &&
            (line2minY < resultY || Mathf.Approximately(line2minY, resultY)) &&
            (line2maxY > resultY || Mathf.Approximately(line2maxY, resultY))
        )
        {
            result.success = true;
            result.hitPoint = new Vector2(resultX, resultY);
        }

        return result;
    }

    public static bool PolygonOverlapPoint2D(Vector2[] polygon, Vector2 point)
    {
        if (polygon.Length < 3)
        {
            return false;
        }

        // A ray, starting from the point, pointing straight up (positive y), will intersect the edges of the polygon an even number of times if the point is inside the polygon.

        int crossings = 0;
        float direction = Mathf.Sign(polygon[1].x - polygon[0].x);
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 pPoint1 = polygon[i];
            Vector2 pPoint2 = polygon[(i + 1) % polygon.Length];
            CastResult2D lineCrossing = LinesIntersect2D(point, new Vector2(point.x, Mathf.Infinity), pPoint1, pPoint2);
            if (lineCrossing.success)
            {
                crossings++;
            }
        }
        return crossings % 2 == 1;
    }

    public static CastResult2D PolygonsAreTouching2D(Vector2[] polygon1, Vector2[] polygon2)
    {
        // If any edges from polygon1 cross any edges from polygon2, the two polygons are touching.
        // The only other way the polygons could touch is if one polygon is entirely inside the other.
        // In this case, only one vertex from each polygon needs to be tested for overlap.

        if (polygon2.Length > 0 && PolygonOverlapPoint2D(polygon1, polygon2[0]))
        {
            return new CastResult2D(true, default, polygon2[0]);
        }
        if (polygon1.Length > 0 && PolygonOverlapPoint2D(polygon2, polygon1[0]))
        {
            return new CastResult2D(true, default, polygon1[0]);
        }

        for (int p1PointIndex = 0; p1PointIndex < polygon1.Length; p1PointIndex++)
        {
            Vector2 p1Point1 = polygon1[p1PointIndex];
            Vector2 p1Point2 = polygon1[(p1PointIndex + 1) % polygon1.Length];

            float p1PointsSlope = (p1Point1.y - p1Point2.y) / (p1Point1.x - p1Point2.x);
            for (int p2PointIndex = 0; p2PointIndex < polygon2.Length; p2PointIndex++)
            {
                Vector2 p2Point1 = polygon2[p2PointIndex];
                Vector2 p2Point2 = polygon2[(p2PointIndex + 1) % polygon2.Length];
                CastResult2D lineCrossing = LinesIntersect2D(p1Point1, p1Point2, p2Point1, p2Point2);
                if (lineCrossing.success)
                {
                    return lineCrossing;
                }
            }
        }

        return new CastResult2D();
    }

    public static CastResult2D PolygonCast2D(Vector2[] polygon1, Vector2[] polygon2, Vector2 ray)
    {
        CastResult2D polygonsTouching = PolygonsAreTouching2D(polygon1, polygon2);
        if (polygonsTouching.success)
        {
            return new CastResult2D(true, default, polygonsTouching.hitPoint);
        }

        // The only points which could cause the cast to hit something are the vertices of both polygons.
        // Therefore, only the vertices need to be tested to see if they would be hit in the cast.

        CastResult2D result = new CastResult2D(false, ray);
        for (int p1PointIndex = 0; p1PointIndex < polygon1.Length; p1PointIndex++)
        {
            Vector2 p1Point1 = polygon1[p1PointIndex];
            Vector2 p1Point2 = polygon1[(p1PointIndex + 1) % polygon1.Length];
            for (int p2PointIndex = 0; p2PointIndex < polygon2.Length; p2PointIndex++)
            {
                Vector2 p2Point1 = polygon2[p2PointIndex];
                Vector2 p2Point2 = polygon2[(p2PointIndex + 1) % polygon2.Length];

                // cast ray from p1Point1 to the line between p2Point1 and p2Point2
                CastResult2D rayHit = LinesIntersect2D(p1Point1, p1Point1 + result.ray, p2Point1, p2Point2);
                Vector2 newRay = rayHit.hitPoint - p1Point1;
                if (rayHit.success)
                {
                    result.success = true;
                    result.ray = newRay;
                    result.hitPoint = rayHit.hitPoint;
                }

                // cast ray from p2Point1 to the line between p1Point1 and p1Point2
                rayHit = LinesIntersect2D(p1Point1, p1Point2, p2Point1, p2Point1 - result.ray);
                newRay = p2Point1 - rayHit.hitPoint;
                if (rayHit.success)
                {
                    result.success = true;
                    result.ray = newRay;
                    result.hitPoint = p2Point1;
                }
            }
        }

        return result;
    }

    public static CastResult2D ColliderCast2D(Collider2D collider1, Collider2D collider2, Vector2 ray)
    {
        CastResult2D result = PolygonCast2D(GetColliderVertices2D(collider1), GetColliderVertices2D(collider2), ray);
        result.hitCollider = collider2;
        return result;
    }

    public static CastResult2D[] PolygonCast2D(Vector2[] polygon1, Vector2[][] polygon2s, Vector2 ray)
    {
        CastResult2D[] results = new CastResult2D[polygon2s.Length];
        for (int i = 0; i < polygon2s.Length; i++)
        {
            results[i] = PolygonCast2D(polygon1, polygon2s[i], ray);
        }
        return results;
    }

    public static CastResult2D[] ColliderCast2D(Collider2D collider1, Collider2D[] collider2s, Vector2 ray, bool filter = false, bool sort = false, bool includeSelf = true)
    {
        Vector2[] vertices = GetColliderVertices2D(collider1);
        List<CastResult2D> results = new List<CastResult2D>();
        for (int i = 0; i < collider2s.Length; i++)
        {
            if (includeSelf || collider2s[i] != collider1)
            {
                CastResult2D castResult = PolygonCast2D(vertices, GetColliderVertices2D(collider2s[i]), ray);
                if (castResult.success || !filter)
                {
                    castResult.hitCollider = collider2s[i];
                    results.Add(castResult);
                }
            }
        }
        if (sort)
        {
            results.Sort(delegate (CastResult2D result1, CastResult2D result2) { return result1.ray.magnitude > result2.ray.magnitude ? 1 : -1; });
        }
        return results.ToArray();
    }

    public static Collider2D[] GetAllColliders()
    {
        List<Collider2D> colliders = new List<Collider2D>();
        GameObject[] gameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject gameObject in gameObjects)
        {
            colliders.AddRange(gameObject.GetComponentsInChildren<Collider2D>());
        }
        return colliders.ToArray();
    }

    public static CastResult2D[] ColliderCastAll2D(Collider2D collider1, Vector2 ray, bool filter = false, bool sort = false, bool includeSelf = true)
    {
        return ColliderCast2D(collider1, GetAllColliders(), ray, filter, sort, includeSelf);
    }
}
