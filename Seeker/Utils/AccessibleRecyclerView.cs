using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using System;

namespace Seeker.Utils
{
    public class AccessibleRecyclerView : RecyclerView
    {
        private readonly float trackTouchAreaPx;
        private readonly float touchSlop;
        private bool trackTapCandidate;
        private float trackDownX;
        private float trackDownY;

        public AccessibleRecyclerView(Context context) : base(context)
        {
            trackTouchAreaPx = TypedValue.ApplyDimension(ComplexUnitType.Dip, 32f, context.Resources.DisplayMetrics);
            touchSlop = ViewConfiguration.Get(context).ScaledTouchSlop;
            Initialize();
        }

        public AccessibleRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            trackTouchAreaPx = TypedValue.ApplyDimension(ComplexUnitType.Dip, 32f, context.Resources.DisplayMetrics);
            touchSlop = ViewConfiguration.Get(context).ScaledTouchSlop;
            Initialize();
        }

        public AccessibleRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            trackTouchAreaPx = TypedValue.ApplyDimension(ComplexUnitType.Dip, 32f, context.Resources.DisplayMetrics);
            touchSlop = ViewConfiguration.Get(context).ScaledTouchSlop;
            Initialize();
        }

        private void Initialize()
        {
            Focusable = true;
            FocusableInTouchMode = true;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            bool handledByBase = base.OnTouchEvent(e);

            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                    trackDownX = e.GetX();
                    trackDownY = e.GetY();
                    trackTapCandidate = IsInScrollbarRegion(trackDownX);
                    break;
                case MotionEventActions.Move:
                    if (trackTapCandidate)
                    {
                        if (Math.Abs(e.GetY() - trackDownY) > touchSlop || Math.Abs(e.GetX() - trackDownX) > touchSlop)
                        {
                            trackTapCandidate = false;
                        }
                    }
                    break;
                case MotionEventActions.Up:
                    if (trackTapCandidate)
                    {
                        trackTapCandidate = false;
                        HandleTrackTap(e.GetY());
                        PerformClick();
                        return true;
                    }
                    break;
                case MotionEventActions.Cancel:
                    trackTapCandidate = false;
                    break;
            }

            return handledByBase;
        }

        public override bool PerformClick()
        {
            return base.PerformClick();
        }

        protected override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (HandlePagingKey(keyCode))
            {
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        private bool HandlePagingKey(Keycode keyCode)
        {
            if (!(GetLayoutManager() is LinearLayoutManager layoutManager))
            {
                return false;
            }

            switch (keyCode)
            {
                case Keycode.PageDown:
                    PageScroll(layoutManager, true);
                    return true;
                case Keycode.PageUp:
                    PageScroll(layoutManager, false);
                    return true;
                case Keycode.MoveEnd:
                case Keycode.End:
                    ScrollToEnd(layoutManager);
                    return true;
                case Keycode.MoveHome:
                case Keycode.Home:
                    ScrollToStart(layoutManager);
                    return true;
                default:
                    return false;
            }
        }

        private void PageScroll(LinearLayoutManager layoutManager, bool forward)
        {
            int visibleItems = layoutManager.ChildCount;
            if (visibleItems <= 0)
            {
                return;
            }

            int firstVisible = layoutManager.FindFirstVisibleItemPosition();
            if (firstVisible == NoPosition)
            {
                return;
            }

            int delta = Math.Max(visibleItems - 1, 1);
            int target = forward ? firstVisible + delta : firstVisible - delta;
            target = Math.Max(0, Math.Min(target, layoutManager.ItemCount - 1));
            layoutManager.ScrollToPositionWithOffset(target, 0);
        }

        private void ScrollToStart(LinearLayoutManager layoutManager)
        {
            if (layoutManager.ItemCount > 0)
            {
                layoutManager.ScrollToPositionWithOffset(0, 0);
            }
        }

        private void ScrollToEnd(LinearLayoutManager layoutManager)
        {
            int count = layoutManager.ItemCount;
            if (count > 0)
            {
                layoutManager.ScrollToPosition(count - 1);
            }
        }

        private bool IsInScrollbarRegion(float x)
        {
            bool isRtl = ViewCompat.GetLayoutDirection(this) == LayoutDirection.Rtl;
            if (isRtl)
            {
                return x <= trackTouchAreaPx;
            }

            return x >= Width - trackTouchAreaPx;
        }

        private void HandleTrackTap(float y)
        {
            if (!(GetLayoutManager() is LinearLayoutManager layoutManager))
            {
                return;
            }

            int totalItems = layoutManager.ItemCount;
            if (totalItems <= 0)
            {
                return;
            }

            float proportion = y / Math.Max(1f, Height);
            int target = (int)Math.Round(proportion * (totalItems - 1));
            target = Math.Max(0, Math.Min(target, totalItems - 1));
            layoutManager.ScrollToPositionWithOffset(target, 0);
        }
    }
}
