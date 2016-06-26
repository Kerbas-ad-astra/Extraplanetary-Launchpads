/*
This file is part of Extraplanetary Launchpads.

Extraplanetary Launchpads is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Extraplanetary Launchpads is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Extraplanetary Launchpads.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtraplanetaryLaunchpads {
	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
	public class ExShipInfo : MonoBehaviour
	{
		static Rect winpos;
		static bool showGUI = true;

		int parts_count;
		public BuildCost buildCost;
		CostReport cashed_cost;
		Vector2 scrollPosR, scrollPosO;

		public static void ToggleGUI ()
		{
			showGUI = !showGUI;
			if (!showGUI) {
				InputLockManager.RemoveControlLock ("EL_ShipInfo_window_lock");
			}
		}

		public static void LoadSettings (ConfigNode node)
		{
			string val = node.GetValue ("rect");
			if (val != null) {
				Quaternion pos;
				pos = ConfigNode.ParseQuaternion (val);
				winpos.x = pos.x;
				winpos.y = pos.y;
				winpos.width = pos.z;
				winpos.height = pos.w;
			}
			val = node.GetValue ("visible");
			if (val != null) {
				bool.TryParse (val, out showGUI);
			}
		}

		public static void SaveSettings (ConfigNode node)
		{
			Quaternion pos;
			pos.x = winpos.x;
			pos.y = winpos.y;
			pos.z = winpos.width;
			pos.w = winpos.height;
			node.AddValue ("rect", KSPUtil.WriteQuaternion (pos));
			node.AddValue ("visible", showGUI);
		}

		void addPart (Part part)
		{
			//Debug.Log (String.Format ("[EL GUI] attach: {0}", part));
			buildCost.addPart (part);
			parts_count++;
		}

		int rebuild_list_wait_frames = 0;

		private IEnumerator WaitAndRebuildList (ShipConstruct ship)
		{
			while (--rebuild_list_wait_frames > 0) {
				yield return null;
			}

			buildCost = null;
			cashed_cost = null;
			parts_count = 0;

			if (ship == null || ship.parts == null || ship.parts.Count < 1
				|| ship.parts[0] == null) {
				yield break;
			}

			if (ship.parts.Count > 0) {
				Part root = ship.parts[0].localRoot;

				buildCost = new BuildCost ();
				addPart (root);
				foreach (Part p in root.GetComponentsInChildren<Part>()) {
					if (p != root) {
						addPart (p);
					}
				}
			}
			cashed_cost = buildCost.cost;
		}

		public void RebuildList(ShipConstruct ship)
		{
			// some parts/modules fire the event before doing things
			const int wait_frames = 2;
			if (rebuild_list_wait_frames < wait_frames) {
				rebuild_list_wait_frames += wait_frames;
				if (rebuild_list_wait_frames == wait_frames) {
					StartCoroutine (WaitAndRebuildList (ship));
				}
			}
		}

		void onEditorRestart ()
		{
			buildCost = null;
			cashed_cost = null;
			parts_count = 0;
		}

		void Awake ()
		{
			GameEvents.onEditorShipModified.Add (RebuildList);
			GameEvents.onEditorRestart.Add (onEditorRestart);
		}

		void OnDestroy ()
		{
			GameEvents.onEditorShipModified.Remove (RebuildList);
			GameEvents.onEditorRestart.Remove (onEditorRestart);
		}

		void OnGUI ()
		{
			if (!showGUI || buildCost == null)
				return;

			if (winpos.x == 0 && winpos.y == 0) {
				winpos.x = Screen.width / 2;
				winpos.y = Screen.height / 2;
				winpos.width = 300;
				winpos.height = 100;
			}
			winpos = GUILayout.Window (GetInstanceID (), winpos, InfoWindow,
									  "Build Resources",
									  GUILayout.MinWidth (200));
			if (enabled && winpos.Contains (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y))) {
				InputLockManager.SetControlLock ("EL_ShipInfo_window_lock");
			} else {
				InputLockManager.RemoveControlLock ("EL_ShipInfo_window_lock");
			}
		}

		private void UnitLabel (string title, double amount, string units)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (title + ":");
			GUILayout.FlexibleSpace ();
			GUILayout.Label (amount.ToStringSI(4, unit:units));
			GUILayout.EndHorizontal ();
		}

		private void MassLabel (string title, double mass)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(title + ":");
			GUILayout.FlexibleSpace();
			GUILayout.Label(EL_Utils.FormatMass(mass));
			GUILayout.EndHorizontal();
		}

		private Vector2 ResourcePanel (string title,
									   List<BuildResource> resources,
									   Vector2 scrollPos)
		{
			GUILayout.Label (title + ":");
			GUILayout.BeginVertical (GUILayout.Height (100));
			scrollPos = GUILayout.BeginScrollView (scrollPos);
			foreach (var res in resources) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label (String.Format ("{0}:", res.name));
				GUILayout.FlexibleSpace ();
				GUILayout.Label (String.Format ("{0} ({1})", res.amount.ToStringSI(4, unit:"u"), EL_Utils.FormatMass(res.mass, 4)));
				GUILayout.EndHorizontal ();
			}
			GUILayout.EndScrollView ();
			GUILayout.EndVertical ();
			return scrollPos;
		}

		void InfoWindow (int windowID)
		{
			var cost = cashed_cost;
			double required_mass = 0;
			double resource_mass = 0;
			double kerbalHours = 0;

			foreach (var res in cost.required) {
				kerbalHours += res.kerbalHours * res.amount;
				required_mass += res.mass;
			}
			kerbalHours = Math.Round (kerbalHours, 4);

			foreach (var res in cost.optional) {
				resource_mass += res.mass;
			}

			GUILayout.BeginVertical ();

			MassLabel ("Dry mass", buildCost.mass);
			MassLabel ("Resource mass", resource_mass);
			MassLabel ("Total mass", required_mass + resource_mass);
			UnitLabel ("Build Time", kerbalHours, "Kh");

			cost.optional.Sort ();
			GUILayout.Label (" ");
			scrollPosR = ResourcePanel ("Required", cost.required, scrollPosR);
			scrollPosO = ResourcePanel ("Optional", cost.optional, scrollPosO);

			string ver = ExtraplanetaryLaunchpadsVersionReport.GetVersion ();
			GUILayout.Label(ver);
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}
	}
}
