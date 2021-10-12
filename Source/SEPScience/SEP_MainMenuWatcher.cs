#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEP_MainMenuWatcher - Monobehaviour to attach settings file persistence logic to new game creation

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
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace SEPScience
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class SEP_MainMenuWatcher : MonoBehaviour
	{
		private MainMenu menu;

		private void Start()
		{
			menu = GameObject.FindObjectOfType<MainMenu>();

			if (menu == null)
				return;

			menu.newGameBtn.onTap = (Callback)Delegate.Combine(menu.newGameBtn.onTap, new Callback(onTap));
		}

		private void onTap()
		{
			StartCoroutine(AddListener());
		}

		private IEnumerator AddListener()
		{
			yield return new WaitForSeconds(0.5f);

			var buttons = GameObject.FindObjectsOfType<Button>();

			if (buttons.Length > 0)
			{
				var button = buttons[buttons.Length - 1];

				button.onClick.AddListener(new UnityAction(onSettingsApply));
			}
		}

		private void onSettingsApply()
		{
			StartCoroutine(ApplySettings());
		}

		private IEnumerator ApplySettings()
		{
			while (HighLogic.CurrentGame == null)
				yield return null;

			if (SEP_PersistentSettings.Instance == null)
				yield break;

			SEP_PersistentSettings.Instance.SettingsApplied();
		}
	}
}
