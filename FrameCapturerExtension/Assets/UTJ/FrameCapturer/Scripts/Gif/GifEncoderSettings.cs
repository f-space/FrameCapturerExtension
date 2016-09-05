using System;

namespace UTJ
{
	[Serializable]
	public class GifEncoderSettings : MovieEncoderSettings
	{
		#region Constants

		public new const FrameRateMode DefaultFrameRateMode = FrameRateMode.Constant;

		public new const int DefaultFrameRate = 30;

		public new const int DefaultResolutionWidth = 320;

		public new const int DefaultResolutionHeight = 240;

		public const int DefaultColors = 256;

		public const bool DefaultUseLocalPalette = true;

		public const int MinColors = 2;

		public const int MaxColors = 256;

		#endregion

		#region Fields

		private int colors = DefaultColors;

		private bool useLocalPalette = DefaultUseLocalPalette;

		#endregion

		#region Properties

		public int Colors
		{
			get { return colors; }
			set { colors = Clamp(value, MinColors, MaxColors); }
		}

		public bool UseLocalPalette
		{
			get { return useLocalPalette; }
			set { useLocalPalette = value; }
		}

		#endregion

		#region Constructors

		public GifEncoderSettings()
		{
			FrameRateMode = DefaultFrameRateMode;
			FrameRate = DefaultFrameRate;
			ResolutionWidth = DefaultResolutionWidth;
			ResolutionHeight = DefaultResolutionHeight;
		}

		#endregion
	}
}
