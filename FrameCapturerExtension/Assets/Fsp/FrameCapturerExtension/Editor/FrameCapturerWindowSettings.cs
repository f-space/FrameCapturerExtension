using UnityEditor;
using UnityEngine;
using UTJ;

namespace Fsp.FrameCapaturerExtxension
{
	internal class FrameCapturerWindowSettings : ScriptableObject, IGifRecorderSettings, IMP4RecorderSettings
	{
		#region Constants

		private const int MinMinUpdateRate = EditorMovieRecorder<IMovieEncoder>.MinMinUpdateRate;

		private const int MaxMinUpdateRate = EditorMovieRecorder<IMovieEncoder>.MaxMinUpdateRate;

		#endregion

		#region Fields

#pragma warning disable 0414

		[SerializeField]
		private EncoderType m_EncoderType = EncoderType.Gif;

		[SerializeField]
		private MovieCameraMode m_CameraMode = MovieCameraMode.Main;

		[SerializeField]
		private MovieResolutionMode m_ResolutionMode = MovieResolutionMode.Custom;

		[SerializeField]
		private int m_ResolutionWidth = MovieEncoderSettings.DefaultResolutionWidth;

		[SerializeField]
		private int m_ResolutionHeight = MovieEncoderSettings.DefaultResolutionHeight;

		[SerializeField]
		private FrameRateMode m_FrameRateMode = MovieEncoderSettings.DefaultFrameRateMode;

		[SerializeField]
		private int m_FrameRate = MovieEncoderSettings.DefaultFrameRate;

		[SerializeField]
		private int m_MinUpdateRate = 30;

		[SerializeField]
		private int m_GifColors = GifEncoderSettings.DefaultColors;

		[SerializeField]
		private bool m_GifUseLocalPalette = GifEncoderSettings.DefaultUseLocalPalette;

		[SerializeField]
		private bool m_MP4CaptureVideo = MP4EncoderSettings.DefaultCaptureVideo;

		[SerializeField]
		private bool m_MP4CaptureAudio = MP4EncoderSettings.DefaultCaptureAudio;

		[SerializeField]
		private int m_MP4VideoBitrate = MP4EncoderSettings.DefaultVideoBitrate;

		[SerializeField]
		private int m_MP4AudioBitrate = MP4EncoderSettings.DefaultAudioBitrate;

#pragma warning restore 0414

		private SerializedObject @this;

		private SerializedProperty propEncoderType;

		private SerializedProperty propCameraMode;

		private SerializedProperty propResolutionMode;

		private SerializedProperty propResolutionWidth;

		private SerializedProperty propResolutionHeight;

		private SerializedProperty propFrameRateMode;

		private SerializedProperty propFrameRate;

		private SerializedProperty propMinUpdateRate;

		private SerializedProperty propGifColors;

		private SerializedProperty propGifUseLocalPalette;

		private SerializedProperty propMP4CaptureVideo;

		private SerializedProperty propMP4CaptureAudio;

		private SerializedProperty propMP4VideoBitrate;

		private SerializedProperty propMP4AudioBitrate;

		#endregion

		#region Properties

		public SerializedObject SerializedObject { get { return @this; } }

		public EncoderType EncoderType
		{
			get { return (EncoderType)propEncoderType.intValue; }
			set { propEncoderType.intValue = (int)value; }
		}

		public MovieCameraMode CameraMode
		{
			get { return (MovieCameraMode)propCameraMode.intValue; }
			set { propCameraMode.intValue = (int)value; }
		}

		public MovieResolutionMode ResolutionMode
		{
			get { return (MovieResolutionMode)propResolutionMode.intValue; }
			set { propResolutionMode.intValue = (int)value; }
		}

		public int ResolutionWidth
		{
			get { return propResolutionWidth.intValue; }
			set { propResolutionWidth.intValue = Mathf.Clamp(value, MovieEncoderSettings.MinResolution, MovieEncoderSettings.MaxResolution); }
		}

		public int ResolutionHeight
		{
			get { return propResolutionHeight.intValue; }
			set { propResolutionHeight.intValue = Mathf.Clamp(value, MovieEncoderSettings.MinResolution, MovieEncoderSettings.MaxResolution); }
		}

		public FrameRateMode FrameRateMode
		{
			get { return (FrameRateMode)propFrameRateMode.intValue; }
			set { propFrameRateMode.intValue = (int)value; }
		}

		public int FrameRate
		{
			get { return propFrameRate.intValue; }
			set { propFrameRate.intValue = Mathf.Clamp(value, MovieEncoderSettings.MinFrameRate, MovieEncoderSettings.MaxFrameRate); }
		}

		public int MinUpdateRate
		{
			get { return propMinUpdateRate.intValue; }
			set { propMinUpdateRate.intValue = Mathf.Clamp(value, MinMinUpdateRate, MaxMinUpdateRate); }
		}

		public int GifColors
		{
			get { return propGifColors.intValue; }
			set { propGifColors.intValue = Mathf.Clamp(value, GifEncoderSettings.MinColors, GifEncoderSettings.MaxColors); }
		}

		public bool GifUseLocalPalette
		{
			get { return propGifUseLocalPalette.boolValue; }
			set { propGifUseLocalPalette.boolValue = value; }
		}

		public bool MP4CaptureVideo
		{
			get { return propMP4CaptureVideo.boolValue; }
			set { propMP4CaptureVideo.boolValue = value; }
		}

		public bool MP4CaptureAudio
		{
			get { return propMP4CaptureAudio.boolValue; }
			set { propMP4CaptureAudio.boolValue = value; }
		}

		public int MP4VideoBitrate
		{
			get { return propMP4VideoBitrate.intValue; }
			set { propMP4VideoBitrate.intValue = Mathf.Clamp(value, MP4EncoderSettings.MinVideoBitrate, MP4EncoderSettings.MaxVideoBitrate); }
		}

		public int MP4AudioBitrate
		{
			get { return propMP4AudioBitrate.intValue; }
			set { propMP4AudioBitrate.intValue = Mathf.Clamp(value, MP4EncoderSettings.MinAudioBitrate, MP4EncoderSettings.MaxAudioBitrate); }
		}

		#endregion

		#region Messages

		protected void OnEnable()
		{
			@this = new SerializedObject(this);

			propEncoderType = @this.FindProperty("m_EncoderType");
			propCameraMode = @this.FindProperty("m_CameraMode");
			propResolutionMode = @this.FindProperty("m_ResolutionMode");
			propResolutionWidth = @this.FindProperty("m_ResolutionWidth");
			propResolutionHeight = @this.FindProperty("m_ResolutionHeight");
			propFrameRateMode = @this.FindProperty("m_FrameRateMode");
			propFrameRate = @this.FindProperty("m_FrameRate");
			propMinUpdateRate = @this.FindProperty("m_MinUpdateRate");
			propGifColors = @this.FindProperty("m_GifColors");
			propGifUseLocalPalette = @this.FindProperty("m_GifUseLocalPalette");
			propMP4CaptureVideo = @this.FindProperty("m_MP4CaptureVideo");
			propMP4CaptureAudio = @this.FindProperty("m_MP4CaptureAudio");
			propMP4VideoBitrate = @this.FindProperty("m_MP4VideoBitrate");
			propMP4AudioBitrate = @this.FindProperty("m_MP4AudioBitrate");
		}

		protected void OnDisable()
		{
			Dispose(ref @this);

			Dispose(ref propEncoderType);
			Dispose(ref propCameraMode);
			Dispose(ref propResolutionMode);
			Dispose(ref propResolutionWidth);
			Dispose(ref propResolutionHeight);
			Dispose(ref propFrameRateMode);
			Dispose(ref propFrameRate);
			Dispose(ref propMinUpdateRate);
			Dispose(ref propGifColors);
			Dispose(ref propGifUseLocalPalette);
			Dispose(ref propMP4CaptureVideo);
			Dispose(ref propMP4CaptureAudio);
			Dispose(ref propMP4VideoBitrate);
			Dispose(ref propMP4AudioBitrate);
		}

		#endregion

		#region Methods

		public static bool Load(out FrameCapturerWindowSettings instance, string key)
		{
			return SerializationHelper.Load(out instance, key);
		}

		public static bool Save(ref FrameCapturerWindowSettings instance, string key, bool dispose)
		{
			return SerializationHelper.Save(ref instance, key, dispose);
		}

		private static void Dispose(ref SerializedObject @object)
		{
			if (@object != null)
			{
				@object.Dispose();
				@object = null;
			}
		}

		private static void Dispose(ref SerializedProperty property)
		{
			if (property != null)
			{
				property.Dispose();
				property = null;
			}
		}

		#endregion
	}
}
