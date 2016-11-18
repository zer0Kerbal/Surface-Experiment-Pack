#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEP_Style - Script for marking and processing UI styles

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
	public class SEP_Style : MonoBehaviour
	{
		public enum ElementTypes
		{
			None,
			Window,
			Box,
			Button,
			ToggleButton,
			VertScroll,
			Slider,
			SliderBackground,
		}

		[SerializeField]
		private ElementTypes elementType = ElementTypes.None;

		public ElementTypes ElementType
		{
			get { return elementType; }
		}

		private void setSelectable(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			Selectable select = GetComponent<Selectable>();

			if (select == null)
				return;

			select.image.sprite = normal;
			select.image.type = Image.Type.Sliced;
			select.transition = Selectable.Transition.SpriteSwap;

			SpriteState spriteState = select.spriteState;
			spriteState.highlightedSprite = highlight;
			spriteState.pressedSprite = active;
			spriteState.disabledSprite = inactive;
			select.spriteState = spriteState;
		}

		public void setScrollbar(Sprite background, Sprite thumb)
		{
			Image back = GetComponent<Image>();

			if (back == null)
				return;

			back.sprite = background;

			Scrollbar scroll = GetComponent<Scrollbar>();

			if (scroll == null)
				return;

			if (scroll.targetGraphic == null)
				return;

			Image scrollThumb = scroll.targetGraphic.GetComponent<Image>();

			if (scrollThumb == null)
				return;

			scrollThumb.sprite = thumb;
		}

		public void setImage(Sprite sprite, Image.Type type)
		{
			Image image = GetComponent<Image>();

			if (image == null)
				return;

			image.sprite = sprite;
			image.type = type;
		}

		public void setButton(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			setSelectable(normal, highlight, active, inactive);
		}

		public void setToggle(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			setSelectable(normal, highlight, active, inactive);

			Image onImage = GetComponentsInChildren<Image>(true)[1];

			if (onImage == null)
				return;

			onImage.sprite = active;
			onImage.type = Image.Type.Sliced;
		}

		public void setSlider(Sprite background, Sprite foreground, Color backColor, Color foreColor)
		{
			if (background == null || foreground == null)
				return;

			Slider slider = GetComponent<Slider>();

			if (slider == null)
				return;

			Image back = slider.GetComponentInChildren<Image>();

			if (back == null)
				return;

			back.sprite = background;
			back.color = backColor;
			back.type = Image.Type.Sliced;

			RectTransform fill = slider.fillRect;

			if (fill == null)
				return;

			Image front = fill.GetComponentInChildren<Image>();

			if (front == null)
				return;

			front.sprite = foreground;
			front.color = foreColor;
			front.type = Image.Type.Sliced;
		}

		public void setSlider(Sprite foreground, Color foreColor)
		{
			if (foreground == null)
				return;

			Slider slider = GetComponent<Slider>();

			if (slider == null)
				return;

			RectTransform fill = slider.fillRect;

			if (fill == null)
				return;

			Image front = fill.GetComponentInChildren<Image>();

			if (front == null)
				return;

			front.sprite = foreground;
			front.color = foreColor;
			front.type = Image.Type.Sliced;
		}

	}
}
