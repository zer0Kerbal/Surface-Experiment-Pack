#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEP_UI_Loader - KSPAddon for loading and processing asset bundles

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

using System.IO;
using System.Reflection;
using SEPScience.SEP_UI;
using SEPScience.Unity;
using SEPScience.Unity.Unity;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI.Screens.Flight.Dialogs;
using TMPro;

namespace SEPScience.SEP_UI
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class SEP_UI_Loader : MonoBehaviour
	{
		private static bool loaded;
		private static bool TMPLoaded;
		private static bool UILoaded;

		private static Sprite sliderFrontForeground;
		private static Sprite sliderBackBackground;
		private static Sprite sliderBackForeground;
		private static Color sliderFrontForeColor;
		private static Color sliderBackBackColor;
		private static Color sliderBackForeColor;

		private static GameObject[] loadedPrefabs;

		private static GameObject windowPrefab;
		private static GameObject compactPrefab;

		public static GameObject WindowPrefab
		{
			get { return windowPrefab; }
		}

		public static GameObject CompactPrefab
		{
			get { return compactPrefab; }
		}

		private void Awake()
		{
			if (loaded)
				Destroy(gameObject);

			if (loadedPrefabs == null)
			{
				string path = KSPUtil.ApplicationRootPath + "GameData/SurfaceExperimentPackage/Resources";

				AssetBundle prefabs = AssetBundle.LoadFromFile(path + "/sep_prefab");

				if (prefabs != null)
					loadedPrefabs = prefabs.LoadAllAssets<GameObject>();
			}

			if (loadedPrefabs != null)
			{
				if (!TMPLoaded)
					processTMPPrefabs();

				if (UISkinManager.defaultSkin != null && !UILoaded)
				{
					if (sliderFrontForeground == null || sliderBackBackground == null || sliderBackForeground == null)
					{
						ExperimentsResultDialog scienceDialogPrefab = UnityEngine.Object.Instantiate<GameObject>(AssetBase.GetPrefab("ScienceResultsDialog")).GetComponent<ExperimentsResultDialog>();

						if (scienceDialogPrefab != null)
						{
							Slider[] sliders = scienceDialogPrefab.GetComponentsInChildren<Slider>(); ;

							Slider backSlider = sliders[0];
							Slider frontSlider = sliders[1];

							sliderBackBackground = processSliderSprites(backSlider, true, ref sliderBackBackColor);
							sliderBackForeground = processSliderSprites(backSlider, false, ref sliderBackForeColor);

							sliderFrontForeground = processSliderSprites(frontSlider, false, ref sliderFrontForeColor);
						}
					}

					processUIPrefabs();
				}
			}

			if (TMPLoaded && UILoaded)
				loaded = true;

			Destroy(gameObject);
		}


		private void processTMPPrefabs()
		{
			for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
			{
				GameObject o = loadedPrefabs[i];

				if (o.name == "SEP_Window")
					windowPrefab = o;
				else if (o.name == "SEP_Compact")
					compactPrefab = o;

				if (o != null)
					processTMP(o);
			}

			TMPLoaded = true;
		}

		private void processTMP(GameObject obj)
		{
			TextHandler[] handlers = obj.GetComponentsInChildren<TextHandler>(true);

			if (handlers == null)
				return;

			for (int i = 0; i < handlers.Length; i++)
				TMProFromText(handlers[i]);
		}

		private void TMProFromText(TextHandler handler)
		{
			if (handler == null)
				return;

			Text text = handler.GetComponent<Text>();

			if (text == null)
				return;

			string t = text.text;
			Color c = text.color;
			int i = text.fontSize;
			bool r = text.raycastTarget;
			FontStyles sty = getStyle(text.fontStyle);
			TextAlignmentOptions align = getAnchor(text.alignment);
			float spacing = text.lineSpacing;
			GameObject obj = text.gameObject;

			MonoBehaviour.DestroyImmediate(text);

			SEP_TextMeshProHolder tmp = obj.AddComponent<SEP_TextMeshProHolder>();

			tmp.text = t;
			tmp.color = c;
			tmp.fontSize = i;
			tmp.raycastTarget = r;
			tmp.alignment = align;
			tmp.fontStyle = sty;
			tmp.lineSpacing = spacing;

			tmp.font = Resources.Load("Fonts/Calibri SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;
			tmp.fontSharedMaterial = Resources.Load("Fonts/Materials/Calibri Dropshadow", typeof(Material)) as Material;

			tmp.enableWordWrapping = true;
			tmp.isOverlay = false;
			tmp.richText = true;
		}

		private FontStyles getStyle(FontStyle style)
		{
			switch (style)
			{
				case FontStyle.Normal:
					return FontStyles.Normal;
				case FontStyle.Bold:
					return FontStyles.Bold;
				case FontStyle.Italic:
					return FontStyles.Italic;
				case FontStyle.BoldAndItalic:
					return FontStyles.Bold;
				default:
					return FontStyles.Normal;
			}
		}

		private TextAlignmentOptions getAnchor(TextAnchor anchor)
		{
			switch (anchor)
			{
				case TextAnchor.UpperLeft:
					return TextAlignmentOptions.TopLeft;
				case TextAnchor.UpperCenter:
					return TextAlignmentOptions.Top;
				case TextAnchor.UpperRight:
					return TextAlignmentOptions.TopRight;
				case TextAnchor.MiddleLeft:
					return TextAlignmentOptions.MidlineLeft;
				case TextAnchor.MiddleCenter:
					return TextAlignmentOptions.Midline;
				case TextAnchor.MiddleRight:
					return TextAlignmentOptions.MidlineRight;
				case TextAnchor.LowerLeft:
					return TextAlignmentOptions.BottomLeft;
				case TextAnchor.LowerCenter:
					return TextAlignmentOptions.Bottom;
				case TextAnchor.LowerRight:
					return TextAlignmentOptions.BottomRight;
				default:
					return TextAlignmentOptions.Center;
			}
		}

		private static Sprite processSliderSprites(Slider slider, bool back, ref Color color)
		{
			if (slider == null)
				return null;

			if (back)
			{
				Image background = slider.GetComponentInChildren<Image>();

				if (background == null)
					return null;

				color = background.color;

				return background.sprite;
			}
			else
			{
				RectTransform fill = slider.fillRect;

				if (fill == null)
					return null;

				Image fillImage = fill.GetComponent<Image>();

				if (fillImage == null)
					return null;

				color = fillImage.color;

				return fillImage.sprite;
			}
		}

		private void processUIPrefabs()
		{
			for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
			{
				GameObject o = loadedPrefabs[i];

				if (o != null)
					processUIComponents(o);
			}

			UILoaded = true;
		}

		private void processUIComponents(GameObject obj)
		{
			SEP_Style[] styles = obj.GetComponentsInChildren<SEP_Style>(true);

			if (styles == null)
				return;

			for (int i = 0; i < styles.Length; i++)
				processComponents(styles[i]);
		}

		private void processComponents(SEP_Style style)
		{
			if (style == null)
				return;

			UISkinDef skin = UISkinManager.defaultSkin;

			if (skin == null)
				return;

			switch (style.ElementType)
			{
				case SEP_Style.ElementTypes.Window:
					style.setImage(skin.window.normal.background, Image.Type.Sliced);
					break;
				case SEP_Style.ElementTypes.Box:
					style.setImage(skin.box.normal.background, Image.Type.Sliced);
					break;
				case SEP_Style.ElementTypes.Button:
					style.setButton(skin.button.normal.background, skin.button.highlight.background, skin.button.active.background, skin.button.disabled.background);
					break;
				case SEP_Style.ElementTypes.ToggleButton:
					style.setToggle(skin.button.normal.background, skin.button.highlight.background, skin.button.active.background, skin.button.disabled.background);
					break;
				case SEP_Style.ElementTypes.VertScroll:
					style.setScrollbar(skin.verticalScrollbar.normal.background, skin.verticalScrollbarThumb.normal.background);
					break;
				case SEP_Style.ElementTypes.Slider:
					style.setSlider(sliderFrontForeground, sliderFrontForeColor);
					break;
				case SEP_Style.ElementTypes.SliderBackground:
					style.setSlider(sliderBackBackground, sliderBackForeground, sliderBackBackColor, sliderBackForeColor);
					break;
			}
		}
	}
}
