using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MaxRects 矩形装箱算法（BSSF：最适短边优先）。坐标以左上为原点。
/// 由 KAtlasPackerEditor 复用，仅用于编辑器图集打包，故文件以 Editor 结尾。
/// </summary>
public class MaxRectsPacker
{
    private readonly int _binW;
    private readonly int _binH;
    private readonly List<IRect> _free = new List<IRect>();
    private readonly List<IRect> _used = new List<IRect>();

    public MaxRectsPacker(int w, int h)
    {
        _binW = w;
        _binH = h;
        _free.Add(new IRect { x = 0, y = 0, w = w, h = h });
    }

    public bool Insert(int w, int h, out IRect placed)
    {
        placed = default;
        int bestIndex = -1;
        int bestShort = int.MaxValue;
        int bestLong = int.MaxValue;

        for (int i = 0; i < _free.Count; i++)
        {
            var f = _free[i];
            if (f.w >= w && f.h >= h)
            {
                int leftoverH = f.w - w;
                int leftoverV = f.h - h;
                int shortFit = System.Math.Min(leftoverH, leftoverV);
                int longFit = System.Math.Max(leftoverH, leftoverV);
                if (shortFit < bestShort || (shortFit == bestShort && longFit < bestLong))
                {
                    bestShort = shortFit;
                    bestLong = longFit;
                    bestIndex = i;
                }
            }
        }

        if (bestIndex < 0) return false;

        var chosen = _free[bestIndex];
        placed = new IRect { x = chosen.x, y = chosen.y, w = w, h = h };
        _used.Add(placed);
        SplitFreeRect(placed);
        PruneFreeRects();
        return true;
    }

    private void SplitFreeRect(IRect usedNode)
    {
        for (int i = _free.Count - 1; i >= 0; i--)
        {
            var f = _free[i];
            if (!Overlaps(f, usedNode)) continue;

            _free.RemoveAt(i);

            // 水平方向切分
            if (usedNode.x < f.x + f.w && usedNode.x + usedNode.w > f.x)
            {
                if (usedNode.y > f.y && usedNode.y < f.y + f.h)
                    _free.Add(new IRect { x = f.x, y = f.y, w = f.w, h = usedNode.y - f.y });
                if (usedNode.y + usedNode.h < f.y + f.h)
                    _free.Add(new IRect { x = f.x, y = usedNode.y + usedNode.h, w = f.w, h = f.y + f.h - (usedNode.y + usedNode.h) });
            }

            // 垂直方向切分
            if (usedNode.y < f.y + f.h && usedNode.y + usedNode.h > f.y)
            {
                if (usedNode.x > f.x && usedNode.x < f.x + f.w)
                    _free.Add(new IRect { x = f.x, y = f.y, w = usedNode.x - f.x, h = f.h });
                if (usedNode.x + usedNode.w < f.x + f.w)
                    _free.Add(new IRect { x = usedNode.x + usedNode.w, y = f.y, w = f.x + f.w - (usedNode.x + usedNode.w), h = f.h });
            }
        }
    }

    private void PruneFreeRects()
    {
        for (int i = _free.Count - 1; i >= 0; i--)
        {
            for (int j = i - 1; j >= 0; j--)
            {
                if (Contains(_free[j], _free[i]))
                {
                    _free.RemoveAt(i);
                    break;
                }
                if (Contains(_free[i], _free[j]))
                {
                    _free.RemoveAt(j);
                    if (j < i) i--;
                }
            }
        }
    }

    private static bool Overlaps(IRect a, IRect b)
    {
        return a.x < b.x + b.w && a.x + a.w > b.x &&
               a.y < b.y + b.h && a.y + a.h > b.y;
    }

    private static bool Contains(IRect outer, IRect inner)
    {
        return inner.x >= outer.x && inner.y >= outer.y &&
               inner.x + inner.w <= outer.x + outer.w &&
               inner.y + inner.h <= outer.y + outer.h;
    }

    public struct IRect
    {
        public int x, y, w, h;
    }
}
