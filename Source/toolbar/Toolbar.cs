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

namespace ExtraplanetaryLaunchpads {
	using Toolbar;

	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
	public class ExToolbar_ShipInfo : MonoBehaviour
	{
		private IButton ExEditorButton;

		public void Awake ()
		{
			if (ToolbarManager.Instance == null) {
				return;
			}
			ExEditorButton = ToolbarManager.Instance.add ("ExtraplanetaryLaunchpads", "ExEditorButton");
			ExEditorButton.TexturePath = "ExtraplanetaryLaunchpads/Textures/icon_button";
			ExEditorButton.ToolTip = "EL Build Resources Display";
			ExEditorButton.OnClick += (e) => ExShipInfo.ToggleGUI ();
		}

		void OnDestroy()
		{
			if (ExEditorButton != null) {
				ExEditorButton.Destroy ();
			}
		}
	}

	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class ExToolbar_BuildWindow : MonoBehaviour
	{
		private IButton ExBuildWindowButton;

		public void Awake ()
		{
			if (ToolbarManager.Instance == null) {
				return;
			}
			ExBuildWindowButton = ToolbarManager.Instance.add ("ExtraplanetaryLaunchpads", "ExBuildWindowButton");
			ExBuildWindowButton.TexturePath = "ExtraplanetaryLaunchpads/Textures/icon_button";
			ExBuildWindowButton.ToolTip = "EL Build Window";
			ExBuildWindowButton.OnClick += (e) => ExBuildWindow.ToggleGUI ();
		}

		void OnDestroy()
		{
			if (ExBuildWindowButton != null) {
				ExBuildWindowButton.Destroy ();
			}
		}
	}

	[KSPAddon (KSPAddon.Startup.SpaceCentre, false)]
	public class ExToolbar_SettingsWindow : MonoBehaviour
	{
		private IButton ExSettingsButton;

		public void Awake ()
		{
			if (ToolbarManager.Instance == null) {
				return;
			}
			ExSettingsButton = ToolbarManager.Instance.add ("ExtraplanetaryLaunchpads", "ExSettingsButton");
			ExSettingsButton.TexturePath = "ExtraplanetaryLaunchpads/Textures/icon_button";
			ExSettingsButton.ToolTip = "EL Settings Window";
			ExSettingsButton.OnClick += (e) => ExSettings.ToggleGUI ();
		}

		void OnDestroy()
		{
			if (ExSettingsButton != null) {
				ExSettingsButton.Destroy ();
			}
		}
	}
}
