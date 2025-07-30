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
            Math.Round(Settings.UseVanillaBioResistance, 2).ToString()));
        Settings.UseVanillaBioResistance = listing_Standard.Slider(Settings.UseVanillaBioResistance, 0f, 40f);
        listing_Standard.Label(string.Concat("HEB.SettingsShowSkills".Translate() + ": ",
            Math.Round(Settings.RevealSkillsResistance, 2).ToString()));
        Settings.RevealSkillsResistance = listing_Standard.Slider(Settings.RevealSkillsResistance, 0f, 40f);
        listing_Standard.Label(string.Concat("HEB.SettingsShowTraits".Translate() + ": ",
            Math.Round(Settings.RevealTraitsResisitance, 2).ToString()));
        Settings.RevealTraitsResisitance = listing_Standard.Slider(Settings.RevealTraitsResisitance, 0f, 40f);
        listing_Standard.Label(string.Concat("HEB.SettingsShowBackstory".Translate() + ": ",
            Math.Round(Settings.RevealBackstoryResistance, 2).ToString()));
        Settings.RevealBackstoryResistance = listing_Standard.Slider(Settings.RevealBackstoryResistance, 0f, 40f);
        listing_Standard.Label(string.Concat("HEB.SettingsShowPassions".Translate() + ": ",
            Math.Round(Settings.RevealPassionSkillsResistance, 2).ToString()));
        Settings.RevealPassionSkillsResistance =
            listing_Standard.Slider(Settings.RevealPassionSkillsResistance, 0f, 40f);
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