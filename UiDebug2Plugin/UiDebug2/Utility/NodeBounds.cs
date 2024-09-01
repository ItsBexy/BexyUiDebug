using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

using static System.Math;

namespace UiDebug2.Utility;

public unsafe struct NodeBounds
{
    internal NodeBounds(AtkResNode* node)
    {
        if (node == null)
        {
            return;
        }

        var w = node->Width;
        var h = node->Height;
        this.Points = w == 0 && h == 0 ?
                     new() { new(0) } :
                     new() { new(0), new(w, 0), new(w, h), new(0, h) };

        this.TransformPoints(node);
    }

    internal NodeBounds(Vector2 point, AtkResNode* node)
    {
        this.Points = new() { point };
        this.TransformPoints(node);
    }

    internal List<Vector2> Points { get; set; } = new();

    internal static Vector2 TransformPoint(Vector2 p, Vector2 o, float r, Vector2 s)
    {
        var cosR = (float)Cos(r);
        var sinR = (float)Sin(r);
        var d = (p - o) * s;

        return new(o.X + (d.X * cosR) - (d.Y * sinR),
                   o.Y + (d.X * sinR) + (d.Y * cosR));
    }

    internal readonly void Draw(Vector4 col, int thickness = 1)
    {
        if (this.Points == null || this.Points.Count == 0)
        {
            return;
        }

        if (this.Points.Count == 1)
        {
            ImGui.GetBackgroundDrawList().AddCircle(this.Points[0], 10, Dalamud.Interface.ColorHelpers.RgbaVector4ToUint(col with { W = col.W / 2 }), 12, thickness);
            ImGui.GetBackgroundDrawList().AddCircle(this.Points[0], thickness, Dalamud.Interface.ColorHelpers.RgbaVector4ToUint(col), 12, thickness + 1);
        }
        else
        {
            var path = new ImVectorWrapper<Vector2>(this.Points.Count);
            foreach (var p in this.Points)
            {
                path.Add(p);
            }

            ImGui.GetBackgroundDrawList()
                 .AddPolyline(ref path[0], path.Length, Dalamud.Interface.ColorHelpers.RgbaVector4ToUint(col), ImDrawFlags.Closed, thickness);

            path.Dispose();
        }
    }

    internal readonly void DrawFilled(Vector4 col, int thickness = 1)
    {
        if (this.Points == null || this.Points.Count == 0)
        {
            return;
        }

        if (this.Points.Count == 1)
        {
            ImGui.GetBackgroundDrawList()
                 .AddCircleFilled(this.Points[0], 10, Dalamud.Interface.ColorHelpers.RgbaVector4ToUint(col with { W = col.W / 2 }), 12);
            ImGui.GetBackgroundDrawList().AddCircle(this.Points[0], 10, Dalamud.Interface.ColorHelpers.RgbaVector4ToUint(col), 12, thickness);
        }
        else
        {
            var path = new ImVectorWrapper<Vector2>(this.Points.Count);
            foreach (var p in this.Points)
            {
                path.Add(p);
            }

            ImGui.GetBackgroundDrawList()
                 .AddConvexPolyFilled(ref path[0], path.Length, Dalamud.Interface.ColorHelpers.RgbaVector4ToUint(col with { W = col.W / 2 }));
            ImGui.GetBackgroundDrawList()
                 .AddPolyline(ref path[0], path.Length, Dalamud.Interface.ColorHelpers.RgbaVector4ToUint(col), ImDrawFlags.Closed, thickness);

            path.Dispose();
        }
    }

    internal void TransformPoints(AtkResNode* transformNode)
    {
        while (transformNode != null)
        {
            var offset = new Vector2(transformNode->X, transformNode->Y);
            var origin = offset + new Vector2(transformNode->OriginX, transformNode->OriginY);
            var rotation = transformNode->Rotation;
            var scale = new Vector2(transformNode->ScaleX, transformNode->ScaleY);

            this.Points = this.Points.Select(b => TransformPoint(b + offset, origin, rotation, scale)).ToList();

            transformNode = transformNode->ParentNode;
        }
    }

    internal readonly bool ContainsPoint(Vector2 p)
    {
        var count = this.Points.Count;
        var inside = false;

        for (var i = 0; i < count; i++)
        {
            var p1 = this.Points[i];
            var p2 = this.Points[(i + 1) % count];

            if (p.Y > Min(p1.Y, p2.Y) &&
                p.Y <= Max(p1.Y, p2.Y) &&
                p.X <= Max(p1.X, p2.X) &&
                (p1.X.Equals(p2.X) || p.X <= ((p.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y)) + p1.X))
            {
                inside = !inside;
            }
        }

        return inside;
    }
}
