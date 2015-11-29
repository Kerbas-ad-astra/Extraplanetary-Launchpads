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
	// Thanks to Taranis Elsu and his Fuel Balancer mod for the inspiration.
	public class ResourceInfo {
		public List<IResourceContainer> containers = new List<IResourceContainer>();
	}

	public class VesselResources {
		public Dictionary<string, ResourceInfo> resources;
		public delegate void ResourceProcessor (VesselResources vr, string resource);

		public void AddPart (Part part)
		{
			foreach (PartResource resource in part.Resources) {
				ResourceInfo resourceInfo;
				if (!resources.ContainsKey (resource.resourceName)) {
					resourceInfo = new ResourceInfo ();
					resources[resource.resourceName] = resourceInfo;
				}
				resourceInfo = resources[resource.resourceName];
				resourceInfo.containers.Add (new PartResourceContainer (resource));
			}
		}

		public void RemovePart (Part part)
		{
			var remove_list = new List<string> ();
			foreach (var resinfo in resources) {
				string resource = resinfo.Key;
				ResourceInfo resourceInfo = resinfo.Value;
				for (int i = resourceInfo.containers.Count - 1; i >= 0; i--) {
					var container = resourceInfo.containers[i];
					if (container.part == part) {
						resourceInfo.containers.Remove (container);
					}
				}
				if (resourceInfo.containers.Count == 0) {
					remove_list.Add (resource);
				}
			}
			foreach (string resource in remove_list) {
				resources.Remove (resource);
			}
		}

		public VesselResources ()
		{
			resources = new Dictionary<string, ResourceInfo>();
		}

		public VesselResources (Part rootPart)
		{
			resources = new Dictionary<string, ResourceInfo>();
			AddPart (rootPart);
		}

		public VesselResources (Vessel vessel)
		{
			resources = new Dictionary<string, ResourceInfo>();
			foreach (Part part in vessel.parts) {
				AddPart (part);
			}
		}

		public VesselResources (Vessel vessel, HashSet<uint> blacklist)
		{
			resources = new Dictionary<string, ResourceInfo>();
			foreach (Part part in vessel.parts) {
				if (!blacklist.Contains (part.flightID)) {
					AddPart (part);
				}
			}
		}

		// Completely empty the vessel of any and all resources.
		public void RemoveAllResources (HashSet<string> resources_to_remove = null)
		{
			foreach (KeyValuePair<string, ResourceInfo> pair in resources) {
				string resource = pair.Key;
				if (resources_to_remove != null && !resources_to_remove.Contains (resource)) {
					continue;
				}
				ResourceInfo resourceInfo = pair.Value;
				foreach (var container in resourceInfo.containers) {
					container.amount = 0.0;
				}
			}
		}

		// Return the vessel's total capacity for the resource.
		// If the vessel has no such resource 0.0 is returned.
		public double ResourceCapacity (string resource)
		{
			if (!resources.ContainsKey (resource))
				return 0.0;
			ResourceInfo resourceInfo = resources[resource];
			double capacity = 0.0;
			foreach (var container in resourceInfo.containers) {
				capacity += container.maxAmount;
			}
			return capacity;
		}

		// Return the vessel's total available amount of the resource.
		// If the vessel has no such resource 0.0 is returned.
		public double ResourceAmount (string resource)
		{
			if (!resources.ContainsKey (resource))
				return 0.0;
			ResourceInfo resourceInfo = resources[resource];
			double amount = 0.0;
			foreach (var container in resourceInfo.containers) {
				amount += container.amount;
			}
			return amount;
		}

		// Transfer a resource into (positive amount) or out of (negative
		// amount) the vessel. No attempt is made to balance the resource
		// across parts: they are filled/emptied on a first-come-first-served
		// basis.
		// If the vessel has no such resource no action is taken.
		// Returns the amount of resource not transfered (0 = all has been
		// transfered).
		public double TransferResource (string resource, double amount)
		{
			if (!resources.ContainsKey (resource))
				return amount;
			ResourceInfo resourceInfo = resources[resource];
			foreach (var container in resourceInfo.containers) {
				double adjust = amount;
				double space = container.maxAmount - container.amount;
				if (adjust < 0  && -adjust > container.amount) {
					// Ensure the resource amount never goes negative
					adjust = -container.amount;
				} else if (adjust > 0 && adjust > space) {
					// ensure the resource amount never excees the maximum
					adjust = space;
				}
				container.amount += adjust;
				amount -= adjust;
			}
			return amount;
		}

		public double ResourceMass ()
		{
			double mass = 0;
			foreach (KeyValuePair<string, ResourceInfo> pair in resources) {
				string resource = pair.Key;
				var def = PartResourceLibrary.Instance.GetDefinition (resource);
				float density = def.density;
				ResourceInfo resourceInfo = pair.Value;
				foreach (var container in resourceInfo.containers) {
					mass += density * container.amount;
				}
			}
			return mass;
		}

		public void Process (ResourceProcessor resProc)
		{
			foreach (var resource in resources.Keys) {
				resProc (this, resource);
			}
		}
	}
}
