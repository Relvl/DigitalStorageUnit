﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using DigitalStorageUnit.Common.HarmonyPatches;
using DigitalStorageUnit.Storage.Editables;
using UnityEngine;
using Verse;

namespace DigitalStorageUnit.Storage;

[StaticConstructorOnStartup]
public class Building_MassStorageUnitPowered : Building_MassStorageUnit
{
    private static Texture2D StoragePawnAccessSwitchIcon = ContentFinder<Texture2D>.Get("UI/dsu", true);

    //Initialized on spawn
    private CompPowerTrader compPowerTrader;

    public override bool Powered => compPowerTrader?.PowerOn ?? false;

    public override bool CanStoreMoreItems => (Powered) && Spawned && (ModExtensionCrate == null || StoredItemsCount < MaxNumberItemsInternal);
    public override bool CanReceiveIO => base.CanReceiveIO && Powered && Spawned;

    public override bool ForbidPawnInput => !pawnAccess || !CanStoreMoreItems;

    public float ExtraPowerDraw => StoredItems.Count * 10f;

    private bool pawnAccess = true;

    public override void Notify_ReceivedThing(Thing newItem)
    {
        base.Notify_ReceivedThing(newItem);
        UpdatePowerConsumption();
    }

    public override void Notify_LostThing(Thing newItem)
    {
        base.Notify_LostThing(newItem);
        UpdatePowerConsumption();
    }

    public void UpdatePowerConsumption()
    {
        compPowerTrader ??= GetComp<CompPowerTrader>();
        FridgePowerPatchUtil.UpdatePowerDraw(this, compPowerTrader);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref pawnAccess, "pawnAccess", true);
        compPowerTrader ??= GetComp<CompPowerTrader>();
    }

    protected override void ReceiveCompSignal(string signal)
    {
        base.ReceiveCompSignal(signal);
        switch (signal)
        {
            case "PowerTurnedOn":
                RefreshStorage();
                break;
            default:
                break;
        }
    }

    public override void Tick()
    {
        base.Tick();
        if (this.IsHashIntervalTick(60))
        {
            UpdatePowerConsumption();
        }
    }

    public override void PostMapInit()
    {
        base.PostMapInit();
        compPowerTrader ??= GetComp<CompPowerTrader>();
        RefreshStorage();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;
        if (Prefs.DevMode)
        {
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Debug actions", action = () => { Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>(DebugActions()))); }
            };
        }

        yield return new Command_Toggle
        {
            defaultLabel = "PRFPawnAccessLabel".Translate(),
            isActive = () => pawnAccess,
            toggleAction = () => pawnAccess = !pawnAccess,
            defaultDesc = "PRFPawnAccessDesc".Translate(),
            icon = StoragePawnAccessSwitchIcon
        };
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        if (mode == DestroyMode.Deconstruct)
        {
            if (def.GetModExtension<DefModExtension_Crate>()?.destroyContainsItems ?? false)
            {
                StoredItems.Where(t => !t.Destroyed).ToList().ForEach(x => x.Destroy());
            }
        }

        base.DeSpawn(mode);
    }

    protected virtual IEnumerable<FloatMenuOption> DebugActions()
    {
        yield return new FloatMenuOption("Update power consumption", UpdatePowerConsumption);
        yield return new FloatMenuOption("Log item count", () => Log.Message(StoredItemsCount.ToString()));
    }

    public override string GetUIThingLabel()
    {
        if ((def.GetModExtension<DefModExtension_Crate>()?.limit).HasValue)
        {
            return "PRFCrateUIThingLabel".Translate(StoredItemsCount, def.GetModExtension<DefModExtension_Crate>().limit);
        }

        return base.GetUIThingLabel();
    }

    public override string GetITabString(int itemsSelected)
    {
        if ((def.GetModExtension<DefModExtension_Crate>()?.limit).HasValue)
        {
            return "PRFItemsTabLabel_Crate".Translate(StoredItemsCount, def.GetModExtension<DefModExtension_Crate>().limit, itemsSelected);
        }

        return base.GetITabString(itemsSelected);
    }
}