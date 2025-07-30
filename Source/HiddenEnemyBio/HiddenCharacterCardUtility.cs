using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HiddenEnemyBio;

[StaticConstructorOnStartup]
public static class HiddenCharacterCardUtility
{
    private const float NonArchiteBaselinerChance = 0.5f;

    public const int MainRectsY = 100;

    private const float MainRectsHeight = 355f;

    private const int ConfigRectTitlesHeight = 40;

    private const int FactionIconSize = 22;

    private const int IdeoIconSize = 22;

    private const int GenderIconSize = 22;

    private const float RowHeight = 22f;

    private const float LeftRectHeight = 250f;

    private const float RightRectHeight = 258f;

    public const int MaxNameLength = 12;

    public const int MaxNickLength = 16;

    public const int MaxTitleLength = 25;

    public const int QuestLineHeight = 20;

    public const float RandomizeButtonWidth = 200f;

    public const float HighlightMargin = 6f;

    private const int QuestIconSize = 24;

    private const int QuestIconExtraPaddingLeft = -7;

    private static Vector2 leftRectScrollPos = Vector2.zero;

    private static bool warnedChangingXenotypeWillRandomizePawn;

    private static Rect highlightRect;

    private static readonly Vector2 BasePawnCardSize = new(480f, 455f);

    private static readonly Color FavColorBoxColor = new(0.25f, 0.25f, 0.25f);

    private static readonly Texture2D QuestIcon = ContentFinder<Texture2D>.Get("UI/Icons/Quest");

    private static readonly Texture2D UnrecruitableIcon = ContentFinder<Texture2D>.Get("UI/Icons/UnwaveringlyLoyal");

    private static readonly Color StackElementBackground = new(1f, 1f, 1f, 0.1f);

    private static List<CustomXenotype> cachedCustomXenotypes;

    private static readonly List<ExtraFaction> tmpExtraFactions = [];

    private static readonly Color TitleCausedWorkTagDisableColor = new(0.67f, 0.84f, 0.9f);

    private static readonly List<GenUI.AnonymousStackElement>
        tmpStackElements = [];

    private static readonly StringBuilder tmpInspectStrings = new();

    private static readonly Regex ValidNameRegex = new("^[\\p{L}0-9 '\\-.]*$");

    private static List<CustomXenotype> CustomXenotypes
    {
        get
        {
            if (cachedCustomXenotypes != null)
            {
                return cachedCustomXenotypes;
            }

            cachedCustomXenotypes = [];
            foreach (var item in GenFilePaths.AllCustomXenotypeFiles.OrderBy(f => f.LastWriteTime))
            {
                var filePath = GenFilePaths.AbsFilePathForXenotype(Path.GetFileNameWithoutExtension(item.Name));
                PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Xenotype,
                    delegate
                    {
                        if (GameDataSaveLoader.TryLoadXenotype(filePath, out var xenotype))
                        {
                            cachedCustomXenotypes.Add(xenotype);
                        }
                    }, true);
            }

            return cachedCustomXenotypes;
        }
    }

    public static List<CustomXenotype> CustomXenotypesForReading => CustomXenotypes;

    public static void DrawCharacterCard(Rect rect, Pawn pawn, Action randomizeCallback = null,
        Rect creationRect = default, bool showName = true)
    {
        Widgets.BeginGroup(randomizeCallback != null ? creationRect : rect);
        var rect3 = new Rect(0f, 0f, 300f, showName ? 30 : 0);
        if (showName)
        {
            if (randomizeCallback != null && pawn.Name is NameTriple nameTriple)
            {
                var rect4 = new Rect(rect3);
                rect4.width *= 0.333f;
                var rect5 = new Rect(rect3);
                rect5.width *= 0.333f;
                rect5.x += rect5.width;
                var rect6 = new Rect(rect3);
                rect6.width *= 0.333f;
                rect6.x += rect5.width * 2f;
                var name = nameTriple.First;
                var name2 = nameTriple.Nick;
                var name3 = nameTriple.Last;
                DoNameInputRect(rect4, ref name, 12);
                if (nameTriple.Nick == nameTriple.First || nameTriple.Nick == nameTriple.Last)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                }

                DoNameInputRect(rect5, ref name2, 16);
                GUI.color = Color.white;
                DoNameInputRect(rect6, ref name3, 12);
                if (nameTriple.First != name || nameTriple.Nick != name2 || nameTriple.Last != name3)
                {
                    pawn.Name = new NameTriple(name, string.IsNullOrEmpty(name2) ? name : name2, name3);
                }

                TooltipHandler.TipRegionByKey(rect4, "FirstNameDesc");
                TooltipHandler.TipRegionByKey(rect5, "ShortIdentifierDesc");
                TooltipHandler.TipRegionByKey(rect6, "LastNameDesc");
            }
            else
            {
                rect3.width = 999f;
                Text.Font = GameFont.Medium;
                var text = pawn.Name.ToStringFull.CapitalizeFirst();
                Widgets.Label(rect3, text);
                if (pawn.guilt is { IsGuilty: true })
                {
                    var x = Text.CalcSize(text).x;
                    var rect7 = new Rect(x + 10f, 0f, 32f, 32f);
                    GUI.DrawTexture(rect7, TexUI.GuiltyTex);
                    TooltipHandler.TipRegion(rect7, () => pawn.guilt.Tip, 6321623);
                }

                Text.Font = GameFont.Small;
            }
        }

        var allowsChildSelection = ScenarioUtility.AllowsChildSelection(Find.Scenario);
        if (ModsConfig.BiotechActive && randomizeCallback != null)
        {
            Widgets.DrawHighlight(highlightRect.ExpandedBy(6f));
        }

        if (randomizeCallback != null)
        {
            var rect8 = new Rect(creationRect.width - 200f - 6f, 6f, 200f, rect3.height);
            if (Widgets.ButtonText(rect8, "Randomize".Translate()))
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                randomizeCallback();
            }

            UIHighlighter.HighlightOpportunity(rect8, "RandomizePawn");
            if (ModsConfig.BiotechActive)
            {
                LifestageAndXenotypeOptions(pawn, rect8, allowsChildSelection, randomizeCallback);
            }
        }

        if (randomizeCallback != null)
        {
            Widgets.InfoCardButton(rect3.xMax + 4f, (rect3.height - 24f) / 2f, pawn);
        }
        else if (!pawn.health.Dead)
        {
            var num = PawnCardSize(pawn).x - 85f;
            if (pawn.IsFreeColonist && pawn.Spawned && !pawn.IsQuestLodger() && showName)
            {
                var rect9 = new Rect(num, 0f, 30f, 30f);
                if (Mouse.IsOver(rect9))
                {
                    TooltipHandler.TipRegion(rect9, PawnBanishUtility.GetBanishButtonTip(pawn));
                }

                if (Widgets.ButtonImage(rect9, TexButton.Banish))
                {
                    if (pawn.Downed)
                    {
                        Messages.Message(
                            "MessageCantBanishDownedPawn".Translate(pawn.LabelShort, pawn).AdjustedFor(pawn), pawn,
                            MessageTypeDefOf.RejectInput, false);
                    }
                    else
                    {
                        PawnBanishUtility.ShowBanishPawnConfirmationDialog(pawn);
                    }
                }

                num -= 40f;
            }

            if ((pawn.IsColonist || DebugSettings.ShowDevGizmos) && showName)
            {
                var rect10 = new Rect(num, 0f, 30f, 30f);
                TooltipHandler.TipRegionByKey(rect10, "RenameColonist");
                if (Widgets.ButtonImage(rect10, TexButton.Rename))
                {
                    Find.WindowStack.Add(pawn.NamePawnDialog());
                }

                num -= 40f;
            }

            if (pawn.IsFreeColonist && !pawn.IsQuestLodger() && pawn.royalty != null &&
                pawn.royalty.AllTitlesForReading.Count > 0)
            {
                var rect11 = new Rect(num, 0f, 30f, 30f);
                TooltipHandler.TipRegionByKey(rect11, "RenounceTitle");
                if (Widgets.ButtonImage(rect11, TexButton.RenounceTitle))
                {
                    FloatMenuUtility.MakeMenu(pawn.royalty.AllTitlesForReading,
                        title => "RenounceTitle".Translate() + ": " +
                                 "TitleOfFaction".Translate(title.def.GetLabelCapFor(pawn),
                                     title.faction.GetCallLabel()), delegate(RoyalTitle title)
                        {
                            return delegate
                            {
                                var list = pawn.royalty.PermitsFromFaction(title.faction);
                                RoyalTitleUtility.FindLostAndGainedPermits(title.def, null, out _,
                                    out var lostPermits);
                                var stringBuilder = new StringBuilder();
                                if (lostPermits.Count > 0 || list.Count > 0)
                                {
                                    stringBuilder.AppendLine(
                                        "RenounceTitleWillLoosePermits".Translate(pawn.Named("PAWN")) + ":");
                                    foreach (var item in lostPermits)
                                    {
                                        stringBuilder.AppendLine("- " + item.LabelCap + " (" +
                                                                 FirstTitleWithPermit(item).GetLabelFor(pawn) + ")");
                                    }

                                    foreach (var item2 in list)
                                    {
                                        stringBuilder.AppendLine("- " + item2.Permit.LabelCap + " (" +
                                                                 item2.Title.GetLabelFor(pawn) + ")");
                                    }

                                    stringBuilder.AppendLine();
                                }

                                var permitPoints = pawn.royalty.GetPermitPoints(title.faction);
                                if (permitPoints > 0)
                                {
                                    stringBuilder.AppendLineTagged("RenounceTitleWillLosePermitPoints".Translate(
                                        pawn.Named("PAWN"), permitPoints.Named("POINTS"),
                                        title.faction.Named("FACTION")));
                                }

                                if (pawn.abilities.abilities.Any())
                                {
                                    stringBuilder.AppendLine();
                                    stringBuilder.AppendLineTagged(
                                        "RenounceTitleWillKeepPsylinkLevels".Translate(pawn.Named("PAWN")));
                                }

                                if (!title.faction.def.renounceTitleMessage.NullOrEmpty())
                                {
                                    stringBuilder.AppendLine();
                                    stringBuilder.AppendLine(title.faction.def.renounceTitleMessage);
                                }

                                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                                    "RenounceTitleDescription".Translate(pawn.Named("PAWN"),
                                        "TitleOfFaction".Translate(title.def.GetLabelCapFor(pawn),
                                            title.faction.GetCallLabel()).Named("TITLE"),
                                        stringBuilder.ToString().TrimEndNewlines().Named("EFFECTS")), delegate
                                    {
                                        pawn.royalty.SetTitle(title.faction, null, false);
                                        pawn.royalty.ResetPermitsAndPoints(title.faction, title.def);
                                    }, true));
                            };

                            RoyalTitleDef FirstTitleWithPermit(RoyalTitlePermitDef permitDef)
                            {
                                return title.faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.First(t =>
                                    t.permits != null && t.permits.Contains(permitDef));
                            }
                        });
                }

                num -= 40f;
            }

            if (pawn.guilt is { IsGuilty: true } && pawn.IsFreeColonist && !pawn.IsQuestLodger())
            {
                var rect12 = new Rect(num + 5f, 0f, 30f, 30f);
                TooltipHandler.TipRegionByKey(rect12, "ExecuteColonist");
                if (Widgets.ButtonImage(rect12, TexButton.ExecuteColonist))
                {
                    pawn.guilt.awaitingExecution = !pawn.guilt.awaitingExecution;
                    if (pawn.guilt.awaitingExecution)
                    {
                        Messages.Message("MessageColonistMarkedForExecution".Translate(pawn), pawn,
                            MessageTypeDefOf.SilentInput, false);
                    }
                }

                if (pawn.guilt.awaitingExecution)
                {
                    var position = default(Rect);
                    position.x += rect12.x + 22f;
                    position.width = 15f;
                    position.height = 15f;
                    GUI.DrawTexture(position, Widgets.CheckboxOnTex);
                }
            }
        }

        var num2 = rect3.height + 10f;
        var num3 = num2;
        num2 = DoTopStack(pawn, rect, randomizeCallback != null, num2);
        if (num2 - num3 < 78f)
        {
            num2 += 15f;
        }

        var leftRect = new Rect(0f, num2, 250f, (randomizeCallback != null ? creationRect : rect).height - num2);
        DoLeftSection(rect, leftRect, pawn);
        var rect13 = new Rect(leftRect.xMax, num2, 258f,
            (randomizeCallback != null ? creationRect : rect).height - num2);
        if (HiddenBioUtil.ShouldRevealSkills(pawn) || HiddenBioUtil.ShouldRevealPassionSkills(pawn))
        {
            Widgets.BeginGroup(rect13);
            var mode = Current.ProgramState != ProgramState.Playing
                ? SkillUI.SkillDrawMode.Menu
                : SkillUI.SkillDrawMode.Gameplay;
            SkillUI.DrawSkillsOf(pawn, Vector2.zero, mode, rect13);
            Widgets.EndGroup();
        }
        else
        {
            Widgets.Label(rect13, "HEB.UnknownSkillsPassions".Translate().Colorize(ColoredText.SubtleGrayColor));
            TooltipHandler.TipRegion(rect13,
                "HEB.UnknownSkillsPassionsTip".Translate(pawn.Named("PAWN"),
                    HiddenEnemyBioMod.Instance.Settings.RevealPassionSkillsResistance));
        }

        Widgets.EndGroup();
    }

    private static string GetTitleTipString(Pawn pawn, Faction faction, RoyalTitle title, int favor)
    {
        var def = title.def;
        var taggedString = "RoyalTitleTooltipHasTitle".Translate(pawn.Named("PAWN"), faction.Named("FACTION"),
            def.GetLabelCapFor(pawn).Named("TITLE"));
        taggedString += "\n\n" + faction.def.royalFavorLabel.CapitalizeFirst() + ": " + favor;
        var nextTitle = def.GetNextTitle(faction);
        if (nextTitle != null)
        {
            taggedString += "\n" + "RoyalTitleTooltipNextTitle".Translate() + ": " + nextTitle.GetLabelCapFor(pawn) +
                            " (" + "RoyalTitleTooltipNextTitleFavorCost".Translate(nextTitle.favorCost.ToString(),
                                faction.Named("FACTION")) + ")";
        }
        else
        {
            taggedString += "\n" + "RoyalTitleTooltipFinalTitle".Translate();
        }

        if (title.def.canBeInherited)
        {
            var heir = pawn.royalty.GetHeir(faction);
            if (heir != null)
            {
                taggedString +=
                    "\n\n" + "RoyalTitleTooltipInheritance".Translate(pawn.Named("PAWN"), heir.Named("HEIR"));
                if (heir.Faction == null)
                {
                    taggedString += " " + "RoyalTitleTooltipHeirNoFaction".Translate(heir.Named("HEIR"));
                }
                else if (heir.Faction != faction)
                {
                    taggedString += " " +
                                    "RoyalTitleTooltipHeirDifferentFaction".Translate(heir.Named("HEIR"),
                                        heir.Faction.Named("FACTION"));
                }
            }
            else
            {
                taggedString += "\n\n" + "RoyalTitleTooltipNoHeir".Translate(pawn.Named("PAWN"));
            }
        }
        else
        {
            taggedString += "\n\n" +
                            "LetterRoyalTitleCantBeInherited".Translate(title.def.Named("TITLE")).CapitalizeFirst() +
                            " " + "LetterRoyalTitleNoHeir".Translate(pawn.Named("PAWN"));
        }

        taggedString += "\n\n" +
                        (title.conceited ? "RoyalTitleTooltipConceited" : "RoyalTitleTooltipNonConceited").Translate(
                            pawn.Named("PAWN"));
        taggedString += "\n\n" + RoyalTitleUtility.GetTitleProgressionInfo(faction, pawn);
        return (taggedString + ("\n\n" + "ClickToLearnMore".Translate().Colorize(ColoredText.SubtleGrayColor)))
            .Resolve();
    }

    private static List<object> GetWorkTypeDisableCauses(Pawn pawn, WorkTags workTag)
    {
        var list = new List<object>();
        if (pawn.story is { Childhood: not null } && (pawn.story.Childhood.workDisables & workTag) != 0)
        {
            list.Add(pawn.story.Childhood);
        }

        if (pawn.story is { Adulthood: not null } && (pawn.story.Adulthood.workDisables & workTag) != 0)
        {
            list.Add(pawn.story.Adulthood);
        }

        if (pawn.health is { hediffSet: not null })
        {
            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                var curStage = hediff.CurStage;
                if (curStage != null && (curStage.disabledWorkTags & workTag) != 0)
                {
                    list.Add(hediff);
                }
            }
        }

        if (pawn.story.traits != null)
        {
            foreach (var trait in pawn.story.traits.allTraits)
            {
                if (trait.Suppressed)
                {
                    continue;
                }

                if ((trait.def.disabledWorkTags & workTag) != 0)
                {
                    list.Add(trait);
                }
            }
        }

        if (pawn.royalty != null)
        {
            foreach (var item in pawn.royalty.AllTitlesForReading)
            {
                if (item.conceited && (item.def.disabledWorkTags & workTag) != 0)
                {
                    list.Add(item);
                }
            }
        }

        if (ModsConfig.IdeologyActive && pawn.Ideo != null)
        {
            var role = pawn.Ideo.GetRole(pawn);
            if (role != null && (role.def.roleDisabledWorkTags & workTag) != 0)
            {
                list.Add(role);
            }
        }

        if (ModsConfig.BiotechActive && pawn.genes != null)
        {
            foreach (var item2 in pawn.genes.GenesListForReading)
            {
                if (item2.Active && (item2.def.disabledWorkTags & workTag) != 0)
                {
                    list.Add(item2);
                }
            }
        }

        foreach (var item3 in QuestUtility.GetWorkDisabledQuestPart(pawn))
        {
            if ((item3.disabledWorkTags & workTag) != 0 && !list.Contains(item3.quest))
            {
                list.Add(item3.quest);
            }
        }

        return list;
    }

    private static Color GetDisabledWorkTagLabelColor(Pawn pawn, WorkTags workTag)
    {
        foreach (var workTypeDisableCause in GetWorkTypeDisableCauses(pawn, workTag))
        {
            if (workTypeDisableCause is RoyalTitleDef)
            {
                return TitleCausedWorkTagDisableColor;
            }
        }

        return Color.white;
    }

    private static void LifestageAndXenotypeOptions(Pawn pawn, Rect randomizeRect, bool allowsChildSelection,
        Action randomizeCallback)
    {
        highlightRect = randomizeRect;
        highlightRect.yMax += randomizeRect.height + Text.LineHeight + 8f;
        var startingPawnIndex = StartingPawnUtility.PawnIndex(pawn);
        var width = (randomizeRect.width - 4f) / 2f;
        var x2 = randomizeRect.x;
        var rect = new Rect(x2, randomizeRect.y + randomizeRect.height + 4f, width, randomizeRect.height);
        x2 += rect.width + 4f;
        Text.Anchor = TextAnchor.MiddleCenter;
        var rect2 = rect;
        rect2.y += rect.height + 4f;
        rect2.height = Text.LineHeight;
        Widgets.Label(rect2, pawn.DevelopmentalStage.ToString().Translate().CapitalizeFirst());
        Text.Anchor = TextAnchor.UpperLeft;
        var rect3 = new Rect(rect.x, rect.y, rect.width, rect2.yMax - rect.yMin);
        if (Mouse.IsOver(rect3))
        {
            Widgets.DrawHighlight(rect3);
            if (Find.WindowStack.FloatMenu == null)
            {
                var taggedString = GetLabel().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" +
                                   "DevelopmentalAgeSelectionDesc".Translate();
                if (!allowsChildSelection)
                {
                    taggedString += "\n\n" + "MessageDevelopmentalStageSelectionDisabledByScenario".Translate()
                        .Colorize(ColorLibrary.RedReadable);
                }

                TooltipHandler.TipRegion(rect3, taggedString.Resolve());
            }
        }

        if (Widgets.ButtonImageWithBG(rect, GetDevelopmentalStageIcon(), new Vector2(22f, 22f)))
        {
            if (allowsChildSelection)
            {
                var index2 = startingPawnIndex;
                var existing2 = StartingPawnUtility.GetGenerationRequest(index2);
                var options = new List<FloatMenuOption>
                {
                    new("Adult".Translate().CapitalizeFirst(), delegate
                    {
                        if (existing2.AllowedDevelopmentalStages.Has(DevelopmentalStage.Adult))
                        {
                            return;
                        }

                        existing2.AllowedDevelopmentalStages = DevelopmentalStage.Adult;
                        existing2.AllowDowned = false;
                        StartingPawnUtility.SetGenerationRequest(index2, existing2);
                        randomizeCallback();
                    }, DevelopmentalStageExtensions.AdultTex.Texture, Color.white),
                    new("Child".Translate().CapitalizeFirst(), delegate
                    {
                        if (existing2.AllowedDevelopmentalStages.Has(DevelopmentalStage.Child))
                        {
                            return;
                        }

                        existing2.AllowedDevelopmentalStages = DevelopmentalStage.Child;
                        existing2.AllowDowned = false;
                        StartingPawnUtility.SetGenerationRequest(index2, existing2);
                        randomizeCallback();
                    }, DevelopmentalStageExtensions.ChildTex.Texture, Color.white),
                    new("Baby".Translate().CapitalizeFirst(), delegate
                    {
                        if (existing2.AllowedDevelopmentalStages.Has(DevelopmentalStage.Baby))
                        {
                            return;
                        }

                        existing2.AllowedDevelopmentalStages = DevelopmentalStage.Baby;
                        existing2.AllowDowned = true;
                        StartingPawnUtility.SetGenerationRequest(index2, existing2);
                        randomizeCallback();
                    }, DevelopmentalStageExtensions.BabyTex.Texture, Color.white)
                };
                Find.WindowStack.Add(new FloatMenu(options));
            }
            else
            {
                Messages.Message("MessageDevelopmentalStageSelectionDisabledByScenario".Translate(), null,
                    MessageTypeDefOf.RejectInput, false);
            }
        }

        var rect4 = new Rect(x2, randomizeRect.y + randomizeRect.height + 4f, width, randomizeRect.height);
        Text.Anchor = TextAnchor.MiddleCenter;
        var rect5 = rect4;
        rect5.y += rect4.height + 4f;
        rect5.height = Text.LineHeight;
        Widgets.Label(rect5, GetXenotypeLabel(startingPawnIndex).Truncate(rect5.width));
        Text.Anchor = TextAnchor.UpperLeft;
        var rect6 = new Rect(rect4.x, rect4.y, rect4.width, rect5.yMax - rect4.yMin);
        if (Mouse.IsOver(rect6))
        {
            Widgets.DrawHighlight(rect6);
            if (Find.WindowStack.FloatMenu == null)
            {
                TooltipHandler.TipRegion(rect6,
                    GetXenotypeLabel(startingPawnIndex).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" +
                    "XenotypeSelectionDesc".Translate());
            }
        }

        if (!Widgets.ButtonImageWithBG(rect4, GetXenotypeIcon(startingPawnIndex), new Vector2(22f, 22f)))
        {
            return;
        }

        var list = new List<FloatMenuOption>
        {
            new("AnyNonArchite".Translate().CapitalizeFirst(), delegate
            {
                var allowedXenotypes = DefDatabase<XenotypeDef>.AllDefs
                    .Where(x => !x.Archite && x != XenotypeDefOf.Baseliner).ToList();
                SetupGenerationRequest(startingPawnIndex, null, null, allowedXenotypes, 0.5f,
                    existing => existing.ForcedXenotype != null || existing.ForcedCustomXenotype != null,
                    randomizeCallback, false);
            }),
            new("XenotypeEditor".Translate() + "...", delegate
            {
                Find.WindowStack.Add(new Dialog_CreateXenotype(startingPawnIndex, delegate
                {
                    cachedCustomXenotypes = null;
                    randomizeCallback();
                }));
            })
        };
        foreach (var item in DefDatabase<XenotypeDef>.AllDefs.OrderBy(x => 0f - x.displayPriority))
        {
            var xenotype = item;
            list.Add(new FloatMenuOption(xenotype.LabelCap,
                delegate
                {
                    SetupGenerationRequest(startingPawnIndex, xenotype, null, null, 0f,
                        existing => existing.ForcedXenotype != xenotype, randomizeCallback);
                }, xenotype.Icon, XenotypeDef.IconColor, MenuOptionPriority.Default,
                delegate(Rect r) { TooltipHandler.TipRegion(r, xenotype.descriptionShort ?? xenotype.description); },
                null, 24f, r => Widgets.InfoCardButton(r.x, r.y + 3f, xenotype), extraPartRightJustified: true));
        }

        foreach (var customXenotype in CustomXenotypes)
        {
            var customInner = customXenotype;
            list.Add(new FloatMenuOption(customInner.name.CapitalizeFirst() + " (" + "Custom".Translate() + ")",
                delegate
                {
                    SetupGenerationRequest(startingPawnIndex, null, customInner, null, 0f,
                        existing => existing.ForcedCustomXenotype != customInner, randomizeCallback);
                }, customInner.IconDef.Icon, XenotypeDef.IconColor, MenuOptionPriority.Default, null, null, 24f,
                delegate(Rect r)
                {
                    if (!Widgets.ButtonImage(new Rect(r.x, r.y + ((r.height - r.width) / 2f), r.width, r.width),
                            TexButton.Delete, GUI.color))
                    {
                        return false;
                    }

                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "ConfirmDelete".Translate(customInner.name.CapitalizeFirst()), delegate
                        {
                            var path = GenFilePaths.AbsFilePathForXenotype(customInner.name);
                            if (!File.Exists(path))
                            {
                                return;
                            }

                            File.Delete(path);
                            cachedCustomXenotypes = null;
                        }, true));
                    return true;
                }, extraPartRightJustified: true));
        }

        Find.WindowStack.Add(new FloatMenu(list));
        return;

        Texture2D GetDevelopmentalStageIcon()
        {
            return StartingPawnUtility.GetGenerationRequest(startingPawnIndex).AllowedDevelopmentalStages.Icon()
                .Texture;
        }

        string GetLabel()
        {
            var generationRequest = StartingPawnUtility.GetGenerationRequest(startingPawnIndex);
            if (generationRequest.AllowedDevelopmentalStages.Has(DevelopmentalStage.Baby))
            {
                return "Baby".Translate();
            }

            return generationRequest.AllowedDevelopmentalStages.Has(DevelopmentalStage.Child)
                ? "Child".Translate()
                : "Adult".Translate();
        }
    }

    private static void SetupGenerationRequest(int index, XenotypeDef xenotype, CustomXenotype customXenotype,
        List<XenotypeDef> allowedXenotypes, float forceBaselinerChance, Func<PawnGenerationRequest, bool> validator,
        Action randomizeCallback, bool randomize = true)
    {
        var existing = StartingPawnUtility.GetGenerationRequest(index);
        if (!validator(existing))
        {
            return;
        }

        if (!warnedChangingXenotypeWillRandomizePawn && randomize)
        {
            Find.WindowStack.Add(new Dialog_MessageBox("WarnChangingXenotypeWillRandomizePawn".Translate(),
                "Yes".Translate(), delegate
                {
                    warnedChangingXenotypeWillRandomizePawn = true;
                    existing.ForcedXenotype = xenotype;
                    existing.ForcedCustomXenotype = customXenotype;
                    existing.AllowedXenotypes = allowedXenotypes;
                    existing.ForceBaselinerChance = forceBaselinerChance;
                    StartingPawnUtility.SetGenerationRequest(index, existing);
                    randomizeCallback();
                }, "No".Translate()));
        }
        else
        {
            existing.ForcedXenotype = xenotype;
            existing.ForcedCustomXenotype = customXenotype;
            existing.AllowedXenotypes = allowedXenotypes;
            existing.ForceBaselinerChance = forceBaselinerChance;
            StartingPawnUtility.SetGenerationRequest(index, existing);
            if (randomize)
            {
                randomizeCallback();
            }
        }
    }

    private static string GetXenotypeLabel(int startingPawnIndex)
    {
        var generationRequest = StartingPawnUtility.GetGenerationRequest(startingPawnIndex);
        if (generationRequest.ForcedCustomXenotype != null)
        {
            return generationRequest.ForcedCustomXenotype.name.CapitalizeFirst();
        }

        return generationRequest.ForcedXenotype?.LabelCap ?? "AnyLower".Translate().CapitalizeFirst();
    }

    private static Texture2D GetXenotypeIcon(int startingPawnIndex)
    {
        var generationRequest = StartingPawnUtility.GetGenerationRequest(startingPawnIndex);
        if (generationRequest.ForcedXenotype != null)
        {
            return generationRequest.ForcedXenotype.Icon;
        }

        return generationRequest.ForcedCustomXenotype != null
            ? generationRequest.ForcedCustomXenotype.IconDef.Icon
            : GeneUtility.UniqueXenotypeTex.Texture;
    }

    private static float DoTopStack(Pawn pawn, Rect rect, bool creationMode, float curY)
    {
        tmpStackElements.Clear();
        var num = rect.width - 10f;
        var width = creationMode ? num - 20f - Page_ConfigureStartingPawns.PawnPortraitSize.x : num;
        Text.Font = GameFont.Small;
        var mainDesc = pawn.MainDesc(false, !(ModsConfig.BiotechActive && creationMode));
        if (ModsConfig.BiotechActive && creationMode)
        {
            tmpStackElements.Add(new GenUI.AnonymousStackElement
            {
                drawer = delegate(Rect r)
                {
                    GUI.DrawTexture(r, pawn.gender.GetIcon());
                    if (Mouse.IsOver(r))
                    {
                        TooltipHandler.TipRegion(r,
                            () => pawn.gender.GetLabel(pawn.AnimalOrWildMan()).CapitalizeFirst(), 7594764);
                    }
                },
                width = 22f
            });
        }

        tmpStackElements.Add(new GenUI.AnonymousStackElement
        {
            drawer = delegate(Rect r)
            {
                Widgets.Label(r, mainDesc);
                if (Mouse.IsOver(r))
                {
                    TooltipHandler.TipRegion(r, () => pawn.ageTracker.AgeTooltipString, 6873641);
                }
            },
            width = Text.CalcSize(mainDesc).x + 5f
        });
        if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.GenesListForReading.Any())
        {
            var num2 = 22f;
            num2 += Text.CalcSize(pawn.genes.XenotypeLabelCap).x + 14f;
            tmpStackElements.Add(new GenUI.AnonymousStackElement
            {
                drawer = delegate(Rect r)
                {
                    var rect11 = new Rect(r.x, r.y, r.width, r.height);
                    GUI.color = StackElementBackground;
                    GUI.DrawTexture(rect11, BaseContent.WhiteTex);
                    GUI.color = Color.white;
                    if (Mouse.IsOver(rect11))
                    {
                        Widgets.DrawHighlight(rect11);
                    }

                    var position5 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
                    GUI.color = XenotypeDef.IconColor;
                    GUI.DrawTexture(position5, pawn.genes.XenotypeIcon);
                    GUI.color = Color.white;
                    Widgets.Label(new Rect(r.x + 22f + 5f, r.y, r.width + 22f - 1f, r.height),
                        pawn.genes.XenotypeLabelCap);
                    if (Mouse.IsOver(r))
                    {
                        TooltipHandler.TipRegion(r, () =>
                            ("Xenotype".Translate() + ": " + pawn.genes.XenotypeLabelCap).Colorize(ColoredText
                                .TipSectionTitleColor) + "\n\n" + pawn.genes.XenotypeDescShort + "\n\n" +
                            "ViewGenesDesc".Translate(pawn.Named("PAWN")).ToString().StripTags()
                                .Colorize(ColoredText.SubtleGrayColor), 883938493);
                    }

                    if (!Widgets.ButtonInvisible(r))
                    {
                        return;
                    }

                    if (Current.ProgramState == ProgramState.Playing &&
                        Find.WindowStack.WindowOfType<Dialog_InfoCard>() == null &&
                        Find.WindowStack.WindowOfType<Dialog_GrowthMomentChoices>() == null)
                    {
                        InspectPaneUtility.OpenTab(typeof(ITab_Genes));
                    }
                    else
                    {
                        Find.WindowStack.Add(new Dialog_ViewGenes(pawn));
                    }
                },
                width = num2
            });
            curY += GenUI.DrawElementStack(new Rect(0f, curY, width, 50f), 22f, tmpStackElements,
                    delegate(Rect r, GenUI.AnonymousStackElement obj) { obj.drawer(r); }, obj => obj.width,
                    allowOrderOptimization: false)
                .height + 4f;
            tmpStackElements.Clear();
        }

        if (pawn.Faction is { Hidden: false })
        {
            tmpStackElements.Add(new GenUI.AnonymousStackElement
            {
                drawer = delegate(Rect r)
                {
                    var rect9 = new Rect(r.x, r.y, r.width, r.height);
                    var color6 = GUI.color;
                    GUI.color = StackElementBackground;
                    GUI.DrawTexture(rect9, BaseContent.WhiteTex);
                    GUI.color = color6;
                    Widgets.DrawHighlightIfMouseover(rect9);
                    var rect10 = new Rect(r.x, r.y, r.width, r.height);
                    var position4 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
                    GUI.color = pawn.Faction.Color;
                    GUI.DrawTexture(position4, pawn.Faction.def.FactionIcon);
                    GUI.color = color6;
                    Widgets.Label(new Rect(rect10.x + rect10.height + 5f, rect10.y, rect10.width - 10f, rect10.height),
                        pawn.Faction.Name);
                    if (Widgets.ButtonInvisible(rect9))
                    {
                        if (creationMode || Find.WindowStack.AnyWindowAbsorbingAllInput)
                        {
                            Find.WindowStack.Add(new Dialog_FactionDuringLanding());
                        }
                        else
                        {
                            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Factions);
                        }
                    }

                    if (!Mouse.IsOver(rect9))
                    {
                        return;
                    }

                    var text = "Faction".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" +
                               "FactionDesc".Translate(pawn.Named("PAWN")).Resolve() + "\n\n" +
                               "ClickToViewFactions".Translate().Colorize(ColoredText.SubtleGrayColor);
                    var tip4 = new TipSignal(text, pawn.Faction.loadID * 37);
                    TooltipHandler.TipRegion(rect9, tip4);
                },
                width = Text.CalcSize(pawn.Faction.Name).x + 22f + 15f
            });
        }

        tmpExtraFactions.Clear();
        QuestUtility.GetExtraFactionsFromQuestParts(pawn, tmpExtraFactions);
        GuestUtility.GetExtraFactionsFromGuestStatus(pawn, tmpExtraFactions);
        foreach (var tmpExtraFaction in tmpExtraFactions)
        {
            if (pawn.Faction == tmpExtraFaction.faction)
            {
                continue;
            }

            var localExtraFaction = tmpExtraFaction;
            var factionName = localExtraFaction.faction.Name;
            var drawExtraFactionIcon = localExtraFaction.factionType == ExtraFactionType.HomeFaction ||
                                       localExtraFaction.factionType == ExtraFactionType.MiniFaction;
            tmpStackElements.Add(new GenUI.AnonymousStackElement
            {
                drawer = delegate(Rect r)
                {
                    var rect6 = new Rect(r.x, r.y, r.width, r.height);
                    var rect7 = drawExtraFactionIcon ? rect6 : r;
                    var color5 = GUI.color;
                    GUI.color = StackElementBackground;
                    GUI.DrawTexture(rect7, BaseContent.WhiteTex);
                    GUI.color = color5;
                    Widgets.DrawHighlightIfMouseover(rect7);
                    if (drawExtraFactionIcon)
                    {
                        var rect8 = new Rect(r.x, r.y, r.width, r.height);
                        var position3 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
                        GUI.color = localExtraFaction.faction.Color;
                        GUI.DrawTexture(position3, localExtraFaction.faction.def.FactionIcon);
                        GUI.color = color5;
                        Widgets.Label(new Rect(rect8.x + rect8.height + 5f, rect8.y, rect8.width - 10f, rect8.height),
                            factionName);
                    }
                    else
                    {
                        Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), factionName);
                    }

                    if (Widgets.ButtonInvisible(rect6))
                    {
                        Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Factions);
                    }

                    if (!Mouse.IsOver(rect7))
                    {
                        return;
                    }

                    var tip3 = new TipSignal(
                        (localExtraFaction.factionType.GetLabel().CapitalizeFirst()
                             .Colorize(ColoredText.TipSectionTitleColor) + "\n\n" +
                         "ExtraFactionDesc".Translate(pawn.Named("PAWN")) + "\n\n" +
                         "ClickToViewFactions".Translate().Colorize(ColoredText.SubtleGrayColor)).Resolve(),
                        localExtraFaction.faction.loadID ^ 0x738AC053);
                    TooltipHandler.TipRegion(rect7, tip3);
                },
                width = Text.CalcSize(factionName).x + (drawExtraFactionIcon ? 22 : 0) + 15f
            });
        }

        if (!Find.IdeoManager.classicMode && pawn.Ideo != null && ModsConfig.IdeologyActive)
        {
            var width2 = Text.CalcSize(pawn.Ideo.name).x + 22f + 15f;
            tmpStackElements.Add(new GenUI.AnonymousStackElement
            {
                drawer = delegate(Rect r)
                {
                    GUI.color = StackElementBackground;
                    GUI.DrawTexture(r, BaseContent.WhiteTex);
                    GUI.color = Color.white;
                    IdeoUIUtility.DrawIdeoPlate(r, pawn.Ideo, pawn);
                },
                width = width2
            });
        }

        if (ModsConfig.IdeologyActive)
        {
            var role = pawn.Ideo?.GetRole(pawn);
            if (role != null)
            {
                var roleLabel = role.LabelForPawn(pawn);
                var y = curY;
                tmpStackElements.Add(new GenUI.AnonymousStackElement
                {
                    drawer = delegate(Rect r)
                    {
                        var color4 = GUI.color;
                        var rect4 = new Rect(r.x, r.y, r.width, r.height);
                        GUI.color = StackElementBackground;
                        GUI.DrawTexture(rect4, BaseContent.WhiteTex);
                        GUI.color = color4;
                        if (Mouse.IsOver(rect4))
                        {
                            Widgets.DrawHighlight(rect4);
                        }

                        var rect5 = new Rect(r.x, r.y, r.width + 22f + 9f, r.height);
                        var position2 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
                        GUI.color = pawn.Ideo.Color;
                        GUI.DrawTexture(position2, role.Icon);
                        GUI.color = Color.white;
                        Widgets.Label(new Rect(rect5.x + 22f + 5f, rect5.y, rect5.width - 10f, rect5.height),
                            roleLabel);
                        if (Widgets.ButtonInvisible(rect4))
                        {
                            InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Social));
                        }

                        if (!Mouse.IsOver(rect4))
                        {
                            return;
                        }

                        var tip2 = new TipSignal(() => role.GetTip(), (int)y * 39);
                        TooltipHandler.TipRegion(rect4, tip2);
                    },
                    width = Text.CalcSize(roleLabel).x + 22f + 14f
                });
            }
        }

        int count;
        if (pawn.royalty != null && pawn.royalty.AllTitlesForReading.Count > 0)
        {
            foreach (var title in pawn.royalty.AllTitlesForReading)
            {
                var localTitle = title;
                var labelCapFor = localTitle.def.GetLabelCapFor(pawn);
                count = pawn.royalty.GetFavor(localTitle.faction);
                var titleLabel = labelCapFor + " (" + count + ")";
                var y = curY;
                tmpStackElements.Add(new GenUI.AnonymousStackElement
                {
                    drawer = delegate(Rect r)
                    {
                        var color3 = GUI.color;
                        var rect2 = new Rect(r.x, r.y, r.width, r.height);
                        GUI.color = StackElementBackground;
                        GUI.DrawTexture(rect2, BaseContent.WhiteTex);
                        GUI.color = color3;
                        var favor = pawn.royalty.GetFavor(localTitle.faction);
                        if (Mouse.IsOver(rect2))
                        {
                            Widgets.DrawHighlight(rect2);
                        }

                        var rect3 = new Rect(r.x, r.y, r.width + 22f + 9f, r.height);
                        var position = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
                        GUI.color = title.faction.Color;
                        GUI.DrawTexture(position, localTitle.faction.def.FactionIcon);
                        GUI.color = color3;
                        Widgets.Label(new Rect(rect3.x + 22f + 5f, rect3.y, rect3.width - 10f, rect3.height),
                            titleLabel);
                        if (Widgets.ButtonInvisible(rect2))
                        {
                            Find.WindowStack.Add(new Dialog_InfoCard(localTitle.def, localTitle.faction, pawn));
                        }

                        if (!Mouse.IsOver(rect2))
                        {
                            return;
                        }

                        var tip = new TipSignal(
                            () => GetTitleTipString(pawn, localTitle.faction, localTitle, favor), (int)y * 37);
                        TooltipHandler.TipRegion(rect2, tip);
                    },
                    width = Text.CalcSize(titleLabel).x + 22f + 14f
                });
            }
        }

        if (ModsConfig.IdeologyActive && !pawn.DevelopmentalStage.Baby() && pawn.story is { favoriteColor: not null })
        {
            tmpStackElements.Add(new GenUI.AnonymousStackElement
            {
                drawer = delegate(Rect r)
                {
                    var orIdeoColor = string.Empty;
                    if (pawn.Ideo is { hidden: false })
                    {
                        orIdeoColor = "OrIdeoColor".Translate(pawn.Named("PAWN"));
                    }

                    Widgets.DrawRectFast(r, pawn.story.favoriteColor.color);
                    GUI.color = FavColorBoxColor;
                    Widgets.DrawBox(r);
                    GUI.color = Color.white;
                    TooltipHandler.TipRegion(r,
                        () => "FavoriteColorTooltip".Translate(pawn.Named("PAWN"),
                            0.6f.ToStringPercent().Named("PERCENTAGE"), orIdeoColor.Named("ORIDEO")).Resolve(),
                        837472764);
                },
                width = 22f
            });
        }

        if (pawn.guest is { Recruitable: false })
        {
            tmpStackElements.Add(new GenUI.AnonymousStackElement
            {
                drawer = delegate(Rect r)
                {
                    var color2 = GUI.color;
                    GUI.color = StackElementBackground;
                    GUI.DrawTexture(r, BaseContent.WhiteTex);
                    GUI.color = color2;
                    GUI.DrawTexture(r, UnrecruitableIcon);
                    if (!Mouse.IsOver(r))
                    {
                        return;
                    }

                    Widgets.DrawHighlight(r);
                    TooltipHandler.TipRegion(r,
                        () => "Unrecruitable".Translate().AsTipTitle().CapitalizeFirst() + "\n\n" +
                              "UnrecruitableDesc".Translate(pawn.Named("PAWN")).Resolve(), 15877733);
                },
                width = 22f
            });
        }

        QuestUtility.AppendInspectStringsFromQuestParts(delegate(string str, Quest quest)
        {
            tmpStackElements.Add(new GenUI.AnonymousStackElement
            {
                drawer = delegate(Rect r)
                {
                    var color = GUI.color;
                    GUI.color = StackElementBackground;
                    GUI.DrawTexture(r, BaseContent.WhiteTex);
                    GUI.color = color;
                    DoQuestLine(r, str, quest);
                },
                width = GetQuestLineSize(str).x
            });
        }, pawn, out count);
        curY += GenUI.DrawElementStack(new Rect(0f, curY, width, 50f), 22f, tmpStackElements,
            delegate(Rect r, GenUI.AnonymousStackElement obj) { obj.drawer(r); }, obj => obj.width,
            allowOrderOptimization: false).height;
        if (tmpStackElements.Any())
        {
            curY += 10f;
        }

        return curY;
    }

    private static void DoLeftSection(Rect rect, Rect leftRect, Pawn pawn)
    {
        Widgets.BeginGroup(leftRect);
        var pawnLocal = pawn;
        var abilities = (from a in pawn.abilities.abilities
            orderby a.def.level, a.def.EntropyGain
            select a).ToList();
        var numSections = abilities.Any() ? 5 : 4;
        var num2 = Enum.GetValues(typeof(BackstorySlot)).Length * 22f;
        float stackHeight;
        if (pawn.story is { title: not null })
        {
            num2 += 22f;
        }

        var list = new List<LeftRectSection>
        {
            new()
            {
                rect = new Rect(0f, 0f, leftRect.width, num2),
                drawer = delegate(Rect sectionRect)
                {
                    var num8 = sectionRect.y;
                    Text.Font = GameFont.Small;
                    foreach (BackstorySlot value6 in Enum.GetValues(typeof(BackstorySlot)))
                    {
                        var backstory = pawn.story.GetBackstory(value6);
                        if (backstory == null)
                        {
                            continue;
                        }

                        if (HiddenBioUtil.ShouldRevealBackstory(pawn))
                        {
                            var rect8 = new Rect(sectionRect.x, num8, leftRect.width, 22f);
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Widgets.Label(rect8,
                                value6 == BackstorySlot.Adulthood ? "Adulthood".Translate() : "Childhood".Translate());
                            Text.Anchor = TextAnchor.UpperLeft;
                            var text = backstory.TitleCapFor(pawn.gender);
                            var rect9 = new Rect(rect8);
                            rect9.x += 90f;
                            rect9.width = Text.CalcSize(text).x + 10f;
                            var color4 = GUI.color;
                            GUI.color = StackElementBackground;
                            GUI.DrawTexture(rect9, BaseContent.WhiteTex);
                            GUI.color = color4;
                            Text.Anchor = TextAnchor.MiddleCenter;
                            Widgets.Label(rect9, text.Truncate(rect9.width));
                            Text.Anchor = TextAnchor.UpperLeft;
                            if (Mouse.IsOver(rect9))
                            {
                                Widgets.DrawHighlight(rect9);
                            }

                            if (Mouse.IsOver(rect9))
                            {
                                TooltipHandler.TipRegion(rect9, backstory.FullDescriptionFor(pawn).Resolve());
                            }

                            num8 += rect8.height + 4f;
                        }
                        else
                        {
                            var rect10 = new Rect(sectionRect.x, num8, leftRect.width, 22f);
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Widgets.Label(rect10,
                                value6 == BackstorySlot.Adulthood ? "Adulthood".Translate() : "Childhood".Translate());
                            Text.Anchor = TextAnchor.UpperLeft;
                            var text2 = "HEB.UnknownBackstory".Translate().Colorize(ColoredText.SubtleGrayColor);
                            var rect11 = new Rect(rect10);
                            rect11.x += 90f;
                            rect11.width = Text.CalcSize(text2).x + 10f;
                            var color5 = GUI.color;
                            GUI.color = StackElementBackground;
                            GUI.DrawTexture(rect11, BaseContent.WhiteTex);
                            GUI.color = color5;
                            Text.Anchor = TextAnchor.MiddleCenter;
                            Widgets.Label(rect11, text2.Truncate(rect11.width));
                            Text.Anchor = TextAnchor.UpperLeft;
                            if (Mouse.IsOver(rect11))
                            {
                                Widgets.DrawHighlight(rect11);
                            }

                            if (Mouse.IsOver(rect11))
                            {
                                TooltipHandler.TipRegion(rect11,
                                    "HEB.UnknownBackstoryTipNew".Translate(pawn.Named("PAWN"),
                                        HiddenEnemyBioMod.Instance.Settings.RevealBackstoryResistance));
                            }

                            num8 += rect10.height + 4f;
                        }
                    }

                    if (pawn.story is not { title: not null })
                    {
                        return;
                    }

                    var rect12 = new Rect(sectionRect.x, num8, leftRect.width, 22f);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(rect12, "BackstoryTitle".Translate() + ":");
                    Text.Anchor = TextAnchor.UpperLeft;
                    var rect13 = new Rect(rect12);
                    rect13.x += 90f;
                    rect13.width -= 90f;
                    Widgets.Label(rect13, "HEB.UnknownBackstory".Translate());
                }
            }
        };
        num2 = 30f;
        var traits = pawn.story.traits.allTraits;
        if (traits == null || traits.Count == 0)
        {
            num2 += 22f;
            stackHeight = 22f;
        }
        else
        {
            var rect2 = GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 22f,
                pawn.story.traits.TraitsSorted, delegate { }, trait => Text.CalcSize(trait.LabelCap).x + 10f,
                allowOrderOptimization: false);
            num2 += rect2.height;
            stackHeight = rect2.height;
        }

        var height = stackHeight;
        list.Add(new LeftRectSection
        {
            rect = new Rect(0f, 0f, leftRect.width, num2),
            drawer = delegate(Rect sectionRect)
            {
                var currentY3 = sectionRect.y;
                Widgets.Label(new Rect(sectionRect.x, currentY3, 200f, 30f), "Traits".Translate().AsTipTitle());
                currentY3 += 24f;
                if (traits == null || traits.Count == 0)
                {
                    var color2 = GUI.color;
                    GUI.color = Color.gray;
                    var rect7 = new Rect(sectionRect.x, currentY3, leftRect.width, 24f);
                    if (Mouse.IsOver(rect7))
                    {
                        Widgets.DrawHighlight(rect7);
                    }

                    Widgets.Label(rect7,
                        pawn.DevelopmentalStage.Baby() ? "TraitsDevelopLaterBaby".Translate() : "None".Translate());
                    TooltipHandler.TipRegionByKey(rect7, "None");
                    GUI.color = color2;
                }
                else
                {
                    GenUI.DrawElementStack(new Rect(sectionRect.x, currentY3, leftRect.width - 5f, height), 22f,
                        pawn.story.traits.TraitsSorted, delegate(Rect r, Trait trait)
                        {
                            var color3 = GUI.color;
                            GUI.color = StackElementBackground;
                            GUI.DrawTexture(r, BaseContent.WhiteTex);
                            GUI.color = color3;
                            if (Mouse.IsOver(r))
                            {
                                Widgets.DrawHighlight(r);
                            }

                            if (trait.Suppressed)
                            {
                                GUI.color = ColoredText.SubtleGrayColor;
                            }
                            else if (trait.sourceGene != null)
                            {
                                GUI.color = ColoredText.GeneColor;
                            }

                            if (HiddenBioUtil.ShouldRevealTrait(pawn, trait))
                            {
                                Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), trait.LabelCap);
                                GUI.color = Color.white;
                                if (!Mouse.IsOver(r))
                                {
                                    return;
                                }

                                var trLocal = trait;
                                TooltipHandler.TipRegion(
                                    tip: new TipSignal(() => trLocal.TipString(pawn), (int)currentY3 * 37),
                                    rect: r);
                            }
                            else
                            {
                                Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height),
                                    "HEB.UnknownTrait".Translate());
                                GUI.color = Color.white;
                                if (Mouse.IsOver(r))
                                {
                                    TooltipHandler.TipRegion(
                                        tip: new TipSignal(
                                            () => "HEB.UnknownTraitTipNew".Translate(pawn.Named("PAWN"),
                                                HiddenEnemyBioMod.Instance.Settings.RevealTraitsResisitance),
                                            (int)currentY3 * 37), rect: r);
                                }
                            }
                        },
                        trait => HiddenBioUtil.ShouldRevealTrait(pawn, trait)
                            ? Text.CalcSize(trait.LabelCap).x + 10f
                            : Text.CalcSize("HEB.UnknownTrait".Translate()).x + 10f, allowOrderOptimization: false);
                }
            }
        });
        num2 = 30f;
        var disabledTags = pawn.CombinedDisabledWorkTags;
        var disabledTagsList = WorkTagsFrom(disabledTags).ToList();
        var allowWorkTagVerticalLayout = false;
        GenUI.StackElementWidthGetter<WorkTags> workTagWidthGetter =
            tag => Text.CalcSize(tag.LabelTranslated().CapitalizeFirst()).x + 10f;
        if (disabledTags == WorkTags.None)
        {
            num2 += 22f;
        }
        else
        {
            disabledTagsList.Sort(delegate(WorkTags a, WorkTags b)
            {
                var num7 = GetWorkTypeDisableCauses(pawn, a).Any(c => c is RoyalTitleDef) ? 1 : -1;
                var value5 = GetWorkTypeDisableCauses(pawn, b).Any(c => c is RoyalTitleDef) ? 1 : -1;
                return num7.CompareTo(value5);
            });
            var rect3 = GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 22f,
                disabledTagsList, delegate { }, workTagWidthGetter, allowOrderOptimization: false);
            num2 += rect3.height;
            stackHeight = rect3.height;
            num2 += 12f;
            allowWorkTagVerticalLayout = GenUI.DrawElementStackVertical(new Rect(0f, 0f, rect.width, stackHeight), 22f,
                disabledTagsList, delegate { }, workTagWidthGetter).width <= leftRect.width;
        }

        list.Add(new LeftRectSection
        {
            rect = new Rect(0f, 0f, leftRect.width, num2),
            drawer = delegate(Rect sectionRect)
            {
                var currentY2 = sectionRect.y;
                Widgets.Label(new Rect(sectionRect.x, currentY2, 200f, 24f),
                    "IncapableOf".Translate(pawn).AsTipTitle());
                currentY2 += 24f;
                if (!HiddenBioUtil.ShouldRevealIncapable(pawn))
                {
                    GUI.color = Color.gray;
                    var rect5 = new Rect(sectionRect.x, currentY2, leftRect.width, 24f);
                    if (Mouse.IsOver(rect5))
                    {
                        Widgets.DrawHighlight(rect5);
                    }

                    Widgets.Label(rect5, "HEB.UnknownIncapabilities".Translate());
                    TooltipHandler.TipRegion(rect5,
                        "HEB.UnknownIncapabilitiesTipNew".Translate(pawn.Named("PAWN"),
                            HiddenEnemyBioMod.Instance.Settings.RevealBackstoryResistance));
                }
                else if (disabledTags == WorkTags.None)
                {
                    GUI.color = Color.gray;
                    var rect6 = new Rect(sectionRect.x, currentY2, leftRect.width, 24f);
                    if (Mouse.IsOver(rect6))
                    {
                        Widgets.DrawHighlight(rect6);
                    }

                    Widgets.Label(rect6, "None".Translate());
                    TooltipHandler.TipRegionByKey(rect6, "None");
                }
                else
                {
                    GenUI.StackElementDrawer<WorkTags> stackElementDrawer = delegate(Rect r, WorkTags tag)
                    {
                        var color = GUI.color;
                        GUI.color = StackElementBackground;
                        GUI.DrawTexture(r, BaseContent.WhiteTex);
                        GUI.color = color;
                        GUI.color = GetDisabledWorkTagLabelColor(pawn, tag);
                        if (Mouse.IsOver(r))
                        {
                            Widgets.DrawHighlight(r);
                        }

                        Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height),
                            tag.LabelTranslated().CapitalizeFirst());
                        if (!Mouse.IsOver(r))
                        {
                            return;
                        }

                        var tagLocal = tag;
                        TooltipHandler.TipRegion(
                            tip: new TipSignal(
                                () => GetWorkTypeDisabledCausedBy(pawnLocal, tagLocal) + "\n" +
                                      GetWorkTypesDisabledByWorkTag(tagLocal), (int)currentY2 * 32), rect: r);
                    };
                    if (allowWorkTagVerticalLayout)
                    {
                        GenUI.DrawElementStackVertical(
                            new Rect(sectionRect.x, currentY2, leftRect.width - 5f, leftRect.height / numSections), 22f,
                            disabledTagsList, stackElementDrawer, workTagWidthGetter);
                    }
                    else
                    {
                        GenUI.DrawElementStack(
                            new Rect(sectionRect.x, currentY2, leftRect.width - 5f, leftRect.height / numSections), 22f,
                            disabledTagsList, stackElementDrawer, workTagWidthGetter, 5f);
                    }
                }

                GUI.color = Color.white;
            }
        });
        if (abilities.Any())
        {
            num2 = 30f;
            var rect4 = GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 32f, abilities,
                delegate { }, _ => 32f);
            num2 += rect4.height;
            stackHeight = rect4.height;
            list.Add(new LeftRectSection
            {
                rect = new Rect(0f, 0f, leftRect.width, num2),
                drawer = delegate(Rect sectionRect)
                {
                    var currentY = sectionRect.y;
                    Widgets.Label(new Rect(sectionRect.x, currentY, 200f, 24f),
                        "Abilities".Translate(pawn).AsTipTitle());
                    currentY += 24f;
                    GenUI.DrawElementStack(new Rect(sectionRect.x, currentY, leftRect.width - 5f, stackHeight), 32f,
                        abilities, delegate(Rect r, Ability abil)
                        {
                            GUI.DrawTexture(r, BaseContent.ClearTex);
                            if (Mouse.IsOver(r))
                            {
                                Widgets.DrawHighlight(r);
                            }

                            if (Widgets.ButtonImage(r, abil.def.uiIcon, false))
                            {
                                Find.WindowStack.Add(new Dialog_InfoCard(abil.def));
                            }

                            if (!Mouse.IsOver(r))
                            {
                                return;
                            }

                            var abilCapture = abil;
                            var tip = new TipSignal(
                                () => abilCapture.Tooltip + "\n\n" + "ClickToLearnMore".Translate()
                                    .Colorize(ColoredText.SubtleGrayColor), (int)currentY * 37);
                            TooltipHandler.TipRegion(r, tip);
                        }, _ => 32f);
                    GUI.color = Color.white;
                }
            });
        }

        var num3 = leftRect.height / list.Count;
        var num4 = 0f;
        for (var i = 0; i < list.Count; i++)
        {
            var value = list[i];
            if (value.rect.height > num3)
            {
                num4 += value.rect.height - num3;
                value.calculatedSize = value.rect.height;
            }
            else
            {
                value.calculatedSize = num3;
            }

            list[i] = value;
        }

        var startScrollView = false;
        var num5 = 0f;
        if (num4 > 0f)
        {
            var value2 = list[0];
            var num6 = value2.rect.height + 12f;
            num4 -= value2.calculatedSize - num6;
            value2.calculatedSize = num6;
            list[0] = value2;
        }

        while (num4 > 0f)
        {
            var continueIteration = true;
            for (var j = 0; j < list.Count; j++)
            {
                var value3 = list[j];
                if (value3.calculatedSize - value3.rect.height > 0f)
                {
                    value3.calculatedSize -= 1f;
                    num4 -= 1f;
                    continueIteration = false;
                }

                list[j] = value3;
            }

            if (!continueIteration)
            {
                continue;
            }

            for (var k = 0; k < list.Count; k++)
            {
                var value4 = list[k];
                if (k > 0)
                {
                    value4.calculatedSize = Mathf.Max(value4.rect.height, num3);
                }
                else
                {
                    value4.calculatedSize = value4.rect.height + 22f;
                }

                num5 += value4.calculatedSize;
                list[k] = value4;
            }

            startScrollView = true;
            break;
        }

        if (startScrollView)
        {
            Widgets.BeginScrollView(new Rect(0f, 0f, leftRect.width, leftRect.height), ref leftRectScrollPos,
                new Rect(0f, 0f, leftRect.width - 16f, num5));
        }

        var num = 0f;
        foreach (var leftRectSection in list)
        {
            leftRectSection.drawer(new Rect(0f, num, leftRect.width - 5f, leftRectSection.rect.height));
            num += leftRectSection.calculatedSize;
        }

        if (startScrollView)
        {
            Widgets.EndScrollView();
        }

        Widgets.EndGroup();
    }

    private static string GetWorkTypeDisabledCausedBy(Pawn pawn, WorkTags workTag)
    {
        var workTypeDisableCauses = GetWorkTypeDisableCauses(pawn, workTag);
        var stringBuilder = new StringBuilder();
        foreach (var item in workTypeDisableCauses)
        {
            if (item is BackstoryDef def)
            {
                stringBuilder.AppendLine("IncapableOfTooltipBackstory".Translate() + ": " +
                                         def.TitleFor(pawn.gender).CapitalizeFirst());
            }
            else if (item is Trait trait)
            {
                stringBuilder.AppendLine("IncapableOfTooltipTrait".Translate() + ": " + trait.LabelCap);
            }
            else if (item is Hediff hediff)
            {
                stringBuilder.AppendLine("IncapableOfTooltipHediff".Translate() + ": " + hediff.LabelCap);
            }
            else if (item is RoyalTitle title)
            {
                stringBuilder.AppendLine("IncapableOfTooltipTitle".Translate() + ": " +
                                         title.def.GetLabelFor(pawn));
            }
            else if (item is Quest quest)
            {
                stringBuilder.AppendLine("IncapableOfTooltipQuest".Translate() + ": " + quest.name);
            }
            else if (item is Precept_Role role)
            {
                stringBuilder.AppendLine("IncapableOfTooltipRole".Translate() + ": " +
                                         role.LabelForPawn(pawn));
            }
            else if (item is Gene gene)
            {
                stringBuilder.AppendLine("IncapableOfTooltipGene".Translate() + ": " + gene.LabelCap);
            }
        }

        return stringBuilder.ToString();
    }

    private static string GetWorkTypesDisabledByWorkTag(WorkTags workTag)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("IncapableOfTooltipWorkTypes".Translate().Colorize(ColoredText.TipSectionTitleColor));
        foreach (var allDef in DefDatabase<WorkTypeDef>.AllDefs)
        {
            if ((allDef.workTags & workTag) <= WorkTags.None)
            {
                continue;
            }

            stringBuilder.Append("- ");
            stringBuilder.AppendLine(allDef.pawnLabel);
        }

        return stringBuilder.ToString();
    }

    private static Vector2 PawnCardSize(Pawn pawn)
    {
        var basePawnCardSize = BasePawnCardSize;
        tmpInspectStrings.Length = 0;
        QuestUtility.AppendInspectStringsFromQuestParts(tmpInspectStrings, pawn, out var count);
        if (count >= 2)
        {
            basePawnCardSize.y += (count - 1) * 20;
        }

        return basePawnCardSize;
    }

    private static void DoNameInputRect(Rect rect, ref string name, int maxLength)
    {
        var text = Widgets.TextField(rect, name);
        if (text.Length <= maxLength && ValidNameRegex.IsMatch(text))
        {
            name = text;
        }
    }

    private static IEnumerable<WorkTags> WorkTagsFrom(WorkTags tags)
    {
        foreach (var allSelectedItem in tags.GetAllSelectedItems<WorkTags>())
        {
            if (allSelectedItem != 0)
            {
                yield return allSelectedItem;
            }
        }
    }

    private static Vector2 GetQuestLineSize(string line)
    {
        var vector = Text.CalcSize(line);
        return new Vector2(17f + vector.x + 10f, Mathf.Max(24f, vector.y));
    }

    private static void DoQuestLine(Rect rect, string line, Quest quest)
    {
        var rect2 = rect;
        rect2.xMin += 22f;
        rect2.height = Text.CalcSize(line).y;
        var x = Text.CalcSize(line).x;
        var rect3 = new Rect(rect.x, rect.y, Mathf.Min(x, rect2.width) + 24f + -7f + 5f, rect.height);
        if (!quest.hidden)
        {
            Widgets.DrawHighlightIfMouseover(rect3);
            TooltipHandler.TipRegionByKey(rect3, "ClickToViewInQuestsTab");
        }

        GUI.DrawTexture(new Rect(rect.x + -7f, rect.y - 2f, 24f, 24f), QuestIcon);
        Widgets.Label(rect2, line.Truncate(rect2.width));
        if (quest.hidden || !Widgets.ButtonInvisible(rect3))
        {
            return;
        }

        Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
        ((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
    }

    private struct LeftRectSection
    {
        public Rect rect;

        public Action<Rect> drawer;

        public float calculatedSize;
    }
}