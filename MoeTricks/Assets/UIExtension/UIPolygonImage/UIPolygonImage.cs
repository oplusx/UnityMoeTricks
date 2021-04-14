using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PolygonCollider2D))]
public class UIPolygonImage : Image
{
    public PolygonCollider2D _polygon = null;
    [Header("Fill Data From Sprite Custom Physics Shape. Manual opreation is better.")]
    public bool autoSpritePhysics;
    public float physicsFactorY = 0.8f;
    public float physicsFactorX = 1f;
    private PolygonCollider2D polygon
    {
        get
        {
            if (_polygon == null)
                _polygon = GetComponent<PolygonCollider2D>();
            return _polygon;
        }
    }
    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, eventCamera, out worldPos);
        return polygon.OverlapPoint(new Vector2(worldPos.x,worldPos.y));
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        if(autoSpritePhysics)
        {
            GeneratePolygonFromSprite();
            SetNativeSize();
        }
    }
    public void GeneratePolygonFromSprite()
    {
        int count = sprite.GetPhysicsShapeCount();
        polygon.pathCount = count;
        List<Vector2> points = new List<Vector2>();
        for (int index = 0; index < count; ++index)
        {
            points.Clear();
            sprite.GetPhysicsShape(index, points);
            for (int posIndex = 0; posIndex < points.Count; ++posIndex)
            {
                points[posIndex] = points[posIndex] * new Vector2(rectTransform.sizeDelta.x * physicsFactorX, rectTransform.sizeDelta.y * physicsFactorY);
            }
            polygon.SetPath(index, points.ToArray());
        }
    }
#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();
        float w = (rectTransform.sizeDelta.x * 0.5f) + 0.1f;
        float h = (rectTransform.sizeDelta.y * 0.5f) + 0.1f;
        polygon.points = new Vector2[]
        {
            new Vector2(-w,-h),
            new Vector2(w,-h),
            new Vector2(w,h),
            new Vector2(-w,h)
          };
    }
#endif
}

