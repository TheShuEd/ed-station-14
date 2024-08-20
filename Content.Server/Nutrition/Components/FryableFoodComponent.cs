using Content.Server.Nutrition.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Nutrition.Components;

/// <summary>
/// A component that transforms this entity into another, while keeping the reactants inside, by temperature processing (frying, boiling or any other variants)
/// </summary>
[RegisterComponent, Access(typeof(FryableFoodSystem))]
public sealed partial class FryableFoodComponent : Component
{
    [DataField]
    public string Solution = "food";

    [DataField(required: true)]
    public List<FryableFoodEntry> Entries = new();
}

/// <summary>
///
/// </summary>
[DataRecord]
public record struct FryableFoodEntry
{
    /// <summary>
    /// the level of external temperature required to achieve that the entity is transformed
    /// </summary>
    public float? ExternalTemperature;
    /// <summary>
    /// the level of internal temperature required to achieve that the entity is transformed
    /// </summary>
    public float? InternalTemperature;
    /// <summary>
    /// the entity into which this entity will be transformed.
    /// </summary>
    public EntProtoId Proto;
    /// <summary>
    /// transformation sound
    /// </summary>
    public SoundSpecifier Sound;

    public ProtoId<MixingCategoryPrototype> MixingCategory;

    public FryableFoodEntry(float? externalT, float? internalT, EntProtoId proto, SoundSpecifier sound, ProtoId<MixingCategoryPrototype> mixing)
    {
        ExternalTemperature = externalT;
        InternalTemperature = internalT;
        Proto = proto;
        Sound = sound;
        MixingCategory = mixing;
    }
}
