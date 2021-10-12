#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

TextHighlighter - Script for handling text color changes on mouse-over

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
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace SEPScience.Unity
{
	public class TextHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
	{
		[SerializeField]
		private Color NormalColor = Color.white;
		[SerializeField]
		private Color HighlightColor = Color.yellow;

		private ScrollRect scroller;
		private TextHandler _attachedText;

		private void Awake()
		{
			_attachedText = GetComponent<TextHandler>();
		}

		public void setScroller(ScrollRect s)
		{
			scroller = s;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (_attachedText == null)
				return;

			_attachedText.OnColorUpdate.Invoke(HighlightColor);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (_attachedText == null)
				return;

			_attachedText.OnColorUpdate.Invoke(NormalColor);
		}

		public void OnScroll(PointerEventData eventData)
		{
			if (scroller == null)
				return;

			scroller.OnScroll(eventData);
		}

	}
}
