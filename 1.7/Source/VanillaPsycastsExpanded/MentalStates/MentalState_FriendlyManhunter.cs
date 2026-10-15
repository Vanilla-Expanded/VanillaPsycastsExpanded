// Verse.AI.MentalState_Manhunter
using RimWorld;
using Verse;
using Verse.AI;
namespace VanillaPsycastsExpanded
{
	public class MentalState_FriendlyManhunter : MentalState_Manhunter
	{
        public override bool ForceHostileTo(Faction f)
        {
            return pawn.Faction != f;
        }

        public override bool ForceHostileTo(Thing t)
        {
            return false;
        }
        public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
