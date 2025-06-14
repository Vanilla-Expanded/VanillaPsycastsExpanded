namespace VanillaPsycastsExpanded.UI
{
    using System;
    using System.Collections.Generic;
    using Verse;

    public class Command_ActionWithFloat : Command_Action
    {
        public Func<IEnumerable<FloatMenuOption>> floatMenuGetter;

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => this.floatMenuGetter?.Invoke();
    }
}