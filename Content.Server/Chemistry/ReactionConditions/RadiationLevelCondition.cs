using Content.Server.Radiation.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;

namespace Content.Server.Chemistry.ReactionConditions;

[UsedImplicitly]
[DataDefinition]
public sealed partial class RadiationLevelCondition : ReactionCondition
{

    [Dependency] private readonly RadiationSystem _radiation = default!;

    /// <summary>
    /// The minimum radiation level the reaction can occur at.
    /// </summary>
    [DataField]
    public float MinimumRad = 0.0f;

    /// <summary>
    /// The maximum radiation level the reaction can occur at.
    /// </summary>
    [DataField]
    public float MaximumRad = float.PositiveInfinity;

    public override bool Check(Entity<SolutionComponent> soln, ReactionMixerComponent? mixerComponent)
    {
        _radiation.
    }

    public override string? ReagentConditionGuidebookText()
    {
        throw new NotImplementedException();
    }
}
