using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HiddenEnemyBio;

[HarmonyPatch(typeof(CharacterCardUtility), nameof(CharacterCardUtility.DrawCharacterCard))]
public static class CharacterCardUtility_DrawCharacterCard
{
    private static bool Prefix(Rect rect, Pawn pawn, Action randomizeCallback, Rect creationRect, bool showName)
    {
        if (HiddenBioUtil.ShouldDefaultBioVisible(pawn))
        {
            return true;
        }

        HiddenCharacterCardUtility.DrawCharacterCard(rect, pawn, randomizeCallback, creationRect, showName);
        return false;
    }
}