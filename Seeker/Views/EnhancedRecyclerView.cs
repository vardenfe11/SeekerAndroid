using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using System;

namespace Seeker.Views
{
    public class EnhancedRecyclerView : RecyclerView
    {
        private bool fastScrollTouchActive = false;
        private int fastScrollTouchAreaPx;

        public EnhancedRecyclerView(Context context) : base(context)
        {
            Initialize(context);
        }

        public EnhancedRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(context);
        }

        public EnhancedRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize(context);
        }

        protected EnhancedRecyclerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Initialize(Context);
        }

        private void Initialize(Context context)
        {
            var metrics = context?.Resources?.DisplayMetrics ?? Resources?.DisplayMetrics;
            fastScrollTouchAreaPx = metrics != null ? (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 32f, metrics) : 48;
            Focusable = true;
            FocusableInTouchMode = true;
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            if (HandleFastScrollTouch(e))
            {
                return true;
            }
            return base.OnInterceptTouchEvent(e);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (HandleFastScrollTouch(e))
            {
                return true;
            }
            return base.OnTouchEvent(e);
        }

        private bool HandleFastScrollTouch(MotionEvent e)
        {
            if (LayoutManager == null || Adapter == null)
            {
                fastScrollTouchActive = false;
                return false;
            }

            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                case MotionEventActions.ButtonPress:
                    if (IsInFastScrollRegion(e.GetX()))
                    {
                        fastScrollTouchActive = true;
                        Parent?.RequestDisallowInterceptTouchEvent(true);
                        ScrollToFastScrollPosition(e.GetY());
                        return true;
                    }
                    break;
                case MotionEventActions.Move:
                    if (fastScrollTouchActive)
                    {
                        ScrollToFastScrollPosition(e.GetY());
                        return true;
                    }
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                case MotionEventActions.ButtonRelease:
                    if (fastScrollTouchActive)
                    {
                        ScrollToFastScrollPosition(e.GetY());
                        fastScrollTouchActive = false;
                        return true;
                    }
                    break;
            }

            return fastScrollTouchActive;
        }

        private bool IsInFastScrollRegion(float x)
        {
            return x >= Width - fastScrollTouchAreaPx;
        }

        private void ScrollToFastScrollPosition(float y)
        {
            var adapter = Adapter;
            if (adapter == null)
            {
                return;
            }

            int itemCount = adapter.ItemCount;
            if (itemCount == 0)
            {
                return;
            }

            int height = Height - PaddingTop - PaddingBottom;
            if (height <= 0)
            {
                return;
            }

            float clampedY = Math.Max(0f, Math.Min(y - PaddingTop, height));
            float proportion = clampedY / height;
            int targetPosition = (int)Math.Round(proportion * (itemCount - 1));
            targetPosition = Math.Max(0, Math.Min(itemCount - 1, targetPosition));

            if (LayoutManager is LinearLayoutManager linear)
            {
                linear.ScrollToPositionWithOffset(targetPosition, 0);
            }
            else
            {
                ScrollToPosition(targetPosition);
            }
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (HandleKeyboardScroll(keyCode))
            {
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }

        private bool HandleKeyboardScroll(Keycode keyCode)
        {
            if (!(LayoutManager is LinearLayoutManager linear) || Adapter == null)
            {
                return false;
            }

            switch (keyCode)
            {
                case Keycode.PageDown:
                    return ScrollByPage(linear, true);
                case Keycode.PageUp:
                    return ScrollByPage(linear, false);
                case Keycode.MoveEnd:
                    ScrollToPosition(Adapter.ItemCount - 1);
                    return true;
                case Keycode.MoveHome:
                    ScrollToPosition(0);
                    return true;
                default:
                    return false;
            }
        }

        private bool ScrollByPage(LinearLayoutManager layoutManager, bool forward)
        {
            int visibleCount = layoutManager.ChildCount;
            if (visibleCount <= 0)
            {
                return false;
            }

            int firstVisible = layoutManager.FindFirstVisibleItemPosition();
            if (firstVisible == RecyclerView.NoPosition)
            {
                return false;
            }

            int target = forward ? firstVisible + visibleCount : firstVisible - visibleCount;
            target = Math.Max(0, Math.Min(Adapter.ItemCount - 1, target));
            layoutManager.ScrollToPositionWithOffset(target, 0);
            return true;
        }
    }
}
