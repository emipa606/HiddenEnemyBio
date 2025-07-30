using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HiddenEnemyBio;

[HarmonyPatch(typeof(SkillUI), nameof(SkillUI.DrawSkillsOf))]
internal class SkillUI_DrawSkillsOf
{
    private static bool Prefix(Pawn p, Vector2 offset, SkillUI.SkillDrawMode mode, Rect container,
        float ___levelLabelWidth, List<SkillDef> ___skillDefsInListOrderCached)
    {
        if (HiddenBioUtil.ShouldDefaultBioVisible(p))
        {
            return true;
        }

        if (HiddenBioUtil.ShouldRevealSkills(p))
        {
            return true;
        }

        if (!HiddenBioUtil.ShouldRevealPassionSkills(p))
        {
            return true;
        }

        drawPassionSkillsOf(p, offset, mode, container, ___levelLabelWidth, ___skillDefsInListOrderCached);
        return false;
    }

    private static void drawPassionSkillsOf(Pawn p, Vector2 offset, SkillUI.SkillDrawMode mode, Rect container,
        float ___levelLabelWidth, List<SkillDef> ___skillDefsInListOrderCached)
    {
        Text.Font = GameFont.Small;
        if (p.DevelopmentalStage.Baby())
        {
            var color = GUI.color;
            GUI.color = Color.gray;
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(offset.x, offset.y, 230f, container.height), "SkillsDevelopLaterBaby".Translate());
            GUI.color = color;
            Text.Anchor = anchor;
            return;
        }

        var allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
        foreach (var skillDef in allDefsListForReading)
        {
            var x = Text.CalcSize(skillDef.skillLabel.CapitalizeFirst()).x;
            if (x > ___levelLabelWidth)
            {
                ___levelLabelWidth = x;
            }
        }

        var skillDrawn = false;
        for (var j = 0; j < ___skillDefsInListOrderCached.Count; j++)
        {
            var skillDef = ___skillDefsInListOrderCached[j];
            var y = (j * 27f) + offset.y;
            if ((int)p.skills.GetSkill(skillDef).passion <= 0)
            {
                continue;
            }

            SkillUI.DrawSkill(p.skills.GetSkill(skillDef), new Vector2(offset.x, y), mode);
            skillDrawn = true;
        }

        if (!skillDrawn)
        {
            Widgets.Label(new Rect(offset.x, offset.y, 230f, container.height),
                "HEB.NoPassions".Translate(p.Named("PAWN")));
        }
    }
}