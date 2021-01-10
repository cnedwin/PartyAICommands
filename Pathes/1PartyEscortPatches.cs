﻿using HarmonyLib;
using System;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;

namespace PartyAIOverhaulCommands
{
  [HarmonyPatch(typeof (MobileParty), "SetMoveEscortParty")]
  [HarmonyPatch(new Type[] {typeof (MobileParty)})]
  public class SetMoveEscortPartyPatch
  {
    private static void Postfix(MobileParty __instance, MobileParty mobileParty) => SetMoveEngagePartyPatch.Postfix(__instance, mobileParty);

    private static void Finalizer(Exception __exception)
    {
      if (__exception == null)
        return;
      int num = (int) MessageBox.Show(__exception.FlattenException());
    }
  }
}
