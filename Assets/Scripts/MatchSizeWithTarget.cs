using UnityEngine;

[ExecuteAlways]
public class MatchSizeWithTarget : MonoBehaviour
{
    public RectTransform target;

    void Update()
    {
        if (target == null) return;

        RectTransform self = GetComponent<RectTransform>();
        self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target.rect.width);
        self.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, target.rect.height);
    }
}

