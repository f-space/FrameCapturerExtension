using UnityEngine;

namespace UTJ
{
	[AddComponentMenu("UTJ/FrameCapturer/ExrOffscreenRecorder")]
	[RequireComponent(typeof(Camera))]
	public class ExrOffscreenRecorder : OffscreenImageSequenceRecorder<ExrEncoder>
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

		protected override OffscreenImageSequenceRecordingUnit<ExrEncoder> CreateRecordingUnit()
		{
			ExrEncoder encoder = new ExrEncoder();
			string description = "ExrOffscreenRecorder: Copy";

			return new OffscreenImageSequenceRecordingUnit<ExrEncoder>(encoder, true, description);
		}

		#endregion
	}
}

