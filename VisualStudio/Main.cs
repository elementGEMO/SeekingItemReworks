using BepInEx;
using R2API;
using SeekerItems;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace SeekingItemReworks
{
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(ColorsAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "noodlegemo";
        public const string PluginName = "SeekingItemReworks";
        public const string PluginVersion = "2.0.0";
        public void Awake()
        {
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { }; - Used for Solo Multiplayer testing

            MainConfig.SetUp(this);
            if (MainConfig.EnableLogs.Value) Log.Init(Logger);
            ItemInfo.SetUp();

            SetUpCommon();
            SetUpUncommon();
            SetUpLegendary();
            SetUpEquipment();
        }
        private void SetUpCommon()
        {
            // -- Seekers of the Storm Content -- \\
            WarpedEchoBehavior.Init();
            ChronicExpansionBehavior.Init();
            KnockbackFinBehavior.Init();
            BolsteringLanternBehavior.Init();
            AntlerShieldBehavior.Init();
        }
        private void SetUpUncommon()
        {
            // -- Seekers of the Storm Content -- \\
            ChanceDollBehavior.Init();
            SaleStarBehavior.Init();
            UnstableTransmitterBehavior.Init();
            NoxiousThornBehavior.Init();

            // -- Risk of Rain 2 Content -- \\
            OldWarStealthkitBehavior.Init();
        }

        private void SetUpLegendary()
        {
            // -- Seekers of the Storm Content -- \\
            GrowthNectarBehavior.Init();
            WarBondsBehavior.Init();

            // -- Survivors off the Void Content -- \\
            BensRaincoatBehavior.Init();
        }

        private void SetUpEquipment()
        {
            // -- Seekers of the Storm Content -- \\
            SeedOfLifeBehavior.Init();
        }
    }
}