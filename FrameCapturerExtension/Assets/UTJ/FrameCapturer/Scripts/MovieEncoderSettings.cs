using System;

namespace UTJ
{
	[Serializable]
	public abstract class MovieEncoderSettings
	{
		#region Constants

		public const FrameRateMode DefaultFrameRateMode = FrameRateMode.Variable;

		public const int DefaultFrameRate = 30;

		public const int DefaultResolutionWidth = 512;

		public const int DefaultResolutionHeight = 512;

		public const int MinFrameRate = 1;

		public const int MaxFrameRate = 120;

		public const int MinResolution = 1;

		public const int MaxResolution = 2048;

		#endregion

		#region Fields

		private FrameRateMode frameRateMode = DefaultFrameRateMode;

		private int frameRate = DefaultFrameRate;

		private int resolutionWidth = DefaultResolutionWidth;

		private int resolutionHeight = DefaultResolutionHeight;

		#endregion

		#region Properties

		public FrameRateMode FrameRateMode
		{
			get { return frameRateMode; }
			set { frameRateMode = ClampEnum(value); }
		}

		public int FrameRate
		{
			get { return frameRate; }
			set { frameRate = Clamp(value, MinFrameRate, MaxFrameRate); }
		}

		public int ResolutionWidth
		{
			get { return resolutionWidth; }
			set { resolutionWidth = Clamp(value, MinResolution, MaxResolution); }
		}

		public int ResolutionHeight
		{
			get { return resolutionHeight; }
			set { resolutionHeight = Clamp(value, MinResolution, MaxResolution); }
		}

		#endregion

		#region Methods

		protected int Min(int value1, int value2)
		{
			return (value1 < value2 ? value1 : value2);
		}

		protected int Max(int value1, int value2)
		{
			return (value1 > value2 ? value1 : value2);
		}

		protected int Clamp(int value, int min, int max)
		{
			return (value <= min ? min : value >= max ? max : value);
		}

		protected T ClampEnum<T>(T value) where T : struct, IConvertible
		{
			T[] values = (T[])Enum.GetValues(typeof(T));

			return values[Clamp(value.ToInt32(null), 0, values.Length - 1)];
		}

		#endregion
	}
}
