using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Verse;

namespace StackXXL
{
    public enum StackSize
    {
        Default,
        XL,
        XXL
    }

    public class StackXXLSettings : ModSettings
    {
        public double SizeXL = 10d;
        public double SizeXXL = 20d;
        public StackSize ResourcesStack = StackSize.XL;
        public StackSize MedicineStack = StackSize.XL;
        public StackSize SilverStack = StackSize.XL;
        public StackSize TextilesStack = StackSize.XL;
        public StackSize DrugsStack = StackSize.XL;
        public StackSize MeatStack = StackSize.XL;
        public StackSize RawStack = StackSize.XL;
        public StackSize MealsStack = StackSize.XL;
        public StackSize BodyPartsStack = StackSize.Default;
        public StackSize OthersStackableStack = StackSize.XL;
        public StackSize OthersSingleStack = StackSize.Default;
        public bool DebugMode;

        internal bool LoadedFromFile { get; private set; }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                LoadedFromFile = true;

            Scribe_Values.Look(ref SizeXL, "sizeXL", 10d);
            Scribe_Values.Look(ref SizeXXL, "sizeXXL", 20d);
            Scribe_Values.Look(ref ResourcesStack, "resourcesStack", StackSize.XL);
            Scribe_Values.Look(ref MedicineStack, "medicineStack", StackSize.XL);
            Scribe_Values.Look(ref SilverStack, "silverStack", StackSize.XL);
            Scribe_Values.Look(ref TextilesStack, "textilesStack", StackSize.XL);
            Scribe_Values.Look(ref DrugsStack, "drugsStack", StackSize.XL);
            Scribe_Values.Look(ref MeatStack, "meatStack", StackSize.XL);
            Scribe_Values.Look(ref RawStack, "rawStack", StackSize.XL);
            Scribe_Values.Look(ref MealsStack, "mealsStack", StackSize.XL);
            Scribe_Values.Look(ref BodyPartsStack, "bodyPartsStack", StackSize.Default);
            Scribe_Values.Look(ref OthersStackableStack, "othersStackableStack", StackSize.XL);
            Scribe_Values.Look(ref OthersSingleStack, "othersSingleStack", StackSize.Default);
            Scribe_Values.Look(ref DebugMode, "debugMode", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                Validate();
        }

        internal void Validate()
        {
            if (!IsValidMultiplier(SizeXL))
                SizeXL = 10d;
            if (!IsValidMultiplier(SizeXXL))
                SizeXXL = 20d;
            ResourcesStack = ValidSize(ResourcesStack, StackSize.XL);
            MedicineStack = ValidSize(MedicineStack, StackSize.XL);
            SilverStack = ValidSize(SilverStack, StackSize.XL);
            TextilesStack = ValidSize(TextilesStack, StackSize.XL);
            DrugsStack = ValidSize(DrugsStack, StackSize.XL);
            MeatStack = ValidSize(MeatStack, StackSize.XL);
            RawStack = ValidSize(RawStack, StackSize.XL);
            MealsStack = ValidSize(MealsStack, StackSize.XL);
            BodyPartsStack = ValidSize(BodyPartsStack, StackSize.Default);
            OthersStackableStack = ValidSize(OthersStackableStack, StackSize.XL);
            OthersSingleStack = ValidSize(OthersSingleStack, StackSize.Default);
        }

        private static StackSize ValidSize(StackSize size, StackSize fallback)
        {
            return Enum.IsDefined(typeof(StackSize), size) ? size : fallback;
        }

        private static bool IsValidMultiplier(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 1d && value <= float.MaxValue;
        }

        internal static bool TryParseMultiplier(string text, out double value)
        {
            // Legacy HugsLib doubles were saved using the user's culture. Float parsing
            // deliberately excludes thousands separators, which can mask a decimal comma.
            bool parsed = double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
                double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            if (!parsed && text != null && text.IndexOf(',') >= 0 && text.IndexOf('.') < 0)
                parsed = double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            return parsed && IsValidMultiplier(value);
        }

        internal bool TryImportLegacySettings()
        {
            string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "HugsLib", "ModSettings.xml");
            if (!File.Exists(path))
                return false;

            try
            {
                var document = new XmlDocument { XmlResolver = null };
                var readerSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
                using (XmlReader reader = XmlReader.Create(path, readerSettings))
                    document.Load(reader);
                XmlNode node = document.SelectSingleNode("/settings/StackXXL");
                if (node == null)
                    return false;

                SizeXL = ReadMultiplier(node, "sizeXL", SizeXL);
                SizeXXL = ReadMultiplier(node, "sizeXXL", SizeXXL);
                ResourcesStack = ReadSize(node, "resourcesStack", ResourcesStack);
                // HugsLib omitted defaults; these two defaults depended on resources.
                MedicineStack = ReadSize(node, "medicineStack", ResourcesStack);
                SilverStack = ReadSize(node, "silverStack", ResourcesStack);
                TextilesStack = ReadSize(node, "textilesStack", TextilesStack);
                DrugsStack = ReadSize(node, "drugsStack", DrugsStack);
                MeatStack = ReadSize(node, "meatStack", MeatStack);
                RawStack = ReadSize(node, "rawStack", RawStack);
                MealsStack = ReadSize(node, "mealsStack", MealsStack);
                BodyPartsStack = ReadSize(node, "bodyPartsStack", BodyPartsStack);
                OthersStackableStack = ReadSize(node, "othersStackableStack", OthersStackableStack);
                OthersSingleStack = ReadSize(node, "othersSingleStack", OthersSingleStack);
                bool debug;
                if (bool.TryParse(node["debugMode"]?.InnerText, out debug))
                    DebugMode = debug;
                Validate();
                Log.Message("[Stack XXL] Imported existing settings into RimWorld's mod settings.");
                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("[Stack XXL] Could not import legacy settings: " + exception.Message);
                return false;
            }
        }

        private static double ReadMultiplier(XmlNode node, string key, double fallback)
        {
            double value;
            return TryParseMultiplier(node[key]?.InnerText, out value) ? value : fallback;
        }

        private static StackSize ReadSize(XmlNode node, string key, StackSize fallback)
        {
            StackSize value;
            return Enum.TryParse(node[key]?.InnerText, out value) ? ValidSize(value, fallback) : fallback;
        }
    }
}
