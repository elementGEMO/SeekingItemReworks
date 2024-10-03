using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class OldWarStealthKit : ItemTemplate
    {
        public OldWarStealthKit(int descType)
        {
            ItemInternal = "PHASING";

            if (descType == 1)
            {
                ItemInfo = "Turn invisible and cleanse debuffs at low health.";
                ItemDesc = string.Format(
                    "Falling below " + "25% health ".Style(FontColor.cIsHealth) + "causes you to become " + "invisible ".Style(FontColor.cIsUtility) + "for " + "5s".Style(FontColor.cIsUtility) + ", boost " + "movement speed ".Style(FontColor.cIsUtility) + "by " + "40% ".Style(FontColor.cIsUtility) + "and " + "cleanse {0} ".Style(FontColor.cIsUtility) + "debuffs ".Style(FontColor.cIsDamage) + "(+{1} per stack)".Style(FontColor.cStack) + ". Recharges every " + "30 ".Style(FontColor.cIsUtility) + "(-50% per stack) ".Style(FontColor.cStack) + "seconds.",
                    Base_Cleanse.Value, Stack_Cleanse.Value
                );
            }
        }

        public static string StaticName = "Old War Stealthkit";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<int> Base_Cleanse;
        public static ConfigEntry<int> Stack_Cleanse;
    }

    public static class OldWarStealthkitBehavior
    {
        public static void Init()
        {
            if (OldWarStealthKit.Rework.Value == 1)
            {
                IL.RoR2.Items.PhasingBodyBehavior.FixedUpdate += ReplaceEffect;
            }
        }

        private static void ReplaceEffect(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchCall(typeof(RoR2.Items.BaseItemBodyBehavior), "get_body"),
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.Cloak))
            ))
            {
                cursor.Emit(OpCodes.Ldarg_0);

                cursor.EmitDelegate<Action<RoR2.Items.PhasingBodyBehavior>>(itemBase =>
                {
                    int numBuffs = itemBase.body.activeBuffsListCount;
                    int itemCount = itemBase.body.inventory.GetItemCount(RoR2Content.Items.Phasing);
                    int numCleansed = 1 + itemCount;

                    for (int i = 0; i < numBuffs; i++)
                    {
                        BuffDef foundBuff = BuffCatalog.GetBuffDef(itemBase.body.activeBuffsList[i]);
                        if (foundBuff.isDebuff && numCleansed > 0)
                        {
                            numCleansed--; i--;
                            itemBase.body.RemoveBuff(foundBuff);
                        }
                        else if (numCleansed <= 0) { break; }
                    }

                    if (numCleansed < 1 + itemCount)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/CleanseEffect"), new EffectData
                        {
                            origin = itemBase.body.transform.position
                        }, true);
                    }
                });
            }
            else
            {
                Log.Warning(OldWarStealthKit.StaticName + " #1 - IL Fail #1");
            }
        }
    }
}
