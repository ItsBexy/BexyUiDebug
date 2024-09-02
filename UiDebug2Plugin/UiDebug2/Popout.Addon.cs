using System;
using System.Numerics;

using Dalamud.Interface.Windowing;
using ImGuiNET;
using UiDebug2.Browsing;

namespace UiDebug2;

internal class AddonPopoutWindow : Window, IDisposable
{
    private readonly AddonTree addonTree;

    public AddonPopoutWindow(AddonTree tree, string name)
        : base(name)
    {
        this.addonTree = tree;
        this.PositionCondition = ImGuiCond.Once;

        var pos = ImGui.GetMousePos() + new Vector2(50, -50);
        var workSize = ImGui.GetMainViewport().WorkSize;
        var pos2 = new Vector2(Math.Min(workSize.X - 750, pos.X), Math.Min(workSize.Y - 250, pos.Y));

        this.Position = pos2;
        this.SizeCondition = ImGuiCond.Once;
        this.Size = new(700, 200);
        this.IsOpen = true;
        this.SizeConstraints = new() { MinimumSize = new(100, 100) };
    }

    public override void Draw()
    {
        ImGui.BeginChild($"{this.WindowName}child", new(-1, -1), true);
        this.addonTree.Draw();
        ImGui.EndChild();
    }

    public void Dispose()
    {
    }
}
