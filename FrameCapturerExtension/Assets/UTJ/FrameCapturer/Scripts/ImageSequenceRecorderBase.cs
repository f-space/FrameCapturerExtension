using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace UTJ
{
	public abstract class ImageSequenceRecorderBase<TUnit, T> : MonoBehaviour
		where TUnit : class, IImageSequenceRecordingUnit<T>
		where T : IImageSequenceEncoder
	{
		#region Fields

		[SerializeField]
		[Tooltip("output directory. filename is generated automatically.")]
		private DataPath m_OutputDirectory;

		[SerializeField]
		private int m_BeginFrame;

		[SerializeField]
		private int m_EndFrame;

		[SerializeField]
		private Shader m_CopyShader;

		private TUnit unit;

		#endregion

		#region Properties

		public DataPath OutputDirectory
		{
			get { return m_OutputDirectory; }
			set { m_OutputDirectory = value; }
		}

		public int BeginFrame
		{
			get { return m_BeginFrame; }
			set { m_BeginFrame = value; }
		}

		public int EndFrame
		{
			get { return m_EndFrame; }
			set { m_EndFrame = value; }
		}

		public Shader CopyShader
		{
			get { return m_CopyShader; }
			set { m_CopyShader = value; }
		}

		public TUnit RecordingUnit { get { return unit; } }

		public T Encoder { get { return (T)unit.Encoder; } }

		public bool Recording { get { return unit.Recording; } }

		#endregion

		#region Messages

		protected void Awake()
		{
			TUnit unit = CreateRecordingUnit();

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


		protected void Update()
		{
			int frame = Time.frameCount;

			if (frame == m_BeginFrame)
			{
				BeginRecording();
			}
			if (frame == m_EndFrame + 1)
			{
				EndRecording();
			}
		}

		protected IEnumerator OnPostRender()
		{
			int frame = Time.frameCount;
			if (frame >= m_BeginFrame && frame <= m_EndFrame)
			{
				yield return new WaitForEndOfFrame();

				Export(frame);
			}
		}

#if UNITY_EDITOR

		protected void Reset()
		{
			m_OutputDirectory = new DataPath(DataPath.Root.PersistentDataPath, "");
			m_BeginFrame = 1;
			m_EndFrame = 100;
			m_CopyShader = ResourceHelper.LoadCopyShader();
		}

		protected void OnValidate()
		{
			m_BeginFrame = Mathf.Max(1, m_BeginFrame);
			m_EndFrame = Mathf.Max(m_BeginFrame, m_EndFrame);
		}

#endif // UNITY_EDITOR

		#endregion

		#region Methods

		protected abstract TUnit CreateRecordingUnit();

		protected virtual void ApplySettings(Camera camera)
		{
			unit.CopyShader = m_CopyShader;
		}

		private bool BeginRecording()
		{
			if (unit.Recording) return false;

			Camera camera = GetComponent<Camera>();
			if (!camera) return false;

			ApplySettings(camera);

			OutputDirectory.CreateDirectory();

			unit.Camera = camera;
			unit.BeginRecording();

			return true;
		}

		private bool EndRecording()
		{
			if (!unit.Recording) return false;

			unit.EndRecording();
			unit.Camera = null;

			return true;
		}

		private void Export(int frame)
		{
			Debug.LogFormat("{0}: exporting frame {1}", GetType().Name, frame);

			string path = OutputDirectory.GetPath();
			int number = frame;

			unit.Export(path, number);
		}

		#endregion
	}
}
