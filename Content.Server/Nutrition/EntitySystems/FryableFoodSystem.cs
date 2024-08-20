using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.DoAfter;
using Content.Server.Nutrition.Components;
using Content.Server.Temperature.Components;
using Content.Shared.Nutrition;
using Content.Shared.Nutrition.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Temperature;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Server.Nutrition.EntitySystems;

public sealed class FryableFoodSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FryableFoodComponent, OnTemperatureChangeEvent>(OnTemperatureChanged);
    }

    private void OnTemperatureChanged(Entity<FryableFoodComponent> ent, ref OnTemperatureChangeEvent args)
    {
        if (!TryComp<InternalTemperatureComponent>(ent, out var internalTemp))
            return;
        if (!TryComp<TemperatureComponent>(ent, out var externalTemp))
            return;


        foreach (var entry in ent.Comp.Entries)
        {
            if (entry.ExternalTemperature is not null && externalTemp.CurrentTemperature < entry.ExternalTemperature)
                continue;

            if (entry.InternalTemperature is not null && internalTemp.Temperature < entry.InternalTemperature)
                continue;

            TryCook(ent, entry);
            QueueDel(ent);
            break;
        }
    }

    private void TryCook(Entity<FryableFoodComponent> ent, FryableFoodEntry entry)
    {
        _audio.PlayPvs(entry.Sound, ent);

        var newFood = SpawnAtPosition(entry.Proto, Transform(ent).Coordinates);

        if(!_solutionContainer.TryGetSolution(ent.Owner, ent.Comp.Solution, out var startSoln, out var startSolution))
            return;

        if(!_solutionContainer.TryGetSolution(newFood, ent.Comp.Solution, out var endSoln, out var endSolution))
            return;

        _solutionContainer.RemoveAllSolution(endSoln.Value); //Remove all YML reagents
        endSolution.MaxVolume = startSolution.MaxVolume;
        _solutionContainer.TryAddSolution(endSoln.Value, startSolution);
        _solutionContainer.mix
    }
}
