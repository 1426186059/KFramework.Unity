using System;
using UnityEngine;
using UnityEngine.UI;

public static class KTweenExtensions
{
    /// <summary>打字机效果（替代 DOTween.DOText）</summary>
    public static KTween.TweenItem DOText(Text text, string content, float duration)
    {
        int totalChars = content.Length;
        return KTween.AddTween(text.gameObject, duration, fPercent =>
        {
            int charsToShow = Mathf.FloorToInt(fPercent * totalChars);
            text.text = content.Substring(0, Mathf.Clamp(charsToShow, 0, totalChars));
        }, null);
    }

    /// <summary>Vector2 值动画（替代 DOTween.To）</summary>
    public static KTween.TweenItem To(Func<Vector2> getter, Action<Vector2> setter, Vector2 to, float duration, GameObject target = null)
    {
        var go = target ?? new GameObject("SimpleTweenToRunner");
        Vector2 from = getter();
        var item = KTween.AddTween(go, duration, fPercent =>
        {
            setter(Vector2.Lerp(from, to, fPercent));
        }, null);
        if (target == null)
            item.SetOnCompleteFunc(() => UnityEngine.Object.Destroy(go));
        return item;
    }

    /// <summary>Image 颜色渐变动画（简化 SimpleTweenEx.color 调用）</summary>
    public static KTween.TweenItem DOColor(Image image, Color to, float duration)
    {
        return KTweenEx.color(image, to, duration);
    }

    /// <summary>沿路径点移动（多段线性，KTweenEx 无此功能）</summary>
    public static KTween.TweenItem DOMovePath(GameObject obj, Vector3[] path, float duration)
    {
        return KTweenEx.moveBezier(obj, path, duration);
    }

    public static KTween.TweenItem DOMoveLocalPath(GameObject obj, Vector3[] path, float duration)
    {
        return KTweenEx.moveLocalBezier(obj, path, duration);
    }

}
