using UnityEngine;

namespace UTJ
{
	[AddComponentMenu("UTJ/FrameCapturer/MP4OffscreenRecorder")]
	[RequireComponent(typeof(Camera))]
	public class MP4OffscreenRecorder : MP4Recorder
	{
		#region Fields

		[SerializeField]
		private RenderTexture m_Target;

		#endregion

		#region Properties

		public RenderTexture Target
		{
			get { return m_Target; }
			set { m_Target = value; }
		}

		#endregion

		#region Messages

#if UNITY_EDITOR

		protected new void Reset()
		{
			base.Reset();

			this.m_Target = null;
		}

#endif // UNITY_EDITOR

		#endregion

		#region Methods

		protected override IMovieRecordingUnit<MP4Encoder> CreateRecordingUnit()
		{
			MP4Encoder encoder = new MP4Encoder();
			string description = "MP4OffscreenRecorder: copy frame buffer";

			return new OffscreenMovieRecordingUnit<MP4Encoder>(encoder, true, description);
		}

		protected override void ApplySettings(Camera camera)
		{
			base.ApplySettings(camera);

			var unit = (OffscreenMovieRecordingUnit<MP4Encoder>)RecordingUnit;

			unit.Target = m_Target;
		}

		#endregion
	}

}
