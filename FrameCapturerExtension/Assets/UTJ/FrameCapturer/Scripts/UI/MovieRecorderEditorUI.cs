using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UTJ
{
	public class MovieRecorderEditorUI : MonoBehaviour, IMovieRecorderUI
	{
		#region Fields

		[SerializeField]
		private IMovieRecorder m_Recorder;

		private Image background;

		private Text infoText;

		private RawImage previewImage;

		private Slider timeSlider;

		private InputField currentFrameInputField;

		private StringBuilder builder;

		private RenderTexture previewTexture;

		private int currentFrame;

		private int beginFrame;

		private int endFrame;

		private DirtyFlag dirtyFlags;

		#endregion

		#region Properties

		public IMovieRecorder Recorder { get { return m_Recorder; } }

		public bool Recording
		{
			get { return m_Recorder.Recording; }
			set
			{
				if (value)
				{
					BeginRecording();
				}
				else
				{
					EndRecording();
				}
			}
		}


		public int CurrentFrame
		{
			get { return currentFrame; }
			set
			{
				if (currentFrame != value)
				{
					int frameCount = m_Recorder.Encoder.FrameCount;

					currentFrame = Mathf.Max(0, Mathf.Min(frameCount - 1, value));

					dirtyFlags |= DirtyFlag.FrameChange;
				}
			}
		}

		public int BeginFrame
		{
			get { return beginFrame; }
			set
			{
				if (beginFrame != value)
				{
					int frameCount = m_Recorder.FrameCount;
					int end = (endFrame >= 0 ? endFrame : frameCount);

					beginFrame = Mathf.Max(0, Mathf.Min(end - 1, value));

					dirtyFlags |= DirtyFlag.RangeChange;
				}
			}
		}

		public int EndFrame
		{
			get { return endFrame; }
			set
			{
				if (endFrame != value)
				{
					int frameCount = m_Recorder.FrameCount;
					int begin = beginFrame;

					endFrame = Mathf.Min(frameCount, Mathf.Max(begin + 1, value));
					if (endFrame == frameCount) endFrame = -1;

					dirtyFlags |= DirtyFlag.RangeChange;
				}
			}
		}


		#endregion

		#region Messages

		protected void Awake()
		{
			background = GetComponent<Image>();
			infoText = transform.Find("TextInfo").GetComponent<Text>();
			previewImage = transform.Find("ImagePreview").GetComponent<RawImage>();
			timeSlider = transform.Find("TimeSlider").GetComponent<Slider>();
			currentFrameInputField = transform.Find("InputCurrentFrame").GetComponent<InputField>();

			ResetFrames();

			Assert.IsNotNull(background);
			Assert.IsNotNull(infoText);
			Assert.IsNotNull(previewImage);
			Assert.IsNotNull(timeSlider);
			Assert.IsNotNull(currentFrameInputField);
		}

		protected void OnEnable()
		{
#if UNITY_EDITOR
			if (m_Recorder == null)
			{
				m_Recorder = FindObjectsOfType<MonoBehaviour>().OfType<IMovieRecorder>().FirstOrDefault();
			}

			Assert.IsNotNull(m_Recorder);
#endif // UNITY_EDITOR
		}

		protected void OnDisable()
		{
			DisposalHelper.Dispose(ref previewTexture);
		}

		protected void Update()
		{
			if (Recording)
			{
				CurrentFrame = m_Recorder.FrameCount - 1;

				dirtyFlags |= DirtyFlag.Recording;
			}

			UpdateBackground();
			UpdateInformation();
			UpdatePreview();
			UpdateTimeline();
			UpdateCurrentFrame();
		}

		#endregion

		#region Methods

		public void SetCurrentFrame(float value)
		{
			CurrentFrame = (int)value;
		}

		public void SetBeginFrame()
		{
			BeginFrame = currentFrame;
		}

		public void SetEndFrame()
		{
			EndFrame = currentFrame + 1;
		}

		public void EraseFrames()
		{
			if (m_Recorder.Editable)
			{
				int frameCount = m_Recorder.FrameCount;
				int begin = beginFrame;
				int end = (endFrame >= 0 ? endFrame : frameCount);
				int size = end - begin;

				if (size > 0 && size < frameCount)
				{
					m_Recorder.EraseFrame(beginFrame, endFrame);

					ResetFrames();
				}
			}
		}

		public void Reset()
		{
			bool recording = Recording;

			if (recording) EndRecording();

			m_Recorder.Clear();

			ResetFrames();

			if (recording) BeginRecording();
		}

		public void Save()
		{
			string path;
			m_Recorder.Save(out path, beginFrame, endFrame);
		}

		private void BeginRecording()
		{
			if (m_Recorder.BeginRecording())
			{
				dirtyFlags |= DirtyFlag.Background;
			}
		}

		private void EndRecording()
		{
			if (m_Recorder.EndRecording())
			{
				ResetFrames();

				dirtyFlags |= DirtyFlag.Background;
			}
		}

		private void UpdateBackground()
		{
			if ((dirtyFlags & DirtyFlag.Background) == 0) return;

			if (Recording)
			{
				background.color = new Color(1.0f, 0.5f, 0.5f, 0.5f);
			}
			else
			{
				background.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			}

			dirtyFlags &= ~DirtyFlag.Background;
		}

		private void UpdateInformation()
		{
			if ((dirtyFlags & DirtyFlag.Information) == 0) return;

			const int capacity = 128;

			if (builder == null)
			{
				builder = new StringBuilder(capacity);
			}

			int frameCount = m_Recorder.FrameCount;
			int expectedFileSize = (m_Recorder.Seekable ? m_Recorder.GetExpectedFileSize(beginFrame, endFrame) : -1);

			int begin = beginFrame;
			int end = (endFrame >= 0 ? endFrame : frameCount);
			int size = end - begin;

			builder.Length = 0;
			builder.Append(frameCount);
			builder.Append(" recoded frames\n");
			builder.Append(size);
			builder.Append(" output frames (");
			builder.Append(begin);
			builder.Append(" - ");
			builder.Append(Mathf.Max(0, end - 1));
			builder.Append(")\n");
			builder.Append("expected file size: ");
			builder.Append(Mathf.Max(0, expectedFileSize));

			infoText.text = builder.ToString();

			dirtyFlags &= ~DirtyFlag.Information;
		}

		private void UpdatePreview()
		{
			if ((dirtyFlags & DirtyFlag.Preview) == 0) return;

			RenderTexture texture;
			if (Recording)
			{
				texture = m_Recorder.RecordingUnit.ScratchBuffer;
			}
			else
			{
				UpdatePreviewTexture();
				RenderPreviewTexture();

				texture = previewTexture;
			}

			previewImage.texture = texture;
			if (texture != null)
			{
				const float MaxXScale = 1.8f;

				float s = (float)texture.width / texture.height;
				float xs = Mathf.Min(s, MaxXScale);
				float ys = MaxXScale / s;
				previewImage.rectTransform.localScale = new Vector3(xs, ys, 1.0f);
			}

			dirtyFlags &= ~DirtyFlag.Preview;
		}

		private void UpdateTimeline()
		{
			if ((dirtyFlags & DirtyFlag.Timeline) == 0) return;

			int frameCount = m_Recorder.FrameCount;

			timeSlider.maxValue = Mathf.Max(0, frameCount - 1);
			timeSlider.value = currentFrame;

			dirtyFlags &= ~DirtyFlag.Timeline;
		}

		private void UpdateCurrentFrame()
		{
			if ((dirtyFlags & DirtyFlag.CurrentFrame) == 0) return;

			currentFrameInputField.text = currentFrame.ToString();

			dirtyFlags &= ~DirtyFlag.CurrentFrame;
		}

		private void UpdatePreviewTexture()
		{
			RenderTexture buffer = m_Recorder.RecordingUnit.ScratchBuffer;
			if (buffer != null)
			{
				if (previewTexture != null)
				{
					bool created = previewTexture.IsCreated();
					bool resized = (previewTexture.width != buffer.width || previewTexture.height != buffer.height);

					if (created && !resized) return;

					DisposalHelper.Dispose(ref previewTexture);
				}

				RenderTexture texture = new RenderTexture(buffer.width, buffer.height, 0, RenderTextureFormat.ARGB32);
				texture.wrapMode = TextureWrapMode.Repeat;
				texture.Create();

				previewTexture = DisposalHelper.Mark(texture);
			}
		}

		private void RenderPreviewTexture()
		{
			if (m_Recorder.Seekable && previewTexture != null)
			{
				m_Recorder.GetFrameData(previewTexture, currentFrame);
			}
		}

		private void ResetFrames()
		{
			currentFrame = 0;
			beginFrame = 0;
			endFrame = -1;

			dirtyFlags = DirtyFlag.FrameChange | DirtyFlag.RangeChange;
		}

		#endregion

		#region DirtyFlag

		[Flags]
		private enum DirtyFlag
		{
			None = 0x00,
			Background = 0x01,
			Information = 0x02,
			Preview = 0x04,
			Timeline = 0x08,
			CurrentFrame = 0x10,

			Recording = Information | Preview | Timeline | CurrentFrame,
			FrameChange = Preview | Timeline | CurrentFrame,
			RangeChange = Information,
			All = Background | FrameChange | RangeChange,
		}

		#endregion
	}
}
