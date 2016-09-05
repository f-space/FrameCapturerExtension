using UnityEngine;

namespace UTJ
{
	[AddComponentMenu("UTJ/FrameCapturer/PngRecorder")]
	[RequireComponent(typeof(Camera))]
	public class PngRecorder : ImageSequenceRecorder<PngEncoder>
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

		protected override ImageSequenceRecordingUnit<PngEncoder> CreateRecordingUnit()
		{
			PngEncoder encoder = new PngEncoder();
			string description = "PngRecorder: Copy FrameBuffer";
			string gdescription = "PngRecorder: Copy G-Buffer";

			return new PngRecordingUnit(encoder, true, description, gdescription);
		}

		#endregion
	}
}
