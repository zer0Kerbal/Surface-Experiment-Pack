#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEP_ExperimentSection - Unity UI element for SEP experiments

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
using SEPScience.Unity.Interfaces;

namespace SEPScience.Unity.Unity
{
	public class SEP_ExperimentSection : MonoBehaviour
	{
		[SerializeField]
		private TextHandler Title = null;
		[SerializeField]
		private Slider BaseSlider = null;
		[SerializeField]
		private Slider FrontSlider = null;
		[SerializeField]
		private TextHandler Remaining = null;
		[SerializeField]
		private Selectable ExperimentSelectable = null;
		[SerializeField]
		private Sprite[] ToggleIcons = null;

		private IExperimentSection experimentInterface;
		private SEP_VesselSection parent;
		private bool toggleState;

		private void OnDestroy()
		{
			if (parent == null)
				return;

			parent.RemoveExperimentSection(this);

			gameObject.SetActive(false);
		}

		private void Update()
		{
			if (experimentInterface == null)
				return;

			if (!experimentInterface.IsVisible)
				return;						

			experimentInterface.Update();

			if (Remaining != null)
				Remaining.OnTextUpdate.Invoke(experimentInterface.DaysRemaining);

			if (BaseSlider != null && FrontSlider != null)
			{
				BaseSlider.normalizedValue = Mathf.Clamp01(experimentInterface.Calibration);

				FrontSlider.normalizedValue = Mathf.Clamp01(experimentInterface.Progress);
			}

			UpdateToggleButton(experimentInterface.IsRunning);
		}

		public void toggleVisibility(bool on)
		{
			if (experimentInterface == null)
				return;

			experimentInterface.IsVisible = on;

			gameObject.SetActive(on);
		}

		public bool experimentRunning
		{
			get
			{
				if (experimentInterface == null)
					return false;

				return experimentInterface.IsRunning;
			}
		}

		public void setExperiment(IExperimentSection experiment, SEP_VesselSection vessel)
		{
			if (experiment == null)
				return;

			if (vessel == null)
				return;

			parent = vessel;

			experimentInterface = experiment;

			if (Title != null)
				Title.OnTextUpdate.Invoke(experiment.Name);

			if (Remaining != null)
				Remaining.OnTextUpdate.Invoke(experimentInterface.DaysRemaining);

			toggleState = !experimentInterface.IsRunning;

			UpdateToggleButton(experimentInterface.IsRunning);

			if (BaseSlider != null && FrontSlider != null)
			{
				BaseSlider.normalizedValue = Mathf.Clamp01(experimentInterface.Calibration);

				FrontSlider.normalizedValue = Mathf.Clamp01(experimentInterface.Progress);
			}

			experimentInterface.setParent(this);
		}

		public void toggleExperiment()
		{
			if (experimentInterface == null)
				return;

			experimentInterface.ToggleExperiment(!experimentInterface.IsRunning);

			UpdateToggleButton(experimentInterface.IsRunning);
		}

		private void UpdateToggleButton(bool isOn)
		{
			if (toggleState == isOn)
				return;

			toggleState = isOn;

			if (ExperimentSelectable == null)
				return;

			if (ToggleIcons.Length < 6)
				return;

			ExperimentSelectable.image.sprite = isOn ? ToggleIcons[3] : ToggleIcons[0];
			ExperimentSelectable.image.type = Image.Type.Simple;
			ExperimentSelectable.transition = Selectable.Transition.SpriteSwap;

			SpriteState state = ExperimentSelectable.spriteState;
			state.highlightedSprite = isOn ? ToggleIcons[4] : ToggleIcons[1];
			state.pressedSprite = isOn ? ToggleIcons[5] : ToggleIcons[2];
			state.disabledSprite = null;
			ExperimentSelectable.spriteState = state;
		}

	}
}
