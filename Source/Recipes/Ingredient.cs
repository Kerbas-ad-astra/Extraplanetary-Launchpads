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
	public class Ingredient
	{
		public string name;
		public double ratio;

		public Ingredient (string name, double ratio)
		{
			this.name = name;
			this.ratio = ratio;
		}
		public Ingredient (ConfigNode.Value ingredient)
		{
			name = ingredient.name;
			if (!double.TryParse (ingredient.value, out ratio)) {
				ratio = 0;
			}
		}
		public bool isReal
		{
			get {
				PartResourceDefinition res_def;
				res_def = PartResourceLibrary.Instance.GetDefinition (name);
				return res_def != null;
			}
		}
	}
}
