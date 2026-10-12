using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace StackXXL
{
    public class StackXXLMod : Mod
    {
        private readonly StackXXLSettings settings;
        private Vector2 scrollPosition;
        private float settingsHeight = 800f;
        private string sizeXLBuffer;
        private string sizeXXLBuffer;
        private bool stackSizesApplied;

        public StackXXLMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<StackXXLSettings>();
            if (!settings.LoadedFromFile && settings.TryImportLegacySettings())
                WriteSettings();
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }

        public override void WriteSettings()
        {
            settings.Validate();
            base.WriteSettings();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GameFont previousFont = Text.Font;
            Text.Font = GameFont.Small;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 20f, settingsHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            var listing = new Listing_Standard { maxOneColumn = true };
            listing.Begin(viewRect);

            listing.Label("StackXXL.RestartRequired".Translate());
            listing.Gap();
            DrawMultiplier(listing, "StackXXL.XLSize", ref settings.SizeXL, ref sizeXLBuffer);
            DrawMultiplier(listing, "StackXXL.XXLSize", ref settings.SizeXXL, ref sizeXXLBuffer);
            listing.Label("StackXXL.Multiplier.Validation".Translate());
            listing.GapLine();

            DrawStackSetting(listing, "Resources", settings.ResourcesStack, value => settings.ResourcesStack = value);
            DrawStackSetting(listing, "Medicine", settings.MedicineStack, value => settings.MedicineStack = value);
            DrawStackSetting(listing, "Silver", settings.SilverStack, value => settings.SilverStack = value);
            DrawStackSetting(listing, "Textiles", settings.TextilesStack, value => settings.TextilesStack = value);
            DrawStackSetting(listing, "Drugs", settings.DrugsStack, value => settings.DrugsStack = value);
            DrawStackSetting(listing, "Meat", settings.MeatStack, value => settings.MeatStack = value);
            DrawStackSetting(listing, "Raw", settings.RawStack, value => settings.RawStack = value);
            DrawStackSetting(listing, "Meals", settings.MealsStack, value => settings.MealsStack = value);
            DrawStackSetting(listing, "BodyParts", settings.BodyPartsStack, value => settings.BodyPartsStack = value);
            DrawStackSetting(listing, "Others.Stackable", settings.OthersStackableStack, value => settings.OthersStackableStack = value);
            DrawStackSetting(listing, "Others.Single", settings.OthersSingleStack, value => settings.OthersSingleStack = value);
            listing.GapLine();
            listing.CheckboxLabeled("StackXXL.DebugMode.Title".Translate(), ref settings.DebugMode,
                "StackXXL.DebugMode.Desc".Translate());

            settingsHeight = listing.CurHeight + 12f;
            listing.End();
            Widgets.EndScrollView();
            Text.Font = previousFont;
        }

        private static void DrawMultiplier(Listing_Standard listing, string key, ref double value, ref string buffer)
        {
            string label = (key + ".Title").Translate();
            Rect row = listing.GetRect(Math.Max(30f, Text.CalcHeight(label, listing.ColumnWidth - 140f)));
            Widgets.Label(new Rect(row.x, row.y, row.width - 140f, row.height), label);
            TooltipHandler.TipRegion(row, (key + ".Desc").Translate());

            if (buffer == null)
                buffer = value.ToString(System.Globalization.CultureInfo.CurrentCulture);
            // Verse's numeric widget only supports int/float; retain double precision here.
            double parsed;
            Color previousColor = GUI.color;
            if (!StackXXLSettings.TryParseMultiplier(buffer, out parsed))
                GUI.color = Color.red;
            buffer = Widgets.TextField(new Rect(row.xMax - 130f, row.y, 130f, 30f), buffer);
            GUI.color = previousColor;
            if (StackXXLSettings.TryParseMultiplier(buffer, out parsed))
                value = parsed;
            listing.Gap(6f);
        }

        private static void DrawStackSetting(Listing_Standard listing, string category, StackSize value, Action<StackSize> setter)
        {
            string key = "StackXXL.Stack." + category;
            string label = (key + ".Title").Translate();
            Rect row = listing.GetRect(Math.Max(30f, Text.CalcHeight(label, listing.ColumnWidth - 140f)));
            Widgets.Label(new Rect(row.x, row.y, row.width - 140f, row.height), label);
            TooltipHandler.TipRegion(row, (key + ".Desc").Translate());
            if (Widgets.ButtonText(new Rect(row.xMax - 130f, row.y, 130f, 30f), ("StackXXL.Size." + value).Translate()))
            {
                var options = new List<FloatMenuOption>();
                foreach (StackSize size in Enum.GetValues(typeof(StackSize)))
                {
                    StackSize selected = size;
                    options.Add(new FloatMenuOption(("StackXXL.Size." + size).Translate(), () => setter(selected)));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            listing.Gap(6f);
        }

        private static bool StackIncreaseAllowed(ThingDef d)
        {
            if (d.thingCategories.NullOrEmpty())
                return false;
            return d.IsStuff || d.isTechHediff ||
                (d.category == ThingCategory.Item && !d.isUnfinishedThing && !d.IsCorpse &&
                 !d.destroyOnDrop && !d.IsRangedWeapon && !d.IsApparel && d.stackLimit > 1);
        }

        private static bool IsResources(ThingCategoryDef category)
        {
            return category == ThingCategoryDefOf.Manufactured || category == ThingCategoryDefOf.ResourcesRaw ||
                category == ThingCategoryDefOf.StoneBlocks;
        }

        private static bool IsTextiles(ThingCategoryDef category)
        {
            return category == ThingCategoryDefOf.Leathers || category.defName == "Textiles";
        }

        private static bool IsRaw(ThingCategoryDef category)
        {
            return category.defName.EndsWith("FoodRaw") || category.defName == "PlantMatter" ||
                category.defName == "AnimalProductRaw" || category.defName == "AnimalFeed" ||
                category.defName.StartsWith("Eggs") || category.defName == "CookingSupplies";
        }

        private static bool IsMeals(ThingCategoryDef category)
        {
            return category.defName.EndsWith("Meals") || category.defName == "Foods";
        }

        private static bool IsBodyParts(ThingCategoryDef category)
        {
            return category.defName.EndsWith("Prostheses") || category.defName.StartsWith("BodyParts") ||
                category.defName.EndsWith("Organs");
        }

        private double GetSize(StackSize size)
        {
            switch (size)
            {
                case StackSize.XL:
                    return settings.SizeXL;
                case StackSize.XXL:
                    return settings.SizeXXL;
                default:
                    return 1d;
            }
        }

        private void UpdateStackLimit(ThingDef thing, StackSize size, string category)
        {
            if (settings.DebugMode)
                Log.Message("[Stack XXL] " + thing.defName + " " + thing.thingCategories[0].defName + " " + category);

            // Large valid multipliers must not overflow the game's integer stack limit.
            double limit = Math.Round(thing.stackLimit * GetSize(size));
            thing.stackLimit = (int)Math.Min(int.MaxValue, Math.Max(1d, limit));
        }

        internal void ApplyStackSizes()
        {
            // Settings take effect once per startup, never while editing a running game.
            if (stackSizesApplied)
                return;
            settings.Validate();
            foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefs)
            {
                if (!StackIncreaseAllowed(thing))
                    continue;
                ThingCategoryDef category = thing.thingCategories[0];
                if (thing == ThingDefOf.Silver)
                    UpdateStackLimit(thing, settings.SilverStack, "silver");
                else if (category == ThingCategoryDefOf.Medicine)
                    UpdateStackLimit(thing, settings.MedicineStack, "medicine");
                else if (IsResources(category))
                    UpdateStackLimit(thing, settings.ResourcesStack, "resources");
                else if (IsTextiles(category))
                    UpdateStackLimit(thing, settings.TextilesStack, "textiles");
                else if (category.defName == "Drugs")
                    UpdateStackLimit(thing, settings.DrugsStack, "drugs");
                else if (category == ThingCategoryDefOf.MeatRaw)
                    UpdateStackLimit(thing, settings.MeatStack, "meat");
                else if (IsRaw(category))
                    UpdateStackLimit(thing, settings.RawStack, "raw");
                else if (IsMeals(category))
                    UpdateStackLimit(thing, settings.MealsStack, "meals");
                else if (IsBodyParts(category))
                    UpdateStackLimit(thing, settings.BodyPartsStack, "bodyParts");
                else if (thing.stackLimit > 1)
                    UpdateStackLimit(thing, settings.OthersStackableStack, "othersStackable");
                else
                    UpdateStackLimit(thing, settings.OthersSingleStack, "othersSingle");
            }
            stackSizesApplied = true;
            Log.Message("[Stack XXL] Loaded");
        }
    }

    [StaticConstructorOnStartup]
    internal static class StackXXLStartup
    {
        static StackXXLStartup()
        {
            // Run after defs and every mod's startup constructor, as DefsLoaded did.
            LongEventHandler.ExecuteWhenFinished(() => LoadedModManager.GetMod<StackXXLMod>().ApplyStackSizes());
        }
    }
}
