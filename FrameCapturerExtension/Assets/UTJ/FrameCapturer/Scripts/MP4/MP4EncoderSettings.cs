using System;

namespace UTJ
{
	[Serializable]
	public class MP4EncoderSettings : MovieEncoderSettings
	{
		#region Constants

		public new const FrameRateMode DefaultFrameRateMode = FrameRateMode.Variable;

		public new const int DefaultFrameRate = 30;

		public new const int DefaultResolutionWidth = 640;

		public new const int DefaultResolutionHeight = 480;

		public const bool DefaultCaptureVideo = true;

		public const bool DefaultCaptureAudio = true;

		public const int DefaultVideoBitrate = 8192 * 1000;

		public const int DefaultAudioBitrate = 64 * 1000;

		public const int MinVideoBitrate = 64 * 1000;

		public const int MaxVideoBitrate = 65536 * 1000;

		public const int MinAudioBitrate = 16 * 1000;

		public const int MaxAudioBitrate = 256 * 1000;

		#endregion

		#region Fields

		private bool captureVideo = DefaultCaptureVideo;

		private bool captureAudio = DefaultCaptureAudio;

		private int videoBitrate = DefaultVideoBitrate;

		private int audioBitrate = DefaultAudioBitrate;

		#endregion

		#region Properties

		public bool CaptureVideo
		{
			get { return captureVideo; }
			set { captureVideo = value; }
		}

		public bool CaptureAudio
		{
			get { return captureAudio; }
			set { captureAudio = value; }
		}

		public int VideoBitrate
		{
			get { return videoBitrate; }
			set { videoBitrate = Clamp(value, MinVideoBitrate, MaxVideoBitrate); }
		}

		public int AudioBitrate
		{
			get { return audioBitrate; }
			set { audioBitrate = Clamp(value, MinAudioBitrate, MaxAudioBitrate); }
		}

		#endregion

		#region Constructors

		public MP4EncoderSettings()
		{
			FrameRateMode = DefaultFrameRateMode;
			FrameRate = DefaultFrameRate;
			ResolutionWidth = DefaultResolutionWidth;
			ResolutionHeight = DefaultResolutionHeight;
		}

		#endregion
	}
}
