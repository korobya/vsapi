﻿using Cairo;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Client
{
    /// <summary>
    /// Creates a toggle button for the GUI.
    /// </summary>
    public class GuiElementColorListPicker : GuiElementElementListPickerBase<int>
    {
        public GuiElementColorListPicker(ICoreClientAPI capi, int elem, ElementBounds bounds) : base(capi, elem, bounds)
        {
        }

        public override void DrawElement(int color, Context ctx, ImageSurface surface)
        {
            double[] dcolor = ColorUtil.ToRGBADoubles(color);
            ctx.SetSourceRGBA(dcolor[0], dcolor[1], dcolor[2], 1);
            RoundRectangle(ctx, Bounds.drawX, Bounds.drawY, Bounds.InnerWidth, Bounds.InnerHeight, 1);
            ctx.Fill();
        }
    }


    public static partial class GuiComposerHelpers
    {
        /// <summary>
        /// Gets the toggle button by name in the GUIComposer.
        /// </summary>
        /// <param name="key">The name of the button.</param>
        /// <returns>A button.</returns>
        public static GuiElementColorListPicker GetColorListPicker(this GuiComposer composer, string key)
        {
            return (GuiElementColorListPicker)composer.GetElement(key);
        }


        /// <summary>
        /// Toggles the given button.
        /// </summary>
        /// <param name="key">The name of the button that was set.</param>
        /// <param name="selectedIndex">the index of the button.</param>
        public static void ColorListPickerSetValue(this GuiComposer composer, string key, int selectedIndex)
        {
            int i = 0;
            GuiElementColorListPicker btn;
            while ((btn = composer.GetColorListPicker(key + "-" + i)) != null)
            {
                btn.SetValue(i == selectedIndex);
                i++;
            }
        }


        /// <summary>
        /// Adds multiple buttons with Text.
        /// </summary>
        /// <param name="texts">The texts on all the buttons.</param>
        /// <param name="font">The font for the buttons</param>
        /// <param name="onToggle">The event fired when the button is pressed.</param>
        /// <param name="bounds">The bounds of the buttons.</param>
        /// <param name="key">The key given to the bundle of buttons.</param>
        public static GuiComposer AddColorListPicker(this GuiComposer composer, int[] colors, Action<int> onToggle, ElementBounds startBounds, int maxLineWidth, string key = null)
        {
            return AddElementListPicker<int>(composer, typeof(GuiElementColorListPicker), colors, onToggle, startBounds, maxLineWidth, key);
        }




    }

}
