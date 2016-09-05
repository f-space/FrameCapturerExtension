using UnityEngine;

namespace UTJ
{
	[AddComponentMenu("UTJ/FrameCapturer/PngOffscreenRecorder")]
	[RequireComponent(typeof(Camera))]
	public class PngOffscreenRecorder : OffscreenImageSequenceRecorder<PngEncoder>
	{
		#region Messages

#if UNITY_EDITOR

		protected new void Reset()
		{
			base.Reset();

			OutputDirectory = new DataPath(DataPath.Root.CurrentDirectory, "PngOutput");
		}

#endif // UNITY_EDITOR

		#endregion

		#region Methods

		protected override OffscreenImageSequenceRecordingUnit<PngEncoder> CreateRecordingUnit()
		{
			PngEncoder encoder = new PngEncoder();
			string description = "PngOffscreenRecorder: Copy";

			return new OffscreenImageSequenceRecordingUnit<PngEncoder>(encoder, true, description);
		}

		#endregion
	}
}

