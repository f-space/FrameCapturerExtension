using UnityEngine;
using UnityEngine.Rendering;

namespace UTJ
{
	public class PngRecordingUnit : ImageSequenceRecordingUnit<PngEncoder>
	{
		#region Constants

		private const int GBuffers = 7;

		#endregion

		#region Fields

		private static readonly RenderTextureFormat[] gbufferFormats =
		{
			RenderTextureFormat.ARGBHalf,   // albedo (RGB)
			RenderTextureFormat.RHalf,      // occlusion (R)
			RenderTextureFormat.ARGBHalf,   // specular (RGB)
			RenderTextureFormat.RHalf,      // smoothness (R)
			RenderTextureFormat.ARGBHalf,   // normal (RGB)
			RenderTextureFormat.ARGBHalf,   // emission (RGB)
			RenderTextureFormat.RHalf,      // depth (R)
		};

		#endregion

		#region Properties

		protected override int GBufferSize { get { return GBuffers; } }

		#endregion

		#region Constructors

		public PngRecordingUnit(PngEncoder encoder, bool autoDisposeEncoder = false, string description = null, string gdescription = null)
			: base(encoder, autoDisposeEncoder, description, gdescription) { }

		#endregion

		#region Methods

		protected override RenderTexture CreateGBuffer(int index, int width, int height)
		{
			// last one is depth (1 channel)
			RenderTexture gbuffer = new RenderTexture(width, height, 0, gbufferFormats[index]);
			gbuffer.filterMode = FilterMode.Point;
			gbuffer.Create();

			return gbuffer;
		}

		protected override CommandBuffer CreateCommandBufferForGBuffer(string name, RenderTexture[] destinations)
		{
			var colors1 = new RenderTargetIdentifier[] { destinations[0], destinations[1], destinations[2], destinations[3] };
			var colors2 = new RenderTargetIdentifier[] { destinations[4], destinations[5], destinations[6], destinations[3] };

			CommandBuffer commands = new CommandBuffer();
			commands.name = name;
			commands.SetRenderTarget(colors1, destinations[0]);
			commands.DrawMesh(QuadMesh, Matrix4x4.identity, CopyMaterial, 0, 4);
			commands.SetRenderTarget(colors2, destinations[0]);
			commands.DrawMesh(QuadMesh, Matrix4x4.identity, CopyMaterial, 0, 5);

			return commands;
		}

		#endregion
	}
}
