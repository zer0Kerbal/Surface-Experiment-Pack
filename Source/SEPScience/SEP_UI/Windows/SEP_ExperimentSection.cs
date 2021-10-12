#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

SEP_ExperimentSection - UI interface for holding info about SEP experiments

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

using SEPScience.Unity.Interfaces;
using UnityEngine;
using KSP.Localization;

namespace SEPScience.SEP_UI.Windows
{
	public class SEP_ExperimentSection : IExperimentSection
	{
		private string _name;
		private string _daysremaining;
		private bool _isrunning;
		private bool _isvisible = true;
		private bool _cantransmit;
		private float _calibration;
		private float _progress;

		private Vessel vessel;
		private SEPScience.Unity.Unity.SEP_ExperimentSection experimentUISection;
		private SEP_ExperimentHandler handler;

		public SEP_ExperimentSection(SEP_ExperimentHandler h, Vessel v)
		{
			if (h == null)
				return;

			if (v == null)
				return;

			vessel = v;

			handler = h;
			_name = h.experimentTitle;
			_cantransmit = h.canTransmit;

			_daysremaining = getDaysRemaining();
			_progress = h.completion;
			_calibration = h.calibration;
			_isrunning = h.experimentRunning;
		}

		public void OnDestroy()
		{
			if (experimentUISection != null)
				MonoBehaviour.Destroy(experimentUISection);
		}

		public string Name
		{
			get { return _name; }
		}

		public string DaysRemaining
		{
			get { return _daysremaining; }
		}

		public float Calibration
		{
			get { return _calibration; }
		}

		public float Progress
		{
			get { return _progress; }
		}

		public bool IsRunning
		{
			get { return _isrunning; }
		}

		public bool IsVisible
		{
			get { return _isvisible; }
			set { _isvisible = value; }
		}

		public bool CanTransmit
		{
			get { return _cantransmit; }
		}

		public SEP_ExperimentHandler Handler
		{
			get { return handler; }
		}

		public void ToggleExperiment(bool on)
		{
			if (on && !handler.experimentRunning)
			{
				if (vessel.loaded && handler.host != null)
					handler.host.DeployExperiment();
				else
				{
					handler.experimentRunning = true;
					handler.lastBackgroundCheck = Planetarium.GetUniversalTime();
				}

				_isrunning = true;
			}
			else if (!on && handler.experimentRunning)
			{
				if (vessel.loaded && handler.host != null)
					handler.host.PauseExperiment();
				else
					handler.experimentRunning = false;

				_isrunning = false;
			}
		}

		public void setParent(SEPScience.Unity.Unity.SEP_ExperimentSection section)
		{
			experimentUISection = section;
		}

		public void Update()
		{
			if (handler == null)
				return;

			_daysremaining = getDaysRemaining();
			_progress = handler.completion;
			_calibration = handler.calibration;
			_isrunning = handler.experimentRunning;
		}

		private string getDaysRemaining()
		{
			if (handler == null)
				return Localizer.Format("#LOC_SurfaceExperimentPack_ModuleSEPScienceExperiment_statusError");

			float next = getNextCompletion(handler.completion);

			if (handler.completion >= next)
				return Localizer.Format("#LOC_SurfaceExperimentPack_UI_Complete");

			if (handler.calibration <= 0)
				return "∞";

			if (!handler.experimentRunning)
				return "";

			float calib = handler.calibration;

			if (SEP_Controller.Instance.UsingCommNet)
			{
				if (vessel.Connection != null)
				{
					float signal = (float)vessel.Connection.SignalStrength - 0.5f;

					if (signal < 0)
						signal /= 2;

					float bonus = calib * signal;

					calib += bonus;
				}
			}

			float time = handler.experimentTime / calib;

			time *= 21600;

			time *= next;

			float nowTime = 0;

			if (handler.completion > 0)
				nowTime = (handler.completion / next) * time;

			float f = time - nowTime;

			return KSPUtil.PrintTime(f, 2, false);
		}

		private float getNextCompletion(float f)
		{
			if (f < 0.5f)
				return 0.5f;
			if (f < 0.75f)
				return 0.75f;
			return 1f;
		}

	}
}
