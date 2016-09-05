using UnityEngine;

namespace UTJ
{
	[AddComponentMenu("UTJ/FrameCapturer/GifRecorder")]
	[RequireComponent(typeof(Camera))]
	public class GifRecorder : VideoRecorder<GifEncoder>
	{
		#region Fields

		[SerializeField]
		private int m_Colors;

		[SerializeField]
		private bool m_UseLocalPalette;

		#endregion

		#region Properties

		public int Colors
		{
			get { return m_Colors; }
			set { m_Colors = value; }
		}

		public bool UseLocalPalette
		{
			get { return m_UseLocalPalette; }
			set { m_UseLocalPalette = value; }
		}

		#endregion

		#region Messages

#if UNITY_EDITOR

		protected new void Reset()
		{
			base.Reset();

			ResolutionWidth = 300;
			FrameRateMode = FrameRateMode.Constant;

			m_Colors = 256;
			m_UseLocalPalette = true;
		}

		protected new void OnValidate()
		{
			base.OnValidate();

			m_Colors = Mathf.Clamp(m_Colors, GifEncoderSettings.MinColors, GifEncoderSettings.MaxColors);
		}

#endif // UNITY_EDITOR

		#endregion

		#region Methods

		protected override IMovieRecordingUnit<GifEncoder> CreateRecordingUnit()
		{
			GifEncoder encoder = new GifEncoder();
			string description = "GifRecorder: copy frame buffer";

			return new MovieRecordingUnit<GifEncoder>(encoder, true, description);
		}

		protected override void ApplySettings(Camera camera)
		{
			base.ApplySettings(camera);

			GifEncoderSettings settings = RecordingUnit.Encoder.Settings;

			settings.Colors = m_Colors;
			settings.UseLocalPalette = m_UseLocalPalette;
		}

		#endregion
	}

}
