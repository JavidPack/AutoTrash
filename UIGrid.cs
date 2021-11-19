﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AutoTrash
{
	public class UIGrid : UIElement
	{
		public delegate bool ElementSearchMethod(UIElement element);

		private class UIInnerList : UIElement
		{
			public override bool ContainsPoint(Vector2 point) {
				return true;
			}

			protected override void DrawChildren(SpriteBatch spriteBatch) {
				Vector2 position = this.Parent.GetDimensions().Position();
				Vector2 dimensions = new Vector2(this.Parent.GetDimensions().Width, this.Parent.GetDimensions().Height);
				foreach (UIElement current in this.Elements) {
					Vector2 position2 = current.GetDimensions().Position();
					Vector2 dimensions2 = new Vector2(current.GetDimensions().Width, current.GetDimensions().Height);
					if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2)) {
						current.Draw(spriteBatch);
					}
				}
			}
		}

		public List<UIElement> _items = new List<UIElement>();
		protected UIScrollbar _scrollbar;
		internal UIElement _innerList = new UIGrid.UIInnerList();
		private float _innerListHeight;
		public float ListPadding = 5f;

		public int Count {
			get {
				return this._items.Count;
			}
		}

		int cols = 1;

		public UIGrid(int columns = 1) {
			cols = columns;
			this._innerList.OverflowHidden = false;
			this._innerList.Width.Set(0f, 1f);
			this._innerList.Height.Set(0f, 1f);
			this.OverflowHidden = true;
			base.Append(this._innerList);
		}

		public float GetTotalHeight() {
			return this._innerListHeight;
		}

		public void Goto(UIGrid.ElementSearchMethod searchMethod, bool center = false) {
			for (int i = 0; i < this._items.Count; i++) {
				if (searchMethod(this._items[i])) {
					this._scrollbar.ViewPosition = this._items[i].Top.Pixels;
					if (center) {
						this._scrollbar.ViewPosition = this._items[i].Top.Pixels - GetInnerDimensions().Height / 2 + _items[i].GetOuterDimensions().Height / 2;
					}
					return;
				}
			}
		}

		public virtual void Add(UIElement item) {
			this._items.Add(item);
			this._innerList.Append(item);
			this.UpdateOrder();
			this._innerList.Recalculate();
		}

		public virtual bool Remove(UIElement item) {
			this._innerList.RemoveChild(item);
			this.UpdateOrder();
			return this._items.Remove(item);
		}

		public virtual void Clear() {
			this._innerList.RemoveAllChildren();
			this._items.Clear();
		}

		public override void Recalculate() {
			base.Recalculate();
			this.UpdateScrollbar();
		}

		public override void ScrollWheel(UIScrollWheelEvent evt) {
			base.ScrollWheel(evt);
			if (this._scrollbar != null) {
				this._scrollbar.ViewPosition -= (float)evt.ScrollWheelValue;
			}
		}

		public override void RecalculateChildren() {
			base.RecalculateChildren();
			float top = 0f;
			float left = 0f;
			for (int i = 0; i < this._items.Count; i++) {
				this._items[i].Top.Set(top, 0f);
				this._items[i].Left.Set(left, 0f);
				this._items[i].Recalculate();
				if (i % cols == cols - 1) {
					top += this._items[i].GetOuterDimensions().Height + this.ListPadding;
					left = 0;
				}
				else {
					left += this._items[i].GetOuterDimensions().Width + this.ListPadding;
				}
				//num += this._items[i].GetOuterDimensions().Height + this.ListPadding;
			}
			if (_items.Count > 0) {
				top += ListPadding + _items[0].GetOuterDimensions().Height;
			}
			this._innerListHeight = top;
		}

		private void UpdateScrollbar() {
			if (this._scrollbar == null) {
				return;
			}
			this._scrollbar.SetView(base.GetInnerDimensions().Height, this._innerListHeight);
		}

		public void SetScrollbar(UIScrollbar scrollbar) {
			this._scrollbar = scrollbar;
			this.UpdateScrollbar();
		}

		public void UpdateOrder() {
			this._items.Sort(new Comparison<UIElement>(this.SortMethod));
			this.UpdateScrollbar();
		}

		public int SortMethod(UIElement item1, UIElement item2) {
			return item1.CompareTo(item2);
		}

		public override List<SnapPoint> GetSnapPoints() {
			List<SnapPoint> list = new List<SnapPoint>();
			SnapPoint item;
			if (base.GetSnapPoint(out item)) {
				list.Add(item);
			}
			foreach (UIElement current in this._items) {
				list.AddRange(current.GetSnapPoints());
			}
			return list;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (this._scrollbar != null) {
				this._innerList.Top.Set(-this._scrollbar.GetValue(), 0f);
			}
			this.Recalculate();
		}
	}
}
