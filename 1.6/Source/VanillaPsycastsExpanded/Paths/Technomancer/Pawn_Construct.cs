namespace VanillaPsycastsExpanded.Technomancer;

using RimWorld.Planet;
using Verse;

public class Pawn_Construct : Pawn, IMinHeatGiver
{
    public bool IsActive => this.Spawned || this.GetCaravan() != null;
    public int  MinHeat  => 20;
}