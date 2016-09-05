using System;
using UnityEngine;

namespace UTJ
{
	public interface IImageSequenceEncoder : IDisposable
	{
		#region Properties

		ImageSequenceEncoderSettings Settings { get; }

		bool CaptureFrameBuffer { get; }

		bool CaptureGBuffer { get; }

		bool CaptureOffscreenBuffer { get; }

		bool Initialized { get; }

		bool Recording { get; }

		string Extension { get; }

		#endregion

		#region Methods

		void Initialize();

		void BeginRecording();

		void EndRecording();

		void ExportFrameBuffer(RenderTexture buffer, string path, int number);

		void ExportGBuffer(RenderTexture[] gbuffer, string path, int number);

		void ExportOffscreenBuffer(RenderTexture[] buffers, string path, int number);

		#endregion
	}
}
