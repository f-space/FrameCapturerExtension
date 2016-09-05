using System;
using System.Text;
using UnityEngine;

namespace UTJ
{
	public sealed class PngEncoder : IImageSequenceEncoder
	{
		#region Constants

		private const string Extension = ".png";

		private const int GBuffers = 7;

		private const string NumberFormat = "0000";

		#endregion

		#region Fields

		private PngEncoderSettings settings;

		private fcAPI.fcPNGContext context;

		private int? frameBufferEventID;

		private int?[] gBufferEventIDs;

		private int?[] offscreenBufferEventIDs;

		private bool recording;

		private bool disposed;

		private StringBuilder pathBuffer;

		private static readonly string FrameBufferPrefix = "FrameBuffer_";

		private static readonly string[] GBufferPrefix =
		{
			"Albedo_",
			"Occlusion_",
			"Specular_",
			"Smoothness_",
			"Normal_",
			"Emission_",
			"Depth_",
		};

		private static readonly string OffscreenBufferName = "RenderTarget";

		#endregion

		#region Properties

		public PngEncoderSettings Settings
		{
			get { return settings; }
			set { settings = value ?? new PngEncoderSettings(); }
		}

		public bool CaptureFrameBuffer { get { return settings.CaptureFrameBuffer; } }

		public bool CaptureGBuffer { get { return settings.CaptureGBuffer; } }

		public bool CaptureOffscreenBuffer { get { return settings.CaptureOffscreenBuffer; } }

		public bool Recording { get { return recording; } }

		ImageSequenceEncoderSettings IImageSequenceEncoder.Settings { get { return settings; } }

		bool IImageSequenceEncoder.Initialized { get { return true; } }

		string IImageSequenceEncoder.Extension { get { return Extension; } }

		private StringBuilder PathBuffer { get { return pathBuffer ?? (pathBuffer = new StringBuilder()); } }

		#endregion

		#region Constructors

		public PngEncoder(PngEncoderSettings settings = null)
		{
			Settings = settings;
		}

		~PngEncoder()
		{
			if (!disposed)
			{
				Dispose(false);

				disposed = true;
			}
		}

		#endregion

		#region Methods

		public void BeginRecording()
		{
			if (recording) return;

			context = CreateContext();

			recording = true;
		}

		public void EndRecording()
		{
			if (!recording) return;

			ReleaseUnmanagedResources();

			recording = false;
		}

		public void ExportFrameBuffer(RenderTexture buffer, string path, int number)
		{
			if (!recording) return;

			string filePath = GetFilePath(PathBuffer, path, FrameBufferPrefix, number);

			ExportTexture(ref frameBufferEventID, context, filePath, buffer);
		}

		public void ExportGBuffer(RenderTexture[] gbuffer, string path, int number)
		{
			if (gbuffer.Length != GBuffers) throw new ArgumentOutOfRangeException("gbuffer");

			if (!recording) return;

			if (gBufferEventIDs == null)
			{
				gBufferEventIDs = new int?[GBuffers];
			}

			StringBuilder builder = PathBuffer;
			for (int i = 0; i < gbuffer.Length; i++)
			{
				string filePath = GetFilePath(builder, path, GBufferPrefix[i], number);

				ExportTexture(ref gBufferEventIDs[i], context, filePath, gbuffer[i]);
			}
		}

		public void ExportOffscreenBuffer(RenderTexture[] buffers, string path, int number)
		{
			if (!recording) return;

			if (offscreenBufferEventIDs == null || offscreenBufferEventIDs.Length != buffers.Length)
			{
				offscreenBufferEventIDs = new int?[buffers.Length];
			}

			StringBuilder builder = PathBuffer;
			for (int i = 0; i < buffers.Length; i++)
			{
				string prefix = GetOffscreenBufferPrefix(builder, OffscreenBufferName, i);
				string filePath = GetFilePath(builder, path, prefix, number);

				ExportTexture(ref offscreenBufferEventIDs[i], context, filePath, buffers[i]);
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

		void IImageSequenceEncoder.Initialize() { }

		private void Dispose(bool disposing)
		{
			if (recording) EndRecording();
		}

		private void ReleaseUnmanagedResources()
		{
			if (context.ptr != IntPtr.Zero)
			{
				fcAPI.fcGuard(() =>
				{
					EraseCallback(ref frameBufferEventID);
					EraseCallbacks(ref gBufferEventIDs);
					EraseCallbacks(ref offscreenBufferEventIDs);

					if (context.ptr != IntPtr.Zero)
					{
						fcAPI.fcPngDestroyContext(context);
						context.ptr = IntPtr.Zero;
					}
				});
			}
		}

		private static fcAPI.fcPNGContext CreateContext()
		{
			fcAPI.fcPngConfig config = fcAPI.fcPngConfig.default_value;

			return fcAPI.fcPngCreateContext(ref config);
		}

		private static void ExportTexture(ref int? eventID, fcAPI.fcPNGContext context, string path, RenderTexture texture)
		{
			eventID = fcAPI.fcPngExportTexture(context, path, texture, eventID ?? 0);
			GL.IssuePluginEvent(fcAPI.fcGetRenderEventFunc(), eventID ?? 0);
		}

		private static void EraseCallback(ref int? eventID)
		{
			if (eventID.HasValue)
			{
				fcAPI.fcEraseDeferredCall(eventID.Value);
				eventID = null;
			}
		}

		private static void EraseCallbacks(ref int?[] eventIDs)
		{
			if (eventIDs != null)
			{
				for (int i = 0; i < eventIDs.Length; i++)
				{
					EraseCallback(ref eventIDs[i]);
				}
				eventIDs = null;
			}
		}

		private static string GetFilePath(StringBuilder buffer, string path, string prefix, int number)
		{
			buffer.Length = 0;
			buffer.Append(path);
			buffer.Append('/');
			buffer.Append(prefix);
			buffer.Append(number.ToString(NumberFormat));
			buffer.Append(Extension);

			return buffer.ToString();
		}

		private static string GetOffscreenBufferPrefix(StringBuilder buffer, string name, int index)
		{
			buffer.Length = 0;
			buffer.Append(name);
			buffer.Append('[');
			buffer.Append(index);
			buffer.Append(']');
			buffer.Append('_');

			return buffer.ToString();
		}

		#endregion
	}
}
