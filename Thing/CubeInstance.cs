﻿namespace SMPL
{
	public static partial class Thing
	{
		public struct CubeSide
		{
			public bool IsHidden { get; set; }
			public string TexturePath { get; set; }

			public Vector2 TexCoordA
			{
				set
				{
					var tex = GetTexture();
					TexCoordUnitA = tex == null ? value : value / new Vector2(tex.Size.X, tex.Size.Y);
				}
				get
				{
					var tex = GetTexture();
					return tex == null ? TexCoordUnitA : new Vector2(tex.Size.X, tex.Size.Y) * TexCoordUnitA;
				}
			}
			public Vector2 TexCoordB
			{
				set
				{
					var tex = GetTexture();
					TexCoordUnitB = tex == null ? value : value / new Vector2(tex.Size.X, tex.Size.Y);
				}
				get
				{
					var tex = GetTexture();
					return tex == null ? TexCoordUnitB : new Vector2(tex.Size.X, tex.Size.Y) * TexCoordUnitB;
				}
			}

			public Vector2 TexCoordUnitA { get; set; }
			public Vector2 TexCoordUnitB { get; set; }

			public CubeSide(string texturePath)
			{
				IsHidden = false;
				TexturePath = texturePath;
				TexCoordUnitA = new();
				TexCoordUnitB = new(1);
			}

			internal Vector2 GetTexSize()
			{
				var tex = GetTexture();
				return tex == default ? new Vector2() : new(tex.Size.X, tex.Size.Y);
			}
			internal Texture GetTexture()
			{
				var path = TexturePath.ToBackslashPath();
				return HasTexture() ? Scene.CurrentScene.Textures[path] : null;
			}
			internal bool HasTexture(string trySetTexture = null)
			{
				var texs = Scene.CurrentScene.Textures;

				if((TexturePath == null || texs.ContainsKey(TexturePath) == false) && trySetTexture != null && texs.ContainsKey(trySetTexture))
					TexturePath = trySetTexture;

				var path = TexturePath.ToBackslashPath();
				return path != null && texs.ContainsKey(path);
			}
		}
	}

	internal class CubeInstance : Pseudo3DInstance
	{
		public float PerspectiveUnit { get; set; } = 1.2f;
		public Hitbox BoundingBox3D
		{
			get
			{
				var baseBB = BoundingBox;

				if(baseBB.Lines.Count != 4)
					return bb3D;

				var h = currDepth * Scale;
				var tl = baseBB.Lines[0].A.MoveAtAngle(Tilt, h, false);
				var tr = baseBB.Lines[1].A.MoveAtAngle(Tilt, h, false);
				var br = baseBB.Lines[2].A.MoveAtAngle(Tilt, h, false);
				var bl = baseBB.Lines[3].A.MoveAtAngle(Tilt, h, false);
				var center = tr.PercentToTarget(bl, new(50));
				var percent = new Vector2(PerspectiveUnit.Map(1, 0, 0, 100));

				tl = tl.PercentToTarget(center, percent);
				tr = tr.PercentToTarget(center, percent);
				br = br.PercentToTarget(center, percent);
				bl = bl.PercentToTarget(center, percent);

				bb3D.Lines.Clear();
				bb3D.LocalLines.Clear();
				bb3D.Lines.Add(new(tl, tr));
				bb3D.Lines.Add(new(tr, br));
				bb3D.Lines.Add(new(br, bl));
				bb3D.Lines.Add(new(bl, tl));

				return bb3D;
			}
		}

		public Thing.CubeSide SideFar { get; set; } = defCubeSide;
		public Thing.CubeSide SideNear { get; set; } = defCubeSide;
		public Thing.CubeSide SideLeft { get; set; } = defCubeSide;
		public Thing.CubeSide SideRight { get; set; } = defCubeSide;
		public Thing.CubeSide SideTop { get; set; } = defCubeSide;
		public Thing.CubeSide SideBottom { get; set; } = defCubeSide;

		public override Hitbox BoundingBox
		{
			get
			{
				var tl = -Origin;
				var tr = new Vector2(LocalSize.X, 0) - Origin;
				var br = LocalSize - Origin;
				var bl = new Vector2(0, LocalSize.Y) - Origin;

				bb.Lines.Clear();
				bb.LocalLines.Clear();
				bb.LocalLines.Add(new(tl, tr));
				bb.LocalLines.Add(new(tr, br));
				bb.LocalLines.Add(new(br, bl));
				bb.LocalLines.Add(new(bl, tl));
				bb.TransformLocalLines(UID);
				return bb;
			}
		}

		#region Backend
		private readonly Hitbox bb3D = new();
		private static Thing.CubeSide defCubeSide = new() { TexCoordUnitB = new(1) };
		private readonly SortedDictionary<float, List<Action>> sortedSides = new();

		[JsonConstructor]
		internal CubeInstance() { }
		internal CubeInstance(string uid) : base(uid) { }

		internal override void OnDraw(RenderTarget renderTarget)
		{
			if(IsHidden)
				return;

			var farBB = BoundingBox;
			var nearBB = BoundingBox3D;

			var farTl = farBB.Lines[0].A;
			var farTr = farBB.Lines[1].A;
			var farBr = farBB.Lines[2].A;
			var farBl = farBB.Lines[3].A;
			var nearTl = nearBB.Lines[0].A;
			var nearTr = nearBB.Lines[1].A;
			var nearBr = nearBB.Lines[2].A;
			var nearBl = nearBB.Lines[3].A;
			var tilt = Tilt.Wrap(360);

			var l = farTl.Distance(nearTl) + farBl.Distance(nearBl);
			var r = farTr.Distance(nearTr) + farBr.Distance(nearBr);
			var t = farTl.Distance(nearTl) + farTr.Distance(nearTr);
			var b = farBl.Distance(nearBl) + farBr.Distance(nearBr);

			sortedSides.Clear();
			sortedSides[r] = new();
			sortedSides[l] = new();
			sortedSides[t] = new();
			sortedSides[b] = new();
			sortedSides[r].Add(TryDrawRight);
			sortedSides[l].Add(TryDrawLeft);
			sortedSides[t].Add(TryDrawTop);
			sortedSides[b].Add(TryDrawBottom);

			var persp = PerspectiveUnit == 1f ? 1.01f : PerspectiveUnit;
			var enumerable = persp < 1 ? sortedSides : sortedSides.Reverse();

			TryDrawSide(SideFar, farTl, farTr, farBr, farBl);

			foreach(var side in enumerable)
				for(int i = 0; i < side.Value.Count; i++)
					side.Value[i].Invoke();

			TryDrawSide(SideNear, nearTl, nearTr, nearBr, nearBl);

			void TryDrawRight() => TryDrawSide(SideRight, nearBr, nearTr, farTr, farBr, Angle);
			void TryDrawLeft() => TryDrawSide(SideLeft, nearTl, nearBl, farBl, farTl, Angle + 180);
			void TryDrawTop() => TryDrawSide(SideTop, nearTr, nearTl, farTl, farTr, Angle + 270);
			void TryDrawBottom() => TryDrawSide(SideBottom, nearBl, nearBr, farBr, farBl, Angle + 90);
			void TryDrawSide(Thing.CubeSide cubeSide, Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl, float sideAngle = float.NaN)
			{
				if(cubeSide.IsHidden)
					return;

				var hasTexture = cubeSide.HasTexture(TexturePath);
				var sz = hasTexture ? cubeSide.GetTexSize() : default;
				var tex = hasTexture ? cubeSide.GetTexture() : default;
				var w = tex == null ? 0 : sz.X;
				var h = tex == null ? 0 : sz.Y;
				var w0 = w * cubeSide.TexCoordUnitA.X;
				var ww = w * cubeSide.TexCoordUnitB.X;
				var h0 = h * cubeSide.TexCoordUnitA.Y;
				var hh = h * cubeSide.TexCoordUnitB.Y;

				var prevSmooth = false;
				if(tex != null)
				{
					prevSmooth = tex.Smooth;
					tex.Smooth = IsSmooth;
				}

				var dirLight = float.IsNaN(Thing.SunAngle) ? default : Thing.SunAngle.ToDirection();
				var sunValue = 0f;

				var sideDir = sideAngle.ToDirection();
				var dot = Vector2.Dot(dirLight, sideDir);
				const float MINIMUM_LIGHT = 0.2f;

				sunValue = dot.Map(-1, 1, MINIMUM_LIGHT, 1); // 0.2 -> 1 means even dark spots receive some sun

				if(float.IsNaN(sunValue))
					sunValue = MINIMUM_LIGHT;

				if(dirLight == default)
					sunValue = float.IsNaN(sideAngle) ? 1f : MINIMUM_LIGHT;

				var overallSunEffect = Thing.SunColor.A / 255f;
				sunValue *= overallSunEffect;

				var amb = Thing.ShadowColor;
				var sun = Thing.SunColor;
				var ambVec = Vector3.Lerp(new(), new Vector3(amb.R, amb.G, amb.B), amb.A / 255f);
				var sunCol = new Vector3(sun.R, sun.G, sun.B);
				var lerp = Vector3.Lerp(ambVec, sunCol, sunValue);
				var resultCol = new Color((byte)lerp.X, (byte)lerp.Y, (byte)lerp.Z);

				var verts = new Vertex[]
				{
					new(tl.ToSFML(), Tint, new(w0, h0)),
					new(tr.ToSFML(), Tint, new(ww, h0)),
					new(br.ToSFML(), Tint, new(ww, hh)),
					new(bl.ToSFML(), Tint, new(w0, hh))
				};

				var shader = GetShader(renderTarget);
				if(tex != null)
					shader?.SetUniform("Texture", tex); // different than the main visual texture, should be able to use effects on it

				SetEffectColor("AmbientColor", resultCol);
				renderTarget.Draw(verts, PrimitiveType.Quads, new(GetBlendMode(), Transform.Identity, tex, shader));

				if(tex != null)
					tex.Smooth = prevSmooth;
			}
		}
		#endregion
	}
}
