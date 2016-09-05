using System;
using System.IO;
using UnityEngine;

namespace UTJ
{
	public sealed class MP4Encoder : IMovieEncoder
	{
		#region Constants

		private const string Extension = ".mp4";

		private const string RelativeFAACPackagePath = "/UTJ/FrameCapturer/FAAC_SelfBuild.zip";

		private const string RelativeModulePath = "/UTJ/FrameCapturer/Codec";

		#endregion

		#region Fields

		private MP4EncoderSettings settings;

		private fcAPI.fcMP4Context context;

		private fcAPI.fcStream stream;

		private string tempFilePath;

		private int channels;

		private int? eventID;

		private int videoFrameCount;

		private bool recording;

		private bool initialized;

		private bool disposed;

		#endregion

		#region Properties

		public MP4EncoderSettings Settings
		{
			get { return settings; }
			set { settings = value ?? new MP4EncoderSettings(); }
		}

		public bool Initialized { get { return initialized; } }

		public bool Recording { get { return recording; } }

		public int FrameCount { get { return videoFrameCount; } }

		public fcAPI.fcMP4Context MP4Context { get { return context; } }

		MovieEncoderSettings IMovieEncoder.Settings { get { return settings; } }

		bool IMovieEncoder.Seekable { get { return false; } }

		bool IMovieEncoder.Editable { get { return false; } }

		bool IMovieEncoder.CaptureVideo { get { return settings.CaptureVideo; } }

		bool IMovieEncoder.CaptureAudio { get { return settings.CaptureAudio; } }

		string IMovieEncoder.Extension { get { return Extension; } }

		private string FAACPackagePath
		{
			get { return Application.streamingAssetsPath + RelativeFAACPackagePath; }
		}

		private string ModulePath
		{
			get { return Application.persistentDataPath + RelativeModulePath; }
		}

		#endregion

		#region Constructors

		public MP4Encoder(MP4EncoderSettings settings = null)
		{
			Settings = settings;
		}

		~MP4Encoder()
		{
			if (!disposed)
			{
				Dispose(false);

				disposed = true;
			}
		}

		#endregion

		#region Methods

		public void Initialize()
		{
			Directory.CreateDirectory(ModulePath);

			fcAPI.fcMP4SetFAACPackagePath(FAACPackagePath);
			fcAPI.fcSetModulePath(ModulePath);
			fcAPI.fcMP4DownloadCodecBegin();

			initialized = true;
		}

		public void Reset()
		{
			if (recording) EndRecording();

			if (tempFilePath != null)
			{
				FileInfo file = new FileInfo(tempFilePath);
				if (file.Exists)
				{
					file.Delete();
				}

				tempFilePath = null;
			}

			videoFrameCount = 0;
		}

		public void BeginRecording()
		{
			if (recording) return;

			Reset();

			channels = GetAudioChannels();
			context = CreateContext(settings, channels);
			tempFilePath = GetTempFilePath();
			stream = CreateOutputStream(tempFilePath, context);

			recording = true;
		}

		public void EndRecording()
		{
			if (!recording) return;

			ReleaseUnmanagedResources();

			recording = false;
		}

		public void RecordImage(RenderTexture texture, double time)
		{
			if (!recording) return;

			double timestamp;
			switch (settings.FrameRateMode)
			{
				case FrameRateMode.Variable:
					timestamp = time;
					break;
				case FrameRateMode.Constant:
					timestamp = (double)videoFrameCount / settings.FrameRate;
					break;
				default:
					throw new InvalidOperationException();
			}

			eventID = fcAPI.fcMP4AddVideoFrameTexture(context, texture, timestamp, eventID ?? 0);
			GL.IssuePluginEvent(fcAPI.fcGetRenderEventFunc(), eventID ?? 0);

			videoFrameCount++;
		}

		public void RecordAudio(float[] samples, int channels)
		{
			if (!recording) return;

			if (channels != this.channels)
			{
				throw new InvalidOperationException("audio channels mismatch!");
			}

			fcAPI.fcMP4AddAudioFrame(context, samples, samples.Length);
		}

		public bool Flush(string path)
		{
			if (recording) return false;

			FileInfo file = new FileInfo(tempFilePath);
			if (file.Exists)
			{
				file.CopyTo(path);

				return true;
			}

			return false;
		}

		public void Dispose()
		{
			if (!disposed)
			{
				Dispose(true);
				GC.SuppressFinalize(this);

				disposed = true;
			}
		}

		private void Dispose(bool disposing)
		{
			Reset();
		}

		private void ReleaseUnmanagedResources()
		{
			fcAPI.fcGuard(() =>
			{
				if (eventID.HasValue)
				{
					fcAPI.fcEraseDeferredCall(eventID.Value);
					eventID = null;
				}

				if (context.ptr != IntPtr.Zero)
				{
					fcAPI.fcMP4DestroyContext(context);
					context.ptr = IntPtr.Zero;
				}

				if (stream.ptr != IntPtr.Zero)
				{
					fcAPI.fcDestroyStream(stream);
					stream.ptr = IntPtr.Zero;
				}
			});
		}

		private static fcAPI.fcMP4Context CreateContext(MP4EncoderSettings settings, int channels)
		{
			fcAPI.fcMP4Config config = fcAPI.fcMP4Config.default_value;
			config.video = settings.CaptureVideo;
			config.audio = settings.CaptureAudio;
			config.video_width = settings.ResolutionWidth;
			config.video_height = settings.ResolutionHeight;
			config.video_max_framerate = 60;
			config.video_bitrate = settings.VideoBitrate;
			config.audio_bitrate = settings.AudioBitrate;
			config.audio_sampling_rate = AudioSettings.outputSampleRate;
			config.audio_num_channels = channels;

			return fcAPI.fcMP4CreateContext(ref config);
		}

		private static fcAPI.fcStream CreateOutputStream(string path, fcAPI.fcMP4Context context)
		{
			fcAPI.fcStream stream = fcAPI.fcCreateFileStream(path);
			fcAPI.fcMP4AddOutputStream(context, stream);

			return stream;
		}

		private static int GetAudioChannels()
		{
			return fcAPI.fcGetNumAudioChannels();
		}

		private static string GetTempFilePath()
		{
			return Path.GetTempFileName().Replace(Path.DirectorySeparatorChar, '/');
		}

		#endregion
	}
}
