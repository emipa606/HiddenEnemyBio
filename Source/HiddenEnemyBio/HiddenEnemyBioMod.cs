using System;
using HarmonyLib;
using Mlie;
using UnityEngine;
using Verse;

namespace HiddenEnemyBio;

public class HiddenEnemyBioMod : Mod
{
    private static string currentVersion;
    public static HiddenEnemyBioMod Instance;

    public HiddenEnemyBioMod(ModContentPack content)
        : base(content)
    {
        Instance = this;
        Settings = GetSettings<Settings>();
        new Harmony("cjgunnar.HiddenEnemyBio").PatchAll();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    internal Settings Settings { get; }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(inRect);
        listing_Standard.Label(string.Concat("HEB.SettingsVanillaBio".Translate() + ": ",
            Math.Round(Settings.useVanillaBioResistance, 2).ToString()));
        Settings.useVanillaBioResistance = listing_Standard.Slider(Settings.useVanillaBioResistance, 0f, 40f);
        listing_Standard.Label(string.Concat("HEB.SettingsShowSkills".Translate() + ": ",
            Math.Round(Settings.revealSkillsResistance, 2).ToString()));
        Settings.revealSkillsResistance = listing_Standard.Slider(Settings.revealSkillsResistance, 0f, 40f);
        listing_Standard.Label(string.Concat("HEB.SettingsShowTraits".Translate() + ": ",
            Math.Round(Settings.revealTraitsResisitance, 2).ToString()));
        Settings.revealTraitsResisitance = listing_Standard.Slider(Settings.revealTraitsResisitance, 0f, 40f);
        listing_Standard.Label(string.Concat("HEB.SettingsShowBackstory".Translate() + ": ",
            Math.Round(Settings.revealBackstoryResistance, 2).ToString()));
        Settings.revealBackstoryResistance = listing_Standard.Slider(Settings.revealBackstoryResistance, 0f, 40f);
        listing_Standard.Label(string.Concat("HEB.SettingsShowPassions".Translate() + ": ",
            Math.Round(Settings.revealPassionSkillsResistance, 2).ToString()));
        Settings.revealPassionSkillsResistance =
            listing_Standard.Slider(Settings.revealPassionSkillsResistance, 0f, 40f);
        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("HEB.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }

    public override string SettingsCategory()
    {
        return "HEB.HiddenEnemyBio".Translate();
    }
}