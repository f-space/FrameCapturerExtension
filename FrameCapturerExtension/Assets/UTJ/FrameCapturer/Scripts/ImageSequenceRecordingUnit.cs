using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace UTJ
{
	public abstract class ImageSequenceRecordingUnit<T> : RecordingUnitBase<T>, IImageSequenceRecordingUnit<T> where T : IImageSequenceEncoder
	{
		#region Constants

		private const string DefaultCommandBufferDescription = "ImageSequenceRecordingUnit: Copy FrameBuffer";

		private const string DefaultGCommandBufferDescription = "ImageSequenceRecordingUnit: Copy G-Buffer";

		private const CameraEvent FrameBufferTargetCameraEvent = CameraEvent.AfterImageEffects;

		private const CameraEvent GBufferTargetCameraEvent = CameraEvent.BeforeLighting;

		#endregion

		#region Fields

		private readonly string description;

		private readonly string gdescription;

		private Camera camera;

		private CommandBuffer commandBuffer;

		private CommandBuffer gcommandBuffer;

		private RenderTexture buffer;

		private RenderTexture[] gbuffer;

		#endregion

		#region Properties

		public string FrameBufferDescription { get { return description; } }

		public string GBufferDescription { get { return gdescription; } }

		public Camera Camera
		{
			get { return camera; }
			set { if (!Recording) camera = value; }
		}

		public RenderTexture FrameBuffer { get { return buffer; } }

		public RenderTexture[] GBuffer { get { return gbuffer; } }

		public bool Recording { get { return encoder.Recording; } }

		IImageSequenceEncoder IImageSequenceRecordingUnit.Encoder { get { return Encoder; } }

		Type IImageSequenceRecordingUnit.EncoderType { get { return typeof(T); } }

		protected abstract int GBufferSize { get; }

		#endregion

		#region Constructors

		public ImageSequenceRecordingUnit(T encoder, bool autoDisposeEncoder = false, string description = null, string gdescription = null)
			: base(encoder, autoDisposeEncoder)
		{
			this.description = description ?? DefaultCommandBufferDescription;
			this.gdescription = gdescription ?? DefaultGCommandBufferDescription;

			if (!encoder.Initialized) encoder.Initialize();
		}

		#endregion

		#region Methods

		public void BeginRecording()
		{
			if (encoder.Recording || !camera) return;

			bool offscreen = (camera && camera.targetTexture != null);
			CreateQuadMesh();
			CreateCopyMaterial(offscreen);
			UpdateFrameBuffer();
			UpdateGBuffer();

			CreateCommandBuffers();
			AttachCommandBuffers();

			encoder.BeginRecording();
		}

		public void EndRecording()
		{
			if (!encoder.Recording) return;

			encoder.EndRecording();

			DetachCommandBuffers();
			ReleaseCommandBuffers();
		}

		public void ExportFrameBuffer(string path, int number)
		{
			Assert.IsTrue(encoder.Recording);

			if (encoder.CaptureFrameBuffer && buffer != null)
			{
				encoder.ExportFrameBuffer(buffer, path, number);
			}
		}

		public void ExportGBuffer(string path, int number)
		{
			Assert.IsTrue(encoder.Recording);

			if (encoder.CaptureGBuffer && gbuffer != null)
			{
				encoder.ExportGBuffer(gbuffer, path, number);
			}
		}

		public override void ReleaseResources()
		{
			Assert.IsFalse(encoder.Recording);

			base.ReleaseResources();

			ReleaseAllBuffers();
		}

		void IImageSequenceRecordingUnit.Export(string path, int number)
		{
			ExportFrameBuffer(path, number);
			ExportGBuffer(path, number);
		}

		protected virtual RenderTexture CreateFrameBuffer(int width, int height)
		{
			RenderTexture buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
			buffer.wrapMode = TextureWrapMode.Repeat;
			buffer.Create();

			return buffer;
		}

		protected virtual CommandBuffer CreateCommandBufferForFrameBuffer(string name, RenderTexture destination)
		{
			int tid = Shader.PropertyToID("_TmpFrameBuffer");

			CommandBuffer commands = new CommandBuffer();
			commands.name = name;
			commands.GetTemporaryRT(tid, -1, -1, 0, FilterMode.Point);
			commands.Blit(BuiltinRenderTextureType.CurrentActive, tid);
			commands.SetRenderTarget(destination);
			commands.DrawMesh(QuadMesh, Matrix4x4.identity, CopyMaterial, 0, 0);
			commands.ReleaseTemporaryRT(tid);

			return commands;
		}

		protected abstract RenderTexture CreateGBuffer(int index, int width, int height);

		protected abstract CommandBuffer CreateCommandBufferForGBuffer(string name, RenderTexture[] destinations);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (encoder.Recording) EndRecording();
			}

			base.Dispose(disposing);
		}

		private void UpdateFrameBuffer()
		{
			if (!encoder.CaptureFrameBuffer) return;

			int captureWidth = camera.pixelWidth;
			int captureHeight = camera.pixelHeight;

			if (RequireRegeneration(ref buffer, captureWidth, captureHeight))
			{
				buffer = DisposalHelper.Mark(CreateFrameBuffer(captureWidth, captureHeight));
			}
		}

		private void UpdateGBuffer()
		{
			if (!encoder.CaptureGBuffer) return;

			int captureWidth = camera.pixelWidth;
			int captureHeight = camera.pixelHeight;

			if (gbuffer == null)
			{
				gbuffer = new RenderTexture[GBufferSize];
			}

			for (int i = 0; i < gbuffer.Length; i++)
			{
				if (RequireRegeneration(ref gbuffer[i], captureWidth, captureHeight))
				{
					gbuffer[i] = DisposalHelper.Mark(CreateGBuffer(i, captureWidth, captureHeight));
				}
			}
		}

		private void CreateCommandBuffers()
		{
			if (encoder.CaptureFrameBuffer)
			{
				commandBuffer = CreateCommandBufferForFrameBuffer(description, buffer);
			}
			if (encoder.CaptureGBuffer)
			{
				gcommandBuffer = CreateCommandBufferForGBuffer(gdescription, gbuffer);
			}
		}

		private void ReleaseCommandBuffers()
		{
			if (commandBuffer != null)
			{
				commandBuffer.Release();
				commandBuffer = null;
			}
			if (gcommandBuffer != null)
			{
				gcommandBuffer.Release();
				gcommandBuffer = null;
			}
		}

		private void AttachCommandBuffers()
		{
			if (camera)
			{
				if (encoder.CaptureFrameBuffer && commandBuffer != null)
				{
					camera.AddCommandBuffer(FrameBufferTargetCameraEvent, commandBuffer);
				}
				if (encoder.CaptureGBuffer && gcommandBuffer != null)
				{
					camera.AddCommandBuffer(GBufferTargetCameraEvent, gcommandBuffer);
				}
			}
		}

		private void DetachCommandBuffers()
		{
			if (camera)
			{
				if (encoder.CaptureFrameBuffer && commandBuffer != null)
				{
					camera.RemoveCommandBuffer(FrameBufferTargetCameraEvent, commandBuffer);
				}
				if (encoder.CaptureGBuffer && gcommandBuffer != null)
				{
					camera.RemoveCommandBuffer(GBufferTargetCameraEvent, gcommandBuffer);
				}
			}
		}

		private void ReleaseAllBuffers()
		{
			DisposalHelper.Dispose(ref buffer);

			if (gbuffer != null)
			{
				for (int i = 0; i < gbuffer.Length; i++)
				{
					DisposalHelper.Dispose(ref gbuffer[i]);
				}

				gbuffer = null;
			}
		}

		private static bool RequireRegeneration(ref RenderTexture texture, int width, int height)
		{
			if (texture != null)
			{
				bool created = texture.IsCreated();
				bool resized = (texture.width != width || texture.height != height);

				if (created && !resized) return false;

				DisposalHelper.Dispose(ref texture);
			}

			return true;
		}

		#endregion
	}
}
