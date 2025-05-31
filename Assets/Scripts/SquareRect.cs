using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SquareRect : MonoBehaviour
{
    void Update()
    {
        RectTransform rt = GetComponent<RectTransform>();
        float size = Mathf.Min(rt.rect.width, rt.rect.height);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
    }
}

