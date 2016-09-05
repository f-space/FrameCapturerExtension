using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UTJ
{
	public class MovieRecorderUI : MonoBehaviour, IMovieRecorderUI
	{
		#region Fields

		[SerializeField]
		private IMovieRecorder m_Recorder;

		private Image background;

		private Text infoText;

		private RawImage previewImage;

		private StringBuilder builder;

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

		#endregion

		#region Messages

		protected void Awake()
		{
			background = GetComponent<Image>();
			infoText = transform.Find("TextInfo").GetComponent<Text>();
			previewImage = transform.Find("ImagePreview").GetComponent<RawImage>();

			Assert.IsNotNull(background);
			Assert.IsNotNull(infoText);
			Assert.IsNotNull(previewImage);
		}

#if UNITY_EDITOR

		protected void OnEnable()
		{
			if (m_Recorder == null)
			{
				m_Recorder = FindObjectsOfType<MonoBehaviour>().OfType<IMovieRecorder>().FirstOrDefault();
			}
		}

#endif // UNITY_EDITOR

		protected void Update()
		{
			if (Recording)
			{
				dirtyFlags |= DirtyFlag.Information;
			}

			UpdateBackground();
			UpdateInfoText();
			UpdatePreviewImage();
		}

		#endregion

		#region Methods

		private void BeginRecording()
		{
			if (m_Recorder.BeginRecording())
			{
				dirtyFlags |= DirtyFlag.All;
			}
		}

		private void EndRecording()
		{
			if (m_Recorder.EndRecording())
			{
				string path;
				m_Recorder.Save(out path);

				dirtyFlags |= DirtyFlag.All;
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

		private void UpdateInfoText()
		{
			if ((dirtyFlags & DirtyFlag.Information) == 0) return;

			const int maxLength = 8;
			const string suffix = " recoded frames";

			if (builder == null)
			{
				builder = new StringBuilder(suffix.Length + maxLength);
			}

			int frameCount = m_Recorder.FrameCount;
			builder.Length = 0;
			builder.Append(frameCount);
			builder.Append(suffix);

			infoText.text = builder.ToString();

			dirtyFlags &= ~DirtyFlag.Information;
		}

		private void UpdatePreviewImage()
		{
			if ((dirtyFlags & DirtyFlag.Preview) == 0) return;

			const float MaxXScale = 1.8f;

			RenderTexture texture = m_Recorder.RecordingUnit.ScratchBuffer;
			if (texture != null)
			{
				previewImage.texture = texture;

				float s = (float)texture.width / texture.height;
				float xs = Mathf.Min(s, MaxXScale);
				float ys = MaxXScale / s;
				previewImage.rectTransform.localScale = new Vector3(xs, ys, 1.0f);
			}

			dirtyFlags &= ~DirtyFlag.Preview;
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

			All = Background | Information | Preview
		}

		#endregion
	}
}
