using UnityEngine;
using UnityEngine.Rendering;

namespace UTJ
{
	public class OffscreenMovieRecordingUnit<T> : MovieRecordingUnit<T> where T : IMovieEncoder
	{
		#region Fields

		private RenderTexture target;

		#endregion

		#region Properties

		public RenderTexture Target
		{
			get { return target; }
			set { target = value; }
		}

		#endregion

		#region Constructors

		public OffscreenMovieRecordingUnit(T encoder, bool autoDisposeEncoder = false, string description = null)
			: base(encoder, autoDisposeEncoder, description) { }

		#endregion

		#region Methods

		protected override CommandBuffer CreateCommandBuffer(string name, RenderTexture destination, Mesh quad, Material material)
		{
			CommandBuffer commands = new CommandBuffer();
			commands.name = name;
			commands.SetRenderTarget(destination);

			if (target)
			{
				commands.SetGlobalTexture("_TmpRenderTarget", target);
			}

			commands.DrawMesh(quad, Matrix4x4.identity, material, 0, 3);

			return commands;
		}

		#endregion
	}
}
