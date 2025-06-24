using Mlie;
using UnityEngine;
using Verse;

namespace Spaceship;

public class Controller : Mod
{
    public static Settings Settings;
    public static string CurrentVersion;

    public Controller(ModContentPack content)
        : base(content)
    {
        GetSettings<Settings>();
        CurrentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override string SettingsCategory()
    {
        return "MiningCo. Spaceship";
    }

    public void Save()
    {
        LoadedModManager.GetMod<Controller>().WriteSettings();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoSettingsWindowContents(inRect);
    }
}