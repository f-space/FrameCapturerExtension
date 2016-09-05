using UnityEngine;

namespace UTJ
{
	[AddComponentMenu("UTJ/FrameCapturer/MP4Recorder")]
	[RequireComponent(typeof(Camera))]
	public class MP4Recorder : MovieRecorder<MP4Encoder>
	{
		#region Fields

		[SerializeField]
		private bool m_CaptureVideo;

		[SerializeField]
		private bool m_CaptureAudio;

		[SerializeField]
		private int m_VideoBitrate;

		[SerializeField]
		private int m_AudioBitrate;

		#endregion

		#region Properties

		public bool CaptureVideo
		{
			get { return m_CaptureVideo; }
			set { m_CaptureVideo = value; }
		}

		public bool CaptureAudio
		{
			get { return m_CaptureAudio; }
			set { m_CaptureAudio = value; }
		}

		public int VideoBitrate
		{
			get { return m_VideoBitrate; }
			set { m_VideoBitrate = value; }
		}

		public int AudioBitrate
		{
			get { return m_AudioBitrate; }
			set { m_AudioBitrate = value; }
		}

		#endregion

		#region Messages

#if UNITY_EDITOR

		protected new void Reset()
		{
			base.Reset();

			ResolutionWidth = 640;
			FrameRateMode = FrameRateMode.Variable;

			m_CaptureVideo = true;
			m_CaptureAudio = true;
			m_VideoBitrate = 8192000;
			m_AudioBitrate = 64000;
		}

		protected new void OnValidate()
		{
			base.OnValidate();

			m_VideoBitrate = Mathf.Clamp(m_VideoBitrate, MP4EncoderSettings.MinVideoBitrate, MP4EncoderSettings.MaxVideoBitrate);
			m_AudioBitrate = Mathf.Clamp(m_AudioBitrate, MP4EncoderSettings.MinAudioBitrate, MP4EncoderSettings.MaxAudioBitrate);

			if (m_CaptureAudio && FrameRateMode == FrameRateMode.Constant)
			{
				Debug.LogWarning("MP4Recorder: capture audio with Constant frame rate mode will cause desync");
			}
		}

#endif // UNITY_EDITOR

		#endregion

		#region Methods

		protected override IMovieRecordingUnit<MP4Encoder> CreateRecordingUnit()
		{
			MP4Encoder encoder = new MP4Encoder();
			string description = "MP4Recorder: copy frame buffer";

			return new MovieRecordingUnit<MP4Encoder>(encoder, true, description);
		}

		protected override void ApplySettings(Camera camera)
		{
			base.ApplySettings(camera);

			MP4EncoderSettings settings = RecordingUnit.Encoder.Settings;

			settings.CaptureVideo = m_CaptureVideo;
			settings.CaptureAudio = m_CaptureAudio;
			settings.VideoBitrate = m_VideoBitrate;
			settings.AudioBitrate = m_AudioBitrate;
		}

		#endregion
	}

}
