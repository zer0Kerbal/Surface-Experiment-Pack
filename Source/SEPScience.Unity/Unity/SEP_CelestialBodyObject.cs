#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEP_CelestialBodyObject - Unity UI element for celestial body selection button

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

using UnityEngine;
using UnityEngine.UI;

namespace SEPScience.Unity.Unity
{
	public class SEP_CelestialBodyObject : MonoBehaviour
	{
		[SerializeField]
		private TextHandler BodyTitle = null;
		[SerializeField]
		private Image SelectedImage = null;

		private string body;

		public string Body
		{
			get { return body; }
		}

		public void setBody(string b, int count)
		{
			body = b;

			if (BodyTitle != null)
				BodyTitle.OnTextUpdate.Invoke(string.Format("{0}: {1} Station{2}", b, count, count > 1 ? "s" : ""));

			if (SEP_Window.Window == null)
				return;

			if (SelectedImage == null)
				return;

			if (SEP_Window.Window.CurrentBody == body)
				SelectedImage.gameObject.SetActive(true);
			else
				SelectedImage.gameObject.SetActive(false);
		}

		public void UpdateCount(int count)
		{
			if (BodyTitle != null)
				BodyTitle.OnTextUpdate.Invoke(string.Format("{0}: {1} Station{2}", body, count, count > 1 ? "s" : ""));
		}

		public void DisableBody()
		{
			if (SelectedImage != null)
				SelectedImage.gameObject.SetActive(false);
		}

		public void SetBody()
		{
			if (SEP_Window.Window == null)
				return;

			if (SEP_Window.Window.CurrentBody == body)
				return;

			SEP_Window.Window.SetCurrentBody(body);

			if (SelectedImage != null)
				SelectedImage.gameObject.SetActive(true);
		}
	}
}
