using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;

using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class SeedOfLife : ItemTemplate
    {
        public SeedOfLife(int descType)
        {
            ItemInternal = "HEALANDREVIVE";

            if (descType == 1)
            {
                ItemInfo = "Revives user or allies. Consume on use.";
                ItemDesc = "All allies and yourself " + "return to life".Style(FontColor.cIsHealing) + ". Forces activation " + "upon death".Style(FontColor.cIsUtility) + ". Equipment is " + "consumed ".Style(FontColor.cIsUtility) + "on use.";
            }
        }

        public static string StaticName = "Seed of Life";

        public static ConfigEntry<int> Rework;
    }

    public static class SeedOfLifeBehavior
    {
        public static void Init()
        {
            if (SeedOfLife.Rework.Value == 1)
            {
                IL.RoR2.CharacterMaster.OnBodyDeath += ReviveAll;
            }
        }

        private static void ReviveAll(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<CharacterMaster>("get_inventory"),
                x => x.MatchLdsfld(typeof(DLC2Content.Equipment), nameof(DLC2Content.Equipment.HealAndReviveConsumed))
            ))
            {
                cursor.Emit(OpCodes.Ldarg, 1);
                cursor.EmitDelegate<Action<CharacterBody>>(self => self.equipmentSlot.FireHealAndRevive());

            } else Log.Warning(SeedOfLife.StaticName + " #1 - IL Fail #1");
        }
    }
}
