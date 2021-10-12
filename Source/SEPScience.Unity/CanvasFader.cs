#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

CanvasFader - Monobehaviour for making smooth fade in and fade out for UI windows

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
using UnityEngine.UI;

namespace SEPScience.Unity
{
	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasFader : MonoBehaviour
	{
		private CanvasGroup canvas;
		private IEnumerator fader;
		private bool allowInterrupt = true;

		protected virtual void Awake()
		{
			canvas = GetComponent<CanvasGroup>();
		}

		public bool Fading
		{
			get { return fader != null; }
		}

		protected void Fade(float to, float duration, Action call = null, bool interrupt = true, bool overrule = false)
		{
			if (canvas == null)
				return;

			Fade(canvas.alpha, to, duration, call, interrupt, overrule);
		}

		protected void Alpha(float to)
		{
			if (canvas == null)
				return;

			to = Mathf.Clamp01(to);
			canvas.alpha = to;
		}

		private void Fade(float from, float to, float duration, Action call, bool interrupt, bool overrule)
		{
			if (!allowInterrupt && !overrule)
				return;

			if (fader != null)
				StopCoroutine(fader);

			fader = FadeRoutine(from, to, duration, call, interrupt);
			StartCoroutine(fader);
		}

		private IEnumerator FadeRoutine(float from, float to, float duration, Action call, bool interrupt)
		{
			allowInterrupt = interrupt;

			yield return new WaitForEndOfFrame();

			float f = 0;

			while (f <= 1)
			{
				f += Time.deltaTime / duration;
				Alpha(Mathf.Lerp(from, to, f));
				yield return null;
			}

			if (call != null)
				call.Invoke();

			allowInterrupt = true;

			fader = null;
		}

	}
}
