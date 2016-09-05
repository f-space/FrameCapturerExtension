using UnityEngine;
using UTJ;

namespace Fsp.FrameCapaturerExtxension
{
	public class EditorMP4Recorder : EditorMovieRecorder<MP4Encoder>
	{
		#region Properties

		public override EncoderType EncoderType { get { return EncoderType.MP4; } }

		#endregion

		#region Constructors

		public EditorMP4Recorder() : base(CreateRecordingUnit()) { }

		#endregion

		#region Methods

		protected override void UpdateSettings(IMovieRecorderSettings source, Camera camera)
		{
			base.UpdateSettings(source, camera);

			IMP4RecorderSettings mp4Source = source as IMP4RecorderSettings;
			if (mp4Source != null)
			{
				MP4EncoderSettings settings = Encoder.Settings;

				settings.CaptureVideo = mp4Source.MP4CaptureVideo;
				settings.CaptureAudio = mp4Source.MP4CaptureAudio;
				settings.VideoBitrate = mp4Source.MP4VideoBitrate;
				settings.AudioBitrate = mp4Source.MP4AudioBitrate;
			}
		}

		private static IMovieRecordingUnit<MP4Encoder> CreateRecordingUnit()
		{
			MP4Encoder encoder = new MP4Encoder();
			string description = "EditorMP4Recorder: Copy frame buffer";

			return new MovieRecordingUnit<MP4Encoder>(encoder, true, description);
		}

		#endregion
	}
}
