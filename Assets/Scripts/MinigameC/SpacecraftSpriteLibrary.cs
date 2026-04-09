using UnityEngine;

/// <summary>
/// ScriptableObject that holds references to all spacecraft assembly state sprites
/// and individual part sprites. Used by the SpacecraftAssemblyVisualizer to display
/// the spacecraft as it is built step by step.
/// </summary>
[CreateAssetMenu(fileName = "SpacecraftSpriteLibrary", menuName = "Minigame C/Spacecraft Sprite Library")]
public class SpacecraftSpriteLibrary : ScriptableObject
{
    [Header("Spacecraft Assembly States (0 = empty pad, 6 = complete)")]
    [Tooltip("State 0: Empty launch pad")]
    public Sprite stateBase;
    [Tooltip("State 1: Bus frame structure")]
    public Sprite stateBusFrame;
    [Tooltip("State 2: Bus + solar panels + power")]
    public Sprite stateBusPower;
    [Tooltip("State 3: + Magnetometer instrument")]
    public Sprite stateInstrument1;
    [Tooltip("State 4: + Imager + Spectrometer")]
    public Sprite stateInstruments;
    [Tooltip("State 5: + Antenna + Laser communications")]
    public Sprite stateComms;
    [Tooltip("State 6: + Propulsion (complete spacecraft)")]
    public Sprite stateComplete;

    [Header("Individual Part Sprites (for assembly animations)")]
    public Sprite partBusFrame;
    public Sprite partSolarPanelLeft;
    public Sprite partSolarPanelRight;
    public Sprite partBatteryPack;
    public Sprite partMagnetometer;
    public Sprite partImager;
    public Sprite partSpectrometer;
    public Sprite partRadioAntenna;
    public Sprite partLaserModule;
    public Sprite partPropulsion;

    private Sprite[] _stateSprites;

    private void OnEnable()
    {
        CacheStateSprites();
    }

    private void CacheStateSprites()
    {
        _stateSprites = new Sprite[]
        {
            stateBase,
            stateBusFrame,
            stateBusPower,
            stateInstrument1,
            stateInstruments,
            stateComms,
            stateComplete
        };
    }

    /// <summary>
    /// Returns the spacecraft sprite for the given assembly step.
    /// Step 0 = empty pad, Step 6 = complete spacecraft.
    /// </summary>
    public Sprite GetSpriteForStep(int step)
    {
        if (_stateSprites == null)
            CacheStateSprites();

        int index = Mathf.Clamp(step, 0, _stateSprites.Length - 1);
        return _stateSprites[index];
    }

    /// <summary>
    /// Returns the total number of assembly states (7: states 0 through 6).
    /// </summary>
    public int StateCount => 7;

    /// <summary>
    /// Returns the individual part sprite for the given part name.
    /// </summary>
    public Sprite GetPartSprite(string partName)
    {
        return partName switch
        {
            "bus_frame" => partBusFrame,
            "solar_panel_left" => partSolarPanelLeft,
            "solar_panel_right" => partSolarPanelRight,
            "battery_pack" => partBatteryPack,
            "magnetometer" => partMagnetometer,
            "imager" => partImager,
            "spectrometer" => partSpectrometer,
            "radio_antenna" => partRadioAntenna,
            "laser_module" => partLaserModule,
            "propulsion" => partPropulsion,
            _ => null
        };
    }
}
