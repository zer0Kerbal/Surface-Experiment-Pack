#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

ModuleSEPECViewer - Part Module for adding resource info to EVA right-click menus

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SEPScience
{
	public class ModuleSEPECViewer : PartModule
	{
		private UIPartActionWindow window;

		public override void OnStart(PartModule.StartState state)
		{
			SEP_Utilities.onWindowSpawn.Add(onWindowSpawn);
			SEP_Utilities.onWindowDestroy.Add(onWindowDestroy);
		}

		private void OnDestroy()
		{
			SEP_Utilities.onWindowSpawn.Remove(onWindowSpawn);
			SEP_Utilities.onWindowDestroy.Remove(onWindowDestroy);
		}

		private void LateUpdate()
		{
			if (window == null)
				return;

			if (UIPartActionController.Instance == null)
				return;

			if (FlightGlobals.ActiveVessel == vessel)
				return;

			if (FlightDriver.Pause)
				return;

			int l = part.Resources.Count;

			for (int i = 0; i < l; i++)
			{
				PartResource r = part.Resources[i];

				window.AddResourceFlightControl(r);
			}

			var items = window.ListItems;

			int c = items.Count;

			for (int j = 0; j < c; j++)
			{
				UIPartActionItem item = items[j];

				if (item is UIPartActionResource)
					item.UpdateItem();
			}

			window.PointerUpdate();
		}

		private void onWindowSpawn(UIPartActionWindow win)
		{
			if (win == null)
				return;

			if (win.part.flightID != part.flightID)
				return;

			if (FlightGlobals.ActiveVessel == vessel)
				return;

			window = win;
		}

		private void onWindowDestroy(UIPartActionWindow win)
		{
			if (win == null)
				return;

			if (win.part.flightID != part.flightID)
				return;

			if (window == null)
				return;

			window = null;
		}
	}
}
