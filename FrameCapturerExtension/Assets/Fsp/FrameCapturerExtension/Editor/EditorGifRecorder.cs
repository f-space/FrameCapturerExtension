using UnityEngine;
using UTJ;

namespace Fsp.FrameCapaturerExtxension
{
	public class EditorGifRecorder : EditorMovieRecorder<GifEncoder>
	{
		#region Properties

		public override EncoderType EncoderType { get { return EncoderType.Gif; } }

		#endregion

		#region Constructors

		public EditorGifRecorder() : base(CreateRecordingUnit()) { }

		#endregion

		#region Methods

		protected override void UpdateSettings(IMovieRecorderSettings source, Camera camera)
		{
			base.UpdateSettings(source, camera);

			IGifRecorderSettings gifSource = source as IGifRecorderSettings;
			if (gifSource != null)
			{
				GifEncoderSettings settings = Encoder.Settings;

				settings.Colors = gifSource.GifColors;
				settings.UseLocalPalette = gifSource.GifUseLocalPalette;
			}
		}

		private static IMovieRecordingUnit<GifEncoder> CreateRecordingUnit()
		{
			GifEncoder encoder = new GifEncoder();
			string description = "EditorGifRecorder: Copy frame buffer";

			return new MovieRecordingUnit<GifEncoder>(encoder, true, description);
		}

		#endregion
	}
}
