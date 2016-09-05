namespace Fsp.FrameCapaturerExtxension
{
	public interface IMP4RecorderSettings : IMovieRecorderSettings
	{
		#region Properties

		bool MP4CaptureVideo { get; }

		bool MP4CaptureAudio { get; }

		int MP4VideoBitrate { get; }

		int MP4AudioBitrate { get; }

		#endregion
	}
}
