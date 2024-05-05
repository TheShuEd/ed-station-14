using System.Text.Json.Serialization;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using JetBrains.Annotations;

namespace Content.Shared.Chemistry.Reagent;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class ReactionCondition
{
    [JsonPropertyName("id")] private protected string _id => this.GetType().Name;

    public abstract bool Check(Entity<SolutionComponent> soln, ReactionMixerComponent? mixerComponent);

    public abstract string? ReagentConditionGuidebookText();
}
