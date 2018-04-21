using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AutoBluetooth.Views
{
    public class CircleButton : Button
    {
        public CircleButton(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public CircleButton(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            SetBackgroundResource(Resource.Drawable.Circle);
        }

        protected override void OnDraw(Canvas canvas)
        {
            SetHeight(Width);

            base.OnDraw(canvas);
        }
    }
}