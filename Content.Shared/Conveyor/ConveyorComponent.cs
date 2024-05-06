using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Conveyor;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ConveyorComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<string, FixtureSettings> Fixtures = new();

    /// <summary>
    ///     The amount of units to move the entity by per second.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public float Speed = 2f;

    /// <summary>
    ///     The current state of this conveyor
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public ConveyorState State;

    [ViewVariables, AutoNetworkedField]
    public bool Powered;

    [DataField]
    public ProtoId<SinkPortPrototype> ForwardPort = "Forward";

    [DataField]
    public ProtoId<SinkPortPrototype> ReversePort = "Reverse";

    [DataField]
    public ProtoId<SinkPortPrototype> OffPort = "Off";
}

[Serializable, NetSerializable]
public enum ConveyorVisuals : byte
{
    State
}

[Serializable, NetSerializable]
public enum ConveyorState : byte
{
    Off,
    Forward,
    Reverse
}

[Serializable, NetSerializable, DataRecord]
public partial record struct FixtureSettings()
{
    /// <summary>
    ///     The angle to move entities by in relation to the owner's rotation.
    /// </summary>
    public Angle Angle { get; set; } = Angle.Zero;

    [field: NonSerialized]
    public HashSet<EntityUid> Intersecting { get; set; } = new();
}
