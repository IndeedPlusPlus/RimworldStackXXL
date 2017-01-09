using HugsLib;
using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace StackXXL
{
    public class StackXXLMod : ModBase
    {
        public override string ModIdentifier
        {
            get
            {
                return "StackXXL";
            }
        }

        private SettingHandle<double> sizeXL;
        private SettingHandle<double> sizeXXL;
        private SettingHandle<SizeEnum> resourcesStack;
        private SettingHandle<SizeEnum> silverStack;
        private SettingHandle<SizeEnum> textilesStack;
        private SettingHandle<SizeEnum> drugsStack;
        private SettingHandle<SizeEnum> meatStack;
        private SettingHandle<SizeEnum> rawStack;
        private SettingHandle<SizeEnum> mealsStack;
        private SettingHandle<SizeEnum> bodyPartsStack;
        private SettingHandle<SizeEnum> othersStackableStack;
        private SettingHandle<SizeEnum> othersSingleStack;
        private SettingHandle<bool> debugMode;
        private enum SizeEnum
        {
            Default,
            XL,
            XXL
        }
        private static bool StackIncreaseAllowed(ThingDef d)
        {
            if (d.thingCategories.NullOrEmpty())
                return false;
            return d.IsStuff || d.isBodyPartOrImplant ||
                (
                    (d.category == ThingCategory.Item) && !d.isUnfinishedThing && !d.IsCorpse && !d.destroyOnDrop && !d.IsRangedWeapon && !d.IsApparel && (d.stackLimit > 1)
                );
        }
        private static bool IsSilver(ThingDef d)
        {
            return d == ThingDefOf.Silver;
        }
        private static bool IsResources(ThingDef d)
        {
            var category = d.thingCategories[0];
            return category == ThingCategoryDefOf.Manufactured || category == ThingCategoryDefOf.ResourcesRaw || category == ThingCategoryDefOf.StoneBlocks || "Medicine".Equals(category.defName);
        }

        private static bool IsTextiles(ThingDef d)
        {
            var category = d.thingCategories[0];
            return category == ThingCategoryDefOf.Leathers || "Textiles".Equals(category.defName);
        }

        private static bool IsDrugs(ThingDef d)
        {
            var category = d.thingCategories[0];
            return "Drugs".Equals(category.defName);
        }

        private static bool IsMeat(ThingDef d)
        {
            var category = d.thingCategories[0];
            return category == ThingCategoryDefOf.MeatRaw;
        }

        private static bool IsRaw(ThingDef d)
        {
            var category = d.thingCategories[0];
            return category.defName.EndsWith("FoodRaw") || "PlantMatter".Equals(category.defName) || "AnimalProductRaw".Equals(category.defName) || "AnimalFeed".Equals(category.defName) || category.defName.StartsWith("Eggs") || "CookingSupplies".Equals(category.defName);
        }
        private static bool IsMeals(ThingDef d)
        {
            var category = d.thingCategories[0];
            return category.defName.EndsWith("Meals") || "Foods".Equals(category.defName);
        }
        private static bool IsBodyParts(ThingDef d)
        {
            var category = d.thingCategories[0];
            return category.defName.EndsWith("Prostheses") || category.defName.StartsWith("BodyParts") || category.defName.EndsWith("Organs");
        }

        private double getSize(SettingHandle<SizeEnum> holder)
        {
            switch (holder.Value)
            {
                case SizeEnum.XL:
                    return sizeXL;
                case SizeEnum.XXL:
                    return sizeXXL;
                default:
                    return 1;
            }
        }
        private void logCategory(ThingDef d, String category)
        {
            if (!debugMode)
                return;
            Logger.Message(d.defName + " " + d.thingCategories[0].defName + " " + category);
        }
        private static void updateStackLimit(ThingDef thing, double multiplier)
        {
            thing.stackLimit = Math.Max(1, (int) Math.Round(thing.stackLimit * multiplier));
        }
        private void modifiyStackSizes()
        {
            foreach (var thing in DefDatabase<ThingDef>.AllDefs)
                if (StackIncreaseAllowed(thing))
                {
                    if (IsSilver(thing))
                    {
                        logCategory(thing, "silver");
                        updateStackLimit(thing, getSize(silverStack));
                    }
                    else if (IsResources(thing))
                    {
                        logCategory(thing, "resources");
                        updateStackLimit(thing, getSize(resourcesStack));
                    }
                    else if (IsTextiles(thing))
                    {
                        logCategory(thing, "textiles");
                        updateStackLimit(thing, getSize(textilesStack));
                    }
                    else if (IsDrugs(thing))
                    {
                        logCategory(thing, "drugs");
                        updateStackLimit(thing, getSize(drugsStack));
                    }
                    else if (IsMeat(thing))
                    {
                        logCategory(thing, "meat");
                        updateStackLimit(thing, getSize(meatStack));
                    }
                    else if (IsRaw(thing))
                    {
                        logCategory(thing, "raw");
                        updateStackLimit(thing, getSize(rawStack));
                    }
                    else if (IsMeals(thing))
                    {
                        logCategory(thing, "meals");
                        updateStackLimit(thing, getSize(mealsStack));
                    }
                    else if (IsBodyParts(thing))
                    {
                        logCategory(thing, "bodyParts");
                        updateStackLimit(thing, getSize(bodyPartsStack));
                    }
                    else if (thing.stackLimit > 1)
                    {
                        logCategory(thing, "othersStackable");
                        updateStackLimit(thing, getSize(othersStackableStack));
                    }
                    else
                    {
                        logCategory(thing, "othersSingle");
                        updateStackLimit(thing, getSize(othersSingleStack));
                    }

                }
        }
        public override void DefsLoaded()
        {

            sizeXL = Settings.GetHandle<double>("sizeXL", "StackXXL.XLSize.Title".Translate(), "StackXXL.XLSize.Desc".Translate(), 10.0, Validators.FloatRangeValidator(1, float.MaxValue));
            sizeXXL = Settings.GetHandle<double>("sizeXXL", "StackXXL.XXLSize.Title".Translate(), "StackXXL.XXLSize.Desc".Translate(), 20.0, Validators.FloatRangeValidator(1, float.MaxValue));
            resourcesStack = Settings.GetHandle<SizeEnum>("resourcesStack", "StackXXL.Stack.Resources.Title".Translate(), "StackXXL.Stack.Resources.Desc".Translate(), SizeEnum.XL, null, "StackXXL.Size.");
            silverStack = Settings.GetHandle<SizeEnum>("silverStack", "StackXXL.Stack.Silver.Title".Translate(), "StackXXL.Stack.Silver.Desc".Translate(), resourcesStack.Value, null, "StackXXL.Size.");
            textilesStack = Settings.GetHandle<SizeEnum>("textilesStack", "StackXXL.Stack.Textiles.Title".Translate(), "StackXXL.Stack.Textiles.Desc".Translate(), SizeEnum.XL, null, "StackXXL.Size.");
            drugsStack = Settings.GetHandle<SizeEnum>("drugsStack", "StackXXL.Stack.Drugs.Title".Translate(), "StackXXL.Stack.Drugs.Desc".Translate(), SizeEnum.XL, null, "StackXXL.Size.");
            meatStack = Settings.GetHandle<SizeEnum>("meatStack", "StackXXL.Stack.Meat.Title".Translate(), "StackXXL.Stack.Meat.Desc".Translate(), SizeEnum.XL, null, "StackXXL.Size.");
            rawStack = Settings.GetHandle<SizeEnum>("rawStack", "StackXXL.Stack.Raw.Title".Translate(), "StackXXL.Stack.Raw.Desc".Translate(), SizeEnum.XL, null, "StackXXL.Size.");
            mealsStack = Settings.GetHandle<SizeEnum>("mealsStack", "StackXXL.Stack.Meals.Title".Translate(), "StackXXL.Stack.Meals.Desc".Translate(), SizeEnum.XL, null, "StackXXL.Size.");
            bodyPartsStack = Settings.GetHandle<SizeEnum>("bodyPartsStack", "StackXXL.Stack.BodyParts.Title".Translate(), "StackXXL.Stack.BodyParts.Desc".Translate(), SizeEnum.Default, null, "StackXXL.Size.");
            othersStackableStack = Settings.GetHandle<SizeEnum>("othersStackableStack", "StackXXL.Stack.Others.Stackable.Title".Translate(), "StackXXL.Stack.Others.Stackable.Desc".Translate(), SizeEnum.XL, null, "StackXXL.Size.");
            othersSingleStack = Settings.GetHandle<SizeEnum>("othersSingleStack", "StackXXL.Stack.Others.Single.Title".Translate(), "StackXXL.Stack.Others.Single.Desc".Translate(), SizeEnum.Default, null, "StackXXL.Size.");

            debugMode = Settings.GetHandle<bool>("debugMode", "StackXXL.DebugMode.Title".Translate(), "StackXXL.DebugMode.Desc".Translate(), false);
            if (!ModIsActive)
                return;
            modifiyStackSizes();
            Logger.Message("Loaded");
        }
        public override void Initialize()
        {

        }
    }

}
