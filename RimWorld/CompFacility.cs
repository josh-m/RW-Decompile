using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompFacility : ThingComp
	{
		private List<Thing> linkedBuildings = new List<Thing>();

		private HashSet<Thing> thingsToNotify = new HashSet<Thing>();

		public bool CanBeActive
		{
			get
			{
				CompPowerTrader compPowerTrader = this.parent.TryGetComp<CompPowerTrader>();
				return compPowerTrader == null || compPowerTrader.PowerOn;
			}
		}

		public CompProperties_Facility Props
		{
			get
			{
				return (CompProperties_Facility)this.props;
			}
		}

		public static void DrawLinesToPotentialThingsToLinkTo(ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map)
		{
			CompProperties_Facility compProperties = myDef.GetCompProperties<CompProperties_Facility>();
			Vector3 a = Gen.TrueCenter(myPos, myRot, myDef.size, myDef.Altitude);
			for (int i = 0; i < compProperties.linkableBuildings.Count; i++)
			{
				foreach (Thing current in map.listerThings.ThingsOfDef(compProperties.linkableBuildings[i]))
				{
					CompAffectedByFacilities compAffectedByFacilities = current.TryGetComp<CompAffectedByFacilities>();
					if (compAffectedByFacilities != null && compAffectedByFacilities.CanPotentiallyLinkTo(myDef, myPos, myRot))
					{
						GenDraw.DrawLineBetween(a, current.TrueCenter());
						compAffectedByFacilities.DrawRedLineToPotentiallySupplantedFacility(myDef, myPos, myRot);
					}
				}
			}
		}

		public void Notify_NewLink(Thing thing)
		{
			for (int i = 0; i < this.linkedBuildings.Count; i++)
			{
				if (this.linkedBuildings[i] == thing)
				{
					Log.Error("Notify_NewLink was called but the link is already here.");
					return;
				}
			}
			this.linkedBuildings.Add(thing);
		}

		public void Notify_LinkRemoved(Thing thing)
		{
			for (int i = 0; i < this.linkedBuildings.Count; i++)
			{
				if (this.linkedBuildings[i] == thing)
				{
					this.linkedBuildings.RemoveAt(i);
					return;
				}
			}
			Log.Error("Notify_LinkRemoved was called but there is no such link here.");
		}

		public void Notify_LOSBlockerSpawnedOrDespawned()
		{
			this.RelinkAll();
		}

		public void Notify_ThingChanged()
		{
			this.RelinkAll();
		}

		public override void PostSpawnSetup()
		{
			this.LinkToNearbyBuildings();
		}

		public override void PostDeSpawn(Map map)
		{
			this.thingsToNotify.Clear();
			for (int i = 0; i < this.linkedBuildings.Count; i++)
			{
				this.thingsToNotify.Add(this.linkedBuildings[i]);
			}
			this.UnlinkAll();
			foreach (Thing current in this.thingsToNotify)
			{
				current.TryGetComp<CompAffectedByFacilities>().Notify_FacilityDespawned();
			}
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			for (int i = 0; i < this.linkedBuildings.Count; i++)
			{
				CompAffectedByFacilities compAffectedByFacilities = this.linkedBuildings[i].TryGetComp<CompAffectedByFacilities>();
				if (compAffectedByFacilities.IsFacilityActive(this.parent))
				{
					GenDraw.DrawLineBetween(this.parent.TrueCenter(), this.linkedBuildings[i].TrueCenter());
				}
				else
				{
					GenDraw.DrawLineBetween(this.parent.TrueCenter(), this.linkedBuildings[i].TrueCenter(), CompAffectedByFacilities.InactiveFacilityLineMat);
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			CompProperties_Facility props = this.Props;
			if (props.statOffsets == null)
			{
				return null;
			}
			bool flag = this.AmIActiveForAnyone();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < props.statOffsets.Count; i++)
			{
				StatModifier statModifier = props.statOffsets[i];
				StatDef stat = statModifier.stat;
				stringBuilder.Append(stat.LabelCap);
				stringBuilder.Append(": ");
				stringBuilder.Append(statModifier.value.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset));
				if (!flag)
				{
					stringBuilder.Append(" (");
					stringBuilder.Append("InactiveFacility".Translate());
					stringBuilder.Append(")");
				}
				if (i < props.statOffsets.Count - 1)
				{
					stringBuilder.AppendLine();
				}
			}
			return stringBuilder.ToString();
		}

		private void RelinkAll()
		{
			this.LinkToNearbyBuildings();
		}

		private void LinkToNearbyBuildings()
		{
			this.UnlinkAll();
			CompProperties_Facility props = this.Props;
			if (props.linkableBuildings == null)
			{
				return;
			}
			for (int i = 0; i < props.linkableBuildings.Count; i++)
			{
				foreach (Thing current in this.parent.Map.listerThings.ThingsOfDef(props.linkableBuildings[i]))
				{
					CompAffectedByFacilities compAffectedByFacilities = current.TryGetComp<CompAffectedByFacilities>();
					if (compAffectedByFacilities != null && compAffectedByFacilities.CanLinkTo(this.parent))
					{
						this.linkedBuildings.Add(current);
						compAffectedByFacilities.Notify_NewLink(this.parent);
					}
				}
			}
		}

		private bool AmIActiveForAnyone()
		{
			for (int i = 0; i < this.linkedBuildings.Count; i++)
			{
				CompAffectedByFacilities compAffectedByFacilities = this.linkedBuildings[i].TryGetComp<CompAffectedByFacilities>();
				if (compAffectedByFacilities.IsFacilityActive(this.parent))
				{
					return true;
				}
			}
			return false;
		}

		private void UnlinkAll()
		{
			for (int i = 0; i < this.linkedBuildings.Count; i++)
			{
				this.linkedBuildings[i].TryGetComp<CompAffectedByFacilities>().Notify_LinkRemoved(this.parent);
			}
			this.linkedBuildings.Clear();
		}
	}
}
