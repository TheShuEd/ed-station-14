using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using JetBrains.Annotations;

namespace Content.Shared.Chemistry.Reagent;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class ReagentCondition
{
    public abstract bool Check(Entity<SolutionComponent> soln, ReactionPrototype reaction, ReactionMixerComponent? mixerComponent);

    protected abstract string? ReagentConditionGuidebookText();
}
