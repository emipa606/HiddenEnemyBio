using RimWorld;
using Verse;

namespace HiddenEnemyBio;

internal class HiddenBioUtil
{
    public static bool ShouldDefaultBioVisible(Pawn pawn)
    {
        if (pawn?.story == null)
        {
            return false;
        }

        if (PawnUtility.EverBeenColonistOrTameAnimal(pawn))
        {
            return true;
        }

        if (pawn.guest.IsSlave)
        {
            return true;
        }

        switch (pawn.IsPrisoner)
        {
            case true when !pawn.guest.Recruitable:
                return false;
            case true when pawn.guest.resistance <= HiddenEnemyBioMod.Instance.Settings.useVanillaBioResistance:
                return true;
            case true:
                return false;
            default:
                return pawn.Faction == null || pawn.Faction.IsPlayer ||
                       pawn.Faction.PlayerRelationKind != FactionRelationKind.Hostile;
        }
    }

    public static bool ShouldRevealBackstory(Pawn pawn)
    {
        switch (pawn.IsPrisoner)
        {
            case true when !pawn.guest.Recruitable:
            case true when pawn.guest.resistance > HiddenEnemyBioMod.Instance.Settings.revealBackstoryResistance:
                return false;
            default:
                return pawn.IsPrisoner || pawn.Faction == null || pawn.Faction.IsPlayer ||
                       pawn.Faction.PlayerRelationKind != FactionRelationKind.Hostile;
        }
    }

    public static bool ShouldRevealTrait(Pawn pawn, Trait trait)
    {
        if (trait.Suppressed || trait.sourceGene != null)
        {
            return true;
        }

        switch (pawn.IsPrisoner)
        {
            case true when !pawn.guest.Recruitable:
            case true when pawn.guest.resistance > HiddenEnemyBioMod.Instance.Settings.revealTraitsResisitance:
            case false when pawn.Faction is { IsPlayer: false, PlayerRelationKind: FactionRelationKind.Hostile }:
                return false;
            default:
                return true;
        }
    }

    public static bool ShouldRevealIncapable(Pawn pawn)
    {
        return ShouldRevealBackstory(pawn);
    }

    public static bool ShouldRevealPassionSkills(Pawn pawn)
    {
        switch (pawn.IsPrisoner)
        {
            case true when !pawn.guest.Recruitable:
            case true when pawn.guest.resistance > HiddenEnemyBioMod.Instance.Settings.revealPassionSkillsResistance:
            case false when pawn.Faction is { IsPlayer: false, PlayerRelationKind: FactionRelationKind.Hostile }:
                return false;
            default:
                return true;
        }
    }

    public static bool ShouldRevealSkills(Pawn pawn)
    {
        switch (pawn.IsPrisoner)
        {
            case true when !pawn.guest.Recruitable:
            case true when pawn.guest.resistance > HiddenEnemyBioMod.Instance.Settings.revealSkillsResistance:
            case false when pawn.Faction is { IsPlayer: false, PlayerRelationKind: FactionRelationKind.Hostile }:
                return false;
            default:
                return true;
        }
    }
}