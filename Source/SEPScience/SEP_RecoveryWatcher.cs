#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEPRecoveryWatcher - A watcher for checking returned science and updating science values accordingly

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

using System.Collections.Generic;
using UnityEngine;

namespace SEPScience
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class SEP_RecoveryWatcher : MonoBehaviour
	{
		private static bool loaded;

		private void Start()
		{
			if (loaded)
				Destroy(gameObject);

			loaded = true;

			GameEvents.OnScienceRecieved.Add(watcher);

			DontDestroyOnLoad(gameObject);
		}

		private void watcher(float sci, ScienceSubject sub, ProtoVessel v, bool b)
		{
			if (sub == null)
				return;

			if (!sub.id.StartsWith("SEP"))
				return;

			GameScenes scene = HighLogic.LoadedScene;

			List<ScienceSubject> subjects = ResearchAndDevelopment.GetSubjects();

			if (scene == GameScenes.SPACECENTER || scene == GameScenes.TRACKSTATION)
			{
				//SEPUtilities.log("Recovery detected...\nSubject ID - {0}\nScience Recovered - {1:N2}", logLevels.warning, sub.id, sci);

				SEP_Utilities.checkAndUpdateRelatedSubjects(subjects, sub, sci);
			}
			else if (scene == GameScenes.FLIGHT)
			{
				//SEPUtilities.log("Transmission detected...\nSubject ID - {0}\nScience Recovered - {1:N2}", logLevels.warning, sub.id, sci);

				SEP_Utilities.checkAndUpdateRelatedSubjects(subjects, sub, sci);
			}
		}
	}
}
