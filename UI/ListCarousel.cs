﻿using Newtonsoft.Json;
using SFML.Window;
using SMPL.Graphics;
using SMPL.Tools;

namespace SMPL.UI
{
	public class ListCarousel : List
	{
		[JsonIgnore]
		public Button Previous { get; private set; }
		[JsonIgnore]
		public Button Next { get; private set; }

		public new bool IsHovered
		{
			get
			{
				if (IsHidden || IsDisabled)
					return false;

				var hitbox = new Hitbox(
					Previous.CornerA,
					Next.CornerB,
					Next.CornerC,
					Previous.CornerD,
					Previous.CornerA);

				return hitbox.ConvexContains(Scene.MouseCursorPosition);
			}
		}
		public bool SelectionIsRepeating { get; set; }
		public int SelectionIndex
		{
			get => (int)Value;
			set => Value = value.Limit(0, Buttons.Count, SelectionIsRepeating ? Extensions.Limitation.Overflow : Extensions.Limitation.ClosestBound);
		}
		[JsonIgnore]
		public Button Selection => Buttons.Count == 0 ? null : Buttons[SelectionIndex];

		public ListCarousel(Button previous = null, Button next = null)
		{
			SelectionIsRepeating = true;

			Angle = 0;
			previous ??= new();
			next ??= new();

			Previous = previous;
			Next = next;

			Previous.Clicked += OnPrevious;
			Next.Clicked += OnNext;

			Previous.Held += OnPreviousHold;
			Next.Held += OnNextHold;
		}

		private void OnPreviousHold(Button button) => OnScrollDown(button);
		private void OnNextHold(Button button) => OnScrollUp(button);
		private void OnPrevious(Button button) => OnScrollDown(button);
		private void OnNext(Button button) => OnScrollUp(button);

		protected override void OnScrollUp(Button button)
		{
			if (IsDisabled)
				return;

			SelectionIndex++;
		}
		protected override void OnScrollDown(Button button)
		{
			if (IsDisabled)
				return;

			SelectionIndex--;
		}

		public override void Draw(Camera camera = null)
		{
			ScrollValue = 1;
			RangeA = 0;
			RangeB = Buttons.Count - 1;

			Previous.SetDefaultHitbox();
			Previous.Hitbox.TransformLocalLines(Previous);

			Next.SetDefaultHitbox();
			Next.Hitbox.TransformLocalLines(Next);

			Previous.Size = new(ButtonHeight, ButtonHeight);
			Next.Size = new(ButtonHeight, ButtonHeight);

			Previous.Parent = this;
			Next.Parent = this;

			Previous.LocalPosition = new(-ButtonWidth * 0.5f - Previous.Size.X * 0.5f, 0);
			Next.LocalPosition = new(ButtonWidth * 0.5f + Next.Size.X * 0.5f, 0);

			Previous.Draw(camera);
			Next.Draw(camera);

			if (Selection == null)
				return;

			Selection.Size = new(ButtonWidth, ButtonHeight);
			Selection.Parent = this;
			Selection.LocalPosition = new();
			Selection.SetDefaultHitbox();
			Selection.Hitbox.TransformLocalLines(Selection);

			Selection.Draw(camera);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			Previous.Clicked -= OnPrevious;
			Next.Clicked -= OnNext;
			Previous.Held -= OnPreviousHold;
			Next.Held -= OnNextHold;

			Previous.Destroy();
			Next.Destroy();

			Previous = null;
			Next = null;
		}
	}
}
