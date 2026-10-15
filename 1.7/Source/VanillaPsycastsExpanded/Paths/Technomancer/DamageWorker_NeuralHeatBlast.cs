namespace VanillaPsycastsExpanded.Technomancer
{
    using System.Collections.Generic;
    using Verse;

    public class DamageWorker_NeuralHeatBlast : DamageWorker
    {
        protected override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
        {
            if (t is not Pawn {psychicEntropy: { }, HasPsylink: true} pawn) return;

            if (damagedThings.Contains(t)) return;
            damagedThings.Add(t);
            if (ignoredThings != null && ignoredThings.Contains(t)) return;

            pawn.psychicEntropy.TryAddEntropy(pawn.psychicEntropy.MaxEntropy - pawn.psychicEntropy.EntropyValue, explosion);
        }
    }
}