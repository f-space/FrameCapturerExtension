using UnityEngine;

namespace UTJ
{
	[RequireComponent(typeof(Camera))]
	public abstract class ImageSequenceRecorder<T> : ImageSequenceRecorderBase<ImageSequenceRecordingUnit<T>, T>
		where T : IImageSequenceEncoder
	{
		#region Fields

		[SerializeField]
		private bool m_CaptureFrameBuffer;

		[SerializeField]
		private bool m_CaptureGBuffer;

		#endregion

		#region Properties

		public bool CaptureFrameBuffer
		{
			get { return m_CaptureFrameBuffer; }
			set { m_CaptureFrameBuffer = value; }
		}

		public bool CaptureGBuffer
		{
			get { return m_CaptureGBuffer; }
			set { m_CaptureGBuffer = value; }
		}

		#endregion

		#region Messages

#if UNITY_EDITOR

		protected new void Reset()
		{
			base.Reset();

			m_CaptureFrameBuffer = true;
			m_CaptureGBuffer = true;

			Camera camera = GetComponent<Camera>();
			if (!FrameCapturerUtils.IsRenderingPathDeferred(camera))
			{
				m_CaptureGBuffer = false;
			}
		}

		protected new void OnValidate()
		{
			base.OnValidate();

			Camera camera = GetComponent<Camera>();
			if (m_CaptureGBuffer && !FrameCapturerUtils.IsRenderingPathDeferred(camera))
			{
				m_CaptureGBuffer = false;

				Debug.LogWarningFormat("{0}: Rendering Path must be deferred to use Capture GBuffer mode.", GetType().Name);
			}
		}

#endif // UNITY_EDITOR

		#endregion

		#region Methods

		protected override void ApplySettings(Camera camera)
		{
			base.ApplySettings(camera);

			ImageSequenceEncoderSettings settings = Encoder.Settings;

			settings.CaptureFrameBuffer = m_CaptureFrameBuffer;
			settings.CaptureGBuffer = m_CaptureGBuffer;
			settings.CaptureOffscreenBuffer = false;
		}

		#endregion
	}
}
