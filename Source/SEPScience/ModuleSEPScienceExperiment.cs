﻿#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

ModuleSEPScienceExperiment - Part Module for SEP experiments

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
using FinePrint.Utilities;
using UnityEngine;
using KSP.UI.Screens.Flight.Dialogs;
using Experience.Effects;
using KSP.Localization;

namespace SEPScience
{
    public class ModuleSEPScienceExperiment : PartModule, IScienceDataContainer
    {
		[KSPField]
		public string transmitWarningText;
		[KSPField]
		public string situationFailMessage = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_situationFailMessage"; // "Can't conduct experiment here";
		[KSPField]
		public string collectWarningText;
		[KSPField]
		public bool resettable;
		[KSPField]
		public bool excludeAtmosphere;
		[KSPField]
		public string excludeAtmosphereMessage = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_excludeAtmosphereMessage"; // "This experiment can't be conducted within an atmosphere";
		[KSPField]
		public string includeAtmosphereMessage = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_includeAtmosphereMessage"; // "This experiment can only be conducted within an atmosphere";
		[KSPField]
		public float interactionRange = 1.5f;
		[KSPField(isPersistant = true)]
		public bool IsDeployed;
		[KSPField(isPersistant = true)]
		public bool Inoperable;
		[KSPField]
		public string experimentActionName = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_experimentActionName"; // "Deploy Experiment";
		[KSPField]
		public string stopExperimentName = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_stopExperimentName"; // "Pause Experiment";
		[KSPField]
		public string collectActionName = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_collectActionName"; // "Collect Data";
		[KSPField]
		public string reviewActionName = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_reviewActionName"; // "Review Data";
		[KSPField]
		public string requiredModules;
		[KSPField]
		public string requiredParts;
		[KSPField]
		public string requiredPartsMessage = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_requiredPartsMessage"; // "Required parts are not present on this vessel";
		[KSPField]
		public string requiredModulesMessage = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_requiredModulesMessage"; // "Required part modules are not present on this vessel";
		[KSPField]
		public string controllerModule = "ModuleSEPStation";
		[KSPField]
		public string controllerModuleMessage = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_controllerModuleMessage"; // "Controller module is not connected to the experiment";
		[KSPField]
		public string calibrationEventName = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_calibrationEventName"; // "Calibrate";
		[KSPField]
		public string retractEventName = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_retractEventName"; // "Shut Down";
		[KSPField]
		public bool animated;
		[KSPField]
		public string animationName;
		[KSPField]
		public float animSpeed = 1;
		[KSPField]
		public bool oneShotAnim;
		[KSPField]
		public int complexity = 1;

		//Persistent fields read by the handler module
		[KSPField(isPersistant = true)]
		public bool canTransmit = true;
		[KSPField(isPersistant = true)]
		public bool instantResults;
		[KSPField(isPersistant = true)]
		public bool experimentRunning;
		[KSPField(isPersistant = true)]
		public bool controllerCanTransmit;
		[KSPField(isPersistant = true)]
		public bool controllerAutoTransmit;
		[KSPField(isPersistant = true)]
		public string experimentID;
		[KSPField(isPersistant = true)]
		public float xmitDataScalar = 1;
		[KSPField(isPersistant = true)]
		public float calibration;
		[KSPField(isPersistant = true)]
		public float experimentTime = 50;
		[KSPField(isPersistant = true)]
		public float completion;
		[KSPField(isPersistant = true)]
		public float submittedData;
		[KSPField(isPersistant = true)]
		public double lastBackgroundCheck;
		[KSPField(isPersistant = true)]
		public uint flightID;
		[KSPField(isPersistant = true)]
		public bool usingEC;

		//Right click menu status fields
		[KSPField(guiActive = true)]
		public string status = "#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_statusInactive"; // "Inactive";
		[KSPField]
		public string calibrationLevel = "0%";
		[KSPField]
		public string dataCollected = "0%";
		[KSPField]
		public string daysRemaining = "";

		private SEP_ExperimentHandler handler;
		private Animation anim;
		private string failMessage = "";
		private ExperimentsResultDialog results;
		private ModuleSEPStation controller;
		private UIPartActionWindow window;
		private bool powerIsProblem;
		private int powerTimer;
		public bool hasContainer;

		public SEP_ExperimentHandler Handler
		{
			get { return handler; }
		}

		public ModuleSEPStation Controller
		{
			get { return controller; }
		}

		private List<string> requiredPartList = new List<string>();
		private List<string> requiredModuleList = new List<string>();

		public override void OnAwake()
		{
			GameEvents.onGamePause.Add(onPause);
			GameEvents.onGameUnpause.Add(onUnPause);
			GameEvents.onVesselStandardModification.Add(onVesselModified);
		}

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);

			usingEC = resHandler.inputResources.Count > 0;

			flightID = part.flightID;

			if (complexity > 4)
				complexity = 4;
			else if (complexity < 1)
				complexity = 1;

			if (state == StartState.Editor)
				return;

			if (animated && !string.IsNullOrEmpty(animationName))
				anim = part.FindModelAnimators(animationName)[0];

			if (IsDeployed && animated)
				animator(anim, animationName, 1, 1);

			setupEvents();

			requiredPartList = SEP_Utilities.parsePartStringList(requiredParts);
			requiredModuleList = SEP_Utilities.parseModuleStringList(requiredModules);

			//GameEvents.onVesselSituationChange.Add(sitChange);
			SEP_Utilities.onWindowSpawn.Add(onWindowSpawn);
			SEP_Utilities.onWindowDestroy.Add(onWindowDestroy);
		}

		private void setupEvents()
		{
			Events["DeployExperiment"].guiName = experimentActionName;
			Events["DeployExperiment"].unfocusedRange = interactionRange;			
			Events["PauseExperiment"].guiName = stopExperimentName;
			Events["PauseExperiment"].unfocusedRange = interactionRange;
			Events["ReCalibrate"].active = IsDeployed;
			Events["ReCalibrate"].guiName = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_recalibrate");
			Events["ReCalibrate"].unfocusedRange = interactionRange;
			Events["CalibrateEvent"].active = !IsDeployed;
			Events["CalibrateEvent"].guiName = calibrationEventName;
			Events["CalibrateEvent"].unfocusedRange = interactionRange;
			Events["RetractEvent"].active = IsDeployed;
			Events["RetractEvent"].guiName = retractEventName;
			Events["RetractEvent"].unfocusedRange = interactionRange;
			Events["CollectData"].guiName = collectActionName;
			Events["CollectData"].unfocusedRange = interactionRange;
			Events["ReviewDataEvent"].guiName = reviewActionName;
			Events["ReviewDataEvent"].unfocusedRange = interactionRange;

			calibrationLevel = calibration.ToString("P0");
			dataCollected = completion.ToString("P0");

			Fields["status"].guiName = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_status");
			Fields["calibrationLevel"].guiName = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_calibrationLevel");
			Fields["calibrationLevel"].guiActive = IsDeployed;
			Fields["dataCollected"].guiActive = experimentRunning;
			Fields["dataCollected"].guiName = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_dataCollected");
			Fields["daysRemaining"].guiName = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_daysRemaining");
			Fields["daysRemaining"].guiActive = experimentRunning;
		}

		private void OnDestroy()
		{
			GameEvents.onGamePause.Remove(onPause);
			GameEvents.onGameUnpause.Remove(onUnPause);
			GameEvents.onVesselStandardModification.Remove(onVesselModified);
			//GameEvents.onVesselSituationChange.Remove(sitChange);
			SEP_Utilities.onWindowSpawn.Remove(onWindowSpawn);
			SEP_Utilities.onWindowDestroy.Remove(onWindowDestroy);

			if (handler != null)
				handler.OnDestroy();
		}

		private void Update()
		{
			if (!FlightGlobals.ready)
				return;

			if (IsDeployed && controller != null)
			{
				if (vessel != controller.vessel)
					RetractEvent();
			}

			if (UIPartActionController.Instance == null)
				return;

			if (UIPartActionController.Instance.ItemListContains(part, false))
			{
				if (handler == null)
					return;

				if (!powerIsProblem)
					status = statusString();

				if (experimentRunning)
				{
					dataCollected = handler.completion.ToString("P2");
					daysRemaining = getTimeRemaining();
					Fields["daysRemaining"].guiActive = true;
					Fields["dataCollected"].guiActive = true;
				}
				else
				{
					Fields["daysRemaining"].guiActive = false;
					Fields["dataCollected"].guiActive = false;
				}
			}
		}

		private void FixedUpdate()
		{
			if (handler == null)
				return;

			if (powerIsProblem)
			{
				if (powerTimer < 30)
				{
					powerTimer++;
					return;
				}

				//SEPUtilities.log("Re-deploying SEP Experiment after power out", logLevels.error);

				DeployExperiment();
			}

			if (!handler.experimentRunning)
				return;

			if (!resHandler.UpdateModuleResourceInputs(ref status, 1, 0.9, false, true))
			{
				PauseExperiment();
				Events["DeployExperiment"].active = false;
				Events["PauseExperiment"].active = true;
				experimentRunning = true;
				powerIsProblem = true;
				powerTimer = 0;
			}
			else
				powerIsProblem = false;
		}

		public override string GetInfo()
		{
			string s = base.GetInfo();

			s += string.Format("<color={0}>{1}</color>\n", XKCDColors.HexFormat.Cyan, experimentActionName);
			s += Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_infoTransmit", RUIutils.GetYesNoUIString(canTransmit));

			if (canTransmit)
				s += Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_infoTransmission", xmitDataScalar.ToString("P0"));

			if (excludeAtmosphere)
				s += Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_infoAtmosphere", RUIutils.GetYesNoUIString(true));

			s += Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_infoComplexity", complexity);

			s += Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_infoCompletion", getDays(experimentTime).ToString("N0"));

			if (animated && oneShotAnim)
				s += Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_infoOneShot", RUIutils.GetYesNoUIString(true));

			s += resHandler.PrintModuleResources(1);

			return s;
		}

		private float getDays(float time)
		{
			time *= 21600;

			time /= KSPUtil.dateTimeFormatter.Day;

			return time;
		}

		public override void OnLoad(ConfigNode node)
		{
			StartCoroutine(delayedLoad(node));
		}

		private IEnumerator delayedLoad(ConfigNode node)
		{
			//SEPUtilities.log("Delayed SEP Science Experiment Loading...", logLevels.warning);

			int timer = 0;

			while (!FlightGlobals.ready)
				yield return null;

			while (timer < 10)
			{
				timer++;
				yield return null;
			}

            flightID = part.flightID;

            if (SEP_Controller.Instance.VesselLoaded(vessel))
			{
				handler = SEP_Controller.Instance.getHandler(vessel, part.flightID);

				if (handler == null)
					handler = new SEP_ExperimentHandler(this, node);
				else
					handler.host = this;
			}
			else
				handler = new SEP_ExperimentHandler(this, node);

			if (handler == null)
				yield break;

			//SEPUtilities.log("SEP Science Experiment Loading...", logLevels.warning);

			delayedEvents();
		}

		private void delayedEvents()
		{
			int i = handler.GetScienceCount();

			Events["CollectData"].active = i > 0;
			Events["ReviewDataEvent"].active = i > 0;
			Events["DeployExperiment"].active = IsDeployed && !experimentRunning && handler.completion < handler.getMaxCompletion();
			Events["PauseExperiment"].active = IsDeployed && experimentRunning;
			Events["TransferDataEvent"].active = hasContainer && i > 0;
		}

		public override void OnSave(ConfigNode node)
		{
			if (handler == null)
				return;

			handler.OnSave(node);
		}

		private void onPause()
		{
			if (results != null)
				results.gameObject.SetActive(false);
		}

		private void onUnPause()
		{
			if (results != null)
				results.gameObject.SetActive(true);
		}

		private void onVesselModified(Vessel v)
		{
			if (v == vessel && HighLogic.LoadedSceneIsFlight)
				findContainers();
		}

		private void findContainers()
		{
			for (int i = vessel.Parts.Count - 1; i >= 0; i--)
			{
				Part p = vessel.Parts[i];

				if (p == null)
					continue;

				if (p.State == PartStates.DEAD)
					continue;

				ModuleScienceContainer container = p.FindModuleImplementing<ModuleScienceContainer>();

				if (container == null)
					continue;

				hasContainer = container.canBeTransferredToInVessel;

				if (!hasContainer)
					continue;

				break;
			}
		}

		private string statusString()
		{
			if (animated && anim != null && anim[animationName] != null && anim.IsPlaying(animationName))
				return Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_statusMoving");

			if (handler == null)
				return Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_statusError");

			if (handler.GetScienceCount() >= 1)
				return Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_statusDataReady");

			if (!IsDeployed)
				return Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_statusInactive");

			if (!handler.experimentRunning)
				return Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_statusCalibrated");

			return Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_statusCollecting");
		}

		private string getTimeRemaining()
		{
			if (handler == null)
				return Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_statusError");

			float next = getNextCompletion(handler.completion);

			if (calibration <= 0)
				return "∞";

			float calib = calibration;

			if (SEP_Controller.Instance.UsingCommNet)
			{
				if (vessel.Connection != null)
				{
					float signal = (float)vessel.Connection.SignalStrength - 0.5f;

					if (signal < 0)
						signal /= 2;

					float bonus = calib * signal;

					calib += bonus;
				}
			}

			float time = experimentTime / calib;

			time *= 21600;

			time *= next;

			float nowTime = 0;
			
			if (handler.completion > 0)
				nowTime = (handler.completion / next) * time;

			float f = time - nowTime;

			return KSPUtil.PrintTime(f, 2, false);
		}

		private float getNextCompletion(float f)
		{
			if (f < 0.5f)
				return 0.5f;
			if (f < 0.75f)
				return 0.75f;
			return 1f;
		}

		private void onWindowSpawn(UIPartActionWindow win)
		{
			if (win == null)
				return;

			if (win.part.flightID != part.flightID)
				return;

			if (FlightGlobals.ActiveVessel == vessel)
				return;

			//SEPUtilities.log("Spawning UI Window", logLevels.log);

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

			//SEPUtilities.log("Destroying UI Window", logLevels.log);

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

		public void setController(ModuleSEPStation m)
		{
			controller = m;

			if (handler == null)
				return;

			handler.updateController(controller);
		}

		public ModuleSEPStation findController()
		{
			return vessel.FindPartModulesImplementing<ModuleSEPStation>().Where(c => c.connectedExperimentCount < c.maxExperiments).FirstOrDefault();
		}

		public void updateHandler(bool forward)
		{
			if (handler == null)
				return;

			//SEP_Utilities.log("Updating Handler...", logLevels.warning);

			handler.host = this;
			handler.vessel = vessel;
			handler.submittedData = submittedData;
			handler.experimentRunning = experimentRunning;
			flightID = part.flightID;
			handler.flightID = flightID;

			if (forward)
			{
				handler.lastBackgroundCheck = lastBackgroundCheck;
				handler.calibration = calibration;
				handler.completion = completion;
			}
			else
			{
				lastBackgroundCheck = handler.lastBackgroundCheck;
				calibration = handler.calibration;
				completion = handler.completion;
			}
		}

		[KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, active = true)]
		public void CalibrateEvent()
		{
			if (FlightGlobals.ActiveVessel == null)
				return;

			if (!FlightGlobals.ActiveVessel.isEVA)
			{
				ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_EVAWarning"), 5f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}

			if (!canConduct())
			{
				ScreenMessages.PostScreenMessage(failMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}

			ProtoCrewMember crew = FlightGlobals.ActiveVessel.GetVesselCrew().FirstOrDefault();

			if (crew == null)
				return;

			calibration = calculateCalibration(crew, complexity);			

			IsDeployed = true;

			if (animated)
				animator(anim, animationName, animSpeed, 0);

			submittedData = handler.getSubmittedData();			

			updateHandler(true);

			setController(findController());

			powerIsProblem = false;

			if (controller != null)
				controller.addConnectecExperiment(this);

			SEP_Controller.Instance.updateVessel(vessel);

			calibrationLevel = calibration.ToString("P0");
			
			Fields["calibrationLevel"].guiActive = true;
			Events["ReCalibrate"].active = true;
			Events["DeployExperiment"].active = true;
			Events["CalibrateEvent"].active = false;
			Events["RetractEvent"].active = !oneShotAnim;

			SEP_Utilities.onExperimentActivate.Fire(handler.vessel, handler);
		}

        [KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, active = false)]
        public void ReCalibrate()
        {
            if (!IsDeployed)
            {
                Events["ReCalibrate"].active = false;
                return;
            }

            if (FlightGlobals.ActiveVessel == null)
                return;

            if (!FlightGlobals.ActiveVessel.isEVA)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_EVAWarning"), 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            if (!canConduct())
            {
                ScreenMessages.PostScreenMessage(failMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            ProtoCrewMember crew = FlightGlobals.ActiveVessel.GetVesselCrew().FirstOrDefault();

            if (crew == null)
                return;

            float cal = calculateCalibration(crew, complexity);

            if (cal > calibration)
            {
                calibration = cal;

                if (handler != null)
                    handler.calibration = calibration;

                calibrationLevel = calibration.ToString("P0");
            }
        }

		[KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, active = false)]
		public void RetractEvent()
		{
			IsDeployed = false;

			if (animated)
				animator(anim, animationName, -1 * animSpeed, 1);

			if (controller != null)
				controller.removeConnectedExperiment(this);

			powerIsProblem = false;

			setController(null);

			calibration = 0;
			experimentRunning = false;
			submittedData = 0;

			lastBackgroundCheck = 0;
			completion = 0;

			updateHandler(true);

			SEP_Controller.Instance.updateVessel(vessel);

			Fields["calibrationLevel"].guiActive = false;
			Events["ReCalibrate"].active = false;
			Events["DeployExperiment"].active = false;
			Events["PauseExperiment"].active = false;
			Events["CalibrateEvent"].active = true;
			Events["RetractEvent"].active = false;

			SEP_Utilities.onExperimentDeactivate.Fire(handler.vessel, handler);
		}

		private void animator(Animation a, string name, float speed, float time)
		{
			if (!animated)
				return;

			if (a == null)
				return;

			if (a[name] == null)
				return;

			a[name].speed = speed;

			if (!a.IsPlaying(name))
			{
				a[name].normalizedTime = time;
				a.Blend(name);
			}
		}

		private float calculateCalibration(ProtoCrewMember c, int l)
		{
			int level = c.experienceTrait.CrewMemberExperienceLevel();

			int m = 0;

			if (c.HasEffect<ExternalExperimentSkill>())
				m = 1;
			else if (c.HasEffect<AutopilotSkill>())
				m = 0;
			else if (c.HasEffect<RepairSkill>())
				m = -1;
			else
				m = -5;

			level += m;

			float f = 1;

			int i = level - l;

			float mod = i * 0.25f;

			f += mod;

			if (f >= 2f)
				f = 2f;
			else if (f < 0.25f)
				f = 0.25f;

			return f;
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, externalToEVAOnly = true, active = false)]
		public void ReviewDataEvent()
		{
			ReviewData();

			Events["CollectData"].active = false;
			Events["ReviewDataEvent"].active = false;
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, externalToEVAOnly = true, active = false)]
		public void CollectData()
		{
			transferToEVA();

			Events["CollectData"].active = false;
			Events["ReviewDataEvent"].active = false;
			Events["TransferDataEvent"].active = false;
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, externalToEVAOnly = true, active = false)]
		public void DeployExperiment()
		{
			if (!IsDeployed)
				return;

			if (!vessel.Landed)
				return;

			if (controller != null && !controller.IsDeployed)
				controller.DeployEvent();

			if (instantResults)
				gatherScience();
			else
			{
				experimentRunning = true;

				lastBackgroundCheck = Planetarium.GetUniversalTime();

				submittedData = handler.getSubmittedData();

				updateHandler(true);

				SEP_Controller.Instance.updateVessel(vessel);

				Fields["dataCollected"].guiActive = true;
				Fields["daysRemaining"].guiActive = true;
				Events["DeployExperiment"].active = false;
				Events["PauseExperiment"].active = true;
			}
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, externalToEVAOnly = true, active = false)]
		public void PauseExperiment()
		{
			experimentRunning = false;

			powerIsProblem = false;
			powerTimer = 0;

			submittedData = 0;

			updateHandler(false);

			SEP_Controller.Instance.updateVessel(vessel);

			Fields["dataCollected"].guiActive = false;
			Fields["daysRemaining"].guiActive = false;
			Events["DeployExperiment"].active = true;
			Events["PauseExperiment"].active = false;
		}

		[KSPEvent(guiActive = true, guiName = "Transfer Data", active = false)]
		public void TransferDataEvent()
		{
			if (!hasContainer)
			{
				ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_237432", part.partInfo.title, vessel.vesselName), 6, ScreenMessageStyle.UPPER_CENTER);
				return;
			}

			if (PartItemTransfer.Instance != null)
			{
				ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_238816"), 3f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}

			ExperimentTransfer.Create(part, this, new Callback<PartItemTransfer.DismissAction, Part>(transferData));
		}

		private void transferData(PartItemTransfer.DismissAction dismiss, Part p)
		{
			if (dismiss != PartItemTransfer.DismissAction.ItemMoved)
				return;

			if (p == null)
				return;

			if (handler == null)
				return;

			if (handler.GetScienceCount() <= 0)
			{
				ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_238567", part.partInfo.title), 6, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			
			ModuleScienceContainer container = p.FindModuleImplementing<ModuleScienceContainer>();

			if (container == null)
			{
				ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_237432", part.partInfo.title, p.partInfo.title), 6, ScreenMessageStyle.UPPER_CENTER);
				return;
			}

			onTransferData(container);
		}

		private void onTransferData(ModuleScienceContainer target)
		{
			if (target == null)
				return;

			int i = handler.GetScienceCount();

			if (target.StoreData(new List<IScienceDataContainer> { this }, false))
				ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_237418", target.part.partInfo.title), 6, ScreenMessageStyle.UPPER_LEFT);
			else
				ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_237422", target.part.partInfo.title), 6, ScreenMessageStyle.UPPER_LEFT);
		}

		//private void sitChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> FT)
		//{
		//	if (FT.host == null)
		//		return;

		//	if (FT.host != vessel)
		//		return;

		//	if (FT.to == Vessel.Situations.LANDED)
		//		return;

		//	completion = 0;
		//	calibration = 0;

		//	if (handler != null)
		//	{
		//		handler.completion = 0;
		//		handler.calibration = 0;
		//	}

		//	if (!experimentRunning)
		//		return;

		//	PauseExperiment();
		//}

		public void gatherScience(bool silent = false)
		{
			int level = handler.getMaxLevel(instantResults);

			if (level <= 0)
				return;

			ScienceData data = SEP_Utilities.getScienceData(handler, handler.getExperimentLevel(instantResults), level);

			if (data == null)
			{
				SEP_Utilities.log("Null Science Data returned; something went wrong here...", logLevels.warning);
				return;
			}

			GameEvents.OnExperimentDeployed.Fire(data);

			if (handler == null)
				return;

			handler.GetData().Add(data);

			if (silent)
				transferToEVA();
			else
				ReviewData();
		}

		private void transferToEVA()
		{
			if (handler == null)
				return;

			if (!FlightGlobals.ActiveVessel.isEVA)
			{
				handler.clearData();
				Inoperable = false;
				return;
			}

			List<ModuleScienceContainer> EVACont = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceContainer>();

			if (EVACont.Count <= 0)
				return;

			if (handler.GetScienceCount() > 0)
			{
				if (!EVACont.First().StoreData(new List<IScienceDataContainer> { this }, false))
				{
					handler.clearData();
					Inoperable = false;
				}
			}

			if (controller != null)
				controller.setCollectEvent();
		}

		private bool canConduct()
		{
			failMessage = "";

			ExperimentSituations sit = ScienceUtil.GetExperimentSituation(vessel);

			if (handler == null)
			{
				SEP_Utilities.log("SEP Experiment Handler is null; Stopping any experiments...", logLevels.warning);
				failMessage = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_failExperimentError");
				return false;
			}

			if (handler.basicExperiment == null)
			{
				SEP_Utilities.log("SEP Experiment definition is null; stopping experiment\npossible errors or corruption present in the SEP ScienceDefs file", logLevels.warning);
				failMessage = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_failExperimentError");
				return false;
			}
			if (Inoperable)
			{
				failMessage = Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_failFunctional");
				return false;
			}
			else if (handler.basicExperiment.requireAtmosphere && !vessel.mainBody.atmosphere)
			{
				failMessage = includeAtmosphereMessage;
				return false;
			}
			else if (!handler.basicExperiment.IsAvailableWhile(sit, vessel.mainBody))
			{
				if (!string.IsNullOrEmpty(situationFailMessage))
					failMessage = situationFailMessage;
				return false;
			}
			else if (excludeAtmosphere && vessel.mainBody.atmosphere)
			{
				if (!string.IsNullOrEmpty(excludeAtmosphereMessage))
					failMessage = excludeAtmosphereMessage;
				return false;
			}
			else if (!string.IsNullOrEmpty(controllerModule))
			{
				if (!VesselUtilities.VesselHasModuleName(controllerModule, vessel))
				{
					failMessage = controllerModuleMessage;
					return false;
				}
			}

			if (requiredPartList.Count > 0)
			{
				for (int i = 0; i < requiredPartList.Count; i++)
				{
					string partName = requiredPartList[i];

					if (string.IsNullOrEmpty(partName))
						continue;

					if (!VesselUtilities.VesselHasPartName(partName, vessel))
					{
						failMessage = requiredPartsMessage;
						return false;
					}
				}
			}

			if (requiredModuleList.Count > 0)
			{
				for (int i = 0; i < requiredModuleList.Count; i++)
				{
					string moduleName = requiredModuleList[i];

					if (string.IsNullOrEmpty(moduleName))
						continue;

					if (!VesselUtilities.VesselHasModuleName(moduleName, vessel))
					{
						failMessage = requiredModulesMessage;
						return false;
					}
				}
			}

			return true;
		}

		#region Experiment Results Page

		private void onKeepData(ScienceData data)
		{
			results = null;

			transferToEVA();
		}

		private void onTransmitData(ScienceData data)
		{
			results = null;

			if (handler == null)
				return;

			IScienceDataTransmitter bestTransmitter = ScienceUtil.GetBestTransmitter(vessel);
			if (bestTransmitter != null)
			{

				SEP_Utilities.log("Sending data to vessel comms: {0}", logLevels.log, data.title);
				bestTransmitter.TransmitData(new List<ScienceData> { data });
				DumpData(data);
			}
			else if (CommNet.CommNetScenario.CommNetEnabled)
				ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_237738"), 3f, ScreenMessageStyle.UPPER_CENTER);
			else
				ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_237740"), 3f, ScreenMessageStyle.UPPER_CENTER);
		}

		private void onSendToLab(ScienceData data)
		{
			results = null;

			ScienceLabSearch labSearch = new ScienceLabSearch(vessel, data);

			if (labSearch.NextLabForDataFound)
			{
				StartCoroutine(labSearch.NextLabForData.ProcessData(data, null));
				DumpData(data);
			}
			else
				labSearch.PostErrorToScreen();
		}

		private void onDiscardData(ScienceData data)
		{
			results = null;

			if (handler == null)
				return;

			if (handler.GetData().Contains(data))
				handler.removeData(data);
		}

		#endregion

		#region IScienceDataContainer methods

		public void ReviewData()
		{
			if (handler == null)
				return;

			if (handler.GetScienceCount() <= 0)
				return;

			ScienceData data = handler.GetData()[0];

			results = ExperimentsResultDialog.DisplayResult(new ExperimentResultDialogPage(part, data, data.baseTransmitValue, data.transmitBonus, false, transmitWarningText, true, new ScienceLabSearch(vessel, data), new Callback<ScienceData>(onDiscardData), new Callback<ScienceData>(onKeepData), new Callback<ScienceData>(onTransmitData), new Callback<ScienceData>(onSendToLab)));
		}

		public void ReviewDataItem(ScienceData data)
		{
			ReviewData();
		}

		public void ReturnData(ScienceData data)
		{
			if (data == null)
				return;

			if (handler == null)
				return;

			if (handler.hasData(data))
				return;

			handler.addData(data);

			Events["CollectData"].active = true;
			Events["ReviewDataEvent"].active = true;
			Events["TransferDataEvent"].active = hasContainer;

			if (controller != null)
				controller.setCollectEvent();
		}

		public bool IsRerunnable()
		{
			return true;
		}

		public int GetScienceCount()
		{
			if (handler == null)
				return 0;

			return handler.GetScienceCount();
		}

		public ScienceData[] GetData()
		{
			if (handler == null)
				return new ScienceData[0];

			return handler.GetData().ToArray();
		}

		public void DumpData(ScienceData data)
		{
			if (handler == null)
				return;

			if (handler.hasData(data))
				handler.removeData(data);

			Events["CollectData"].active = false;
			Events["ReviewDataEvent"].active = false;
			Events["TransferDataEvent"].active = false;

			if (controller != null)
				controller.setCollectEvent();
		}

		#endregion

	}
}
