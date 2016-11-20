#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEPExperimentHandler - Class to handle all SEP experiment variables

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

namespace SEPScience
{
	public class SEP_ExperimentHandler
	{
		public bool instantResults;
		public bool canTransmit;
		public bool experimentRunning;
		public bool controllerAutoTransmit;
		public bool usingECResource;
		public float xmitDataScalar;
		public float calibration;
		public float experimentTime;
		public float completion;
		public float submittedData;
		public double lastBackgroundCheck;
		public int flightID;
		public ModuleSEPScienceExperiment host;
		public ProtoPartModuleSnapshot protoHost;
		public Vessel vessel;
		public string experimentTitle;

		public bool loaded;

		public ScienceExperiment basicExperiment;
		public ScienceExperiment detailedExperiment;
		public ScienceExperiment exhaustiveExperiment;
		public string experimentID;

		private List<ScienceData> storedData = new List<ScienceData>();

		public SEP_ExperimentHandler(ModuleSEPScienceExperiment mod, ConfigNode node)
		{
			//SEPUtilities.log("Creating SEP Handler from module...", logLevels.log);

			experimentID = mod.experimentID;
			instantResults = mod.instantResults;
			canTransmit = mod.canTransmit;
			experimentRunning = mod.experimentRunning;
			xmitDataScalar = mod.xmitDataScalar;
			calibration = mod.calibration;
			experimentTime = mod.experimentTime;
			completion = mod.completion;
			submittedData = mod.submittedData;
			lastBackgroundCheck = mod.lastBackgroundCheck;
			controllerAutoTransmit = mod.controllerAutoTransmit;
			usingECResource = mod.usingEC;
			flightID = mod.flightID;
			host = mod;
			vessel = host.vessel;

			if (!loadExperiments(mod.experimentID))
				return;

			OnLoad(node);

			GameEvents.onProtoPartModuleSnapshotSave.Add(onProtoSave);

			loaded = true;
		}

		public SEP_ExperimentHandler(ProtoPartModuleSnapshot snap, Vessel v)
		{
			//SEPUtilities.log("Creating SEP Handler from config node...", logLevels.log);

			ConfigNode node = snap.moduleValues;

			if (node.HasValue("name") && node.GetValue("name") != "ModuleSEPScienceExperiment")
				return;

			node.TryGetValue("experimentID", ref experimentID);

			if (string.IsNullOrEmpty(experimentID))
				return;

			if (!loadExperiments(experimentID))
				return;

			node.TryGetValue("instantResults", ref instantResults);
			node.TryGetValue("canTransmit", ref canTransmit);
			node.TryGetValue("experimentRunning", ref experimentRunning);
			node.TryGetValue("xmitDataScalar", ref xmitDataScalar);
			node.TryGetValue("calibration", ref calibration);
			node.TryGetValue("experimentTime", ref experimentTime);
			node.TryGetValue("completion", ref completion);
			node.TryGetValue("submittedData", ref submittedData);
			node.TryGetValue("lastBackgroundCheck", ref lastBackgroundCheck);
			node.TryGetValue("usingEC", ref usingECResource);
			node.TryGetValue("controllerAutoTransmit", ref controllerAutoTransmit);
			node.TryGetValue("flightID", ref flightID);

			protoHost = snap;
			vessel = v;

			OnLoad(node);

			GameEvents.onProtoPartModuleSnapshotSave.Add(onProtoSave);

			loaded = true;
		}

		private bool loadExperiments(string id)
		{
			basicExperiment = ResearchAndDevelopment.GetExperiment(experimentID + SEPExperiments._Basic.ToString());
			detailedExperiment = ResearchAndDevelopment.GetExperiment(experimentID + SEPExperiments._Detailed.ToString());
			exhaustiveExperiment = ResearchAndDevelopment.GetExperiment(experimentID + SEPExperiments._Exhaustive.ToString());

			if (basicExperiment == null || detailedExperiment == null || exhaustiveExperiment == null)
				return false;

			experimentTitle = detailedExperiment.experimentTitle;

			return true;
		}

		public void onProtoSave(GameEvents.FromToAction<ProtoPartModuleSnapshot, ConfigNode> FTA)
		{
			if (FTA.from.moduleName != "ModuleSEPScienceExperiment")
				return;

			ConfigNode node = FTA.to;

			//This is the case of a loaded vessel moving out of physics range
			if (node == null)
			{
				protoHost = FTA.from;

				int id = 0;

				protoHost.moduleValues.TryGetValue("flightID", ref id);

				if (id != flightID)
					return;

				//SEPUtilities.log("Saving Handler from loaded vessel", logLevels.error);

				protoHost.moduleValues.SetValue("experimentRunning", experimentRunning.ToString());
				protoHost.moduleValues.SetValue("calibration", calibration.ToString());
				protoHost.moduleValues.SetValue("completion", completion.ToString());
				protoHost.moduleValues.SetValue("submittedData", submittedData.ToString());
				protoHost.moduleValues.SetValue("lastBackgroundCheck", lastBackgroundCheck.ToString());
				protoHost.moduleValues.SetValue("controllerAutoTransmit", controllerAutoTransmit.ToString());
				OnSave(protoHost.moduleValues);
			}
			//This is the standard method used when saving
			else
			{
				int id = 0;

				node.TryGetValue("flightID", ref id);

				if (id != flightID)
					return;

				//SEPUtilities.log("Saving Handler from proto vessel", logLevels.error);

				node.SetValue("experimentRunning", experimentRunning.ToString());
				node.SetValue("calibration", calibration.ToString());
				node.SetValue("completion", completion.ToString());
				node.SetValue("submittedData", submittedData.ToString());
				node.SetValue("lastBackgroundCheck", lastBackgroundCheck.ToString());
				node.SetValue("controllerAutoTransmit", controllerAutoTransmit.ToString());
				OnSave(node);
			}
		}

		public void OnDestroy()
		{
			GameEvents.onProtoPartModuleSnapshotSave.Remove(onProtoSave);
		}

		public void OnSave(ConfigNode node)
		{
			node.RemoveNodes("ScienceData");
			foreach (ScienceData data in storedData)
			{
				ConfigNode storedDataNode = node.AddNode("ScienceData");
				data.Save(storedDataNode);
			}
		}

		public void OnLoad(ConfigNode node)
		{
			if (node.HasNode("ScienceData"))
			{
				ConfigNode[] dataNodes = node.GetNodes("ScienceData");

				int l = dataNodes.Length;

				for (int i = 0; i < l; i++)
				{
					ConfigNode data = dataNodes[i];
					storedData.Add(new ScienceData(data));
				}
			}
		}

		public void updateController(ModuleSEPStation station)
		{
			if (station == null)
				controllerAutoTransmit = false;
			else
				controllerAutoTransmit = station.autoTransmit;
		}

		public float getSubmittedData()
		{
			ScienceSubject sub = SEP_Utilities.subjectIsValid(this);

			if (sub == null)
				return 0;

			return sub.science / sub.subjectValue;
		}

		public float currentMaxScience(bool instant)
		{
			int i = getMaxLevel(instant);

			if (i <= 0)
				return 0;

			return getExperimentLevel(i).baseValue;
		}

		public float currentMaxScience(int level)
		{
			if (level <= 0)
				return 0;

			return getExperimentLevel(level).baseValue;
		}

		public int getMaxLevel(bool instant)
		{
			if (instant)
			{
				if (calibration <= 0.5f)
					return 1;
				if (calibration <= 0.75f)
					return 2;
				return 3;
			}
			else
			{
				if (completion < 0.5f)
					return 0;
				if (completion < 0.75f)
					return 1;
				if (completion < 1f)
					return 2;
				return 3;
			}
		}

		public float getMaxCompletion()
		{
			if (calibration <= 0.5f)
				return 0.5f;
			if (calibration <= 0.75f)
				return 0.75f;
			return 1;
		}

		public ScienceExperiment getExperimentLevel(bool instant)
		{
			if (instant)
			{
				if (calibration <= 0.5f)
					return basicExperiment;
				if (calibration <= 0.75f)
					return detailedExperiment;
				return exhaustiveExperiment;
			}
			else
			{
				if (completion <= 0.5f)
					return basicExperiment;
				if (completion <= 0.75f)
					return detailedExperiment;
				return exhaustiveExperiment;
			}
		}

		public ScienceExperiment getExperimentLevel(int level)
		{
			switch (level)
			{
				case 2:
					return detailedExperiment;
				case 3:
					return exhaustiveExperiment;					
			}

			return basicExperiment;
		}

		public void addData(ScienceData data)
		{
			storedData.Clear();

			storedData.Add(data);
		}

		public void removeData(ScienceData data)
		{
			if (storedData.Any(d => d.subjectID == data.subjectID))
				storedData.Remove(data);
		}

		public void clearData()
		{
			storedData.Clear();
		}

		public int GetScienceCount()
		{
			return storedData.Count;
		}

		public List<ScienceData> GetData()
		{
			return storedData;
		}
	}
}
