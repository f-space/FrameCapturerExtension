namespace Fsp.FrameCapaturerExtxension
{
	public interface IGifRecorderSettings : IMovieRecorderSettings
	{
		#region Properties

		int GifColors { get; }

		bool GifUseLocalPalette { get; }

		#endregion
	}
}
