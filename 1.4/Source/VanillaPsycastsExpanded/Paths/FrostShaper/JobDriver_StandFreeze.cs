namespace VanillaPsycastsExpanded
{
    using System;
    using System.Collections.Generic;
    using Verse;
    using Verse.AI;

    public class JobDriver_StandFreeze : JobDriver
	{
		public override string GetReport()
		{
			return "ReportStanding".Translate();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				base.Map.pawnDestinationReservationManager.Reserve(pawn, job, pawn.Position);
				pawn.pather.StopDead();
			};
			DecorateWaitToil(toil);
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			if (job.overrideFacing != Rot4.Invalid)
			{
				toil.handlingFacing = true;
				toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
				{
					pawn.rotationTracker.FaceTarget(pawn.Position + job.overrideFacing.FacingCell);
				});
			}
			yield return toil;
		}

		public virtual void DecorateWaitToil(Toil wait)
		{
		}

        public override void SetInitialPosture()
        {

        }
        public override void Notify_StanceChanged()
		{
			if (pawn.stances.curStance is Stance_Mobile)
			{
				this.EndJobWith(JobCondition.InterruptOptional);
			}
		}
	}
}