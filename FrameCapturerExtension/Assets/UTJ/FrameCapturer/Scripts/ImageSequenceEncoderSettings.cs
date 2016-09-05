using System;

namespace UTJ
{
	[Serializable]
	public abstract class ImageSequenceEncoderSettings
	{
		#region Constants

		public const bool DefaultCaptureFrameBuffer = true;

		public const bool DefaultCaptureGBuffer = true;

		public const bool DefaultOffscreenBuffer = true;

		#endregion

		#region Fields

		private bool captureFrameBuffer = DefaultCaptureFrameBuffer;

		private bool captureGBuffer = DefaultCaptureGBuffer;

		private bool captureOffscreenBuffer = DefaultOffscreenBuffer;

		#endregion

		#region Properties

		public bool CaptureFrameBuffer
		{
			get { return captureFrameBuffer; }
			set { captureFrameBuffer = value; }
		}

		public bool CaptureGBuffer
		{
			get { return captureGBuffer; }
			set { captureGBuffer = value; }
		}

		public bool CaptureOffscreenBuffer
		{
			get { return captureOffscreenBuffer; }
			set { captureOffscreenBuffer = value; }
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
