using Verse;

namespace HiddenEnemyBio;

internal class Settings : ModSettings
{
    public float revealBackstoryResistance = 18f;
    public float revealPassionSkillsResistance = 18f;
    public float revealSkillsResistance = 10f;
    public float revealTraitsResisitance = 15f;
    public float useVanillaBioResistance = 10f;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref useVanillaBioResistance, "useVanillaBioResistance", 10f);
        Scribe_Values.Look(ref revealSkillsResistance, "revealSkillsResistance", 10f);
        Scribe_Values.Look(ref revealTraitsResisitance, "revealTraitsResistance", 15f);
        Scribe_Values.Look(ref revealBackstoryResistance, "revealBackstoryResistance", 18f);
        Scribe_Values.Look(ref revealPassionSkillsResistance, "revealPassionSkillsResistance", 18f);
        base.ExposeData();
    }
}