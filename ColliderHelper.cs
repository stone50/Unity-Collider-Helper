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

    public static CastResult2D PolygonVerticalCast2D(Vector2[] polygon1, Vector2[] polygon2, float ray)
    {
        CastResult2D result = new CastResult2D(false, new Vector2(0, ray));

        float rayDirection = Mathf.Sign(ray);
        for (int p1PointIndex = 0; p1PointIndex < polygon1.Length; p1PointIndex++)
        {
            Vector2 p1Point1 = polygon1[p1PointIndex];
            Vector2 p1Point2 = polygon1[(p1PointIndex + 1) % polygon1.Length];
            for (int p2PointIndex = 0; p2PointIndex < polygon2.Length; p2PointIndex++)
            {
                Vector2 p2Point1 = polygon2[p2PointIndex];
                Vector2 p2Point2 = polygon2[(p2PointIndex + 1) % polygon2.Length];

                // cast ray from p1Point1 to the line between p2Point1 and p2Point2
                float polygon2DeltaX = p2Point1.x - p2Point2.x;
                if (polygon2DeltaX != 0)
                {
                    float polygon2Slope = (p2Point1.y - p2Point2.y) / polygon2DeltaX;
                    if (
                        p1Point1.x >= Mathf.Min(p2Point1.x, p2Point2.x) &&
                        p1Point1.x <= Mathf.Max(p2Point1.x, p2Point2.x)
                    )
                    {
                        float resultY = (polygon2Slope * p1Point1.x) + p2Point1.y - (p2Point1.x * polygon2Slope);
                        Vector2 collisionRay = new Vector2(p1Point1.x - p1Point1.x, resultY - p1Point1.y);
                        if (Mathf.Sign(collisionRay.y) == rayDirection && collisionRay.magnitude < result.ray.magnitude)
                        {
                            result.success = true;
                            result.ray = collisionRay;
                            result.hitPoint = new Vector2(p1Point1.x, resultY);
                        }
                    }
                }

                // cast ray from p2Point1 to the line between p1Point1 and p1Point2
                float polygon1DeltaX = p1Point1.x - p1Point2.x;
                if (polygon1DeltaX != 0)
                {
                    float polygon1Slope = (p1Point1.y - p1Point2.y) / polygon1DeltaX;
                    if (
                        p2Point1.x >= Mathf.Min(p1Point1.x, p1Point2.x) &&
                        p2Point1.x <= Mathf.Max(p1Point1.x, p1Point2.x)
                    )
                    {
                        float resultY = (polygon1Slope * p2Point1.x) + p1Point1.y - (p1Point1.x * polygon1Slope);
                        Vector2 collisionRay = new Vector2(0, resultY - p2Point1.y);
                        if (Mathf.Sign(collisionRay.y) == -rayDirection && collisionRay.magnitude < result.ray.magnitude)
                        {
                            result.success = true;
                            result.ray = -collisionRay;
                            result.hitPoint = p2Point1;
                        }
                    }
                }
            }
        }

        return result;
    }

    public static CastResult2D PolygonCast2D(Vector2[] polygon1, Vector2[] polygon2, Vector2 ray)
    {
        if (ray.x == 0)
        {
            return PolygonVerticalCast2D(polygon1, polygon2, ray.y);
        }

        CastResult2D result = new CastResult2D(false, ray);

        float raySlope = ray.y / ray.x;
        for (int p1PointIndex = 0; p1PointIndex < polygon1.Length; p1PointIndex++)
        {
            Vector2 p1Point1 = polygon1[p1PointIndex];
            Vector2 p1Point2 = polygon1[(p1PointIndex + 1) % polygon1.Length];
            for (int p2PointIndex = 0; p2PointIndex < polygon2.Length; p2PointIndex++)
            {
                Vector2 p2Point1 = polygon2[p2PointIndex];
                Vector2 p2Point2 = polygon2[(p2PointIndex + 1) % polygon2.Length];

                // cast ray from p1Point1 to the line between p2Point1 and p2Point2
                float polygon2DeltaX = p2Point1.x - p2Point2.x;
                if (polygon2DeltaX == 0)
                {
                    float resultY = (raySlope * p2Point1.x) + p1Point1.y - (raySlope * p1Point1.x);
                    if (
                        resultY >= Mathf.Min(p2Point1.y, p2Point2.y) &&
                        resultY <= Mathf.Max(p2Point1.y, p2Point2.y) &&
                        resultY >= Mathf.Min(p1Point1.y, p1Point1.y + ray.y) &&
                        resultY <= Mathf.Max(p1Point1.y, p1Point1.y + ray.y)
                    )
                    {
                        float resultX = p2Point1.x;
                        Vector2 collisionRay = new Vector2(resultX - p1Point1.x, resultY - p1Point1.y);
                        if (collisionRay.magnitude < result.ray.magnitude)
                        {
                            result.success = true;
                            result.ray = collisionRay;
                            result.hitPoint = new Vector2(resultX, resultY);
                        }
                    }
                }
                else
                {
                    float polygon2Slope = (p2Point1.y - p2Point2.y) / polygon2DeltaX;
                    float resultX;
                    if (polygon2Slope == raySlope)
                    {
                        resultX = Mathf.Infinity;
                    }
                    else
                    {
                        resultX = (p2Point1.y - (polygon2Slope * p2Point1.x) - p1Point1.y + (p1Point1.x * raySlope)) / (raySlope - polygon2Slope);
                    }
                    if (
                        resultX >= Mathf.Min(p2Point1.x, p2Point2.x) &&
                        resultX <= Mathf.Max(p2Point1.x, p2Point2.x) &&
                        resultX >= Mathf.Min(p1Point1.x, p1Point1.x + ray.x) &&
                        resultX <= Mathf.Max(p1Point1.x, p1Point1.x + ray.x)
                    )
                    {
                        float resultY = (raySlope * resultX) + p1Point1.y - (p1Point1.x * raySlope);
                        Vector2 collisionRay = new Vector2(resultX - p1Point1.x, resultY - p1Point1.y);
                        if (collisionRay.magnitude < result.ray.magnitude)
                        {
                            result.success = true;
                            result.ray = collisionRay;
                            result.hitPoint = new Vector2(resultX, resultY);
                        }
                    }
                }

                // cast ray from p2Point1 to the line between p1Point1 and p1Point2
                float polygon1DeltaX = p1Point1.x - p1Point2.x;
                if (polygon1DeltaX == 0)
                {
                    float resultY = (raySlope * p1Point1.x) + p2Point1.y - (raySlope * p2Point1.x);
                    if (
                        resultY >= Mathf.Min(p1Point1.y, p1Point2.y) &&
                        resultY <= Mathf.Max(p1Point1.y, p1Point2.y) &&
                        resultY >= Mathf.Min(p2Point1.y, p2Point1.y - ray.y) &&
                        resultY <= Mathf.Max(p2Point1.y, p2Point1.y - ray.y)
                    )
                    {
                        float resultX = p1Point1.x;
                        Vector2 collisionRay = new Vector2(resultX - p2Point1.x, resultY - p2Point1.y);
                        if (collisionRay.magnitude < result.ray.magnitude)
                        {
                            result.success = true;
                            result.ray = -collisionRay;
                            result.hitPoint = p2Point1;
                        }
                    }
                }
                else
                {
                    float polygon1Slope = (p1Point1.y - p1Point2.y) / polygon1DeltaX;
                    float resultX;
                    if (polygon1Slope == raySlope)
                    {
                        resultX = Mathf.Infinity;
                    }
                    else
                    {
                        resultX = (p1Point1.y - (polygon1Slope * p1Point1.x) - p2Point1.y + (p2Point1.x * raySlope)) / (raySlope - polygon1Slope);
                    }
                    if (
                        resultX >= Mathf.Min(p1Point1.x, p1Point2.x) &&
                        resultX <= Mathf.Max(p1Point1.x, p1Point2.x) &&
                        resultX >= Mathf.Min(p2Point1.x, p2Point1.x - ray.x) &&
                        resultX <= Mathf.Max(p2Point1.x, p2Point1.x - ray.x)
                    )
                    {
                        float resultY = (raySlope * resultX) + p2Point1.y - (p2Point1.x * raySlope);
                        Vector2 collisionRay = new Vector2(resultX - p2Point1.x, resultY - p2Point1.y);
                        if (collisionRay.magnitude < result.ray.magnitude)
                        {
                            result.success = true;
                            result.ray = -collisionRay;
                            result.hitPoint = p2Point1;
                        }
                    }
                }
            }
        }

        return result;
    }

    public static CastResult2D ColliderVerticalCast2D(Collider2D collider1, Collider2D collider2, float ray)
    {
        CastResult2D result = PolygonVerticalCast2D(GetColliderVertices2D(collider1), GetColliderVertices2D(collider2), ray);
        result.hitCollider = collider2;
        return result;
    }

    public static CastResult2D ColliderCast2D(Collider2D collider1, Collider2D collider2, Vector2 ray)
    {
        CastResult2D result = PolygonCast2D(GetColliderVertices2D(collider1), GetColliderVertices2D(collider2), ray);
        result.hitCollider = collider2;
        return result;
    }

    public static CastResult2D[] PolygonVerticalCast2D(Vector2[] polygon1, Vector2[][] polygon2s, float ray)
    {
        CastResult2D[] results = new CastResult2D[polygon2s.Length];
        for (int i = 0; i < polygon2s.Length; i++)
        {
            results[i] = PolygonVerticalCast2D(polygon1, polygon2s[i], ray);
        }
        return results;
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

    public static CastResult2D[] ColliderVerticalCast2D(Collider2D collider1, Collider2D[] collider2s, float ray, bool filter = false, bool sort = false, bool includeSelf = true)
    {
        Vector2[] vertices = GetColliderVertices2D(collider1);
        List<CastResult2D> results = new List<CastResult2D>();
        for (int i = 0; i < collider2s.Length; i++)
        {
            if (includeSelf || collider2s[i] != collider1)
            {
                CastResult2D castResult = PolygonVerticalCast2D(vertices, GetColliderVertices2D(collider2s[i]), ray);
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

    public static CastResult2D[] ColliderVerticalCastAll2D(Collider2D collider1, float ray, bool filter = false, bool sort = false, bool includeSelf = true)
    {
        return ColliderVerticalCast2D(collider1, GetAllColliders(), ray, filter, sort, includeSelf);
    }

    public static CastResult2D[] ColliderCastAll2D(Collider2D collider1, Vector2 ray, bool filter = false, bool sort = false, bool includeSelf = true)
    {
        return ColliderCast2D(collider1, GetAllColliders(), ray, filter, sort, includeSelf);
    }
}
