﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HiddenEnemyBio
{
    internal class HiddenBioUtil
    {
        public static bool ShouldDefaultBioVisible(Pawn pawn)
        {
            if (pawn == null || pawn?.story == null) return false;

            if (PawnUtility.EverBeenColonistOrTameAnimal(pawn)) return true;

            if (pawn.guest.IsSlave) return true;

            // unwaveringly loyal never gives information
            if (pawn.IsPrisoner && !pawn.guest.Recruitable) return false;

            // prisoners
            if (pawn.IsPrisoner && pawn.guest.resistance <= Settings.useVanillaBioResistance) return true;

            // high resistance prisoners
            if (pawn.IsPrisoner) return false;

            // enemies have hidden information
            if (pawn.Faction != null && !pawn.Faction.IsPlayer && pawn.Faction.PlayerRelationKind == FactionRelationKind.Hostile) return false;
            return true;
        }

        public static bool ShouldRevealBackstory(Pawn pawn)
        {
            if (pawn.IsPrisoner && !pawn.guest.Recruitable) return false;
            if (pawn.IsPrisoner && pawn.guest.resistance > Settings.revealBackstoryResistance) return false;
            else if (!pawn.IsPrisoner && pawn.Faction != null && !pawn.Faction.IsPlayer && pawn.Faction.PlayerRelationKind == FactionRelationKind.Hostile) return false;
            return true;
        }

        public static bool ShouldRevealTrait(Pawn pawn, Trait trait)
        {
            if(trait.Suppressed || trait.sourceGene != null) return true;
            if (pawn.IsPrisoner && !pawn.guest.Recruitable) return false;
            else if (pawn.IsPrisoner && pawn.guest.resistance > Settings.revealTraitsResisitance) return false;
            else if (!pawn.IsPrisoner && pawn.Faction != null && !pawn.Faction.IsPlayer && pawn.Faction.PlayerRelationKind == FactionRelationKind.Hostile) return false;
            return true;
        }

        public static bool ShouldRevealIncapable(Pawn pawn)
        {
            return ShouldRevealBackstory(pawn);
        }

        public static bool ShouldRevealPassionSkills(Pawn pawn)
        {
            if (pawn.IsPrisoner && !pawn.guest.Recruitable) return false;
            if (pawn.IsPrisoner && pawn.guest.resistance > Settings.revealPassionSkillsResistance) return false;
            else if (!pawn.IsPrisoner && pawn.Faction != null && !pawn.Faction.IsPlayer && pawn.Faction.PlayerRelationKind == FactionRelationKind.Hostile) return false;
            return true;
        }

        public static bool ShouldRevealSkills(Pawn pawn)
        {
            if (pawn.IsPrisoner && !pawn.guest.Recruitable) return false;
            if (pawn.IsPrisoner && pawn.guest.resistance > Settings.revealSkillsResistance) return false;
            else if (!pawn.IsPrisoner && pawn.Faction != null && !pawn.Faction.IsPlayer && pawn.Faction.PlayerRelationKind == FactionRelationKind.Hostile) return false;
            return true;
        }
    }
}
