using UnityEngine;
using Verse;

namespace Spaceship;

public class Settings : ModSettings
{
    public static bool landingPadLightIsEnabled = true;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref landingPadLightIsEnabled, "landingPadLightIsEnabled", true);
    }

    public static void DoSettingsWindowContents(Rect inRect)
    {
        var listing_Standard = new Listing_Standard
        {
            ColumnWidth = inRect.width
        };
        listing_Standard.Begin(inRect);
        listing_Standard.CheckboxLabeled("MCS.enablelights".Translate(), ref landingPadLightIsEnabled,
            "MCS.enablelightsTT".Translate());
        if (Controller.currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("MCS.CurrentModVersion".Translate(Controller.currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }
}