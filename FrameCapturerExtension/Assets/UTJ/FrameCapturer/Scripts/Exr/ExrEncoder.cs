using System;
using System.Text;
using UnityEngine;

namespace UTJ
{
	public sealed class ExrEncoder : IImageSequenceEncoder
	{
		#region Constants

		private const string Extension = ".exr";

		private const int GBuffers = 5;

		private const string NumberFormat = "0000";

		#endregion

		#region Fields

		private ExrEncoderSettings settings;

		private fcAPI.fcEXRContext context;

		private int?[] frameBufferEventIDs;

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

		public ExrEncoderSettings Settings
		{
			get { return settings; }
			set { settings = value ?? new ExrEncoderSettings(); }
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

		public ExrEncoder(ExrEncoderSettings settings = null)
		{
			Settings = settings;
		}

		~ExrEncoder()
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

			if (frameBufferEventIDs == null)
			{
				frameBufferEventIDs = new int?[5];
			}

			string filePath = GetFilePath(PathBuffer, path, FrameBufferPrefix, number);

			int?[] eventIDs = frameBufferEventIDs;
			eventIDs[0] = fcAPI.fcExrBeginFrame(context, filePath, buffer.width, buffer.height, eventIDs[0] ?? 0);
			eventIDs[1] = fcAPI.fcExrAddLayerTexture(context, buffer, 0, "R", eventIDs[1] ?? 0);
			eventIDs[2] = fcAPI.fcExrAddLayerTexture(context, buffer, 1, "G", eventIDs[2] ?? 0);
			eventIDs[3] = fcAPI.fcExrAddLayerTexture(context, buffer, 2, "B", eventIDs[3] ?? 0);
			eventIDs[4] = fcAPI.fcExrEndFrame(context, eventIDs[4] ?? 0);

			for (int i = 0; i < eventIDs.Length; i++)
			{
				GL.IssuePluginEvent(fcAPI.fcGetRenderEventFunc(), eventIDs[i] ?? 0);
			}
		}

		public void ExportGBuffer(RenderTexture[] gbuffer, string path, int number)
		{
			if (gbuffer.Length != GBuffers) throw new ArgumentOutOfRangeException("gbuffer");

			if (!recording) return;

			if (gBufferEventIDs == null)
			{
				gBufferEventIDs = new int?[29];
			}

			int?[] eventIDs = gBufferEventIDs;
			StringBuilder builder = PathBuffer;
			{
				string filePath = GetFilePath(builder, path, GBufferPrefix[0], number);
				RenderTexture texture = gbuffer[0];
				eventIDs[0] = fcAPI.fcExrBeginFrame(context, filePath, texture.width, texture.height, eventIDs[0] ?? 0);
				eventIDs[1] = fcAPI.fcExrAddLayerTexture(context, texture, 0, "R", eventIDs[1] ?? 0);
				eventIDs[2] = fcAPI.fcExrAddLayerTexture(context, texture, 1, "G", eventIDs[2] ?? 0);
				eventIDs[3] = fcAPI.fcExrAddLayerTexture(context, texture, 2, "B", eventIDs[3] ?? 0);
				eventIDs[4] = fcAPI.fcExrEndFrame(context, eventIDs[4] ?? 0);
			}
			{
				string filePath = GetFilePath(builder, path, GBufferPrefix[1], number);
				RenderTexture texture = gbuffer[0];
				eventIDs[5] = fcAPI.fcExrBeginFrame(context, filePath, texture.width, texture.height, eventIDs[5] ?? 0);
				eventIDs[6] = fcAPI.fcExrAddLayerTexture(context, texture, 3, "R", eventIDs[6] ?? 0);
				eventIDs[7] = fcAPI.fcExrEndFrame(context, eventIDs[7] ?? 0);
			}
			{
				string filePath = GetFilePath(builder, path, GBufferPrefix[2], number);
				RenderTexture texture = gbuffer[1];
				eventIDs[8] = fcAPI.fcExrBeginFrame(context, filePath, texture.width, texture.height, eventIDs[8] ?? 0);
				eventIDs[9] = fcAPI.fcExrAddLayerTexture(context, texture, 0, "R", eventIDs[9] ?? 0);
				eventIDs[10] = fcAPI.fcExrAddLayerTexture(context, texture, 1, "G", eventIDs[10] ?? 0);
				eventIDs[11] = fcAPI.fcExrAddLayerTexture(context, texture, 2, "B", eventIDs[11] ?? 0);
				eventIDs[12] = fcAPI.fcExrEndFrame(context, eventIDs[12] ?? 0);
			}
			{
				string filePath = GetFilePath(builder, path, GBufferPrefix[3], number);
				RenderTexture texture = gbuffer[1];
				eventIDs[13] = fcAPI.fcExrBeginFrame(context, filePath, texture.width, texture.height, eventIDs[13] ?? 0);
				eventIDs[14] = fcAPI.fcExrAddLayerTexture(context, texture, 3, "R", eventIDs[14] ?? 0);
				eventIDs[15] = fcAPI.fcExrEndFrame(context, eventIDs[15] ?? 0);
			}
			{
				string filePath = GetFilePath(builder, path, GBufferPrefix[4], number);
				RenderTexture texture = gbuffer[2];
				eventIDs[16] = fcAPI.fcExrBeginFrame(context, filePath, texture.width, texture.height, eventIDs[16] ?? 0);
				eventIDs[17] = fcAPI.fcExrAddLayerTexture(context, texture, 0, "R", eventIDs[17] ?? 0);
				eventIDs[18] = fcAPI.fcExrAddLayerTexture(context, texture, 1, "G", eventIDs[18] ?? 0);
				eventIDs[19] = fcAPI.fcExrAddLayerTexture(context, texture, 2, "B", eventIDs[19] ?? 0);
				eventIDs[20] = fcAPI.fcExrEndFrame(context, eventIDs[20] ?? 0);
			}
			{
				string filePath = GetFilePath(builder, path, GBufferPrefix[5], number);
				RenderTexture texture = gbuffer[3];
				eventIDs[21] = fcAPI.fcExrBeginFrame(context, filePath, texture.width, texture.height, eventIDs[21] ?? 0);
				eventIDs[22] = fcAPI.fcExrAddLayerTexture(context, texture, 0, "R", eventIDs[22] ?? 0);
				eventIDs[23] = fcAPI.fcExrAddLayerTexture(context, texture, 1, "G", eventIDs[23] ?? 0);
				eventIDs[24] = fcAPI.fcExrAddLayerTexture(context, texture, 2, "B", eventIDs[24] ?? 0);
				eventIDs[25] = fcAPI.fcExrEndFrame(context, eventIDs[25] ?? 0);
			}
			{
				string filePath = GetFilePath(builder, path, GBufferPrefix[6], number);
				RenderTexture texture = gbuffer[4];
				eventIDs[26] = fcAPI.fcExrBeginFrame(context, filePath, texture.width, texture.height, eventIDs[26] ?? 0);
				eventIDs[27] = fcAPI.fcExrAddLayerTexture(context, texture, 0, "R", eventIDs[27] ?? 0);
				eventIDs[28] = fcAPI.fcExrEndFrame(context, eventIDs[28] ?? 0);
			}

			for (int i = 0; i < eventIDs.Length; i++)
			{
				GL.IssuePluginEvent(fcAPI.fcGetRenderEventFunc(), eventIDs[i] ?? 0);
			}
		}

		public void ExportOffscreenBuffer(RenderTexture[] buffers, string path, int number)
		{
			if (!recording) return;

			int totalCallbacks = buffers.Length * 6;
			if (offscreenBufferEventIDs == null || offscreenBufferEventIDs.Length != totalCallbacks)
			{
				offscreenBufferEventIDs = new int?[totalCallbacks];
			}

			int?[] eventIDs = offscreenBufferEventIDs;
			StringBuilder builder = PathBuffer;
			for (int i = 0; i < buffers.Length; i++)
			{
				string prefix = GetOffscreenBufferPrefix(builder, OffscreenBufferName, i);
				string filePath = GetFilePath(builder, path, prefix, number);
				RenderTexture texture = buffers[i];

				int offset = i * 6;
				eventIDs[offset + 0] = fcAPI.fcExrBeginFrame(context, filePath, texture.width, texture.height, eventIDs[offset + 0] ?? 0);
				eventIDs[offset + 1] = fcAPI.fcExrAddLayerTexture(context, texture, 0, "R", eventIDs[offset + 1] ?? 0);
				eventIDs[offset + 2] = fcAPI.fcExrAddLayerTexture(context, texture, 1, "G", eventIDs[offset + 2] ?? 0);
				eventIDs[offset + 3] = fcAPI.fcExrAddLayerTexture(context, texture, 2, "B", eventIDs[offset + 3] ?? 0);
				eventIDs[offset + 4] = fcAPI.fcExrAddLayerTexture(context, texture, 3, "A", eventIDs[offset + 4] ?? 0);
				eventIDs[offset + 5] = fcAPI.fcExrEndFrame(context, eventIDs[offset + 5] ?? 0);
			}

			for (int i = 0; i < eventIDs.Length; ++i)
			{
				GL.IssuePluginEvent(fcAPI.fcGetRenderEventFunc(), eventIDs[i] ?? 0);
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
					EraseCallbacks(ref frameBufferEventIDs);
					EraseCallbacks(ref gBufferEventIDs);
					EraseCallbacks(ref offscreenBufferEventIDs);

					if (context.ptr != IntPtr.Zero)
					{
						fcAPI.fcExrDestroyContext(context);
						context.ptr = IntPtr.Zero;
					}
				});
			}
		}

		private static fcAPI.fcEXRContext CreateContext()
		{
			fcAPI.fcExrConfig config = fcAPI.fcExrConfig.default_value;

			return fcAPI.fcExrCreateContext(ref config);
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
