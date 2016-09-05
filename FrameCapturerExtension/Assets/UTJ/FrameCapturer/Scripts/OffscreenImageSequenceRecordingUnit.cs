using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace UTJ
{
	public class OffscreenImageSequenceRecordingUnit<T> : RecordingUnitBase<T>, IImageSequenceRecordingUnit<T> where T : IImageSequenceEncoder
	{
		#region Constants

		private const string DefaultCommandBufferDescription = "OffscreenImageSequenceRecordingUnit: Copy";

		private const CameraEvent TargetCameraEvent = CameraEvent.AfterImageEffects;

		#endregion

		#region Fields

		private readonly string description;

		private Camera camera;

		private RenderTexture[] targets;

		private RenderTexture[] scratchBuffers;

		private CommandBuffer commandBuffer;

		#endregion

		#region Properties

		public string Description { get { return description; } }

		public Camera Camera
		{
			get { return camera; }
			set { if (!Recording) camera = value; }
		}

		public RenderTexture[] Targets
		{
			get { return targets; }
			set { targets = value; }
		}

		public RenderTexture[] ScratchBuffers { get { return scratchBuffers; } }

		public bool Recording { get { return encoder.Recording; } }

		IImageSequenceEncoder IImageSequenceRecordingUnit.Encoder { get { return Encoder; } }

		Type IImageSequenceRecordingUnit.EncoderType { get { return typeof(T); } }

		#endregion

		#region Constructors

		public OffscreenImageSequenceRecordingUnit(T encoder, bool autoDisposeEncoder = false, string description = null)
			: base(encoder, autoDisposeEncoder)
		{
			this.description = description ?? DefaultCommandBufferDescription;

			if (!encoder.Initialized) encoder.Initialize();
		}

		#endregion

		#region Methods

		public void BeginRecording()
		{
			if (encoder.Recording || !camera || targets == null) return;

			bool offscreen = true;
			CreateQuadMesh();
			CreateCopyMaterial(offscreen);
			UpdateScratchBuffers();

			CreateCommandBuffer();
			AttachCommandBuffer();

			encoder.BeginRecording();
		}

		public void EndRecording()
		{
			if (!encoder.Recording) return;

			encoder.EndRecording();

			DetachCommandBuffer();
			ReleaseCommandBuffer();
		}

		public void ExportOffscreenBuffer(string path, int number)
		{
			Assert.IsTrue(encoder.Recording);

			if (encoder.CaptureOffscreenBuffer && scratchBuffers != null)
			{
				encoder.ExportOffscreenBuffer(scratchBuffers, path, number);
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
			ExportOffscreenBuffer(path, number);
		}

		protected virtual RenderTexture CreateScratchBuffer(RenderTexture target)
		{
			RenderTexture buffer = new RenderTexture(target.width, target.height, 0, target.format);
			buffer.Create();

			return buffer;
		}

		protected virtual CommandBuffer CreateCommandBuffer(string name, RenderTexture[] destinations)
		{
			CommandBuffer commands = new CommandBuffer();
			commands.name = name;

			for (int i = 0; i < targets.Length; i++)
			{
				commands.SetRenderTarget(destinations[i]);

				if (targets[i])
				{
					commands.SetGlobalTexture("_TmpRenderTarget", targets[i]);
				}

				commands.DrawMesh(QuadMesh, Matrix4x4.identity, CopyMaterial, 0, 3);
			}

			return commands;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (encoder.Recording) EndRecording();
			}

			base.Dispose(disposing);
		}

		private void UpdateScratchBuffers()
		{
			if (!encoder.CaptureOffscreenBuffer) return;

			if (scratchBuffers == null)
			{
				scratchBuffers = new RenderTexture[targets.Length];
			}

			for (int i = 0; i < scratchBuffers.Length; i++)
			{
				RenderTexture target = targets[i];
				if (target)
				{
					if (RequireRegeneration(ref scratchBuffers[i], target))
					{
						scratchBuffers[i] = DisposalHelper.Mark(CreateScratchBuffer(target));
					}
				}
			}
		}

		private void CreateCommandBuffer()
		{
			if (encoder.CaptureFrameBuffer)
			{
				commandBuffer = CreateCommandBuffer(description, scratchBuffers);
			}
		}

		private void ReleaseCommandBuffer()
		{
			if (commandBuffer != null)
			{
				commandBuffer.Release();
				commandBuffer = null;
			}
		}

		private void AttachCommandBuffer()
		{
			if (camera)
			{
				if (encoder.CaptureFrameBuffer && commandBuffer != null)
				{
					camera.AddCommandBuffer(TargetCameraEvent, commandBuffer);
				}
			}
		}

		private void DetachCommandBuffer()
		{
			if (camera)
			{
				if (encoder.CaptureFrameBuffer && commandBuffer != null)
				{
					camera.RemoveCommandBuffer(TargetCameraEvent, commandBuffer);
				}
			}
		}

		private void ReleaseAllBuffers()
		{
			if (scratchBuffers != null)
			{
				for (int i = 0; i < scratchBuffers.Length; i++)
				{
					DisposalHelper.Dispose(ref scratchBuffers[i]);
				}

				scratchBuffers = null;
			}
		}

		private static bool RequireRegeneration(ref RenderTexture texture, RenderTexture target)
		{
			if (texture != null)
			{
				bool created = texture.IsCreated();
				bool resized = (texture.width != target.width || texture.height != target.height);
				bool formatChanged = (texture.format != target.format);

				if (created && !resized && !formatChanged) return false;

				DisposalHelper.Dispose(ref texture);
			}

			return true;
		}

		#endregion
	}
}
