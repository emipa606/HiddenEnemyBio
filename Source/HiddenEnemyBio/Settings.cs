using Verse;

namespace HiddenEnemyBio;

internal class Settings : ModSettings
{
    public float RevealBackstoryResistance = 18f;
    public float RevealPassionSkillsResistance = 18f;
    public float RevealSkillsResistance = 10f;
    public float RevealTraitsResisitance = 15f;
    public float UseVanillaBioResistance = 10f;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref UseVanillaBioResistance, "useVanillaBioResistance", 10f);
        Scribe_Values.Look(ref RevealSkillsResistance, "revealSkillsResistance", 10f);
        Scribe_Values.Look(ref RevealTraitsResisitance, "revealTraitsResistance", 15f);
        Scribe_Values.Look(ref RevealBackstoryResistance, "revealBackstoryResistance", 18f);
        Scribe_Values.Look(ref RevealPassionSkillsResistance, "revealPassionSkillsResistance", 18f);
        base.ExposeData();
    }
}