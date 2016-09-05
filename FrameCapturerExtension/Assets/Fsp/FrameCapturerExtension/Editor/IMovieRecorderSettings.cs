using UTJ;

namespace Fsp.FrameCapaturerExtxension
{
	public interface IMovieRecorderSettings
	{
		#region Properties

		MovieResolutionMode ResolutionMode { get; }

		int ResolutionWidth { get; }

		int ResolutionHeight { get; }

		FrameRateMode FrameRateMode { get; }

		int FrameRate { get; }

		int MinUpdateRate { get; }

		#endregion
	}
}
