namespace VanillaPsycastsExpanded
{
    using Verse;
    public class Stance_Stand : Stance_Busy
    {
        public Stance_Stand()
        {

        }

        public Stance_Stand(int ticks, LocalTargetInfo focusTarg, Verb verb)
            : base(ticks, focusTarg, verb)
        {

        }
    }
}