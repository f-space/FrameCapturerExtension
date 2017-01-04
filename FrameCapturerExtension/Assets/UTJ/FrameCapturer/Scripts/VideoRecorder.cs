using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace UTJ
{
	[RequireComponent(typeof(Camera))]
	public abstract class VideoRecorder<T> : MonoBehaviour, IMovieRecorder<T> where T : IMovieEncoder
	{
		#region Constants

		public const int MinCaptureEveryNthFrame = 1;

		public const int MaxCaptureEveryNthFrame = Int32.MaxValue;

		#endregion

		#region Fields

		[SerializeField]
		[Tooltip("output directory. filename is generated automatically.")]
		private DataPath m_OutputDirectory;

		[SerializeField]
		private int m_ResolutionWidth;

		[SerializeField]
		private FrameRateMode m_FrameRateMode;

		[SerializeField]
		[Tooltip("relevant only if FrameRateMode is Constant")]
		private int m_FrameRate;

		[SerializeField]
		private int m_CaptureEveryNthFrame;

		[SerializeField]
		[Tooltip("0 is treated as processor count")]
		private Shader m_CopyShader;

		private IMovieRecordingUnit<T> unit;

		#endregion

		#region Properties

		public DataPath OutputDirectory
		{
			get { return m_OutputDirectory; }
			set { m_OutputDirectory = value; }
		}

		public int ResolutionWidth
		{
			get { return m_ResolutionWidth; }
			set { m_ResolutionWidth = value; }
		}

		public FrameRateMode FrameRateMode
		{
			get { return m_FrameRateMode; }
			set { m_FrameRateMode = value; }
		}

		public int FrameRate
		{
			get { return m_FrameRate; }
			set { m_FrameRate = value; }
		}

		public int CaptureEveryNthFrame
		{
			get { return m_CaptureEveryNthFrame; }
			set { m_CaptureEveryNthFrame = value; }
		}

		public Shader CopyShader
		{
			get { return m_CopyShader; }
			set { m_CopyShader = value; }
		}

		public IMovieRecordingUnit<T> RecordingUnit { get { return unit; } }

		public T Encoder { get { return unit.Encoder; } }

		public bool Seekable { get { return unit.Encoder.Seekable; } }

		public bool Editable { get { return unit.Encoder.Editable; } }

		public bool Recording { get { return unit.Recording; } }

		public int FrameCount { get { return unit.FrameCount; } }

		IMovieRecordingUnit IMovieRecorder.RecordingUnit { get { return RecordingUnit; } }

		IMovieEncoder IMovieRecorder.Encoder { get { return Encoder; } }

		Type IMovieRecorder.EncoderType { get { return typeof(T); } }

		#endregion

		#region Messages

		protected void Awake()
		{
			IMovieRecordingUnit<T> unit = CreateRecordingUnit();

			Assert.IsNotNull(unit);

			this.unit = unit;
		}

		protected void OnDestroy()
		{
			if (unit != null)
			{
				unit.Dispose();

				unit = null;
			}
		}

		protected void OnEnable() { }

		protected void OnDisable()
		{
			if (unit.Recording) unit.EndRecording();

			unit.ReleaseResources();
		}

		protected IEnumerator OnPostRender()
		{
			if (unit.Recording && Time.frameCount % m_CaptureEveryNthFrame == 0)
			{
				yield return new WaitForEndOfFrame();

				unit.RecordImage(Time.unscaledTime);
			}
		}

#if UNITY_EDITOR

		protected void Reset()
		{
			m_OutputDirectory = new DataPath(DataPath.Root.PersistentDataPath, "");
			m_ResolutionWidth = 512;
			m_FrameRateMode = FrameRateMode.Variable;
			m_FrameRate = 30;
			m_CaptureEveryNthFrame = 2;
			m_CopyShader = ResourceHelper.LoadCopyShader();
		}

		protected void OnValidate()
		{
			m_ResolutionWidth = Mathf.Clamp(m_ResolutionWidth, MovieEncoderSettings.MinResolution, MovieEncoderSettings.MaxResolution);
			m_FrameRate = Mathf.Clamp(m_FrameRate, MovieEncoderSettings.MinFrameRate, MovieEncoderSettings.MaxFrameRate);
			m_CaptureEveryNthFrame = Mathf.Clamp(m_CaptureEveryNthFrame, MinCaptureEveryNthFrame, MaxCaptureEveryNthFrame);
		}

#endif // UNITY_EDITOR

		#endregion

		#region Methods

		public void Clear()
		{
			unit.Encoder.Reset();
		}

		public bool BeginRecording()
		{
			if (unit.Recording) return false;

			Camera camera = GetComponent<Camera>();
			if (!camera) return false;

			ApplySettings(camera);

			unit.Camera = camera;
			unit.BeginRecording();

			Debug.LogFormat("{0}.BeginRecording()", GetType().Name);

			return true;
		}

		public bool EndRecording()
		{
			if (!unit.Recording) return false;

			unit.EndRecording();
			unit.Camera = null;

			Debug.LogFormat("{0}.EndRecording()", GetType().Name);

			return true;
		}

		public bool Save(out string path, int beginFrame = 0, int endFrame = -1)
		{
			T encoder = unit.Encoder;

			string fileName = GetFileName(encoder.Extension);
			string filePath = GetOutputPath(m_OutputDirectory, fileName);

			m_OutputDirectory.CreateDirectory();

			bool result;
			if (encoder.Seekable)
			{
				ISeekableMovieEncoder seekable = (ISeekableMovieEncoder)encoder;

				result = seekable.Flush(filePath, beginFrame, endFrame);

				Debug.LogFormat("{0}.Flush({2}, {3}): {1}", encoder.GetType().Name, filePath, beginFrame, endFrame);
			}
			else
			{
				result = encoder.Flush(filePath);

				Debug.LogFormat("{0}.Flush(): {1}", encoder.GetType().Name, filePath);
			}

			path = result ? filePath : null;

			return result;
		}

		public int GetExpectedFileSize(int beginFrame = 0, int endFrame = -1)
		{
			if (!unit.Encoder.Seekable) throw new NotSupportedException();

			ISeekableMovieEncoder seekable = (ISeekableMovieEncoder)unit.Encoder;

			return seekable.GetExpectedFileSize(beginFrame, endFrame);
		}

		public void GetFrameData(RenderTexture texture, int frame)
		{
			if (!unit.Encoder.Seekable) throw new NotSupportedException();

			ISeekableMovieEncoder seekable = (ISeekableMovieEncoder)unit.Encoder;

			seekable.GetFrameData(texture, frame);
		}

		public void EraseFrame(int beginFrame = 0, int endFrame = -1)
		{
			if (!unit.Encoder.Editable) throw new NotSupportedException();

			IEditableMovieEncoder editable = (IEditableMovieEncoder)unit.Encoder;

			editable.EraseFrame(beginFrame, endFrame);
		}

		protected abstract IMovieRecordingUnit<T> CreateRecordingUnit();

		protected virtual void ApplySettings(Camera camera)
		{
			MovieEncoderSettings settings = unit.Encoder.Settings;
			float aspect = camera.aspect;

			settings.ResolutionWidth = m_ResolutionWidth;
			settings.ResolutionHeight = (int)(m_ResolutionWidth / aspect);
			settings.FrameRateMode = m_FrameRateMode;
			settings.FrameRate = m_FrameRate;

			unit.CopyShader = m_CopyShader;
		}

		private static string GetFileName(string ext)
		{
			return DateTime.Now.ToString("yyyyMMdd_HHmmss") + ext;
		}

		private static string GetOutputPath(DataPath directory, string name)
		{
			return directory.GetPath() + "/" + name;
		}

		#endregion
	}
}
