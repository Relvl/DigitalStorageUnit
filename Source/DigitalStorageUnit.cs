﻿using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace DigitalStorageUnit;

// ReSharper disable once UnusedType.Global
public class DigitalStorageUnit : Mod
{
    public static DigitalStorageUnitConfig Config { get; private set; } = new();

    public DigitalStorageUnit(ModContentPack content) : base(content)
    {
        try
        {
            // Init the configs
            Config = GetSettings<DigitalStorageUnitConfig>();
            DigitalStorageUnitConfig.ModInstance = this;
            // Init the Harmony
            HarmonyInstance = new Harmony("io.github.Relvl.Rimworld.DigitalStorageUnit");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            // Okay now
            Log.Message($"DigitalStorageUnit {typeof(DigitalStorageUnit).Assembly.GetName().Version} - Harmony patches successful");
        }
        catch (Exception ex)
        {
            Log.Error("DigitalStorageUnit :: Caught exception: " + ex);
        }
    }

    public override void DoSettingsWindowContents(Rect inRect) => Config.DoSettingsWindowContents(inRect);

    public Harmony HarmonyInstance { get; }

    public override string SettingsCategory() => "DSU".Translate();
}