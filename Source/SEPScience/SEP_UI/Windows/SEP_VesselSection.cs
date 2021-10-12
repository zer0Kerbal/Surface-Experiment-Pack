﻿#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEP_VesselSection - UI Interface holding info on vessel's with SEP experiments

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
using System.Linq;
using SEPScience.Unity.Interfaces;
using UnityEngine;
using KSP.Localization;

namespace SEPScience.SEP_UI.Windows
{
	public class SEP_VesselSection : IVesselSection
	{
		private string _name;
		private string _ectotal;
		private string _situation;
		private string _expcount;
		private bool _isvisible = true;
		private bool _cantransmit;
		private bool _isconnected;
		private bool _transmitavailable;
		private Guid _id;

		private double maxVesselEC;
		private double currentVesselEC;

		private Vessel vessel;
		private SEPScience.Unity.Unity.SEP_VesselSection vesselUISection;
		private List<SEP_ExperimentHandler> experiments = new List<SEP_ExperimentHandler>();
		private List<SEP_ExperimentSection> experimentSections = new List<SEP_ExperimentSection>();

		public SEP_VesselSection(Vessel v)
		{
			if (SEP_Controller.Instance == null)
				return;

			if (v == null)
				return;

			vessel = v;
			_name = v.vesselName;
			_id = v.id;

			experiments = SEP_Controller.Instance.getHandlers(v);

			_transmitavailable = SEP_Controller.Instance.TransmissionUpdgrade;
			_cantransmit = experiments.Any(e => e.controllerAutoTransmit);

			currentVesselEC = SEP_Utilities.getTotalVesselEC(v.protoVessel);
			maxVesselEC = SEP_Utilities.getMaxTotalVesselEC(v.protoVessel);

			_ectotal = getECString();

			_situation = getSituationString();

			experimentSections = new List<SEP_ExperimentSection>();
			addExperimentSections();

			_expcount = getExpCountString();

			GameEvents.onVesselWasModified.Add(onVesselModified);
			GameEvents.onVesselSituationChange.Add(onVesselSituationChange);
		}

		public void OnDestroy()
		{
			GameEvents.onVesselWasModified.Remove(onVesselModified);
			GameEvents.onVesselSituationChange.Remove(onVesselSituationChange);
		}

		public void Update()
		{
			if (vessel == null)
				return;

			if (vessel.protoVessel == null)
				return;

			if (vessel.loaded)
				currentVesselEC = SEP_Utilities.getTotalVesselEC(vessel);

			_ectotal = getECString();

			_isconnected = true;
		}

		private string getExpCountString()
		{
			return Localizer.Format("#LOC_SurfaceExperimentPack_UI_ExperimentCount", experiments.Count);
		}

		private string getECString()
		{
			if (vessel.loaded)
				return Localizer.Format("#LOC_SurfaceExperimentPack_UI_ECCount", currentVesselEC.ToString("N0"), maxVesselEC.ToString("N0"));
			else
				return "";
		}

		private string getSituationString()
		{
			return string.Format("{0} - {1}", vessel.mainBody.displayName.LocalizeBodyName(), ScienceUtil.GetExperimentBiome(vessel.mainBody, vessel.latitude, vessel.longitude));
		}

		private bool getConnectionStatus()
		{
			return true;
		}

		public string Name
		{
			get { return _name; }
		}

		public Guid ID
		{
			get { return _id; }
		}

		public float Signal
		{
			get
			{
				if (vessel == null)
					return 0;

				if (vessel.Connection == null)
					return 0;

				return (float)vessel.Connection.SignalStrength;
			}
		}

		public Sprite SignalSprite
		{
			get
			{
				if (!SEP_Utilities.spritesLoaded)
					return null;

				if (vessel == null)
					return SEP_Utilities.CommNetSprites[0];

				if (vessel.Connection == null)
					return SEP_Utilities.CommNetSprites[0];

				if (SEP_Utilities.CommNetSprites.Length < 5)
					return null;

				switch(vessel.Connection.Signal)
				{
					case CommNet.SignalStrength.None:
						return SEP_Utilities.CommNetSprites[0];
					case CommNet.SignalStrength.Red:
						return SEP_Utilities.CommNetSprites[1];
					case CommNet.SignalStrength.Orange:
						return SEP_Utilities.CommNetSprites[2];
					case CommNet.SignalStrength.Yellow:
						return SEP_Utilities.CommNetSprites[3];
					case CommNet.SignalStrength.Green:
						return SEP_Utilities.CommNetSprites[4];
					default:
						return null;
				}
			}
		}

		public bool IsVisible
		{
			get { return _isvisible; }
			set { _isvisible = value; }
		}

		public bool IsConnected
		{
			get { return _isconnected; }
		}

		public bool CanTransmit
		{
			get { return _cantransmit; }
			set
			{
				_cantransmit = value;

				if (!_transmitavailable)
					return;

				if (vessel.loaded)
				{
					var controllers = vessel.FindPartModulesImplementing<ModuleSEPStation>();

					for (int i = controllers.Count - 1; i >= 0; i--)
					{
						ModuleSEPStation station = controllers[i];

						if (station == null)
							continue;

						if (station.autoTransmit != value)
							station.toggleAutoTransmit();
					}
				}
				else
				{
					for (int i = experiments.Count - 1; i >= 0; i--)
					{
						SEP_ExperimentHandler handler = experiments[i];

						if (handler == null)
							continue;

						handler.controllerAutoTransmit = value;
					}

					for (int i = vessel.protoVessel.protoPartSnapshots.Count - 1; i >= 0; i--)
					{
						ProtoPartSnapshot part = vessel.protoVessel.protoPartSnapshots[i];

						if (part == null)
							continue;

						for (int j = part.modules.Count - 1; j >= 0; j--)
						{
							ProtoPartModuleSnapshot mod = part.modules[j];

							if (mod == null)
								continue;

							if (mod.moduleName != "ModuleSEPStation")
								continue;

							mod.moduleValues.SetValue("autoTransmit", value);
						}
					}
				}
			}
		}

		public bool AutoTransmitAvailable
		{
			get { return _transmitavailable; }
		}

		public string ECTotal
		{
			get { return _ectotal; }
		}

		public string Situation
		{
			get { return _situation; }
		}

		public string ExpCount
		{
			get { return _expcount; }
		}

		public IList<IExperimentSection> GetExperiments()
		{
			return new List<IExperimentSection>(experimentSections.ToArray());
		}

		public void setParent(SEPScience.Unity.Unity.SEP_VesselSection section)
		{
			vesselUISection = section;
		}

		public void setAutoTransmit(bool isOn)
		{
			_cantransmit = isOn;

			if (_transmitavailable && vesselUISection != null)
				vesselUISection.setTransmissionSilent(isOn);
		}

		public void StartAll()
		{
			if (!_isconnected)
				return;

			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentHandler handler = experiments[i];

				if (handler == null)
					continue;

				if (handler.experimentRunning)
					continue;

				if (vessel.loaded && handler.host != null)
					handler.host.DeployExperiment();
				else
				{
					handler.experimentRunning = true;
					handler.lastBackgroundCheck = Planetarium.GetUniversalTime();
				}
			}
		}

		public void PauseAll()
		{
			if (!_isconnected)
				return;

			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentHandler handler = experiments[i];

				if (handler == null)
					continue;

				if (!handler.experimentRunning)
					continue;

				if (vessel.loaded && handler.host != null)
					handler.host.PauseExperiment();
				else
					handler.experimentRunning = false;
			}
		}

		public CelestialBody VesselBody
		{
			get
			{
				if (vessel == null)
					return null;

				return vessel.mainBody;
			}
		}

		public void AddExperiment(SEP_ExperimentHandler h)
		{
			if (h == null)
				return;

			if (h.vessel != vessel)
				return;

			if (experiments.Contains(h))
				return;

			SEP_ExperimentSection section = new SEP_ExperimentSection(h, h.vessel);

			if (section == null)
				return;
	
			experiments.Add(h);
			experimentSections.Add(section);

			if (vesselUISection == null)
				return;

			vesselUISection.AddExperimentSection(section);

			_expcount = getExpCountString();

			vesselUISection.setExpCount(_expcount);
		}

		public void RemoveExperiment(SEP_ExperimentHandler h)
		{
			if (h == null)
				return;

			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentHandler handler = experiments[i];

				if (handler == null)
					continue;

				if (handler != h)
					continue;

				experiments.Remove(h);
				break;
			}

			for (int i = experimentSections.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentSection section = experimentSections[i];

				if (section == null)
					continue;

				if (section.Handler != h)
					continue;

				section.OnDestroy();
				experimentSections.Remove(section);
				section = null;

				break;
			}

			_expcount = getExpCountString();

			if (vesselUISection != null)
				vesselUISection.setExpCount(_expcount);
		}

		private void addExperimentSections()
		{
			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentHandler handler = experiments[i];

				if (handler == null)
					return;

				experimentSections.Add(new SEP_ExperimentSection(handler, vessel));
			}
		}

		private void onVesselModified(Vessel v)
		{
			if (v == null)
				return;

			if (v != vessel)
				return;

			maxVesselEC = SEP_Utilities.getMaxTotalVesselEC(v.protoVessel);

			_ectotal = getECString();
		}

		private void onVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> V)
		{
			if (V.host == null)
				return;

			if (V.host != vessel)
				return;

			_situation = getSituationString();

			if (vesselUISection != null)
				vesselUISection.setBiome(_situation);
		}
	}
}
