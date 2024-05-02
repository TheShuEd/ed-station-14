using Content.Server.Objectives.Components;
using Content.Shared.Mind.Components;
using Robust.Shared.Random;

namespace Content.Server.Objectives.Systems;

public sealed class CrewAmbitionSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrewAmbitionComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CrewAmbitionComponent> ambition, ref MapInitEvent args)
    {

    }
}
