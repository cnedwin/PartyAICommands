using System;
using System.Linq;
using System.Windows.Forms;
using HarmonyLib;
using Helpers;
using PartyAIOverhaulCommands;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
namespace PartyAIOverhaulCommands
{

	[HarmonyPatch(typeof(EndCaptivityAction), "ApplyInternal")]
	public class EndCaptivityActionPatch
	{
		private static bool Prefix(Hero prisoner, EndCaptivityDetail detail)
		{
			if (prisoner.Clan == Hero.MainHero.Clan && prisoner.PartyBelongedToAsPrisoner != null && !prisoner.IsActive && prisoner.IsAlive && (prisoner.PartyBelongedTo == null || prisoner.PartyBelongedTo.Party.Owner != prisoner) && (detail == EndCaptivityDetail.ReleasedAfterBattle || detail == EndCaptivityDetail.ReleasedAfterPeace || detail == EndCaptivityDetail.RemovedParty) && prisoner.Clan.CommanderLimit > prisoner.Clan.WarParties.Count((MobileParty p) => !p.IsGarrison && !p.IsMilitia && !p.IsVillager && !p.IsCaravan) && prisoner.PartyBelongedToAsPrisoner.Position2D.DistanceSquared(MobileParty.MainParty.VisualPosition2DWithoutError) < 25f)
			{
				StatisticsDataLogHelper.AddLog(StatisticsDataLogHelper.LogAction.EndCaptivityAction);
				PartyBase partyBelongedToAsPrisoner = prisoner.PartyBelongedToAsPrisoner;
				IFaction faction = (partyBelongedToAsPrisoner != null) ? partyBelongedToAsPrisoner.MapFaction : CampaignData.NeutralFaction;
				IFaction capturerFaction = faction;
				Traverse.Create(CampaignEventDispatcher.Instance).Method("OnHeroPrisonerReleased", new Type[4]
				{
				typeof(Hero),
				typeof(PartyBase),
				typeof(IFaction),
				typeof(EndCaptivityDetail)
				}).GetValue(prisoner, partyBelongedToAsPrisoner, capturerFaction, detail);
				SpawnPartyAtPosition(prisoner, MobileParty.MainParty.VisualPosition2DWithoutError);
				if (prisoner.PartyBelongedTo != null && prisoner?.getOrder() != null)
				{
					PartyOrder order = prisoner.getOrder();
					if (order.Behavior == AiBehavior.EscortParty && order.ScoreMinimum > 1f)
					{
						if (Campaign.Current.Models.MapDistanceModel.GetDistance(prisoner.PartyBelongedTo, MobileParty.MainParty) > 15f)
						{
							prisoner.PartyBelongedTo.SetInititave(0f, 1f, CampaignTime.YearsFromNow(100f).RemainingHoursFromNow);
						}
						else
						{
							prisoner.PartyBelongedTo.SetInititave(order.AttackInitiative, order.AvoidInitiative, CampaignTime.YearsFromNow(100f).RemainingHoursFromNow);
						}
						prisoner.PartyBelongedTo.SetMoveEscortParty(order.TargetParty);
					}
				}
				return false;
			}
			return true;
		}

		private static void SpawnPartyAtPosition(Hero hero, Vec2 position)
		{
			if (!hero.IsActive && hero.IsAlive)
			{
				hero.ChangeState(Hero.CharacterStates.Active);
				GiveGoldAction.ApplyBetweenCharacters(null, hero, 3000, disableNotification: true);
				MobilePartyHelper.SpawnLordParty(hero, position, 0.5f);
			}
		}

		private static void Finalizer(Exception __exception)
		{
			if (__exception != null)
			{
				MessageBox.Show(__exception.FlattenException());
			}
		}
	}
}
