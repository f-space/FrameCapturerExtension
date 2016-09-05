using UnityEngine;
using UnityEngine.Rendering;

namespace UTJ
{
	public class ExrRecordingUnit : ImageSequenceRecordingUnit<ExrEncoder>
	{
		#region Constants

		private const int GBuffers = 5;

		#endregion

		#region Fields

		private static readonly RenderTextureFormat[] gbufferFormats =
		{
			RenderTextureFormat.ARGBHalf,
			RenderTextureFormat.ARGBHalf,
			RenderTextureFormat.ARGBHalf,
			RenderTextureFormat.ARGBHalf,
			RenderTextureFormat.RHalf,
		};

		#endregion

		#region Properties

		protected override int GBufferSize { get { return GBuffers; } }

		#endregion

		#region Constructors

		public ExrRecordingUnit(ExrEncoder encoder, bool autoDisposeEncoder = false, string description = null, string gdescription = null)
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
			var colors = new RenderTargetIdentifier[] { destinations[0], destinations[1], destinations[2], destinations[3] };

			CommandBuffer commands = new CommandBuffer();
			commands.name = name;
			commands.SetRenderTarget(colors, destinations[0]);
			commands.DrawMesh(QuadMesh, Matrix4x4.identity, CopyMaterial, 0, 1);
			commands.SetRenderTarget(destinations[4]); // depth
			commands.DrawMesh(QuadMesh, Matrix4x4.identity, CopyMaterial, 0, 2);

			return commands;
		}

		#endregion
	}
}
