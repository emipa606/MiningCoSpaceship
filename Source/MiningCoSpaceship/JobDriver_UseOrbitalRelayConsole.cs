using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Spaceship;

public class JobDriver_UseOrbitalRelayConsole : JobDriver
{
    private const TargetIndex OrbitalRelayConsoleIndex = TargetIndex.A;

    private TimeSpeed previousTimeSpeed = TimeSpeed.Paused;

    private AirstrikeDef selectedStrikeDef;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TargetThingA, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var orbitalRelay = TargetThingA as Building_OrbitalRelay;
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell)
            .FailOn(() => orbitalRelay?.CanUseConsoleNow == false);
        yield return new Toil
        {
            initAction = delegate
            {
                if (orbitalRelay != null)
                {
                    GetActor().rotationTracker.FaceCell(orbitalRelay.Position);
                }
            }
        };
        yield return new Toil
        {
            initAction = delegate
            {
                var diaNode = new DiaNode("MCS.whatdoyouwant".Translate());
                var cargoSupplyDiaOption = getCargoSupplyDiaOption();
                diaNode.options.Add(cargoSupplyDiaOption);
                var medicalSupplyDiaOption = getMedicalSupplyDiaOption();
                diaNode.options.Add(medicalSupplyDiaOption);
                var airSupportDiaOption = getAirSupportDiaOption(diaNode);
                diaNode.options.Add(airSupportDiaOption);
                if (Util_Misc.Partnership.globalGoodwillFeeInSilver > 0 || Util_Misc.Partnership.feeInSilver[Map] > 0)
                {
                    foreach (var option in diaNode.options)
                    {
                        option.Disable("MCS.partnershipfeeunpaid".Translate());
                    }

                    var feePaymentDiaOption = getFeePaymentDiaOption(orbitalRelay);
                    diaNode.options.Add(feePaymentDiaOption);
                }

                var item = new DiaOption("MCS.disconnect".Translate())
                {
                    resolveTree = true
                };
                diaNode.options.Add(item);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, "MCS.comlink".Translate()));
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return Toils_Reserve.Release(OrbitalRelayConsoleIndex);
    }

    private DiaOption getCargoSupplyDiaOption()
    {
        var diaOption = new DiaOption("MCS.requestcargoship".Translate(Util_Spaceship.cargoSupplyCostInSilver));
        var landingPad = Util_LandingPad.GetBestAvailableLandingPad(Map);
        if (Find.TickManager.TicksGame <= Util_Misc.Partnership.nextRequestedSupplyMinTick[Map])
        {
            diaOption.Disable("MCS.noship".Translate());
        }
        else if (!TradeUtility.ColonyHasEnoughSilver(Map, Util_Spaceship.cargoSupplyCostInSilver))
        {
            diaOption.Disable("MCS.nosilver".Translate());
        }
        else if (landingPad == null)
        {
            diaOption.Disable("MCS.nopad".Translate());
        }
        else if (Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
        {
            diaOption.Disable("MCS.toxicfallout".Translate());
        }

        diaOption.action = delegate
        {
            TradeUtility.LaunchSilver(Map, Util_Spaceship.cargoSupplyCostInSilver);
            Util_Spaceship.SpawnLandingSpaceship(landingPad, SpaceshipKind.CargoRequested);
        };
        var diaNode =
            new DiaNode("MCS.requestapproved".Translate());
        var diaOption2 = new DiaOption("MCS.ok".Translate())
        {
            resolveTree = true
        };
        diaNode.options.Add(diaOption2);
        diaOption.link = diaNode;
        return diaOption;
    }

    private DiaOption getMedicalSupplyDiaOption()
    {
        var diaOption = new DiaOption("MCS.requestmedicship".Translate(Util_Spaceship.medicalSupplyCostInSilver));
        var landingPad = Util_LandingPad.GetBestAvailableLandingPad(Map);
        if (Find.TickManager.TicksGame <= Util_Misc.Partnership.nextMedicalSupplyMinTick[Map])
        {
            diaOption.Disable("MCS.noship".Translate());
        }
        else if (!TradeUtility.ColonyHasEnoughSilver(Map, Util_Spaceship.medicalSupplyCostInSilver))
        {
            diaOption.Disable("MCS.nosilver".Translate());
        }
        else if (landingPad == null)
        {
            diaOption.Disable("MCS.nopad".Translate());
        }
        else if (Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
        {
            diaOption.Disable("MCS.toxicfallout".Translate());
        }

        diaOption.action = delegate
        {
            TradeUtility.LaunchSilver(Map, Util_Spaceship.medicalSupplyCostInSilver);
            Util_Spaceship.SpawnLandingSpaceship(landingPad, SpaceshipKind.Medical);
        };
        var diaNode =
            new DiaNode("MCS.medicrequestapproved".Translate());
        var diaOption2 = new DiaOption("MCS.ok".Translate())
        {
            resolveTree = true
        };
        diaNode.options.Add(diaOption2);
        diaOption.link = diaNode;
        return diaOption;
    }

    private DiaOption getAirSupportDiaOption(DiaNode parentDiaNode)
    {
        var diaOption = new DiaOption("MCS.requestairsupport".Translate())
        {
            link = Find.TickManager.TicksGame < Util_Misc.Partnership.nextAirstrikeMinTick[Map]
                ? getAirSupportDeniedDiaNode(parentDiaNode)
                : getAirSupportGrantedDiaNode(parentDiaNode)
        };

        return diaOption;
    }

    private DiaNode getAirSupportGrantedDiaNode(DiaNode parentDiaNode)
    {
        var diaNode = new DiaNode("MCS.airsupportrequestapproved".Translate());
        foreach (var item in DefDatabase<AirstrikeDef>.AllDefsListForReading)
        {
            var diaOption = new DiaOption("MCS.itemcost".Translate(item.LabelCap, item.costInSilver))
            {
                link = getAirstrikeDetailsDiaNode(diaNode, item)
            };
            diaNode.options.Add(diaOption);
        }

        var diaOption2 = new DiaOption("MCS.back".Translate())
        {
            link = parentDiaNode
        };
        diaNode.options.Add(diaOption2);
        return diaNode;
    }

    private DiaNode getAirstrikeDetailsDiaNode(DiaNode parentNode, AirstrikeDef strikeDef)
    {
        var diaNode = new DiaNode("MCS.airstriketype".Translate(strikeDef.LabelCap, strikeDef.description,
            strikeDef.runsNumber, strikeDef.costInSilver));
        var diaOption = new DiaOption("MCS.confirm".Translate())
        {
            action = delegate
            {
                previousTimeSpeed = Find.TickManager.CurTimeSpeed;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                selectedStrikeDef = strikeDef;
                Util_Misc.SelectAirstrikeTarget(Map, spawnAirstrikeBeacon);
            },
            resolveTree = true
        };
        if (!TradeUtility.ColonyHasEnoughSilver(Map, strikeDef.costInSilver))
        {
            diaOption.Disable("MCS.nosilver".Translate());
        }

        diaNode.options.Add(diaOption);
        var diaOption2 = new DiaOption("MCS.back".Translate())
        {
            link = parentNode
        };
        diaNode.options.Add(diaOption2);
        return diaNode;
    }

    private static DiaNode getAirSupportDeniedDiaNode(DiaNode parentDiaNode)
    {
        var diaNode = new DiaNode("MCS.noairsupport".Translate());
        var diaOption = new DiaOption("MCS.ok".Translate())
        {
            resolveTree = true
        };
        diaNode.options.Add(diaOption);
        var diaOption2 = new DiaOption("MCS.back".Translate())
        {
            link = parentDiaNode
        };
        diaNode.options.Add(diaOption2);
        return diaNode;
    }

    private void spawnAirstrikeBeacon(LocalTargetInfo targetPosition)
    {
        TradeUtility.LaunchSilver(Map, selectedStrikeDef.costInSilver);
        var building_AirstrikeBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.Building_AirstrikeBeacon, targetPosition.Cell, Map) as
                Building_AirstrikeBeacon;
        building_AirstrikeBeacon?.InitializeAirstrike(targetPosition.Cell, selectedStrikeDef);
        building_AirstrikeBeacon?.SetFaction(Faction.OfPlayer);
        Messages.Message("MCS.airstrikeconfirmed".Translate(), building_AirstrikeBeacon, MessageTypeDefOf.CautionInput);
        Find.TickManager.CurTimeSpeed = previousTimeSpeed;
        Util_Misc.Partnership.nextAirstrikeMinTick[Map] = Find.TickManager.TicksGame +
                                                          Mathf.RoundToInt(selectedStrikeDef.ammoResupplyDays * 60000f);
    }

    private DiaOption getFeePaymentDiaOption(Building_OrbitalRelay orbitalRelay)
    {
        var feeCost = Util_Misc.Partnership.globalGoodwillFeeInSilver + Util_Misc.Partnership.feeInSilver[Map];
        var diaOption = new DiaOption("MCS.paypartnershipfee".Translate(feeCost))
        {
            resolveTree = true,
            action = delegate
            {
                TradeUtility.LaunchSilver(orbitalRelay.Map, feeCost);
                Util_Misc.Partnership.globalGoodwillFeeInSilver = 0;
                Util_Misc.Partnership.feeInSilver[Map] = 0;
                Messages.Message("MCS.partnershipfeepayed".Translate(), MessageTypeDefOf.PositiveEvent);
                if (Util_Faction.MiningCoFaction.GoodwillWith(Faction.OfPlayer) < 0)
                {
                    Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer,
                        -Util_Faction.MiningCoFaction.GoodwillWith(Faction.OfPlayer));
                }
            }
        };
        if (!TradeUtility.ColonyHasEnoughSilver(Map, feeCost))
        {
            diaOption.Disable("MCS.notenoughsilver".Translate());
        }

        return diaOption;
    }
}