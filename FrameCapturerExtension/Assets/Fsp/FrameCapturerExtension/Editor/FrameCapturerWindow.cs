using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Fsp.FrameCapaturerExtxension
{
	public partial class FrameCapturerWindow : EditorWindow
	{
		#region Constants

		private const string MenuPath = "Window/Frame Capturer";

		private const string Title = "Frame Capturer";

		private const string PreferenceKey = "FrameCapturerWindowPrefs";

		private const int LastFrame = -1;

		#endregion

		#region Fields

		[SerializeField]
		private FrameCapturerWindowState state;

		[NonSerialized]
		private FrameCapturerWindowSettings settings;

		[NonSerialized]
		private IEditorMovieRecorder recorder;

		[NonSerialized]
		private RenderTexture previewTexture;

		[NonSerialized]
		private int currentFrame;

		[NonSerialized]
		private int beginFrame;

		[NonSerialized]
		private int endFrame;

		private static string[] desiredNextWindowNames = { "InspectorWindow" };

		private static Type[] desiredDockNextTo = FindNextWindowTypes();

		#endregion

		#region Properties

		public bool ReadyToRecord { get { return (EditorApplication.isPlaying && SelectCamera()); } }

		public bool Recording { get { return (recorder != null && recorder.Recording); } }

		public bool Recorded { get { return (recorder != null && recorder.FrameCount > 0); } }

		public bool Operatable { get { return (!Recording && Recorded); } }

		public bool Seekable { get { return (Operatable && recorder.Seekable); } }

		public bool Editable { get { return (Operatable && recorder.Editable); } }

		public int FrameCount { get { return (Recorded ? recorder.FrameCount : 0); } }

		public int CurrentFrame
		{
			get { return (Recorded ? (currentFrame != LastFrame ? currentFrame : recorder.FrameCount - 1) : 0); }
			set
			{
				if (Seekable)
				{
					currentFrame = Mathf.Clamp(value, 0, recorder.FrameCount - 1);
					if (currentFrame == recorder.FrameCount - 1)
					{
						currentFrame = LastFrame;
					}
				}
				else
				{
					currentFrame = LastFrame;
				}
			}
		}

		public int BeginFrame
		{
			get { return (Recorded ? beginFrame : 0); }
			set
			{
				if (Seekable)
				{
					beginFrame = Mathf.Clamp(value, 0, recorder.FrameCount - 1);
					if (beginFrame >= endFrame && endFrame != LastFrame)
					{
						endFrame = beginFrame + 1;
					}
				}
				else
				{
					beginFrame = 0;
				}
			}
		}

		public int EndFrame
		{
			get { return (Recorded ? (endFrame != LastFrame ? endFrame : recorder.FrameCount) : 0); }
			set
			{
				if (Seekable)
				{
					endFrame = Mathf.Clamp(value, 1, recorder.FrameCount);
					if (endFrame == recorder.FrameCount)
					{
						endFrame = LastFrame;
					}
					else if (endFrame <= beginFrame)
					{
						beginFrame = endFrame - 1;
					}
				}
				else
				{
					endFrame = LastFrame;
				}
			}
		}

		#endregion

		#region Messages

		[MenuItem(MenuPath)]
		private static void Open()
		{
			FrameCapturerWindow window = GetWindow<FrameCapturerWindow>(Title, true, desiredDockNextTo);

			window.Show();
		}

		private void OnEnable()
		{
			InitializeState();
			LoadSettings();

			ResetFrames();

			Undo.undoRedoPerformed += OnUndoRedoPerformed;
			EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
		}

		private void OnDisable()
		{
			DisposeRecorder();
			ReleasePreviewTexture();

			SaveSettings(true);

			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
		}

		private void OnDestroy()
		{
			DiscardState();
		}

		private void Update()
		{
			if (Recording) Repaint();
		}

		private void OnGUI()
		{
			state.SerializedObject.Update();
			settings.SerializedObject.Update();

			Layout();

			state.SerializedObject.ApplyModifiedProperties();
			if (settings.SerializedObject.ApplyModifiedProperties())
			{
				SaveSettings();
			}
		}

		private void OnUndoRedoPerformed()
		{
			settings.SerializedObject.Update();

			SaveSettings();

			Repaint();
		}

		private void OnPlaymodeStateChanged()
		{
			if (!EditorApplication.isPlaying && Recording)
			{
				EndRecording();

				Repaint();
			}
		}

		#endregion

		#region Methods

		public void Reset()
		{
			if (Recorded)
			{
				recorder.Reset();

				ResetFrames();
			}
		}

		public void BeginRecording()
		{
			Camera camera = SelectCamera();
			if (Recording || !camera) throw new InvalidOperationException();

			Reset();

			UpdateRecorder();

			recorder.Camera = camera;
			recorder.UpdateSettings(settings, camera);
			recorder.BeginRecording();
		}

		public void EndRecording()
		{
			if (!Recording) throw new InvalidOperationException();

			recorder.EndRecording();
			recorder.Camera = null;
		}

		public void Save()
		{
			if (Recorded)
			{
				string title = "Save Movie";
				string directory = Application.dataPath;
				string defaultName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + recorder.Encoder.Extension;
				string extension = recorder.Encoder.Extension.Substring(1);

				string path = EditorUtility.SaveFilePanel(title, directory, defaultName, extension);
				if (!string.IsNullOrEmpty(path))
				{
					recorder.Save(path);
				}
			}
		}

		public int GetExpectedFileSize()
		{
			if (Seekable)
			{
				int begin = BeginFrame;
				int end = EndFrame;
				int size = end - begin;
				if (size > 0)
				{
					return recorder.GetExpectedFileSize(begin, end);
				}
			}

			return 0;
		}

		public void EraseFrame()
		{
			if (Editable)
			{
				int current = CurrentFrame;
				int begin = BeginFrame;
				int end = EndFrame;
				int size = end - begin;
				if (size > 0)
				{
					recorder.EraseFrame(begin, end);

					CurrentFrame = (current < begin ? current : current < end ? begin : current - size);
					BeginFrame = begin;
					EndFrame = begin + 1;
				}
			}
		}

		public void ClipFrame()
		{
			if (Editable)
			{
				int current = CurrentFrame;
				int begin = BeginFrame;
				int end = EndFrame;
				int size = end - begin;
				if (size > 0)
				{
					if (endFrame != LastFrame) recorder.EraseFrame(end, LastFrame);
					if (beginFrame != 0) recorder.EraseFrame(0, begin);

					CurrentFrame = (current <= begin ? 0 : current >= end ? size - 1 : current - begin);
					BeginFrame = 0;
					EndFrame = size;
				}
			}
		}

		private Camera SelectCamera()
		{
			switch (settings.CameraMode)
			{
				case MovieCameraMode.Custom:
					return state.TargetCamera;
				case MovieCameraMode.Main:
					return Camera.main;
				case MovieCameraMode.Scene:
					return FindSceneCamera();
				default:
					throw new InvalidOperationException();
			}
		}

		private Camera FindSceneCamera()
		{
			SceneView sceneView = SceneView.lastActiveSceneView;
			if (sceneView)
			{
				return sceneView.camera;
			}

			Camera[] cameras = SceneView.GetAllSceneCameras();
			if (cameras.Length != 0)
			{
				return cameras[0];
			}

			return null;
		}

		private void UpdateRecorder()
		{
			if (recorder != null)
			{
				if (recorder.EncoderType == settings.EncoderType) return;

				DisposeRecorder();
			}

			switch (settings.EncoderType)
			{
				case EncoderType.Gif:
					recorder = new EditorGifRecorder();
					break;
				case EncoderType.MP4:
					recorder = new EditorMP4Recorder();
					break;
				default:
					throw new InvalidOperationException();
			}
		}

		private void DisposeRecorder()
		{
			if (Recording) EndRecording();

			if (recorder != null)
			{
				recorder.Dispose();

				recorder = null;
			}
		}

		private void InitializeState()
		{
			if (!state)
			{
				state = CreateInstance<FrameCapturerWindowState>();
			}
		}

		private void DiscardState()
		{
			if (state)
			{
				DestroyImmediate(state);
				state = null;
			}
		}

		private void LoadSettings()
		{
			FrameCapturerWindowSettings.Load(out settings, PreferenceKey);
		}

		private void SaveSettings(bool dispose = false)
		{
			FrameCapturerWindowSettings.Save(ref settings, PreferenceKey, dispose);
		}

		private RenderTexture GetPreviewTexture(RenderTexture buffer)
		{
			bool useCache = false;
			if (previewTexture)
			{
				bool created = previewTexture.IsCreated();
				bool resized = (previewTexture.width != buffer.width || previewTexture.height != buffer.height);

				if (created && !resized)
				{
					useCache = true;
				}
				else
				{
					ReleasePreviewTexture();
				}
			}

			if (!useCache)
			{
				RenderTexture texture = new RenderTexture(buffer.width, buffer.height, 0, RenderTextureFormat.ARGB32);
				texture.wrapMode = TextureWrapMode.Repeat;
				texture.Create();

				texture.hideFlags = HideFlags.DontSave;

				this.previewTexture = texture;
			}

			return previewTexture;
		}

		private void ReleasePreviewTexture()
		{
			if (previewTexture)
			{
				previewTexture.Release();
				DestroyImmediate(previewTexture);
			}

			previewTexture = null;
		}

		private RenderTexture UpdatePreviewTexture()
		{
			RenderTexture buffer = recorder.RecordingUnit.ScratchBuffer;
			if (buffer != null)
			{
				RenderTexture texture;
				if (Recording || !recorder.Encoder.Seekable)
				{
					texture = buffer;
				}
				else
				{
					texture = GetPreviewTexture(buffer);

					RenderPreviewTexture(texture, CurrentFrame);
				}

				return texture;
			}

			return null;
		}

		private void RenderPreviewTexture(RenderTexture texture, int frame)
		{
			if (recorder != null && recorder.Seekable)
			{
				recorder.GetFrameData(texture, frame);
			}
		}

		private void ResetFrames()
		{
			this.currentFrame = LastFrame;
			this.beginFrame = 0;
			this.endFrame = LastFrame;
		}

		private static Type[] FindNextWindowTypes()
		{
			return typeof(EditorWindow).Assembly.GetTypes().Where(type => Array.IndexOf(desiredNextWindowNames, type.Name) != -1).ToArray();
		}

		#endregion
	}
}
