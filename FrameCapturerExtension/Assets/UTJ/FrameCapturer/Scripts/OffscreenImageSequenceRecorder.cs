using UnityEngine;

namespace UTJ
{
	[RequireComponent(typeof(Camera))]
	public abstract class OffscreenImageSequenceRecorder<T> : ImageSequenceRecorderBase<OffscreenImageSequenceRecordingUnit<T>, T>
		where T : IImageSequenceEncoder
	{
		#region Fields

		[SerializeField]
		private RenderTexture[] m_Targets;

		#endregion

		#region Properties

		public RenderTexture[] Targets
		{
			get { return m_Targets; }
			set { m_Targets = value; }
		}

		#endregion

		#region Messages

#if UNITY_EDITOR

		protected new void Reset()
		{
			base.Reset();

			this.m_Targets = null;
		}

#endif // UNITY_EDITOR

		#endregion

		#region Methods

		protected override void ApplySettings(Camera camera)
		{
			base.ApplySettings(camera);

			ImageSequenceEncoderSettings settings = Encoder.Settings;

			settings.CaptureFrameBuffer = false;
			settings.CaptureGBuffer = false;
			settings.CaptureOffscreenBuffer = true;

			RecordingUnit.Targets = m_Targets;
		}

		#endregion
	}
}
