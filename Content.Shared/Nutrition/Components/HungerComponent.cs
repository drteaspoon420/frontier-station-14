using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Shared.Nutrition.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(HungerSystem))]
public sealed partial class HungerComponent : Component
{
    /// <summary>
    /// The current hunger amount of the entity
    /// </summary>
    [DataField("currentHunger"), ViewVariables(VVAccess.ReadWrite)]
    public float CurrentHunger;

    /// <summary>
    /// The base amount at which <see cref="CurrentHunger"/> decays.
    /// </summary>
    [DataField("baseDecayRate"), ViewVariables(VVAccess.ReadWrite)]
    public float BaseDecayRate = 0.02f;

    /// <summary>
    /// The actual amount at which <see cref="CurrentHunger"/> decays.
    /// Affected by <seealso cref="CurrentThreshold"/>
    /// </summary>
    [DataField("actualDecayRate"), ViewVariables(VVAccess.ReadWrite)]
    public float ActualDecayRate;

    /// <summary>
    /// The last threshold this entity was at.
    /// Stored in order to prevent recalculating
    /// </summary>
    [DataField("lastThreshold"), ViewVariables(VVAccess.ReadWrite)]
    public HungerThreshold LastThreshold;

    /// <summary>
    /// The current hunger threshold the entity is at
    /// </summary>
    [DataField("currentThreshold"), ViewVariables(VVAccess.ReadWrite)]
    public HungerThreshold CurrentThreshold;

    /// <summary>
    /// A dictionary relating HungerThreshold to the amount of <see cref="CurrentHunger"/> needed for each one
    /// </summary>
    [DataField("thresholds", customTypeSerializer: typeof(DictionarySerializer<HungerThreshold, float>))]
    public Dictionary<HungerThreshold, float> Thresholds = new()
    {
        { HungerThreshold.Overfed, 200.0f },
        { HungerThreshold.Okay, 150.0f },
        { HungerThreshold.Peckish, 100.0f },
        { HungerThreshold.Starving, 50.0f },
        { HungerThreshold.Dead, 0.0f }
    };

    /// <summary>
    /// A dictionary relating hunger thresholds to corresponding alerts.
    /// </summary>
    [DataField("hungerThresholdAlerts", customTypeSerializer: typeof(DictionarySerializer<HungerThreshold, AlertType>))]
    public Dictionary<HungerThreshold, AlertType> HungerThresholdAlerts = new()
    {
        { HungerThreshold.Peckish, AlertType.Peckish },
        { HungerThreshold.Starving, AlertType.Starving },
        { HungerThreshold.Dead, AlertType.Starving }
    };

    /// <summary>
    /// A dictionary relating HungerThreshold to how much they modify <see cref="BaseDecayRate"/>.
    /// </summary>
    [DataField("hungerThresholdDecayModifiers", customTypeSerializer: typeof(DictionarySerializer<HungerThreshold, float>))]
    public Dictionary<HungerThreshold, float> HungerThresholdDecayModifiers = new()
    {
        { HungerThreshold.Overfed, 1.2f },
        { HungerThreshold.Okay, 1f },
        { HungerThreshold.Peckish, 0.8f },
        { HungerThreshold.Starving, 0.6f },
        { HungerThreshold.Dead, 0.6f }
    };

    /// <summary>
    /// The amount of slowdown applied when an entity is starving
    /// </summary>
    [DataField("starvingSlowdownModifier"), ViewVariables(VVAccess.ReadWrite)]
    public float StarvingSlowdownModifier = 0.75f;

    /// <summary>
    /// Damage dealt when your current threshold is at HungerThreshold.Dead
    /// </summary>
    [DataField("starvationDamage")]
    public DamageSpecifier? StarvationDamage;

    /// <summary>
    /// The time when the hunger will update next.
    /// </summary>
    [DataField("nextUpdateTime", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextUpdateTime;

    /// <summary>
    /// The time between each update.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan UpdateRate = TimeSpan.FromSeconds(1);
}

[Serializable, NetSerializable]
public sealed class HungerComponentState : ComponentState
{
    public float CurrentHunger;

    public float BaseDecayRate;

    public float ActualDecayRate;

    public HungerThreshold LastHungerThreshold;

    public HungerThreshold CurrentThreshold;

    public float StarvingSlowdownModifier;

    public TimeSpan NextUpdateTime;

    public HungerComponentState(float currentHunger,
        float baseDecayRate,
        float actualDecayRate,
        HungerThreshold lastHungerThreshold,
        HungerThreshold currentThreshold,
        float starvingSlowdownModifier,
        TimeSpan nextUpdateTime)
    {
        CurrentHunger = currentHunger;
        BaseDecayRate = baseDecayRate;
        ActualDecayRate = actualDecayRate;
        LastHungerThreshold = lastHungerThreshold;
        CurrentThreshold = currentThreshold;
        StarvingSlowdownModifier = starvingSlowdownModifier;
        NextUpdateTime = nextUpdateTime;
    }
}

[Serializable, NetSerializable]
public enum HungerThreshold : byte
{
    Overfed = 1 << 3,
    Okay = 1 << 2,
    Peckish = 1 << 1,
    Starving = 1 << 0,
    Dead = 0,
}
