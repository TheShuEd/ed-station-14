using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.FixedPoint;

namespace Content.Shared.Chemistry.Reagent;

[ImplicitDataDefinitionForInheritors]
public abstract partial class ReagentCondition
{
    public abstract bool Check(Entity<SolutionComponent> soln, ReactionPrototype reaction, ReactionMixerComponent? mixerComponent);

    public abstract string GetGuideTip();
}
