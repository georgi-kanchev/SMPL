﻿namespace SMPL
{
	internal class NinePatchInstance : SpriteInstance
	{
		public float BorderSize
		{
			get => borderSz;
			set { borderSz = value; base.LocalSize = rawLocalSz - new Vector2(value) * 2; }
		}
		public override Vector2 LocalSize
		{
			get => rawLocalSz;
			set { rawLocalSz = value; base.LocalSize = value - new Vector2(BorderSize) * 2; }
		}
		[JsonIgnore]
		public override Vector2 Size
		{
			get => LocalSize * Scale;
			set => LocalSize = value / Scale;
		}

		public override Hitbox BoundingBox
		{
			get
			{
				var lines = base.BoundingBox.LocalLines;
				var tl = lines[0].A.MoveAtAngle(270, BorderSize, false).MoveAtAngle(180, BorderSize, false);
				var bb = new Hitbox(
					tl,
					lines[1].A.MoveAtAngle(270, BorderSize, false).MoveAtAngle(0, BorderSize, false),
					lines[2].A.MoveAtAngle(0, BorderSize, false).MoveAtAngle(90, BorderSize, false),
					lines[3].A.MoveAtAngle(180, BorderSize, false).MoveAtAngle(90, BorderSize, false),
					tl);
				bb.TransformLocalLines(UID);
				return bb;
			}
		}

		#region Backend
		private Vector2 rawLocalSz;
		private float borderSz = 16;

		[JsonConstructor]
		internal NinePatchInstance() => Init();
		internal NinePatchInstance(string uid) : base(uid) => Init();

		private void Init()
		{
			LocalSize = new(100);
		}

		internal override void OnDraw(RenderTarget renderTarget)
		{
			if(IsHidden)
				return;

			var tex = GetTexture();
			var w = tex == null ? 0 : tex.Size.X;
			var h = tex == null ? 0 : tex.Size.Y;
			var topLeft = new Vector2f(w * TexCoordUnitA.X, h * TexCoordUnitA.Y);
			var bottomRight = new Vector2f(w * TexCoordUnitB.X, h * TexCoordUnitB.Y);
			var topRight = new Vector2f(bottomRight.X, topLeft.Y);
			var bottomLeft = new Vector2f(topLeft.X, bottomRight.Y);
			var lines = base.BoundingBox.Lines;
			var bbLines = BoundingBox.Lines;
			var bsz = BorderSize * Scale;
			var verts = new Vertex[]
			{
				// top left
				new(bbLines[0].A.ToSFML(), Tint, topLeft),
				new(bbLines[0].A.MoveAtAngle(Angle, bsz, false).ToSFML(), Tint, topLeft + new Vector2f(BorderSize, 0)),
				new(lines[0].A.ToSFML(), Tint, topLeft + new Vector2f(BorderSize, BorderSize)),
				new(lines[0].A.MoveAtAngle(Angle + 180, bsz, false).ToSFML(), Tint, topLeft + new Vector2f(0, BorderSize)),

				// top
				new(lines[0].A.MoveAtAngle(Angle + 270, bsz, false).ToSFML(), Tint, topLeft + new Vector2f(BorderSize, 0)),
				new(lines[0].A.ToSFML(), Tint, topLeft + new Vector2f(BorderSize, BorderSize)),
				new(lines[1].A.ToSFML(), Tint, topRight + new Vector2f(-BorderSize, BorderSize)),
				new(lines[1].A.MoveAtAngle(Angle + 270, bsz, false).ToSFML(), Tint, topRight + new Vector2f(-BorderSize, 0)),

				// top right
				new(bbLines[1].A.ToSFML(), Tint, topRight),
				new(bbLines[1].A.MoveAtAngle(Angle + 180, bsz, false).ToSFML(), Tint, topRight + new Vector2f(-BorderSize, 0)),
				new(lines[1].A.ToSFML(), Tint, topRight + new Vector2f(-BorderSize, BorderSize)),
				new(lines[1].A.MoveAtAngle(Angle, bsz, false).ToSFML(), Tint, topRight + new Vector2f(0, BorderSize)),

				// left
				new(lines[0].A.MoveAtAngle(Angle + 180, bsz, false).ToSFML(), Tint, topLeft + new Vector2f(0, BorderSize)),
				new(lines[0].A.ToSFML(), Tint, topLeft + new Vector2f(BorderSize, BorderSize)),
				new(lines[3].A.ToSFML(), Tint, bottomLeft + new Vector2f(BorderSize, -BorderSize)),
				new(lines[3].A.MoveAtAngle(Angle + 180, bsz, false).ToSFML(), Tint, bottomLeft + new Vector2f(0, -BorderSize)),

				// center
				new(lines[0].A.ToSFML(), Tint, topLeft + new Vector2f(BorderSize, BorderSize)),
				new(lines[1].A.ToSFML(), Tint, topRight + new Vector2f(-BorderSize, BorderSize)),
				new(lines[2].A.ToSFML(), Tint, bottomRight - new Vector2f(BorderSize, BorderSize)),
				new(lines[3].A.ToSFML(), Tint, bottomLeft + new Vector2f(BorderSize, -BorderSize)),

				// right
				new(lines[1].A.ToSFML(), Tint, topRight + new Vector2f(-BorderSize, BorderSize)),
				new(lines[1].A.MoveAtAngle(Angle, bsz, false).ToSFML(), Tint, topRight + new Vector2f(0, BorderSize)),
				new(lines[2].A.MoveAtAngle(Angle, bsz, false).ToSFML(), Tint, bottomRight + new Vector2f(0, -BorderSize)),
				new(lines[2].A.ToSFML(), Tint, bottomRight - new Vector2f(BorderSize, BorderSize)),

				// bot left
				new(lines[3].A.MoveAtAngle(Angle + 180, bsz, false).ToSFML(), Tint, bottomLeft + new Vector2f(0, -BorderSize)),
				new(lines[3].A.ToSFML(), Tint, bottomLeft + new Vector2f(BorderSize, -BorderSize)),
				new(lines[3].A.MoveAtAngle(Angle + 90, bsz, false).ToSFML(), Tint, bottomLeft + new Vector2f(BorderSize, 0)),
				new(bbLines[3].A.ToSFML(), Tint, bottomLeft),

				// bot
				new(lines[3].A.ToSFML(), Tint, bottomLeft + new Vector2f(BorderSize, -BorderSize)),
				new(lines[2].A.ToSFML(), Tint, bottomRight - new Vector2f(BorderSize, BorderSize)),
				new(lines[2].A.MoveAtAngle(Angle + 90, bsz, false).ToSFML(), Tint, bottomRight - new Vector2f(BorderSize, 0)),
				new(lines[3].A.MoveAtAngle(Angle + 90, bsz, false).ToSFML(), Tint, bottomLeft + new Vector2f(BorderSize, 0)),

				// bot right
				new(lines[2].A.ToSFML(), Tint, bottomRight - new Vector2f(BorderSize, BorderSize)),
				new(lines[2].A.MoveAtAngle(Angle, bsz, false).ToSFML(), Tint, bottomRight - new Vector2f(0, BorderSize)),
				new(bbLines[2].A.ToSFML(), Tint, bottomRight),
				new(lines[2].A.MoveAtAngle(Angle + 90, bsz, false).ToSFML(), Tint, bottomRight - new Vector2f(BorderSize, 0)),
			};

			renderTarget.Draw(verts, PrimitiveType.Quads, new(GetBlendMode(), Transform.Identity, GetTexture(), GetShader(renderTarget)));
		}
		#endregion
	}
}
