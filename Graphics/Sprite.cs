﻿namespace SMPL.Graphics
{
	internal class Sprite : Visual
	{
		public Vector2 TexCoordsUnitA { get; set; }
		public Vector2 TexCoordsUnitB { get; set; } = new(1, 1);

		public Vector2 LocalSize { get; set; } = new(100, 100);
		[JsonIgnore]
		public Vector2 Size
		{
			get => LocalSize * Scale;
			set => LocalSize = value / Scale;
		}

		public Vector2 OriginUnit { get; set; } = new(0.5f, 0.5f);
		[JsonIgnore]
		public Vector2 Origin
		{
			get => OriginUnit * LocalSize;
			set => OriginUnit = value / LocalSize;
		}

		public override Vector2 CornerClockwise(int index)
		{
			index = index.Limit(0, 4, Extensions.Limitation.Overflow);
			return index switch
			{
				0 => PositionFromSelf(-Origin),
				1 => PositionFromSelf(new Vector2(LocalSize.X, 0) - Origin),
				2 => PositionFromSelf(LocalSize - Origin),
				3 => PositionFromSelf(new Vector2(0, LocalSize.Y) - Origin),
				_ => default,
			};
		}

		#region Backend
		[JsonConstructor]
		internal Sprite() { }
		internal Sprite(string uid) : base(uid) { }
		internal override void OnDraw(RenderTarget renderTarget)
		{
			if(IsHidden)
				return;

			var tex = GetTexture();
			var w = tex == null ? 0 : tex.Size.X;
			var h = tex == null ? 0 : tex.Size.Y;
			var w0 = w * TexCoordsUnitA.X;
			var ww = w * TexCoordsUnitB.X;
			var h0 = h * TexCoordsUnitA.Y;
			var hh = h * TexCoordsUnitB.Y;

			var verts = new Vertex[]
			{
				new(CornerClockwise(0).ToSFML(), Tint, new(w0, h0)),
				new(CornerClockwise(1).ToSFML(), Tint, new(ww, h0)),
				new(CornerClockwise(2).ToSFML(), Tint, new(ww, hh)),
				new(CornerClockwise(3).ToSFML(), Tint, new(w0, hh)),
			};

			renderTarget.Draw(verts, PrimitiveType.Quads, new(GetBlendMode(), Transform.Identity, tex, GetShader(renderTarget)));
		}

		internal override Hitbox GetBoundingBox()
		{
			var hitbox = new Hitbox(
				-Origin,
				new Vector2(LocalSize.X, 0) - Origin,
				LocalSize - Origin,
				new Vector2(0, LocalSize.Y) - Origin,
				-Origin);
			hitbox.TransformLocalLines(UID);
			return hitbox;
		}
		#endregion
	}
}
