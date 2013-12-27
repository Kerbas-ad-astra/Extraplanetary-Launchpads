using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace ExLP {

public class ExWorkshop : PartModule
{
	[KSPField]
	public float ProductivityFactor = 1.0f;

	[KSPField (guiName = "Productivity", guiActive = true)]
	public float Productivity;

	public override string GetInfo()
	{
		return "Workshop";
	}

	private float KerbalContribution(Kerbal kerbal)
	{
		float s = kerbal.stupidity;
		float c = kerbal.courage;
		float contribution;
		
		if (kerbal.isBadass) {
			float a = -2;
			float v = 2 * (1 - s);
			float y = 1 - 2 * s;
			contribution = y + (v + a * c / 2) * c;
		} else {
			contribution = 1 - 2 * s * c * c;
		}
		Debug.Log(String.Format("[EL] contribution: {0} {1} {2} {3}",
								kerbal.crewMemberName, s, c, contribution));
		return contribution;
	}

	private void DetermineProductivity()
	{
		float kh = 0;
		foreach (var crew in part.protoModuleCrew) {
			kh += KerbalContribution(crew.KerbalRef);
		}
		Productivity = kh * ProductivityFactor;
	}

	void onCrewBoard(GameEvents.FromToAction<Part,Part> ft)
	{
		//var kerbal = ft.from.protoModuleCrew[0];
		Part p = ft.to;

		if (p != part)
			return;

		var kerbal = p.protoModuleCrew.Last().KerbalRef;
		Debug.Log(String.Format("[EL] board: {0} {1}", kerbal, p));
		DetermineProductivity();
	}

	void onCrewEVA(GameEvents.FromToAction<Part,Part> ft)
	{
		var kerbal = ft.to.protoModuleCrew[0];
		Part p = ft.from;
		Debug.Log(String.Format("[EL] eva: {0} {1}", p, kerbal));
		DetermineProductivity();
	}

	public override void OnLoad(ConfigNode node)
	{
		if (HighLogic.LoadedScene == GameScenes.FLIGHT) {
			GameEvents.onCrewBoardVessel.Add(onCrewBoard);
			GameEvents.onCrewOnEva.Add(onCrewEVA);
		}
	}

	void OnDestroy()
	{
		GameEvents.onCrewBoardVessel.Remove(onCrewBoard);
		GameEvents.onCrewOnEva.Remove(onCrewEVA);
	}

	public override void OnStart(PartModule.StartState state)
	{
		if (state == PartModule.StartState.None
			|| state == PartModule.StartState.Editor)
			return;
		DetermineProductivity();
	}

	public override void OnFixedUpdate()
	{
		double work = Productivity * TimeWarp.fixedDeltaTime / 60;
		part.RequestResource("KerbalMinutes", -work);
	}
}

}
