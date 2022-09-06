﻿using Cairo;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.API.Client
{
    /// <summary>
    /// Creates a toggle button for the GUI.
    /// </summary>
    public class GuiElementIconListPicker : GuiElementElementListPickerBase<string>
    {
        public GuiElementIconListPicker(ICoreClientAPI capi, string elem, ElementBounds bounds) : base(capi, elem, bounds)
        {
        }

        public override void DrawElement(string icon, Context ctx, ImageSurface surface)
        {
            ctx.SetSourceRGBA(1,1,1,0.2);
            RoundRectangle(ctx, Bounds.drawX, Bounds.drawY, Bounds.InnerWidth, Bounds.InnerHeight, 1);
            ctx.Fill();

            api.Gui.Icons.DrawIcon(ctx, "wp" + icon.UcFirst(), Bounds.drawX + 2, Bounds.drawY + 2, Bounds.InnerWidth - 4, Bounds.InnerHeight - 4, new double[] { 1,1,1,1 });
        }
    }


    public static partial class GuiComposerHelpers
    {
        /// <summary>
        /// Gets the toggle button by name in the GUIComposer.
        /// </summary>
        /// <param name="key">The name of the button.</param>
        /// <returns>A button.</returns>
        public static GuiElementIconListPicker GetIconListPicker(this GuiComposer composer, string key)
        {
            return (GuiElementIconListPicker)composer.GetElement(key);
        }


        /// <summary>
        /// Toggles the given button.
        /// </summary>
        /// <param name="key">The name of the button that was set.</param>
        /// <param name="selectedIndex">the index of the button.</param>
        public static void IconListPickerSetValue(this GuiComposer composer, string key, int selectedIndex)
        {
            int i = 0;
            GuiElementIconListPicker  btn;
            while ((btn = composer.GetIconListPicker(key + "-" + i)) != null)
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
        public static GuiComposer AddIconListPicker(this GuiComposer composer, string[] icons, Action<int> onToggle, ElementBounds startBounds, int maxLineWidth, string key = null)
        {
            return AddElementListPicker<string>(composer, typeof(GuiElementIconListPicker), icons, onToggle, startBounds, maxLineWidth, key);
        }




    }

}
