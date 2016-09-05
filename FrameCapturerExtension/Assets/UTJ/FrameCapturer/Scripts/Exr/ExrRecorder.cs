using UnityEngine;

namespace UTJ
{
	[AddComponentMenu("UTJ/FrameCapturer/ExrRecorder")]
	[RequireComponent(typeof(Camera))]
	public class ExrRecorder : ImageSequenceRecorder<ExrEncoder>
	{
		#region Messages

#if UNITY_EDITOR

		protected new void Reset()
		{
			base.Reset();

			OutputDirectory = new DataPath(DataPath.Root.CurrentDirectory, "ExrOutput");
		}

#endif // UNITY_EDITOR

		#endregion

		#region Methods

		protected override ImageSequenceRecordingUnit<ExrEncoder> CreateRecordingUnit()
		{
			ExrEncoder encoder = new ExrEncoder();
			string description = "ExrRecorder: Copy FrameBuffer";
			string gdescription = "ExrRecorder: Copy G-Buffer";

			return new ExrRecordingUnit(encoder, true, description, gdescription);
		}

		#endregion
	}
}
