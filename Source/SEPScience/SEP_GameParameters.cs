#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEP_GameParameters - SEP settings

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
using UnityEngine;
using SEPScience.SEP_UI.Toolbar;

namespace SEPScience
{
	public class SEP_GameParameters : GameParameters.CustomParameterNode
	{

		[GameParameters.CustomParameterUI("#LOC_SurfaceExperimentPack_Settings_ShowAll", toolTip = "#LOC_SurfaceExperimentPack_SettingsTooltip_ShowAll", autoPersistance = true)]
		public bool showAllVessels;
		[GameParameters.CustomParameterUI("#LOC_SurfaceExperimentPack_Settings_FadeOut", toolTip = "#LOC_SurfaceExperimentPack_SettingsTooltip_FadeOut", autoPersistance = true)]
		public bool fadeOut = true;
		[GameParameters.CustomFloatParameterUI("#LOC_SurfaceExperimentPack_Settings_Scale", asPercentage = true, minValue = 0.5f, maxValue = 2, displayFormat = "N2", autoPersistance = true)]
		public float scale = 1;
		[GameParameters.CustomParameterUI("#LOC_SurfaceExperimentPack_Settings_StockToolbar", autoPersistance = true)]
		public bool stockToolbar = true;
		[GameParameters.CustomParameterUI("#LOC_SurfaceExperimentPack_Settings_OpenHover", toolTip = "#LOC_SurfaceExperimentPack_SettingsTooltip_OpenHover", autoPersistance = true)]
		public bool hoverOpen = true;
		[GameParameters.CustomParameterUI("#LOC_SurfaceExperimentPack_Settings_Default", toolTip = "#LOC_SurfaceExperimentPack_SettingsTooltip_Default", autoPersistance = false)]
		public bool useAsDefault;

		public SEP_GameParameters()
		{
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				if (SEP_PersistentSettings.Instance == null)
					return;

				showAllVessels = SEP_PersistentSettings.Instance.showAllVessels;
				fadeOut = SEP_PersistentSettings.Instance.fadeout;
				scale = SEP_PersistentSettings.Instance.scale;
				stockToolbar = SEP_PersistentSettings.Instance.stockToolbar;
				hoverOpen = SEP_PersistentSettings.Instance.hoverOpen;
			}
		}

		public override GameParameters.GameMode GameMode
		{
			get { return GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE; }
		}

		public override bool HasPresets
		{
			get { return false; }
		}

		public override string Section
		{
			get { return "Surface Experiment Package"; }
		}

		public override string DisplaySection
		{
			get { return "Surface Experiment Package"; }
		}

		public override int SectionOrder
		{
			get { return 0; }
		}

		public override string Title
		{
			get { return "Surface Experiment Package"; }
		}

		public override bool Enabled(System.Reflection.MemberInfo member, GameParameters parameters)
		{
			if (member.Name == "fadeOut")
				return false;

			if (member.Name == "hoverOpen")
				return stockToolbar;

			if (member.Name == "stockToolbar")
				return ToolbarManager.ToolbarAvailable;

			return true;
		}
	}
}
