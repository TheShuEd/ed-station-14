using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;

namespace Content.Server.Chemistry.ReactionConditions;

[UsedImplicitly]
[DataDefinition]
public sealed partial class SpecificTemperatureCondition : ReactionCondition
{
    /// <summary>
    /// The minimum temperature the reaction can occur at.
    /// </summary>
    [DataField]
    public float MinimumTemperature = 0.0f;

    /// <summary>
    /// The maximum temperature the reaction can occur at.
    /// </summary>
    [DataField]
    public float MaximumTemperature = float.PositiveInfinity;

    /// <summary>
    /// invert the specified frames. If true - no reaction will occur in the above temperature range.
    /// </summary>
    [DataField]
    public bool Inverse = false;

    public override bool Check(Entity<SolutionComponent> soln, ReactionMixerComponent? mixerComponent)
    {
        var solution = soln.Comp.Solution;

        if (solution.Temperature < MinimumTemperature || solution.Temperature > MaximumTemperature)
            return Inverse;

        return !Inverse;
    }

    public override string? ReagentConditionGuidebookText() =>
        Loc.GetString("guidebook-reagent-condition-specific-temperature",
            ("minTemp", MinimumTemperature),
            ("maxTemp", MaximumTemperature),
            ("hasMax", !float.IsPositiveInfinity(MaximumTemperature)));
}
