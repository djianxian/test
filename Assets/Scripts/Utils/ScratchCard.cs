using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(RectTransform))]
public class ScratchCard : MonoBehaviour //, IBeginDragHandler, IDragHandler
{
    private static readonly int OffsetPropertyId = Shader.PropertyToID("_Offset");
    private static readonly int ScalePropertyId = Shader.PropertyToID("_Scale");
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

    internal RawImage RawImage
    {
        get
        {
            if (rawImage == null)
            {
                rawImage = GetComponent<RawImage>();
            }

            return rawImage;
        }
    }

    internal RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            return rectTransform;
        }
    }

    [Header("Brush")] public Material brushMaterial;
    public Vector2 brushScale = new Vector2(0.25f, 0.25f);
    public int interpolationMaxCount = 4;
    public int interpolationMinDistance = 8;

    [Header("MaskTexture")] public float maskTextureScale = 10f;
    public float maskTextureMaxSize = 1024f;

    private RawImage rawImage;
    private RectTransform rectTransform;
    private Texture baseTexture;
    private RenderTexture targetTexture;
    private Material brush;
    private Vector2 previousDrawPosition;

    private void Awake()
    {
        SaveBaseTexture();
    }

    private void OnEnable()
    {
        Reload();
    }

    private void OnDisable()
    {
        DisposeTargetTexture();
        DisposeBrush();
    }

    private void OnRectTransformDimensionsChange()
    {
        Reload();
    }

    // public void OnBeginDrag(PointerEventData eventData)
    // {
    //     previousDrawPosition = eventData.position;
    // }
    //
    // public void OnDrag(PointerEventData eventData)
    // {
    //     if (targetTexture == null || brush == null) return;
    //
    //     Vector2 drawPositon = eventData.position;
    //     Vector2 delta = drawPositon - previousDrawPosition;
    //     float distance = delta.magnitude;
    //     int count = Mathf.Min(interpolationMaxCount, (int)distance / interpolationMinDistance);
    //
    //     for(int i = 1; i < count; i++)
    //     {
    //         DrawAt(previousDrawPosition + delta * ((float)i / count));
    //     }
    //     DrawAt(drawPositon);
    //     previousDrawPosition = drawPositon;
    // }

    public void Restory()
    {
        ClearTargetTexture();
    }

    private void SaveBaseTexture()
    {
        if (RawImage.texture != null)
        {
            baseTexture = RawImage.texture;
        }
    }

    private void Reload()
    {
        Rect rect = RectTransform.rect;
        if (rect.width <= 0 || rect.height <= 0) return;

        AcquireTargetTexture(rect);
        CreateBrush();
    }

    private void AcquireTargetTexture(Rect rect)
    {
        float aspect = rect.height / rect.width;
        int width = (int)Mathf.Min(maskTextureMaxSize, rect.width * maskTextureScale);
        int height = (int)(width * aspect);

        if (targetTexture != null && (targetTexture.width != width || targetTexture.height != height))
        {
            DisposeTargetTexture();
        }

        targetTexture = RenderTexture.GetTemporary(width, height, -1, RenderTextureFormat.ARGB32);
        RawImage.texture = targetTexture;
        ClearTargetTexture();
    }

    private void DisposeTargetTexture()
    {
        if (targetTexture != null)
        {
            RenderTexture.ReleaseTemporary(targetTexture);
            targetTexture = null;
        }
    }

    private void ClearTargetWithColor(Color color)
    {
        RenderTexture temp = RenderTexture.active;
        RenderTexture.active = targetTexture;
        GL.Clear(true, true, color);
        RenderTexture.active = temp;
    }

    private void ClearTargetTexture()
    {
        if (baseTexture != null)
        {
            ClearTargetWithColor(Color.clear);
            Graphics.Blit(baseTexture, targetTexture, RawImage.material);
        }
        else
        {
            ClearTargetWithColor(Color.white);
        }
    }

    private void CreateBrush()
    {
        if (brushMaterial == null)
        {
            brushMaterial = Resources.Load<Material>("Materials/ScratchCard_DefaultBrush");
        }

        DisposeBrush();

        brush = new Material(brushMaterial);
    }

    private void DisposeBrush()
    {
        if (brush != null)
        {
            Destroy(brush);
            brush = null;
        }
    }

    private List<Vector2> drawPoints = new();
    private Vector2 targetPoint;

    public void DrawTo(Vector2 screenPoint, bool isClear)
    {
        if (isClear)
        {
            targetPoint = screenPoint;
            StartCoroutine(GoBack());
            return;
        }

        if (drawPoints.Count > 0)
        {
            var lastPoint = drawPoints[drawPoints.Count - 1];
            DOTween.To(() => lastPoint, vec2 =>
            {
                lastPoint = vec2;
                DrawAt(lastPoint);
                drawPoints.Add(lastPoint);
            }, screenPoint, 0.5f);
        }
        else
        {
            DrawAt(screenPoint);
            drawPoints.Add(screenPoint);
        }
    }

    IEnumerator GoBack()
    {
        Vector2 lastPoint = drawPoints[drawPoints.Count - 1];
        drawPoints.RemoveAt(drawPoints.Count - 1);
        while (!lastPoint.Equals(targetPoint))
        {
            ClearTargetTexture();
            lastPoint = drawPoints[drawPoints.Count - 1];
            drawPoints.RemoveAt(drawPoints.Count - 1);
            for (int i = 0; i < drawPoints.Count; i++)
            {
                DrawAt(drawPoints[i]);
            }

            yield return null;
        }
    }

    private void DrawAt(Vector2 screenPoint)
    {
        Vector2 point = ScreenToGraphicLocalPoint(RawImage, screenPoint);
        Vector4 offsetScale = CaculateOffsetScale(RectTransform, point, brushScale);
        brush.SetVector(OffsetPropertyId, new Vector4(offsetScale.x, offsetScale.y));
        brush.SetVector(ScalePropertyId, new Vector4(offsetScale.z, offsetScale.w));
        Graphics.Blit(brush.mainTexture, targetTexture, brush);
    }

    private Vector2 ScreenToGraphicLocalPoint(Graphic graphic, Vector2 screenPoint)
    {
        Canvas canvas = graphic.canvas.rootCanvas;
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransform rectTransform = graphic.rectTransform;

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera,
            out Vector2 localPoint)
            ? localPoint
            : default;
    }

    private Vector4 CaculateOffsetScale(RectTransform rectTransform, Vector2 localPoint, Vector2 brushScale)
    {
        Rect rect = rectTransform.rect;
        localPoint += rectTransform.pivot * rect.size;

        float aspect = rect.height / rect.width;
        float sx = brushScale.x * aspect;
        float sy = brushScale.y;
        float px = (localPoint.x / rect.width) - (0.5f * sx);
        float py = (localPoint.y / rect.height) - (0.5f * sy);

        return new Vector4(px, py, 1.0f / sx, 1.0f / sy);
    }
}