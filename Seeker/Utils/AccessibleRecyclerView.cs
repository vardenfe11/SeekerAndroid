using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;

namespace Seeker.Utils
{
    public class AccessibleRecyclerView : RecyclerView
    {
        private readonly float scrollbarTouchThreshold;
        private readonly float scrollFactor;

        public AccessibleRecyclerView(Context context) : base(context)
        {
            scrollbarTouchThreshold = ConvertDpToPx(context, 32f);
            scrollFactor = ResolveScrollFactor(context);
            InitializeFocusBehavior();
        }

        public AccessibleRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            scrollbarTouchThreshold = ConvertDpToPx(context, 32f);
            scrollFactor = ResolveScrollFactor(context);
            InitializeFocusBehavior();
        }

        public AccessibleRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            scrollbarTouchThreshold = ConvertDpToPx(context, 32f);
            scrollFactor = ResolveScrollFactor(context);
            InitializeFocusBehavior();
        }

        private void InitializeFocusBehavior()
        {
            Focusable = true;
            FocusableInTouchMode = true;
        }

        private static float ConvertDpToPx(Context context, float valueInDp)
        {
            float density = context?.Resources?.DisplayMetrics?.Density ?? 1f;
            return valueInDp * density;
        }

        private static float ResolveScrollFactor(Context context)
        {
            var configuration = ViewConfiguration.Get(context);
            if (configuration == null)
            {
                return 64f;
            }

            try
            {
                return ViewConfigurationCompat.GetScaledVerticalScrollFactor(configuration, context);
            }
            catch
            {
                return configuration.ScaledTouchSlop * 8f;
            }
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (HandlePagingKey(keyCode))
            {
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }

        private bool HandlePagingKey(Keycode keyCode)
        {
            int itemCount = Adapter?.ItemCount ?? 0;
            if (itemCount == 0)
            {
                return false;
            }

            switch (keyCode)
            {
                case Keycode.PageDown:
                    SmoothScrollBy(0, Height);
                    return true;
                case Keycode.PageUp:
                    SmoothScrollBy(0, -Height);
                    return true;
                case Keycode.MoveHome:
                case Keycode.Home:
                    ScrollToPosition(0);
                    return true;
                case Keycode.MoveEnd:
                case Keycode.End:
                    ScrollToPosition(itemCount - 1);
                    return true;
                default:
                    return false;
            }
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            if ((e.Source & InputSourceType.ClassPointer) != 0 && e.Action == MotionEventActions.Scroll)
            {
                float verticalScroll = e.GetAxisValue(Axis.Vscroll);
                if (Math.Abs(verticalScroll) > float.Epsilon)
                {
                    int delta = (int)(-verticalScroll * scrollFactor);
                    ScrollBy(0, delta);
                    return true;
                }
            }
            return base.OnGenericMotionEvent(e);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            bool handled = false;
            if (e.Action == MotionEventActions.Down && IsTouchOnScrollbar(e.GetX()))
            {
                JumpToScrollPosition(e.GetY());
                handled = true;
            }
            return handled || base.OnTouchEvent(e);
        }

        private bool IsTouchOnScrollbar(float touchX)
        {
            if (LayoutDirection == LayoutDirection.Rtl)
            {
                return touchX <= scrollbarTouchThreshold;
            }

            return touchX >= Width - scrollbarTouchThreshold;
        }

        private void JumpToScrollPosition(float touchY)
        {
            int itemCount = Adapter?.ItemCount ?? 0;
            if (itemCount == 0 || Height == 0)
            {
                return;
            }

            float ratio = Math.Max(0f, Math.Min(1f, touchY / Height));
            int target = (int)Math.Round((itemCount - 1) * ratio);
            ScrollToPosition(target);
        }
    }
}
