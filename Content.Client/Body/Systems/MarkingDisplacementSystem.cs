using Content.Client.Body;
using Content.Client.DisplacementMap;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Humanoid.Markings;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client.Body;

/// <summary>
/// Applies displacement maps to marking layers (Hair, FacialHair, etc.) after they are rendered.
/// Works with <see cref="MarkingDisplacementComponent"/> on the organ entity.
/// </summary>
public sealed class CEMarkingDisplacementSystem : EntitySystem
{
    [Dependency] private readonly DisplacementMapSystem _displacement = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MarkingDisplacementComponent, OrganGotInsertedEvent>(OnInserted, after: [typeof(VisualBodySystem)]);
        SubscribeLocalEvent<MarkingDisplacementComponent, OrganGotRemovedEvent>(OnRemoved, after: [typeof(VisualBodySystem)]);
    }

    private void OnInserted(Entity<MarkingDisplacementComponent> ent, ref OrganGotInsertedEvent args)
    {
        ApplyDisplacements(ent, args.Target);
    }

    private void OnRemoved(Entity<MarkingDisplacementComponent> ent, ref OrganGotRemovedEvent args)
    {
        RemoveDisplacements(ent, args.Target);
    }

    private void ApplyDisplacements(Entity<MarkingDisplacementComponent> ent, EntityUid body)
    {
        if (!TryComp<SpriteComponent>(body, out var sprite))
            return;

        if (!TryComp<VisualOrganMarkingsComponent>(ent, out var markings))
            return;

        // Clean up previous displacement layers
        RemoveDisplacements(ent, body);

        foreach (var marking in markings.AppliedMarkings)
        {
            if (!_markingManager.TryGetMarking(marking, out var proto))
                continue;

            if (!ent.Comp.Displacements.TryGetValue(proto.BodyPart, out var displacementData))
                continue;

            foreach (var spriteSpec in proto.Sprites)
            {
                DebugTools.Assert(spriteSpec is SpriteSpecifier.Rsi);
                if (spriteSpec is not SpriteSpecifier.Rsi rsi)
                    continue;

                var layerId = $"{proto.ID}-{rsi.RsiState}";

                if (!_sprite.LayerMapTryGet(body, layerId, out var index, false))
                    continue;

                if (_displacement.TryAddDisplacement(displacementData, (body, sprite), index, layerId,
                    out var displacementKey))
                {
                    ent.Comp.TrackedKeys.Add(displacementKey);
                }
            }
        }
    }

    private void RemoveDisplacements(Entity<MarkingDisplacementComponent> ent, EntityUid body)
    {
        if (ent.Comp.TrackedKeys.Count == 0)
            return;

        foreach (var key in ent.Comp.TrackedKeys)
        {
            _sprite.RemoveLayer(body, key, false);
        }

        ent.Comp.TrackedKeys.Clear();
    }
}
