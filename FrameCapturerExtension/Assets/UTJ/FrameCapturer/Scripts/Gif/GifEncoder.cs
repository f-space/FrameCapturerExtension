using System;
using UnityEngine;

namespace UTJ
{
	public sealed class GifEncoder : IEditableMovieEncoder
	{
		#region Constants

		private const string Extension = ".gif";

		#endregion

		#region Fields

		private GifEncoderSettings settings;

		private fcAPI.fcGIFContext context;

		private int? eventID;

		private int videoFrameCount;

		private bool recording;

		private bool disposed;

		#endregion

		#region Properties

		public GifEncoderSettings Settings
		{
			get { return settings; }
			set { settings = value ?? new GifEncoderSettings(); }
		}

		public bool Recording { get { return recording; } }

		public int FrameCount { get { return (!recording && context.ptr != IntPtr.Zero ? fcAPI.fcGifGetFrameCount(context) : videoFrameCount); } }

		public fcAPI.fcGIFContext GIFContext { get { return context; } }

		MovieEncoderSettings IMovieEncoder.Settings { get { return settings; } }

		bool IMovieEncoder.Seekable { get { return true; } }

		bool IMovieEncoder.Editable { get { return true; } }

		bool IMovieEncoder.CaptureVideo { get { return true; } }

		bool IMovieEncoder.CaptureAudio { get { return false; } }

		bool IMovieEncoder.Initialized { get { return true; } }

		string IMovieEncoder.Extension { get { return Extension; } }

		#endregion

		#region Constructors

		public GifEncoder(GifEncoderSettings settings = null)
		{
			Settings = settings;
		}

		~GifEncoder()
		{
			if (!disposed)
			{
				Dispose(false);

				disposed = true;
			}
		}

		#endregion

		#region Methods

		public void Reset()
		{
			if (recording) EndRecording();

			ReleaseContext();

			videoFrameCount = 0;
		}

		public void BeginRecording()
		{
			if (recording) return;

			Reset();

			context = CreateContext(settings);

			recording = true;
		}

		public void EndRecording()
		{
			if (!recording) return;

			EraseCallback();

			recording = false;
		}

		public void RecordImage(RenderTexture texture, double time)
		{
			if (!recording) return;

			bool localPalette = settings.UseLocalPalette;

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

			eventID = fcAPI.fcGifAddFrameTexture(context, texture, localPalette, timestamp, eventID ?? 0);
			GL.IssuePluginEvent(fcAPI.fcGetRenderEventFunc(), eventID ?? 0);

			videoFrameCount++;
		}

		public bool Flush(string path)
		{
			return Flush(path, 0, -1);
		}

		public bool Flush(string path, int beginFrame, int endFrame)
		{
			bool result = false;
			if (context.ptr != IntPtr.Zero)
			{
				fcAPI.fcGuard(() =>
				{
					int frameCount = fcAPI.fcGifGetFrameCount(context);
					if (CheckRange(beginFrame, endFrame, frameCount))
					{
						result = fcAPI.fcGifWriteFile(context, path, beginFrame, endFrame);
					}
				});
			}

			return result;
		}

		public int GetExpectedFileSize(int beginFrame, int endFrame)
		{
			if (context.ptr != IntPtr.Zero)
			{
				int frameCount = fcAPI.fcGifGetFrameCount(context);
				if (CheckRange(beginFrame, endFrame, frameCount))
				{
					return fcAPI.fcGifGetExpectedDataSize(context, beginFrame, endFrame);
				}
			}

			return -1;
		}

		public void GetFrameData(RenderTexture texture, int frame)
		{
			if (context.ptr != IntPtr.Zero)
			{
				int frameCount = fcAPI.fcGifGetFrameCount(context);
				if (frame >= 0 && frame < frameCount)
				{
					fcAPI.fcGifGetFrameData(context, texture.GetNativeTexturePtr(), frame);
				}
			}
		}

		public void EraseFrame(int beginFrame, int endFrame)
		{
			if (recording) return;

			if (context.ptr != IntPtr.Zero)
			{
				int frameCount = fcAPI.fcGifGetFrameCount(context);
				if (CheckRange(beginFrame, endFrame, frameCount))
				{
					fcAPI.fcGifEraseFrame(context, beginFrame, endFrame);
				}
			}
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

		void IMovieEncoder.Initialize() { }

		void IMovieEncoder.RecordAudio(float[] samples, int channels) { throw new NotSupportedException(); }

		private void Dispose(bool disposing)
		{
			Reset();
		}

		private void EraseCallback()
		{
			if (eventID.HasValue)
			{
				fcAPI.fcGuard(() =>
				{
					if (eventID.HasValue)
					{
						fcAPI.fcEraseDeferredCall(eventID.Value);
						eventID = null;
					}
				});
			}
		}

		private void ReleaseContext()
		{
			if (context.ptr != IntPtr.Zero)
			{
				fcAPI.fcGuard(() =>
				{
					if (context.ptr != IntPtr.Zero)
					{
						fcAPI.fcGifDestroyContext(context);
						context.ptr = IntPtr.Zero;
					}
				});
			}
		}

		private static fcAPI.fcGIFContext CreateContext(GifEncoderSettings settings)
		{
			fcAPI.fcGifConfig config;
			config.width = settings.ResolutionWidth;
			config.height = settings.ResolutionHeight;
			config.num_colors = settings.Colors;
			config.max_active_tasks = 0;

			return fcAPI.fcGifCreateContext(ref config);
		}

		private static bool CheckRange(int beginFrame, int endFrame, int frameCount)
		{
			return (beginFrame >= 0 && (endFrame != -1
				? (beginFrame < endFrame && endFrame <= frameCount)
				: (beginFrame < frameCount)));
		}

		#endregion
	}
}
