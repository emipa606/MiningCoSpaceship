using UnityEngine;
using Verse;

namespace Spaceship;

public class Settings : ModSettings
{
    public static bool LandingPadLightIsEnabled = true;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref LandingPadLightIsEnabled, "landingPadLightIsEnabled", true);
    }

    public static void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard
        {
            ColumnWidth = inRect.width
        };
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("MCS.enablelights".Translate(), ref LandingPadLightIsEnabled,
            "MCS.enablelightsTT".Translate());
        if (Controller.CurrentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("MCS.CurrentModVersion".Translate(Controller.CurrentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }
}