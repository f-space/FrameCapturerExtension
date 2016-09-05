using UnityEngine;

namespace UTJ
{
	[AddComponentMenu("UTJ/FrameCapturer/GifOffscreenRecorder")]
	[RequireComponent(typeof(Camera))]
	public class GifOffscreenRecorder : GifRecorder
	{
		#region Fields

		[SerializeField]
		private RenderTexture m_Target;

		#endregion

		#region Properties

		public RenderTexture Target
		{
			get { return m_Target; }
			set { m_Target = value; }
		}

		#endregion

		#region Messages

#if UNITY_EDITOR

		protected new void Reset()
		{
			base.Reset();

			this.m_Target = null;
		}

#endif // UNITY_EDITOR

		#endregion

		#region Methods

		protected override IMovieRecordingUnit<GifEncoder> CreateRecordingUnit()
		{
			GifEncoder encoder = new GifEncoder();
			string description = "GifOffscreenRecorder: copy frame buffer";

			return new OffscreenMovieRecordingUnit<GifEncoder>(encoder, true, description);
		}

		protected override void ApplySettings(Camera camera)
		{
			base.ApplySettings(camera);

			var unit = (OffscreenMovieRecordingUnit<GifEncoder>)RecordingUnit;

			unit.Target = m_Target;
		}

		#endregion
	}

}
