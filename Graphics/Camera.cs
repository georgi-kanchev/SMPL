﻿using SFML.Graphics;
using System.Numerics;
using System;
using SFML.System;
using SFML.Window;
using SMPL.Graphics;
using SMPL.Tools;
using SMPL.UI;
using Console = SMPL.Tools.Console;
using Newtonsoft.Json;

namespace SMPL.Graphics
{
	/// <summary>
	/// Inherit chain: <see cref="Camera"/> : <see cref="Object"/><br></br><br></br>
	/// A camera under the hood is just a <see cref="SFML.Graphics.Texture"/> that can be drawn onto. The drawn things will appear as if they
	/// were "viewed" from the <see cref="Camera"/>.<br></br><br></br>
	/// Here is how the whole process goes:<br></br>
	/// - The <see cref="Camera"/> and whatever <see cref="Visual"/>s are needed are initialized (usually in <see cref="Scene.OnStart"/>).<br></br>
	/// Each frame:<br></br>
	/// - <see cref="Fill"/> is called to erase what was on the <see cref="Camera"/> last frame
	/// (done in the background for <see cref="Scene.MainCamera"/>).<br></br>
	/// - All of the needed <see cref="Visual"/>s are drawn onto the <see cref="Camera"/>.<br></br>
	/// - Everything drawn onto the <see cref="Camera"/> is then displayed by <see cref="Camera.Display"/>
	/// (this is also done in the background for <see cref="Scene.MainCamera"/>).<br></br>
	/// - The result is ready to be used in <see cref="Texture"/>. It may be saved into a file with <see cref="Snap"/> or
	/// used by a <see cref="Visual"/> and drawn onto another <see cref="Camera"/>
	/// achieving the effects of a minimap, an ingame camera, a scrollable UI window/chatbox/list, in <see cref="Scene.MainCamera"/>'s case:
	/// drawn onto the <see cref="Game.Window"/> etc.<br></br><br></br>
	/// - Note: The <see cref="Camera"/> is an expensive class performance-wise and it shouldn't be recreated frequently if at all.
	/// </summary>
	public class Camera : Object
	{
		private float scale = 1;
		internal RenderTexture renderTexture;

		public RenderTexture RenderTexture => renderTexture;

		/// <summary>
		/// Position in the world.
		/// </summary>
		public new Vector2 Position
		{
			get => renderTexture.GetView().Center.ToSystem();
			set
			{
				var view = renderTexture.GetView();
				base.Position = value;
				view.Center = value.ToSFML();
				renderTexture.SetView(view);
			}
		}
		/// <summary>
		/// Angle in the world.
		/// </summary>
		public new float Angle
		{
			get => renderTexture.GetView().Rotation;
			set
			{
				var view = renderTexture.GetView();
				view.Rotation = value;
				base.Angle = value;
				renderTexture.SetView(view);
			}
		}
		/// <summary>
		/// The scaling of the initial <see cref="Camera"/> resolution.
		/// </summary>
		public new float Scale
		{
			get => scale;
			set
			{
				scale = value;
				var view = renderTexture.GetView();
				view.Size = new Vector2f(renderTexture.Size.X, renderTexture.Size.Y) * scale;
				base.Scale = scale;
				renderTexture.SetView(view);
			}
		}

		/// <summary>
		/// The resolution that this <see cref="Camera"/> was created with.
		/// </summary>
		public Vector2 Resolution { get; private set; }

		/// <summary>
		/// Whether the jagged edges are smoothed out. This is good for high resolution non-pixel art games.
		/// </summary>
		public bool IsSmooth { get => renderTexture.Smooth; set => renderTexture.Smooth = value; }

		/// <summary>
		/// The <see cref="SFML.Graphics.Texture"/> for drawing <see cref="Visual"/>s. May be used by <see cref="Visual"/>s and may be drawn onto
		/// another <see cref="Texture"/>.
		/// </summary>
		[JsonIgnore]
		public Texture Texture => renderTexture.Texture;

		/// <summary>
		/// The mouse cursor's position in the world (<see cref="CurrentScene"/>) relative to this <see cref="Camera"/>.
		/// </summary>
		public Vector2 MouseCursorPosition
		{
			get { var p = Mouse.GetPosition(Game.Window); return PointToCamera(new(p.X, p.Y)); }
			set { var p = PointToWorld(value); Mouse.SetPosition(new((int)p.X, (int)p.Y), Game.Window); }
		}

		/// <summary>
		/// Initially this is the top left corner of the <see cref="Camera"/> in the world.
		/// </summary>
		public Vector2 CornerA
		{
			get => GetPositionFromSelf(new(-renderTexture.GetView().Size.X * 0.5f, -renderTexture.GetView().Size.Y * 0.5f));
		}
		/// <summary>
		/// Initially this is the top right corner of the <see cref="Camera"/> in the world.
		/// </summary>
		public Vector2 CornerB
		{
			get => GetPositionFromSelf(new(renderTexture.GetView().Size.X * 0.5f, -renderTexture.GetView().Size.Y * 0.5f));
		}
		/// <summary>
		/// Initially this is the bottom right corner of the <see cref="Camera"/> in the world.
		/// </summary>
		public Vector2 CornerC
		{
			get => GetPositionFromSelf(new(renderTexture.GetView().Size.X * 0.5f, renderTexture.GetView().Size.Y * 0.5f));
		}
		/// <summary>
		/// Initially this is the bottom left corner of the <see cref="Camera"/> in the world.
		/// </summary>
		public Vector2 CornerD
		{
			get => GetPositionFromSelf(new(-renderTexture.GetView().Size.X * 0.5f, renderTexture.GetView().Size.Y * 0.5f));
		}

		/// <summary>
		/// Create the <see cref="Camera"/> with a certain <paramref name="resolution"/>.
		/// </summary>
		public Camera(Vector2 resolution) => Init((uint)resolution.X.Limit(0, Texture.MaximumSize), (uint)resolution.Y.Limit(0, Texture.MaximumSize));
		/// <summary>
		/// Create the <see cref="Camera"/> with a certain resolution size of [<paramref name="resolutionX"/>, <paramref name="resolutionY"/>].
		/// </summary>
		public Camera(uint resolutionX, uint resolutionY) => Init(resolutionX, resolutionY);

		protected override void OnDestroy()
			=> renderTexture.Dispose();

		/// <summary>
		/// Returns whether the <see cref="Camera"/> can "see" a <paramref name="hitbox"/>. This uses <see cref="Hitbox.ConvexContains(Hitbox)"/>
		/// so concave <see cref="Hitbox"/>es may give wrong results.
		/// </summary>
		public bool Captures(Hitbox hitbox)
		{
			var screen = new Hitbox(CornerA, CornerB, CornerC, CornerD, CornerA);
			return screen.ConvexContains(hitbox);
		}
		/// <summary>
		/// Returns whether the <see cref="Camera"/> can "see" a <paramref name="point"/>.
		/// </summary>
		public bool Captures(Vector2 point)
		{
			var screen = new Hitbox(CornerA, CornerB, CornerC, CornerD, CornerA);
			return screen.ConvexContains(point);
		}
		/// <summary>
		/// Tries to <see cref="Texture"/> into an image file at some <paramref name="imagePath"/>. This is a slow
		/// operation (especially with bigger a <see cref="Texture.Size"/>) and it is better to avoid frequent calls to not hurt
		/// performance. Returns whether the saving was successful.
		/// </summary>
		public void Snap(string imagePath)
      {
			var img = Texture.CopyToImage();
			var result = img.SaveToFile(imagePath);
			img.Dispose();

			if (result == false)
				Console.LogError(1, $"Could not save the image at '{imagePath}'.");
      }
		/// <summary>
		/// Fill the <see cref="Texture"/> with a <paramref name="color"/>.
		/// </summary>
		public void Fill(Color color = default)
      {
			renderTexture.Clear(color);
      }
		/// <summary>
		/// This updates the <see cref="Texture"/> and should be called after all of the drawing is done to this <see cref="Camera"/> for this frame.
		/// </summary>
		public void Display()
      {
			renderTexture.Display();
      }

		internal Vector2 PointToCamera(Vector2 worldPoint)
		{
			return Game.Window.MapPixelToCoords(new((int)worldPoint.X, (int)worldPoint.Y), renderTexture.GetView()).ToSystem();
		}
		internal Vector2 PointToWorld(Vector2 cameraPoint)
		{
			var p = Game.Window.MapCoordsToPixel(cameraPoint.ToSFML(), renderTexture.GetView());
			return new(p.X, p.Y);
		}

		private void Init(uint resolutionX, uint resolutionY)
		{
			resolutionX = (uint)((int)resolutionX).Limit(0, (int)Texture.MaximumSize);
			resolutionY = (uint)((int)resolutionY).Limit(0, (int)Texture.MaximumSize);
			renderTexture = new(resolutionX, resolutionY);
			Position = new();
			Resolution = new((float)resolutionX, (float)resolutionY);
		}
		internal static void DrawMainCameraToWindow()
      {
			Scene.MainCamera.Display();
			var texSz = Scene.MainCamera.renderTexture.Size;
			var viewSz = Game.Window.GetView().Size;
			var verts = new Vertex[]
			{
				new(-viewSz * 0.5f, new Vector2f()),
				new(new Vector2f(viewSz.X * 0.5f, -viewSz.Y * 0.5f), new Vector2f(texSz.X, 0)),
				new(viewSz * 0.5f, new Vector2f(texSz.X, texSz.Y)),
				new(new Vector2f(-viewSz.X * 0.5f, viewSz.Y * 0.5f), new Vector2f(0, texSz.Y))
			};

			Game.Window.Draw(verts, PrimitiveType.Quads, new(Scene.MainCamera.Texture));
      }
	}
}
