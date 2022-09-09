﻿namespace SMPL.GUI
{
	internal class ListInstance : ScrollBarInstance
	{
		public new Hitbox BoundingBox
		{
			get
			{
				var baseBB = base.BoundingBox.Lines;

				var tl = baseBB[0].A.PointMoveAtAngle(Angle + 90, Size.Y, false).PointMoveAtAngle(Angle + 180, Size.Y, false);
				var tr = baseBB[1].A.PointMoveAtAngle(Angle + 90, Size.Y, false).PointMoveAtAngle(Angle, Size.Y, false);
				var br = tr.PointMoveAtAngle(Angle + 90, ItemWidth * Scale, false);
				var bl = tl.PointMoveAtAngle(Angle + 90, ItemWidth * Scale, false);

				bb.Lines.Clear();
				bb.LocalLines.Clear();
				bb.Lines.Add(new(tl, tr));
				bb.Lines.Add(new(tr, br));
				bb.Lines.Add(new(br, bl));
				bb.Lines.Add(new(bl, tl));
				return bb;
			}
		}

		public List<Thing.GUI.ListItem> Items { get; } = new();

		public int VisibleItemCountMax { get; set; } = 5;
		public int VisibleItemCountCurrent => Math.Min(Items.Count, VisibleItemCountMax);

		public float ItemWidth { get; set; } = 300;
		public float ItemHeight
		{
			get
			{
				var max = Math.Max(VisibleItemCountMax, 1);
				var sp = ItemSpacing;
				return (MaxLength + LocalSize.Y * 2f) / max - (sp - (sp / max));
			}
		}
		public float ItemSpacing
		{
			get => spacing;
			set => spacing = value.Limit(0, MaxLength);
		}

		public int ScrollIndex => scrollIndex;

		#region Backend
		private float spacing = 5;
		private int scrollIndex;

		[JsonConstructor]
		internal ListInstance() => Init();
		internal ListInstance(string uid) : base(uid)
		{
			Init();
		}
		private void Init()
		{
			Value = 0;
		}
		private void UpdateButtonBoundingBoxes()
		{
			for(int i = 0; i < Items.Count; i++)
			{
				var item = Items[i];
				var itemBB = item.ButtonDetails.boundingBox;
				var corners = GetItemCorners(i);

				itemBB.LocalLines.Clear();
				itemBB.Lines.Clear();
				itemBB.Lines.Add(new(corners[0], corners[1]));
				itemBB.Lines.Add(new(corners[1], corners[2]));
				itemBB.Lines.Add(new(corners[2], corners[3]));
				itemBB.Lines.Add(new(corners[3], corners[0]));

				var hidden = i.IsBetween(ScrollIndex, ScrollIndex + VisibleItemCountCurrent - 1, true, true) == false;
				item.ButtonDetails.IsHidden = hidden;
				item.TextDetails.IsHidden = hidden;
			}
		}
		private void TryUpdate()
		{
			VisibleItemCountMax = Math.Max(VisibleItemCountMax, 1);
			IsFocused = BoundingBox.IsHovered;

			Step = 1;
			RangeA = 0;
			RangeB = MathF.Max((Items.Count - VisibleItemCountMax).Limit(1, Items.Count - 1), RangeA);

			scrollIndex = (int)Value;

			UpdateButtonBoundingBoxes();
		}
		private void TryButtonEvents()
		{
			for(int i = 0; i < Items.Count; i++)
			{
				var item = Items[i];
				if(item.ButtonDetails.IsHidden && item.TextDetails.IsHidden)
					continue;

				var itemBB = item.ButtonDetails.boundingBox;
				var buttonResult = itemBB.TryButton();

				var events = new List<(bool, Action<string, int, Thing.GUI.ListItem>)>()
				{
					(buttonResult.IsHovered, Event.ListItemHover), (buttonResult.IsUnhovered, Event.ListItemUnhover),
					(buttonResult.IsPressed, Event.ListItemPress), (buttonResult.IsReleased, Event.ListItemRelease),
					(buttonResult.IsClicked, Event.ListItemClick), (buttonResult.IsHeld, Event.ListItemHold),
				};

				for(int j = 0; j < events.Count; j++)
					if(events[j].Item1)
						events[j].Item2.Invoke(UID, i, item);
			}
		}

		internal override void OnDraw(RenderTarget renderTarget)
		{
			if(IsHidden == false)
				base.OnDraw(renderTarget);

			if(IsDisabled == false)
				TryButtonEvents();

			TryUpdate();

			if(IsHidden == false)
				for(int i = 0; i < Items.Count; i++)
				{
					var item = Items[i];
					item.ButtonDetails.Draw(renderTarget);

					var itemCorners = GetItemCorners(i);
					var itemCenter = itemCorners[0].PointPercentTowardPoint(itemCorners[2], new(50));
					item.TextDetails.UpdateGlobalText(itemCenter.ToSFML(), Scale);
					item.TextDetails.Draw(renderTarget);
				}
		}
		private List<Vector2> GetItemCorners(int index)
		{
			var bb = BoundingBox.Lines;
			var tl = bb[3].A.PointMoveAtAngle(Angle, ((ItemHeight + ItemSpacing) * Scale) * (index - ScrollIndex), false);
			var tr = bb[0].A.PointMoveAtAngle(Angle, ((ItemHeight + ItemSpacing) * Scale) * (index - ScrollIndex), false);
			var br = tr.PointMoveAtAngle(Angle, ItemHeight * Scale, false);
			var bl = tl.PointMoveAtAngle(Angle, ItemHeight * Scale, false);
			return new() { tl, tr, br, bl };
		}
		#endregion
	}
}
