#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEPUtilities - Utilities class

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
using System.Reflection;
using KSP.Localization;
using UnityEngine;

namespace SEPScience
{
	public static class SEP_Utilities
	{
		public static List<Type> loadedPartModules = new List<Type>();
		public static bool partModulesLoaded = false;

		public static EventData<UIPartActionWindow> onWindowSpawn = new EventData<UIPartActionWindow>("onWindowSpawn");
		public static EventData<UIPartActionWindow> onWindowDestroy = new EventData<UIPartActionWindow>("onWindowDestroy");
		public static EventData<Vessel, SEP_ExperimentHandler> onExperimentActivate = new EventData<Vessel, SEP_ExperimentHandler>("onExperimentActivate");
		public static EventData<Vessel, SEP_ExperimentHandler> onExperimentDeactivate = new EventData<Vessel, SEP_ExperimentHandler>("onExperimentDeactivate");

		public static List<PartModule> AntennaModules = new List<PartModule>();
		public static DictionaryValueList<string, AvailablePart> AntennaParts = new DictionaryValueList<string, AvailablePart>();
		public static bool antennaModulesLoaded = false;
		
		public static Sprite[] CommNetSprites;
		public static bool spritesLoaded = false;

		public static void log(string message, logLevels l, params object[] objs)
		{
			message = string.Format(message, objs);
			string log = string.Format("[SEP_Science] {0}", message);
			switch (l)
			{
				case logLevels.log:
					Debug.Log(log);
					break;
				case logLevels.warning:
					Debug.LogWarning(log);
					break;
				case logLevels.error:
					Debug.LogError(log);
					break;
			}
		}

		public static string LocalizeBodyName(this string input)
		{
			return Localizer.Format("<<1>>", input);
		}

		public static void loadPartModules()
		{
			partModulesLoaded = true;

			try
			{
				loadedPartModules = AssemblyLoader.loadedAssemblies.Where(a => a.types.ContainsKey(typeof(PartModule))).SelectMany(b => b.types[typeof(PartModule)]).ToList();
			}
			catch (Exception e)
			{
				log("Failure Loading Part Module List:\n", logLevels.error, e);
				loadedPartModules = new List<Type>();
			}
		}

		public static void loadAntennaParts()
		{
			antennaModulesLoaded = true;

			for (int i = PartLoader.LoadedPartsList.Count - 1; i >= 0; i--)
			{
				AvailablePart part = PartLoader.LoadedPartsList[i];

				if (part == null)
					continue;

				if (part.partPrefab == null)
					continue;

				for (int j = part.partPrefab.Modules.Count - 1; j >= 0; j--)
				{
					PartModule mod = part.partPrefab.Modules[j];

					if (mod == null)
						continue;

					if (!mod.IsValidContractObjective("Antenna"))
						continue;

                    //log("Logging Antenna Module: {0}", logLevels.log, part.title);
                    
					AntennaModules.Add(mod);

					if (!AntennaParts.Contains(part.name))
						AntennaParts.Add(part.name, part);
				}
			}
		}

		public static void attachWindowPrefab()
		{
			var prefab = UIPartActionController.Instance.windowPrefab;

			if (prefab == null)
			{
				log("Error in assigning Part Action Window prefab listener...", logLevels.warning);
				return;
			}

			prefab.gameObject.AddOrGetComponent<SEP_UIWindow>();
		}

		public static void loadSprites(Sprite ss1, Sprite ss2, Sprite ss3, Sprite ss4, Sprite ss5)
		{
			CommNetSprites = new Sprite[5] { ss1, ss2, ss3, ss4, ss5 };

			spritesLoaded = true;
		}

		public static List<string> parsePartStringList(string source)
		{
			List<string> list = new List<string>();

			if (string.IsNullOrEmpty(source))
				return list;

			string[] s = source.Split(',');

			int l = s.Length;

			for (int i = 0; i < l; i++)
			{
				string p = s[i];

				AvailablePart a = PartLoader.getPartInfoByName(p.Replace('_', '.'));

				if (a == null)
					continue;

				list.Add(p);
			}

			return list;
		}

		public static List<string> parseModuleStringList(string source)
		{
			List<string> list = new List<string>();

			if (string.IsNullOrEmpty(source))
				return list;

			string[] s = source.Split(',');

			int l = s.Length;

			for (int i = 0; i < l; i++)
			{
				string m = s[i];
				
				for (int j = 0; j < SEP_Utilities.loadedPartModules.Count; j++)
				{
					Type t = SEP_Utilities.loadedPartModules[j];

					if (t == null)
						continue;

					if (t.Name == m)
					{
						list.Add(m);
						break;
					}
				}
			}

			return list;
		}

		private static string currentBiome(ScienceExperiment e, Vessel v)
		{
			if (e == null)
				return "";

			if (v == null)
				return "";

			if (!e.BiomeIsRelevantWhile(ExperimentSituations.SrfLanded))
				return "";

			if (string.IsNullOrEmpty(v.landedAt))
				return ScienceUtil.GetExperimentBiome(v.mainBody, v.latitude, v.longitude);

			return Vessel.GetLandedAtString(v.landedAt);
		}

		private static string currentDisplayBiome(ScienceExperiment e, Vessel v)
		{
			if (e == null)
				return "";

			if (v == null)
				return "";

			if (!e.BiomeIsRelevantWhile(ExperimentSituations.SrfLanded))
				return "";

			if (string.IsNullOrEmpty(v.displaylandedAt))
				return Localizer.Format(ScienceUtil.GetExperimentBiomeLocalized(v.mainBody, v.latitude, v.longitude));

			return Localizer.Format(v.displaylandedAt);
		}

		public static ScienceSubject subjectIsValid(SEP_ExperimentHandler handler)
		{
			ScienceSubject subject = null;

			List<ScienceSubject> subjects = ResearchAndDevelopment.GetSubjects();

			if (subjects == null || subjects.Count <= 0)
				return null;

			for (int i = 1; i <= 3; i++)
			{
				ScienceExperiment exp = handler.getExperimentLevel(i);

				if (exp == null)
					continue;

				string biome = currentBiome(exp, handler.vessel);

				string id = string.Format("{0}@{1}{2}{3}", exp.id, handler.vessel.mainBody.name, ExperimentSituations.SrfLanded, biome.Replace(" ", ""));

				if (subjects.Any(s => s.id == id))
				{
					subject = ResearchAndDevelopment.GetSubjectByID(id);
					//log("Subject ID Confirmed: Science Level - {0:N2}", logLevels.warning, subject.science);
				}

				//log("Subject ID Checked: ID {0}", logLevels.warning, id);
			}

			return subject;
		}

		public static ScienceData getScienceData(SEP_ExperimentHandler handler, ScienceExperiment exp, int level)
		{
			ScienceData data = null;

			string biome = currentBiome(exp, handler.vessel);
			string displayBiome = currentDisplayBiome(exp, handler.vessel);

			ScienceSubject sub = ResearchAndDevelopment.GetExperimentSubject(exp, ExperimentSituations.SrfLanded, handler.vessel.mainBody, biome, displayBiome);
			
			sub.science = handler.submittedData * sub.subjectValue;
			sub.scientificValue = 1 - (sub.science / sub.scienceCap);

			data = new ScienceData(exp.baseValue * exp.dataScale, handler.xmitDataScalar, 1, sub.id, sub.title, false, (uint)handler.flightID);

			//log("Science Data Generated: {0} - Data: {1:F2} - Xmit: {2:F2}", logLevels.warning, data.subjectID, data.dataAmount, data.baseTransmitValue);

			return data;
		}

		public static ScienceSubject checkAndUpdateRelatedSubjects(SEP_ExperimentHandler handler, int level, float data, float submitted)
		{
			ScienceSubject subject = null;

			for (int i = 1; i <= level; i++)
			{
				ScienceExperiment exp = handler.getExperimentLevel(i);

				if (exp == null)
					continue;

				string biome = currentBiome(exp, handler.vessel);
				string displayBiome = currentDisplayBiome(exp, handler.vessel);

				subject = ResearchAndDevelopment.GetExperimentSubject(exp, ExperimentSituations.SrfLanded, handler.vessel.mainBody, biome, displayBiome);
				
				if (i == level)
					subject.science = submitted * subject.subjectValue;
				else
					subject.science = data * subject.subjectValue;

				if (subject.science > subject.scienceCap)
					subject.science = subject.scienceCap;

				subject.scientificValue = 1 - (subject.science / subject.scienceCap);

				//log("Related Subject Checked: ID {0} - Science Level {1:N0}", logLevels.warning, subject.id, subject.science);
			}

			return subject;
		}

		//public static void checkAndUpdateRelatedSubjects(ScienceSubject sub, float data)
		//{
		//	string s = sub.id.Substring(sub.id.Length - 1, 1);

		//	int level = 0;

		//	if (!int.TryParse(s, out level))
		//		return;

		//	if (!sub.IsFromSituation(ExperimentSituations.SrfLanded))
		//		return;

		//	string biome = getBiomeString(sub.id);

		//	string exp = getExperimentString(sub.id, level);

		//	if (string.IsNullOrEmpty(exp))
		//		return;

		//	string body = getBodyString(sub.id);

		//	if (string.IsNullOrEmpty(body))
		//		return;

		//	CelestialBody Body= FlightGlobals.Bodies.FirstOrDefault(b => b.bodyName == body);

		//	if (Body == null)
		//		return;

		//	for (int i = 1; i < level; i++)
		//	{
		//		string fullExp = exp + ((SEPExperiments)i).ToString();

		//		ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment(fullExp);

		//		if (experiment == null)
		//			continue;

		//		ScienceSubject subject = ResearchAndDevelopment.GetExperimentSubject(experiment, ExperimentSituations.SrfLanded, Body, biome);
		//		subject.title = experiment.experimentTitle + situationCleanup(Body, ExperimentSituations.SrfLanded, biome);

		//		subject.science += (data / HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier);

		//		if (subject.science > subject.scienceCap)
		//			subject.science = subject.scienceCap;

		//		subject.scientificValue = 1 - (subject.science / subject.scienceCap);

		//		//log("Related Subject Checked From Event: ID {0} - Science Level {1:N0}", logLevels.warning, subject.id, subject.science);
		//	}
		//}

		public static void checkAndUpdateRelatedSubjects(List<ScienceSubject> allSubs, ScienceSubject sub, float data)
		{
			data /= HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

			if (data <= 0)
				return;

			if (!sub.IsFromSituation(ExperimentSituations.SrfLanded))
				return;

			string biome = getBiomeString(sub.id);

			string exp = getExperimentFullString(sub.id);

			if (string.IsNullOrEmpty(exp))
				return;

			int level = getExperimentLevel(exp);

			if (level == 0)
				return;

			exp = getExperimentStartString(exp, level);

			if (string.IsNullOrEmpty(exp))
				return;

			string body = getBodyString(sub.id);

			if (string.IsNullOrEmpty(body))
				return;

			CelestialBody Body = FlightGlobals.Bodies.FirstOrDefault(b => b.bodyName == body);

			if (Body == null)
				return;

			ScienceSubject subject = null;

			//log("Science Subject Parsed [{0}]: Level: {1} - Experiment: {2}", logLevels.warning, sub.id, level, exp);

			for (int i = 1; i <= 3; i++)
			{
				if (i == level)
					continue;

				string fullExp = exp + ((SEPExperiments)i).ToString();

				ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment(fullExp);

				if (experiment == null)
					continue;

				string id = string.Format("{0}@{1}{2}{3}", experiment.id, body, ExperimentSituations.SrfLanded, biome);

				if (allSubs.Any(a => a.id == id))
				{
					subject = ResearchAndDevelopment.GetSubjectByID(id);
					//log("Subject ID Confirmed: Science Amount - {0:N2}", logLevels.warning, subject.science);
				}
				else
					continue;

				if (i < level)
				{
					subject.science = sub.science;

					if (subject.science > subject.scienceCap)
						subject.science = subject.scienceCap;

					subject.scientificValue = 1 - (subject.science / subject.scienceCap);
				}
				else
				{
					if (subject.science < sub.science)
					{
						subject.science = sub.science;

						if (subject.science > subject.scienceCap)
							subject.science = subject.scienceCap;

						subject.scientificValue = 1 - (subject.science / subject.scienceCap);
					}
				}

				//log("Related Subject Checked From Recovery: ID {0} - Science Level {1:N0}", logLevels.warning, subject.id, subject.science);
			}
		}

		private static string getBiomeString(string sub)
		{
			string sit = ExperimentSituations.SrfLanded.ToString();

			int lastIndex = sub.IndexOf(sit) + sit.Length;

			string b = sub.Substring(lastIndex);

			b = b.Substring(0, b.Length);

			return b;
		}

		public static int getExperimentLevel(string exp)
		{
			int UnderIndex = exp.LastIndexOf('_');

			if (UnderIndex == -1)
				return 0;

			string levelString = exp.Substring(UnderIndex, exp.Length - UnderIndex);

			switch (levelString)
			{
				case "_Basic":
					return 1;
				case "_Detailed":
					return 2;
				case "_Exhaustive":
					return 3;
				default:
					return 0;
			}
		}

		public static string getExperimentFullString(string sub)
		{
			int AtIndex = sub.IndexOf('@');

			if (AtIndex == -1)
				return "";

			return sub.Substring(0, AtIndex);
		}

		private static string getExperimentStartString(string exp, int l)
		{
			int i = exp.IndexOf(((SEPExperiments)l).ToString());

			if (i == -1)
				return "";

			exp = exp.Substring(0, i);

			return exp;
		}

		private static string getBodyString(string sub)
		{
			int AtIndex = sub.IndexOf('@');

			int SitIndex = sub.IndexOf("Srf");

			if (AtIndex == -1 || SitIndex == -1)
				return "";

			if (AtIndex >= SitIndex)
				return "";

			return sub.Substring(AtIndex + 1, SitIndex - AtIndex - 1);
		}

		public static float getTotalVesselEC(Vessel v)
		{
			double ec = 0;

			for (int i = v.Parts.Count - 1; i >= 0; i--)
			{
				Part p = v.Parts[i];

				if (p == null)
					continue;

				for (int j = p.Resources.Count - 1; j >= 0; j--)
				{
					PartResource r = p.Resources[j];

					if (r == null)
						continue;

					if (r.resourceName != "ElectricCharge")
						continue;

					ec += r.amount;
				}
			}

			return (float)ec;
		}

		public static float getTotalVesselEC(ProtoVessel v)
		{
			double ec = 0;

			int l = v.protoPartSnapshots.Count;

			for (int i = 0; i < l; i++)
			{
				ProtoPartSnapshot part = v.protoPartSnapshots[i];

				if (part == null)
					continue;

				int r = part.resources.Count;

				for (int j = 0; j < r; j++)
				{
					ProtoPartResourceSnapshot resource = part.resources[j];

					if (resource == null)
						continue;

					if (resource.resourceName != "ElectricCharge")
						continue;

					double amount = resource.amount;

					ec += amount;
				}
			}

			//log("Vessel EC: {0:N4}", logLevels.warning, ec);

			return (float)ec;
		}

		public static float getMaxTotalVesselEC(ProtoVessel v)
		{
			double ec = 0;

			int l = v.protoPartSnapshots.Count;

			for (int i = 0; i < l; i++)
			{
				ProtoPartSnapshot part = v.protoPartSnapshots[i];

				if (part == null)
					continue;

				int r = part.resources.Count;

				for (int j = 0; j < r; j++)
				{
					ProtoPartResourceSnapshot resource = part.resources[j];

					if (resource == null)
						continue;

					if (resource.resourceName != "ElectricCharge")
						continue;

					double amount = resource.maxAmount;

					ec += amount;
				}
			}

			//log("Vessel EC: {0:N4}", logLevels.warning, ec);

			return (float)ec;
		}
	}

	public enum logLevels
	{
		log = 1,
		warning = 2,
		error = 3,
	}

	public enum SEPComplexity
	{
		Simple = 1,
		Moderate = 2,
		Complex = 3,
		Fiendish = 4,
	}

	public enum SEPExperiments
	{
		_Basic = 1,
		_Detailed = 2,
		_Exhaustive = 3,
	}
}
