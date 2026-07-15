using Robust.Shared.Serialization;

namespace Content.Shared.Actions;

/// <summary>
///     Appearance data keys and sprite layer keys for action entities.
/// </summary>
[Serializable, NetSerializable]
public enum ActionVisuals : byte
{
    /// <summary>Appearance key. Mirrors ActionComponent.Toggled.</summary>
    Toggled,

    /// <summary>Appearance key. Runtime-assigned SpriteSpecifier, for actions with a dynamic icon.</summary>
    DynamicIcon,

    /// <summary>Appearance key. Runtime-assigned tint override, e.g. decal-placement actions.</summary>
    Color,

    /// <summary>Layer key. The action's base icon.</summary>
    Icon,

    /// <summary>Layer key. Optional, shown only while toggled on.</summary>
    IconToggled,
}
