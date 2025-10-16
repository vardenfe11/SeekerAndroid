using System;
using Android.Content;
using Android.Util;
using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace Seeker
{
    /// <summary>
    /// RecyclerView with improved accessibility support for keyboard, mouse and scrollbar gestures.
    /// </summary>
    public class AccessibleRecyclerView : RecyclerView
    {
        private readonly int scrollbarTouchInset;
        private const int DefaultTrackTouchAdditionalInsetDp = 16;

        public AccessibleRecyclerView(Context context) : base(context)
        {
            scrollbarTouchInset = ResolveScrollbarTouchInset(context, null);
            Initialize();
        }

        public AccessibleRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            scrollbarTouchInset = ResolveScrollbarTouchInset(context, attrs);
            Initialize();
        }

        public AccessibleRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            scrollbarTouchInset = ResolveScrollbarTouchInset(context, attrs);
            Initialize();
        }

        private void Initialize()
        {
            Focusable = true;
            FocusableInTouchMode = true;
            VerticalScrollBarEnabled = false;
        }

        private static int ResolveScrollbarTouchInset(Context context, IAttributeSet attrs)
        {
            int baseSize = 0;
            if (context?.Resources != null)
            {
                baseSize = context.Resources.GetDimensionPixelSize(Android.Resource.Dimension.ScrollbarSize);
                int additional = (int)(context.Resources.DisplayMetrics.Density * DefaultTrackTouchAdditionalInsetDp);
                baseSize += additional;
            }

            if (baseSize == 0)
            {
                baseSize = (int)(ViewConfiguration.Get(context).ScaledTouchSlop * 2);
            }

            return baseSize;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (HandleScrollbarTap(e))
            {
                return true;
            }

            return base.OnTouchEvent(e);
        }

        private bool HandleScrollbarTap(MotionEvent e)
        {
            if (e == null)
            {
                return false;
            }

            if (e.Action != MotionEventActions.Down)
            {
                return false;
            }

            if (!CanScrollVertically(-1) && !CanScrollVertically(1))
            {
                return false;
            }

            float x = e.GetX();
            if (x < Width - scrollbarTouchInset)
            {
                return false;
            }

            int range = ComputeVerticalScrollRange() - ComputeVerticalScrollExtent();
            if (range <= 0)
            {
                return false;
            }

            float proportion = e.GetY() / Height;
            proportion = Math.Max(0f, Math.Min(1f, proportion));
            int targetOffset = (int)(proportion * range);
            int currentOffset = ComputeVerticalScrollOffset();
            int dy = targetOffset - currentOffset;
            if (dy == 0)
            {
                return false;
            }

            ScrollBy(0, dy);
            return true;
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (HandleKeyboardNavigation(keyCode))
            {
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        private bool HandleKeyboardNavigation(Keycode keyCode)
        {
            if (LayoutManager == null)
            {
                return false;
            }

            switch (keyCode)
            {
                case Keycode.PageDown:
                    ScrollByPage(forward: true);
                    return true;
                case Keycode.PageUp:
                    ScrollByPage(forward: false);
                    return true;
                case Keycode.MoveEnd:
                    ScrollToEdge(forward: true);
                    return true;
                case Keycode.MoveHome:
                    ScrollToEdge(forward: false);
                    return true;
                default:
                    return false;
            }
        }

        private void ScrollByPage(bool forward)
        {
            int distance = Height;
            if (distance <= 0)
            {
                distance = 1;
            }

            int dy = forward ? distance : -distance;
            SmoothScrollBy(0, dy);
        }

        private void ScrollToEdge(bool forward)
        {
            var adapter = GetAdapter();
            if (adapter == null || adapter.ItemCount == 0)
            {
                return;
            }

            if (LayoutManager is LinearLayoutManager linearLayoutManager)
            {
                int target = forward ? adapter.ItemCount - 1 : 0;
                linearLayoutManager.ScrollToPositionWithOffset(target, 0);
            }
            else
            {
                int target = forward ? adapter.ItemCount - 1 : 0;
                ScrollToPosition(target);
            }
        }
    }
}
