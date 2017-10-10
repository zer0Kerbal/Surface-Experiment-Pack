#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

ModuleSEPStation - Part module for the SEP Central Station

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SEPScience.SEP_UI.Windows;
using UnityEngine;
using Experience.Effects;
using KSP.Localization;

namespace SEPScience
{
	public class ModuleSEPStation : PartModule, IScalarModule, IContractObjectiveModule
	{
		[KSPField]
		public int maxExperiments = 8;
		[KSPField]
		public string conductExperimentsEventName = "#LOC_SurfaceExperimentPack_ModuleSEPStation_conductExperimentsEventName"; // "Begin All Experiments";
		//[KSPField]
		//public string transmitUnlockTech = "";
		[KSPField]
		public float interactionRange = 1.5f;
		[KSPField(isPersistant = true)]
		public bool IsDeployed;
		[KSPField]
		public bool animated;
		[KSPField]
		public string animationName;
		[KSPField]
		public float animSpeed = 1;
		[KSPField]
		public string deployEventName = "#LOC_SurfaceExperimentPack_ModuleSEPStation_deployEventName"; // "Deploy";
		[KSPField]
		public string retractEventName = "#LOC_SurfaceExperimentPack_ModuleSEPStation_retractEventName"; // "Retract";
		[KSPField(isPersistant = true)]
		public bool autoTransmit;
		
		[KSPField]
		public string exp0;
		[KSPField]
		public string exp1;
		[KSPField]
		public string exp2;
		[KSPField]
		public string exp3;
		[KSPField]
		public string exp4;
		[KSPField]
		public string exp5;
		[KSPField]
		public string exp6;
		[KSPField]
		public string exp7;
		[KSPField]
		public string exp8;

		private UIPartActionWindow window;

		private EventData<float> onStop;
		private EventData<float, float> onMove;
		private float scalar;

		private List<ModuleSEPScienceExperiment> experiments = new List<ModuleSEPScienceExperiment>();
		public int connectedExperimentCount
		{
			get { return experiments.Count; }
		}

		private Animation anim;

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);

			if (state == StartState.Editor)
				return;

			if (maxExperiments > 8)
				maxExperiments = 8;

			experiments = new List<ModuleSEPScienceExperiment>(maxExperiments);

			if (animated && !string.IsNullOrEmpty(animationName))
			{
				anim = part.FindModelAnimators(animationName)[0];
				anim.wrapMode = WrapMode.Clamp;
				anim.playAutomatically = false;
				anim.cullingType = AnimationCullingType.BasedOnRenderers;
				anim.Stop(animationName);
				animator(anim, animationName, -1, 0);
			}

			if (IsDeployed)
			{
				animator(anim, animationName, 1, 1, true);
				scalar = 1;
			}

			//if (string.IsNullOrEmpty(transmitUnlockTech))
			//	transmissionUpgrade = true;
			//else
			//{
			//	if (ResearchAndDevelopment.GetTechnologyState(transmitUnlockTech) == RDTech.State.Available)
			//		transmissionUpgrade = true;
			//	else
			//		transmissionUpgrade = false;
			//}

			SEP_Utilities.onWindowSpawn.Add(onWindowSpawn);
			SEP_Utilities.onWindowDestroy.Add(onWindowDestroy);
		}

		private void OnDestroy()
		{
			SEP_Utilities.onWindowSpawn.Remove(onWindowSpawn);
			SEP_Utilities.onWindowDestroy.Remove(onWindowDestroy);
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			StartCoroutine(delayedStartup());
		}

		private IEnumerator delayedStartup()
		{
			//SEPUtilities.log("Delayed SEP Station Starting...", logLevels.warning);

			int timer = 0;

			while (!FlightGlobals.ready)
				yield return null;

			while (timer < 60)
			{
				timer++;
				yield return null;
			}

			//SEPUtilities.log("SEP Station Starting...", logLevels.warning);

			getConnectedExperiments();
			setCollectEvent();
			setControllerFields();
			setupEvents();
		}

		private void setupEvents()
		{
			Events["ConductExperiments"].guiName = conductExperimentsEventName;
			Events["ConductExperiments"].unfocusedRange = interactionRange;
			Events["DeployEvent"].active = !IsDeployed;
			Events["DeployEvent"].guiName = deployEventName;
			Events["DeployEvent"].unfocusedRange = interactionRange;
			Events["RetractEvent"].active = IsDeployed;
			Events["RetractEvent"].guiName = retractEventName;
			Events["RetractEvent"].unfocusedRange = interactionRange;
			Events["toggleAutoTransmit"].active = SEP_Controller.Instance.TransmissionUpdgrade;
			Events["toggleAutoTransmit"].guiName = autoTransmit ? Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPStation_autoTransmitOff") : Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPStation_autoTransmitOn");
			Events["toggleAutoTransmit"].unfocusedRange = interactionRange;
			Events["CollectEvent"].guiName = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPStation_collectData");
			Events["CollectEvent"].unfocusedRange = interactionRange;
			Events["ReviewEvent"].guiName = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPStation_reviewData");
			Events["ReviewEvent"].unfocusedRange = interactionRange;
		}

		public override string GetInfo()
		{
			string s = base.GetInfo();

			s += Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPStation_infoMax", maxExperiments);

			return s;
		}

		private void Update()
		{
			if (!FlightGlobals.ready)
				return;

			if (UIPartActionController.Instance != null)
			{
				if (UIPartActionController.Instance.ItemListContains(part, false))
					updateControllerFields();
			}
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

		private void LateUpdate()
		{
			if (window == null)
				return;

			if (FlightGlobals.ActiveVessel == vessel)
				return;

			if (FlightDriver.Pause)
				return;

			if (!window.Pinned && !InputLockManager.IsAllLocked(ControlTypes.All))
			{
				int l = Fields.Count;

				for (int i = 0; i < l; i++)
				{
					BaseField b = Fields[i];

					if (!b.guiActive)
						continue;

					window.AddFieldControl(b, part, this);
				}
			}

			var items = window.ListItems;

			int c = items.Count;

			for (int j = 0; j < c; j++)
			{
				UIPartActionItem item = items[j];

				if (item is UIPartActionLabel)
					item.UpdateItem();
			}
		}

		private void getConnectedExperiments()
		{
			var modules = vessel.FindPartModulesImplementing<ModuleSEPScienceExperiment>().Where(m => m.IsDeployed).ToList();

			int l = modules.Count;

			for (int i = 0; i < l; i++)
			{
				if (experiments.Count >= maxExperiments)
					break;

				ModuleSEPScienceExperiment exp = modules[i];

				if (exp == null)
					continue;

				if (exp.Handler == null)
					continue;

				if (!exp.IsDeployed)
					continue;

				if (exp.Controller != null)
					continue;

				exp.setController(this);
				experiments.Add(exp);
			}

			Events["ConductExperiments"].active = experiments.Count > 0;
		}

		public void addConnectecExperiment(ModuleSEPScienceExperiment mod)
		{
			if (experiments.Count >= maxExperiments)
			{
				SEP_Utilities.log("SEP Control Station module at capacity...", logLevels.log);
				return;
			}

			if (mod == null)
				return;

			if (!mod.IsDeployed)
				return;

			if (mod.Handler == null)
				return;

			experiments.Add(mod);
			setControllerFields();
			setCollectEvent();
		}

		public void removeConnectedExperiment(ModuleSEPScienceExperiment mod)
		{
			if (!experiments.Contains(mod))
				return;

			experiments.Remove(mod);
			setControllerFields();
			setCollectEvent();
		}
		private void updateConnectedExperiments()
		{
			if (!IsDeployed)
				return;

			int l = experiments.Count;

			for (int i = 0; i < l; i++)
			{
				ModuleSEPScienceExperiment exp = experiments[i];

				if (exp == null)
					continue;

				if (!exp.IsDeployed)
					continue;

				if (exp.Handler == null)
					continue;

				if (exp.Controller != this)
					continue;

				exp.controllerAutoTransmit = autoTransmit;

				exp.Handler.updateController(this);
			}
		}

		private void closeConnectedExperiments()
		{
			int l = experiments.Count;

			for (int i = 0; i < l; i++)
			{
				ModuleSEPScienceExperiment exp = experiments[i];

				if (exp == null)
					continue;

				exp.setController(null);
				experiments[i] = null;
			}
		}

		private void setControllerFields()
		{
			for (int i = 0; i < maxExperiments; i++)
			{
				string s = "exp" + i.ToString();

				Fields[s].guiActive = false;

				if (IsDeployed)
				{
					if (i >= experiments.Count)
						continue;

					ModuleSEPScienceExperiment exp = experiments[i];

					if (exp == null)
						continue;

					if (!exp.IsDeployed)
						continue;

					if (exp.Handler == null)
						continue;

					if (exp.Controller != this)
						continue;

					Fields[s].guiActive = true;
					if (exp.Handler != null)
						Fields[s].guiName = exp.Handler.experimentTitle;
				}
			}
		}

		private void updateControllerFields()
		{
			if (!IsDeployed)
				return;

			int l = experiments.Count;

			for (int i = 0; i < l; i++)
			{
				ModuleSEPScienceExperiment exp = experiments[i];

				if (exp == null)
					continue;

				if (!exp.IsDeployed)
					continue;

				if (exp.Handler == null)
					continue;

				if (exp.Controller != this)
					continue;

				string val = "";

				if (exp.experimentRunning)
					val = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPStation_completion", exp.Handler.completion.ToString("P0"));
				else
					val = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPStation_calibration", exp.Handler.calibration.ToString("P0"));

				switch(i)
				{
					case 0:
						exp0 = val;
						break;
					case 1:
						exp1 = val;
						break;
					case 2:
						exp2 = val;
						break;
					case 3:
						exp3 = val;
						break;
					case 4:
						exp4 = val;
						break;
					case 5:
						exp5 = val;
						break;
					case 6:
						exp6 = val;
						break;
					case 7:
						exp7 = val;
						break;
					default:
						break;
				}
			}
		}

		public void setCollectEvent()
		{
			Events["CollectEvent"].active = false;
			Events["ReviewEvent"].active = false;

			int l = experiments.Count;

			for (int i = 0; i < l; i++)
			{
				ModuleSEPScienceExperiment exp = experiments[i];

				if (exp == null)
					continue;

				if (!exp.IsDeployed)
					continue;

				if (exp.GetData().Length > 0)
				{
					Events["CollectEvent"].active = true;
					Events["ReviewEvent"].active = true;
					break;
				}
			}
		}

		[KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, active = false)]
		public void CollectEvent()
		{
			int l = experiments.Count;

			for (int i = 0; i < l; i++)
			{
				ModuleSEPScienceExperiment experiment = experiments[i];

				if (experiment == null)
					continue;

				experiment.CollectData();
			}

			Events["CollectEvent"].active = false;
		}

		[KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, active = false)]
		public void ReviewEvent()
		{
			int l = experiments.Count;

			for (int i = 0; i < l; i++)
			{
				ModuleSEPScienceExperiment experiment = experiments[i];

				if (experiment == null)
					continue;

				experiment.ReviewDataEvent();
			}
		}

		[KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, active = true)]
		public void DeployEvent()
		{
			IsDeployed = true;

			if (animated)
				animator(anim, animationName, animSpeed, 0);

			getConnectedExperiments();

			scalar = 1;
			
			setControllerFields();

			Events["ConductExperiments"].active = true;
			Events["DeployEvent"].active = false;
			Events["RetractEvent"].active = true;
		}

		[KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, active = false)]
		public void RetractEvent()
		{
			IsDeployed = false;

			if (animated)
				animator(anim, animationName, -1 * animSpeed, 1);

			closeConnectedExperiments();

			scalar = 0;

			setControllerFields();

			Events["CollectEvent"].active = false;
			Events["ReviewEvent"].active = false;
			Events["ConductExperiments"].active = false;
			Events["DeployEvent"].active = true;
			Events["RetractEvent"].active = false;
		}

		private void animator(Animation a, string name, float speed, float time, bool force = false)
		{
			if (a == null)
				return;

			a[name].speed = speed;

			if (!a.IsPlaying(name))
			{
				a[name].normalizedTime = time;
				a.Blend(name);
			}
			else if (force)
				a[name].normalizedTime = time;
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, externalToEVAOnly = true, active = false)]
		public void ConductExperiments()
		{
			if (FlightGlobals.ActiveVessel == null)
				return;

			if (!FlightGlobals.ActiveVessel.isEVA)
			{
				ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_EVAWarning"), 5f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}

			int l = experiments.Count;

			for (int i = 0; i < l; i++)
			{
				ModuleSEPScienceExperiment experiment = experiments[i];

				if (experiment == null)
					continue;

				experiment.DeployExperiment();
			}
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, externalToEVAOnly = true, active = false)]
		public void toggleAutoTransmit()
		{				
			autoTransmit = !autoTransmit;

			updateConnectedExperiments();

			SEP_VesselSection section = SEP_AppLauncher.Instance.getVesselSection(vessel);

			if (section != null)
				section.setAutoTransmit(autoTransmit);

			Events["toggleAutoTransmit"].guiName = autoTransmit ? Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPStation_autoTransmitOff") : Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPStation_autoTransmitOn");
		}

		#region IContractObjectiveModule

		public bool CheckContractObjectiveValidity()
		{
			return true;
		}

		public string GetContractObjectiveType()
		{
			return "Antenna";
		}

		#endregion

		#region IScalar

		public bool CanMove
		{
			get { return true; }
		}

		public float GetScalar
		{
			get { return scalar; }
		}

		public EventData<float, float> OnMoving
		{
			get { return onMove; }
		}

		public EventData<float> OnStop
		{
			get { return onStop; }
		}

		public bool IsMoving()
		{
			if (anim == null)
				return false;

			if (anim.isPlaying && anim[animationName] != null && anim[animationName].speed != 0f)
				return true;

			return false;
		}

		public void SetScalar(float t)
		{
			if (t > 0 && !IsDeployed)
				DeployEvent();

			scalar = t;
		}

		public string ScalarModuleID
		{
			get { return "sepcontroller"; }
		}

		public void SetUIRead(bool state)
		{

		}
		public void SetUIWrite(bool state)
		{

		}

		#endregion

	}
}
