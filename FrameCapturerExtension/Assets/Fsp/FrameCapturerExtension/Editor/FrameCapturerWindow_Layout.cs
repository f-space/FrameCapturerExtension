using System;
using UnityEditor;
using UnityEngine;
using UTJ;

namespace Fsp.FrameCapaturerExtxension
{
	public partial class FrameCapturerWindow
	{
		#region Constants

		private const float PreviewAspect = 16.0f / 10.0f;

		#endregion

		#region Fields

		private GUIStyle titleStyle, recordButtonStyle;

		#endregion

		#region Methods

		private void Layout()
		{
			InitializeGUIStyles();

			DrawHeader();

			RenderPreview();
			RecordButtonGUI();
			SeekPanelGUI();
			CommandPanelGUI();
			SavePanelGUI();

			DrawHorizontalLine();

			EncoderSelectorGUI();
			CaptureSettingsGUI();
			switch (settings.EncoderType)
			{
				case EncoderType.Gif:
					GifSettingsGUI();
					break;
				case EncoderType.MP4:
					MP4SettingsGUI();
					break;
				default:
					throw new InvalidOperationException();
			}
		}

		private void InitializeGUIStyles()
		{
			if (titleStyle == null)
			{
				titleStyle = new GUIStyle(GUI.skin.label);
				titleStyle.fontSize = 16;
				titleStyle.padding = new RectOffset(20, 20, 10, 10);
				titleStyle.alignment = TextAnchor.MiddleCenter;
			}

			if (recordButtonStyle == null)
			{
				recordButtonStyle = new GUIStyle(GUI.skin.button);
				recordButtonStyle.fontSize = 16;
				recordButtonStyle.fontStyle = FontStyle.Bold;
				recordButtonStyle.fixedWidth = 80.0f;
				recordButtonStyle.fixedHeight = 30.0f;
				recordButtonStyle.margin = new RectOffset(0, 0, 10, 10);
				recordButtonStyle.alignment = TextAnchor.MiddleCenter;
			}
		}

		private void DrawHeader()
		{
			GUILayout.Box("Frame Capturer", titleStyle);
		}

		private void RenderPreview()
		{
			float width = EditorGUIUtility.currentViewWidth * 0.9f;
			float height = width / PreviewAspect;

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			Rect previewArea = GUILayoutUtility.GetRect(width, height);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			EditorGUI.DrawRect(previewArea, Color.black);
			if (Recorded)
			{
				RenderTexture texture = UpdatePreviewTexture();
				if (texture != null)
				{
					Rect imageArea;
					FitToRect(ref previewArea, texture.width, texture.height, out imageArea);
					FlipRectY(ref imageArea, out imageArea);

					EditorGUI.DrawPreviewTexture(imageArea, texture);
				}
			}
		}

		private void RecordButtonGUI()
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (!Recording)
			{
				recordButtonStyle.normal.textColor = Color.red;

				GUI.enabled = ReadyToRecord;
				if (GUILayout.Button("REC", recordButtonStyle)) BeginRecording();
				GUI.enabled = true;
			}
			else
			{
				recordButtonStyle.normal.textColor = Color.blue;

				if (GUILayout.Button("STOP", recordButtonStyle)) EndRecording();
			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		private void SeekPanelGUI()
		{
			GUI.enabled = Seekable;

			GUILayoutOption[] miniButtonOptions = { GUILayout.Width(20.0f), GUILayout.Height(20.0f) };
			string format = string.Format("{{0,{0}}}/{{1,{0}}} frames", FrameCount.ToString().Length);
			string info = string.Format(format, EndFrame - BeginFrame, FrameCount);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("[", miniButtonOptions)) BeginFrame = CurrentFrame;
			if (GUILayout.Button("<", miniButtonOptions)) CurrentFrame--;
			CurrentFrame = EditorGUILayout.IntSlider(CurrentFrame, 0, FrameCount - 1);
			if (GUILayout.Button(">", miniButtonOptions)) CurrentFrame++;
			if (GUILayout.Button("]", miniButtonOptions)) EndFrame = CurrentFrame + 1;
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10.0f);

			EditorGUILayout.BeginHorizontal();
			BeginFrame = EditorGUILayout.DelayedIntField(BeginFrame, GUILayout.MaxWidth(50.0f));
			GUILayout.Label(" - ");
			EndFrame = EditorGUILayout.DelayedIntField(EndFrame, GUILayout.MaxWidth(50.0f));
			GUILayout.Label(info);
			GUILayout.FlexibleSpace();
			GUILayout.Label(GetFileSizeLabel(GetExpectedFileSize()));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10.0f);

			GUI.enabled = true;
		}

		private void CommandPanelGUI()
		{
			EditorGUILayout.BeginHorizontal();

			GUI.enabled = Editable;
			if (GUILayout.Button("Erase")) EraseFrame();
			if (GUILayout.Button("Clip")) ClipFrame();
			GUI.enabled = true;

			GUILayout.Space(10.0f);

			GUI.enabled = Operatable;
			if (GUILayout.Button("Reset")) Reset();
			GUI.enabled = true;

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10.0f);
		}

		private void SavePanelGUI()
		{
			EditorGUILayout.Space();

			GUI.enabled = Operatable;
			EditorGUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Save", GUILayout.Width(100.0f))) Save();

			EditorGUILayout.EndHorizontal();
			GUI.enabled = true;
		}

		private void DrawHorizontalLine()
		{
			GUILayout.Space(10.0f);
			Rect splitter = GUILayoutUtility.GetRect(0.0f, 1.0f, GUILayout.ExpandWidth(true));
			EditorGUI.DrawRect(splitter, GUI.contentColor);
			GUILayout.Space(5.0f);
		}

		private void EncoderSelectorGUI()
		{
			string[] texts = { "GIF", "MP4" };
			settings.EncoderType = (EncoderType)GUILayout.SelectionGrid((int)settings.EncoderType, texts, texts.Length);

			EditorGUILayout.Space();
		}

		private void CaptureSettingsGUI()
		{
			EditorGUILayout.LabelField("Capture Settings");
			EditorGUI.indentLevel++;

			settings.CameraMode = (MovieCameraMode)EditorGUILayout.EnumPopup("Camera Mode", settings.CameraMode);
			if (settings.CameraMode == MovieCameraMode.Custom)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(state.TargetCameraProperty, new GUIContent("Target"));
				EditorGUI.indentLevel--;
			}

			settings.ResolutionMode = (MovieResolutionMode)EditorGUILayout.EnumPopup("Resolution Mode", settings.ResolutionMode);

			bool width, height;
			SelectResolutionParameters(settings.ResolutionMode, out width, out height);
			EditorGUI.indentLevel++;
			if (width) settings.ResolutionWidth = EditorGUILayout.DelayedIntField("Width", settings.ResolutionWidth);
			if (height) settings.ResolutionHeight = EditorGUILayout.DelayedIntField("Height", settings.ResolutionHeight);
			EditorGUI.indentLevel--;

			settings.FrameRateMode = (FrameRateMode)EditorGUILayout.EnumPopup("Frame Rate Mode", settings.FrameRateMode);
			settings.FrameRate = EditorGUILayout.DelayedIntField("Frame Rate (fps)", settings.FrameRate);
			settings.MinUpdateRate = EditorGUILayout.DelayedIntField("Min Update Rate", settings.MinUpdateRate);

			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
		}

		private void GifSettingsGUI()
		{
			EditorGUILayout.LabelField("GIF Settings");
			EditorGUI.indentLevel++;

			settings.GifColors = EditorGUILayoutPow2Slider("Number of Colors", settings.GifColors, 2, 256);
			settings.GifUseLocalPalette = EditorGUILayout.Toggle("Use Local Palette", settings.GifUseLocalPalette);

			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
		}

		private void MP4SettingsGUI()
		{
			EditorGUILayout.LabelField("MP4 Settings");
			EditorGUI.indentLevel++;

			settings.MP4CaptureVideo = EditorGUILayout.Toggle("Video Capture (OpenH264)", settings.MP4CaptureVideo);
			if (settings.MP4CaptureVideo)
			{
				EditorGUI.indentLevel++;
				settings.MP4VideoBitrate = EditorGUILayout.IntField("Bitrate", settings.MP4VideoBitrate);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.HelpBox("OpenH264 Video Codec provided by Cisco Systems, Inc.", MessageType.Info);

			settings.MP4CaptureAudio = EditorGUILayout.Toggle("Audio Capture (FAAC)", settings.MP4CaptureAudio);
			if (settings.MP4CaptureAudio)
			{
				EditorGUI.indentLevel++;
				settings.MP4AudioBitrate = EditorGUILayout.IntField("Bitrate", settings.MP4AudioBitrate);
				EditorGUI.indentLevel--;
			}

			EditorGUI.indentLevel--;

			if (settings.MP4CaptureAudio && settings.FrameRateMode == FrameRateMode.Constant)
			{
				EditorGUILayout.HelpBox("Audio capture with FrameRateMode.Constant will cause desync.", MessageType.Warning);
			}

			EditorGUILayout.Space();
		}

		private static int EditorGUILayoutPow2Slider(string label, int value, int leftValue, int rightValue, params GUILayoutOption[] options)
		{
			int result = value;
			int log2Value = (int)Mathf.Log(value, 2.0f);
			int log2Left = (int)Mathf.Log(leftValue, 2.0f);
			int log2Right = (int)Mathf.Log(rightValue, 2.0f);

			float fieldWidth = EditorGUIUtility.fieldWidth;
			float width = EditorGUIUtility.currentViewWidth;
			float height = EditorGUIUtility.singleLineHeight;
			GUIStyle style = EditorStyles.numberField;

			Rect position = GUILayoutUtility.GetRect(width, height, style, options);
			Rect area = EditorGUI.PrefixLabel(position, new GUIContent(label));
			Rect sliderPosition = new Rect(area.x, area.y, Mathf.Max(0.0f, area.width - (fieldWidth + 5.0f)), area.height);
			Rect intFieldPosition = new Rect(area.xMax - fieldWidth, area.y, fieldWidth, area.height);

			EditorGUI.BeginChangeCheck();
			int sliderValue = (1 << Mathf.RoundToInt(GUI.HorizontalSlider(sliderPosition, log2Value, log2Left, log2Right)));
			if (EditorGUI.EndChangeCheck())
			{
				result = sliderValue;
			}

			EditorGUI.BeginChangeCheck();
			int inputFieldValue = GUIDelayedIntField(intFieldPosition, value);
			if (EditorGUI.EndChangeCheck())
			{
				result = Mathf.Clamp(inputFieldValue, leftValue, rightValue);
			}

			return result;
		}

		private static int GUIDelayedIntField(Rect position, int value)
		{
			int result;

			return int.TryParse(GUI.TextField(position, value.ToString(), 16), out result) ? result : value;
		}

		private static void FitToRect(ref Rect rect, float width, float height, out Rect result)
		{
			float scaleX = rect.width / width;
			float scaleY = rect.height / height;

			float x, y, w, h;
			if (scaleX < scaleY)
			{
				w = rect.width;
				h = height * scaleX;
				x = rect.x;
				y = rect.y + (rect.height - h) * 0.5f;
			}
			else
			{
				w = width * scaleY;
				h = rect.height;
				x = rect.x + (rect.width - w) * 0.5f;
				y = rect.y;
			}

			result = new Rect(x, y, w, h);
		}

		private static void FlipRectY(ref Rect source, out Rect result)
		{
			float x = source.x;
			float y = source.y + source.height;
			float w = source.width;
			float h = -source.height;

			result = new Rect(x, y, w, h);
		}

		private static string GetFileSizeLabel(int bytes)
		{
			string[] suffix = { " B", "KB", "MB", "GB", "TB" };

			int size = bytes, unit = 0, rem = 0;
			while (size >= 1024)
			{
				rem = size % 1024;
				size /= 1024;
				unit++;
			}

			int frac = (rem * 20 + 1024) / 2048;
			if (frac >= 10)
			{
				frac = 0;
				size++;
				if (size >= 1024)
				{
					size = 0;
					unit++;
				}
			}

			while (unit >= suffix.Length)
			{
				size *= 1024;
				unit--;
			}

			return String.Format("{0,4}.{1} {2}", size, frac, suffix[unit]);
		}

		private static void SelectResolutionParameters(MovieResolutionMode mode, out bool width, out bool height)
		{
			switch (mode)
			{
				case MovieResolutionMode.Custom:
					width = true;
					height = true;
					break;
				case MovieResolutionMode.Width:
					width = true;
					height = false;
					break;
				case MovieResolutionMode.Height:
					width = false;
					height = true;
					break;
				default:
					width = false;
					height = false;
					break;
			}
		}

		#endregion
	}
}
