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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace ExtraplanetaryLaunchpads {

	public class ExTarget : PartModule, IModuleInfo, ITargetable
	{
		[KSPField]
		public string TargetName = "Target";
		[KSPField]
		public string TargetTransform;
		public Transform targetTransform;

		public override string GetInfo ()
		{
			return "Targetable";
		}

		public string GetPrimaryField ()
		{
			return null;
		}

		public string GetModuleTitle ()
		{
			return "EL Target";
		}

		public Callback<Rect> GetDrawModulePanelCallback ()
		{
			return null;
		}

		// ITargetable interface
		public Vector3 GetFwdVector ()
		{
			return targetTransform.forward;
		}
		public string GetName ()
		{
			return vessel.vesselName + " " + TargetName;
		}
		public Vector3 GetObtVelocity ()
		{
			return vessel.obt_velocity;
		}
		public Orbit GetOrbit ()
		{
			return vessel.orbit;
		}
		public OrbitDriver GetOrbitDriver ()
		{
			return vessel.orbitDriver;
		}
		public Vector3 GetSrfVelocity ()
		{
			return vessel.srf_velocity;
		}
		public Transform GetTransform ()
		{
			return targetTransform;
		}
		public Vessel GetVessel ()
		{
			return vessel;
		}
		public VesselTargetModes GetTargetingMode ()
		{
			return VesselTargetModes.DirectionVelocityAndOrientation;
		}
		public bool GetActiveTargetable()
		{
			return false;
		}

		public override void OnLoad (ConfigNode node)
		{
			targetTransform = part.FindModelTransform (TargetTransform);
			if (targetTransform == null) {
				targetTransform = transform;
			}
		}

		public void Update ()
		{
			if (FlightGlobals.fetch != null) {
				if (FlightGlobals.fetch.VesselTarget == this as ITargetable) {
					Events["SetAsTarget"].active = false;
					Events["UnsetTarget"].active = true;
				} else {
					Events["SetAsTarget"].active = true;
					Events["UnsetTarget"].active = false;
				}
			}
		}

		[KSPEvent (guiName = "Set as Target", guiActiveUnfocused = true,
				   externalToEVAOnly = false, guiActive = false,
				   unfocusedRange = 200f, active = true)]
		public void SetAsTarget ()
		{
			FlightGlobals.fetch.SetVesselTarget (this);
		}

		[KSPEvent (guiName = "Unset Target", guiActiveUnfocused = true,
				   externalToEVAOnly = false, guiActive = false,
				   unfocusedRange = 200f, active = false)]
		public void UnsetTarget ()
		{
			FlightGlobals.fetch.SetVesselTarget (null);
		}

		public override void OnAwake ()
		{
		}
	}
}
