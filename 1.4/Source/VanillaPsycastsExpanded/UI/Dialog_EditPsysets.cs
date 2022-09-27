namespace VanillaPsycastsExpanded;

using UI;
using UnityEngine;
using Verse;

public class Dialog_EditPsysets : Window
{
    private readonly ITab_Pawn_Psycasts parent;

    public Dialog_EditPsysets(ITab_Pawn_Psycasts parent)
    {
        this.parent   = parent;
        this.doCloseX = true;
    }

    protected override float   Margin      => 3f;
    public override    Vector2 InitialSize => new(this.parent.Size.x * 0.3f, Mathf.Max(300f, this.NeededHeight));

    private float NeededHeight => this.parent.RequestedPsysetsHeight + this.Margin * 2f;

    public override void DoWindowContents(Rect inRect)
    {
        this.parent.DoPsysets(inRect);
        if (this.windowRect.height < this.NeededHeight) this.windowRect.height = this.NeededHeight;
    }
}