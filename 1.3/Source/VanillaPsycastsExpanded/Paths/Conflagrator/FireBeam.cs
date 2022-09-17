namespace VanillaPsycastsExpanded.Conflagrator
{
    using RimWorld;
    using Verse;

    public class FireBeam : PowerBeam
    {
        public override void StartStrike()
        {
            base.StartStrike();
            Mote mote = this.Position.GetFirstThing<Mote>(this.Map);
            mote.Destroy();
            mote               = (Mote) ThingMaker.MakeThing(VPE_DefOf.VPE_Mote_FireBeam);
            mote.exactPosition = this.Position.ToVector3Shifted();
            mote.Scale         = 90f;
            mote.rotationRate  = 1.2f;
            GenSpawn.Spawn(mote, this.Position, this.Map);
        }
    }
}