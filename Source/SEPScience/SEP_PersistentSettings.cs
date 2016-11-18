#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEP_PersistentSettings - A persistent storage module for saving settings to disk

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
using System.Reflection;
using System.IO;
using UnityEngine;

namespace SEPScience
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class SEP_PersistentSettings : MonoBehaviour
	{
		[Persistent]
		public bool showAllVessels;
		[Persistent]
		public bool fadeout = true;
		[Persistent]
		public bool stockToolbar = true;
		[Persistent]
		public bool hoverOpen = true;
		[Persistent]
		public float scale = 1;

		private const string fileName = "PluginData/Settings.cfg";
		private string fullPath;
		private SEP_GameParameters settings;

		private static SEP_PersistentSettings instance;

		public static SEP_PersistentSettings Instance
		{
			get { return instance; }
		}

		private void Awake()
		{
			if (instance != null)
				Destroy(gameObject);

			DontDestroyOnLoad(gameObject);
			
			instance = this;

			fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName).Replace("\\", "/");
			GameEvents.OnGameSettingsApplied.Add(SettingsApplied);

			if (Load())
				SEP_Utilities.log("[SEP Science] Settings file loaded", logLevels.log);
		}

		public void SettingsApplied()
		{
			if (HighLogic.CurrentGame != null)
				settings = HighLogic.CurrentGame.Parameters.CustomParams<SEP_GameParameters>();

			if (settings == null)
				return;

			if (settings.useAsDefault)
			{
				showAllVessels = settings.showAllVessels;
				fadeout = settings.fadeOut;
				stockToolbar = settings.stockToolbar;
				hoverOpen = settings.hoverOpen;
				scale = settings.scale;

				if (Save())
					SEP_Utilities.log("[SEP Science] Settings file saved", logLevels.log);
			}
		}

		public bool Load()
		{
			bool b = false;

			try
			{
				if (File.Exists(fullPath))
				{
					ConfigNode node = ConfigNode.Load(fullPath);
					ConfigNode unwrapped = node.GetNode(GetType().Name);
					ConfigNode.LoadObjectFromConfig(this, unwrapped);
					b = true;
				}
				else
				{
					SEP_Utilities.log("[SEP Science] Settings file could not be found [{0}]", logLevels.warning, fullPath);
					b = false;
				}
			}
			catch (Exception e)
			{
				SEP_Utilities.log("[SEP Science] Error while loading settings file from [{0}]\n{1}", logLevels.error, fullPath, e);
				b = false;
			}

			return b;
		}

		public bool Save()
		{
			bool b = false;

			try
			{
				ConfigNode node = AsConfigNode();
				ConfigNode wrapper = new ConfigNode(GetType().Name);
				wrapper.AddNode(node);
				wrapper.Save(fullPath);
				b = true;
			}
			catch (Exception e)
			{
					SEP_Utilities.log("[SEP Science] Error while saving settings file from [{0}]\n{1}", logLevels.error, fullPath, e);
				b = false;
			}

			return b;
		}

		private ConfigNode AsConfigNode()
		{
			try
			{
				ConfigNode node = new ConfigNode(GetType().Name);

				node = ConfigNode.CreateConfigFromObject(this, node);
				return node;
			}
			catch (Exception e)
			{
					SEP_Utilities.log("[SEP Science] Failed to generate settings file node...\n{0}", logLevels.error, e);
				return new ConfigNode(GetType().Name);
			}
		}
	}
}
