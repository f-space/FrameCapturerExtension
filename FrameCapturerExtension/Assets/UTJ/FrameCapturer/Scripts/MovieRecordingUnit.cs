using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace UTJ
{
	public class MovieRecordingUnit<T> : RecordingUnitBase<T>, IMovieRecordingUnit<T> where T : IMovieEncoder
	{
		#region Constants

		private const string DefaultCommandBufferDescription = "MovieRecordingUnit: copy frame buffer";

		private const CameraEvent TargetCameraEvent = CameraEvent.AfterImageEffects;

		#endregion

		#region Fields

		private readonly string description;

		private Camera camera;

		private CommandBuffer commandBuffer;

		private RenderTexture scratchBuffer;

		#endregion

		#region Properties

		public string Description { get { return description; } }

		public Camera Camera
		{
			get { return camera; }
			set { if (!Recording) camera = value; }
		}

		public RenderTexture ScratchBuffer { get { return scratchBuffer; } }

		public bool Recording { get { return encoder.Recording; } }

		public int FrameCount { get { return encoder.FrameCount; } }

		IMovieEncoder IMovieRecordingUnit.Encoder { get { return Encoder; } }

		Type IMovieRecordingUnit.EncoderType { get { return typeof(T); } }

		#endregion

		#region Constructors

		public MovieRecordingUnit(T encoder, bool autoDisposeEncoder = false, string description = null)
			: base(encoder, autoDisposeEncoder)
		{
			this.description = description ?? DefaultCommandBufferDescription;

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
			UpdateScratchBuffer();

			commandBuffer = CreateCommandBuffer(description, scratchBuffer, QuadMesh, CopyMaterial);

			if (camera) camera.AddCommandBuffer(TargetCameraEvent, commandBuffer);

			encoder.BeginRecording();
		}

		public void EndRecording()
		{
			if (!encoder.Recording) return;

			encoder.EndRecording();

			if (camera) camera.RemoveCommandBuffer(TargetCameraEvent, commandBuffer);

			if (commandBuffer != null)
			{
				commandBuffer.Release();
				commandBuffer = null;
			}

			// scratch buffer is kept
		}

		public void RecordImage(double time)
		{
			Assert.IsTrue(encoder.Recording);

			if (encoder.CaptureVideo) encoder.RecordImage(scratchBuffer, time);
		}

		public void RecordAudio(float[] samples, int channels)
		{
			Assert.IsNotNull(samples);
			Assert.IsTrue(encoder.Recording);

			if (encoder.CaptureAudio) encoder.RecordAudio(samples, channels);
		}

		public override void ReleaseResources()
		{
			Assert.IsFalse(encoder.Recording);

			base.ReleaseResources();

			ReleaseScratchBuffer();
		}

		protected virtual CommandBuffer CreateCommandBuffer(string name, RenderTexture destination, Mesh quad, Material material)
		{
			int tid = Shader.PropertyToID("_TmpFrameBuffer");

			CommandBuffer commands = new CommandBuffer();
			commands.name = name;
			commands.GetTemporaryRT(tid, -1, -1, 0, FilterMode.Bilinear);
			commands.Blit(BuiltinRenderTextureType.CurrentActive, tid);
			commands.SetRenderTarget(destination);
			commands.DrawMesh(quad, Matrix4x4.identity, material, 0, 0);
			commands.ReleaseTemporaryRT(tid);

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

		private void UpdateScratchBuffer()
		{
			int captureWidth = encoder.Settings.ResolutionWidth;
			int captureHeight = encoder.Settings.ResolutionHeight;

			if (scratchBuffer != null)
			{
				bool created = scratchBuffer.IsCreated();
				bool resized = (scratchBuffer.width != captureWidth || scratchBuffer.height != captureHeight);

				if (created && !resized)
				{
					// update is not needed
					return;
				}

				ReleaseScratchBuffer();
			}

			RenderTexture texture = new RenderTexture(captureWidth, captureHeight, 0, RenderTextureFormat.ARGB32);
			texture.wrapMode = TextureWrapMode.Repeat;
			texture.Create();

			scratchBuffer = DisposalHelper.Mark(texture);
		}

		private void ReleaseScratchBuffer()
		{
			DisposalHelper.Dispose(ref scratchBuffer);
		}

		#endregion
	}
}
