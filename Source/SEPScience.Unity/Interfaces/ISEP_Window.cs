﻿#region license
/* Copyright (c) 2016, DMagic
All rights reserved.

ISEP_Window - Interface for the SEP window

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

namespace SEPScience.Unity.Interfaces
{
	public interface ISEP_Window
	{
		bool IsVisible { get; set; }

		bool IsMinimized { get; set; }

		bool ShowAllVessels { get; }

		float Scale { get; }

		string Version { get; }

		Vector3 WindowPos { get; set; }

		IList<IVesselSection> GetVessels { get; }

		IList<IVesselSection> GetBodyVessels(string body);

		IVesselSection CurrentVessel { get; }

		IList<string> GetBodies { get; }

		string CurrentBody { get; set; }

		void SetAppState(bool on);

		void UpdateWindow();

		void ChangeVessel(bool forward);
	}
}
