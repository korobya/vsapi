﻿using System;
using Cairo;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common;
using Vintagestory.API.Client;

namespace Vintagestory.API.Client
{
    public delegate string InfoTextDelegate(IItemStack stack);

    class GuiElementItemstackInfo : GuiElementTextBase
    {
        public static double ItemStackSize = GuiElementPassiveItemSlot.unscaledItemSize * 2.5;
        public static int MarginTop = 24;
        public static int BoxWidth = 400;
        public static int MinBoxHeight = 80;

        static double[] backTint = ElementGeometrics.DialogStrongBgColor;
        static double[] textTint = ElementGeometrics.DialogDefaultTextColor;

        ItemStack itemstack;


        GuiElementStaticText titleElement;
        GuiElementStaticText descriptionElement;

        int textureId;
        double maxWidth;

        InfoTextDelegate OnRequireInfoText;

        public GuiElementItemstackInfo(ICoreClientAPI capi, ElementBounds bounds, InfoTextDelegate OnRequireInfoText) : base(capi, "", CairoFont.WhiteSmallText(), bounds)
        {
            this.OnRequireInfoText = OnRequireInfoText;

            ElementBounds textBounds = bounds.CopyOnlySize();

            descriptionElement = new GuiElementStaticText(capi, "", EnumTextOrientation.Left, textBounds.CopyOffsetedSibling(ItemStackSize + 50, MarginTop, -ItemStackSize - 50, 0), Font);

            CairoFont titleFont = Font.Clone();
            titleFont.FontWeight = FontWeight.Bold;
            titleElement = new GuiElementStaticText(capi, "", EnumTextOrientation.Left, textBounds, titleFont);

            maxWidth = bounds.fixedWidth;
        }


        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            Recompose();
        }

        void RecalcBounds(string title, string desc)
        {
            double currentWidth = 0;
            double currentHeight = 0;
            string[] lines = desc.Split(new char[] { '\n' });
            
            

            for (int i = 0; i < lines.Length; i++)
            {
                currentWidth = Math.Max(currentWidth, descriptionElement.Font.GetTextExtents(lines[i]).Width / ClientSettingsApi.GUIScale + 10);
            }

            currentWidth += 40 + scaled(GuiElementPassiveItemSlot.unscaledItemSize) * 3;
            currentWidth = Math.Max(currentWidth, titleElement.Font.GetTextExtents(title).Width / ClientSettingsApi.GUIScale + 10);
            currentWidth = Math.Min(currentWidth, maxWidth);

            double descWidth = currentWidth - scaled(ItemStackSize) - 50;

            Bounds.fixedWidth = currentWidth;
            descriptionElement.Bounds.fixedWidth = descWidth;
            titleElement.Bounds.fixedWidth = currentWidth;
            descriptionElement.Bounds.CalcWorldBounds();

            // Height depends on the width
            double lineheight = descriptionElement.GetMultilineTextHeight();
            currentHeight = Math.Max(lineheight, 50 + scaled(GuiElementPassiveItemSlot.unscaledItemSize) * 3);
            titleElement.Bounds.fixedHeight = currentHeight;
            descriptionElement.Bounds.fixedHeight = currentHeight;
            Bounds.fixedHeight = currentHeight / ClientSettingsApi.GUIScale;

            //Console.WriteLine("bounds recalced heightis now " + bounds.fixedHeight);
        }


        void Recompose()
        {
            if (itemstack == null) return;

            string title = itemstack.GetName();
            string desc = OnRequireInfoText(itemstack);
            desc.TrimEnd();


            titleElement.SetValue(title);
            descriptionElement.SetValue(desc);

            RecalcBounds(title, desc);


            Bounds.CalcWorldBounds();

            ElementBounds textBounds = Bounds.CopyOnlySize();
            textBounds.CalcWorldBounds();


            ImageSurface surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            Context ctx = genContext(surface);

            ctx.SetSourceRGBA(0, 0, 0, 0);
            ctx.Paint();

            ctx.SetSourceRGBA(backTint[0], backTint[1], backTint[2], backTint[3]);
            RoundRectangle(ctx, textBounds.bgDrawX, textBounds.bgDrawY, textBounds.OuterWidthInt, textBounds.OuterHeightInt, ElementGeometrics.DialogBGRadius);
            ctx.FillPreserve();
            ctx.SetSourceRGBA(backTint[0] / 2, backTint[1] / 2, backTint[2] / 2, backTint[3]);
            ctx.Stroke();

            ctx.SetSourceRGBA(ElementGeometrics.DialogAlternateBgColor);
            RoundRectangle(ctx, textBounds.drawX, textBounds.drawY + scaled(MarginTop), scaled(ItemStackSize) + scaled(40), scaled(ItemStackSize) + scaled(40), 0);
            ctx.Fill();


            titleElement.ComposeElements(ctx, surface);

            descriptionElement.ComposeElements(ctx, surface);

            generateTexture(surface, ref textureId);

            ctx.Dispose();
            surface.Dispose();
        }


        public override void RenderInteractiveElements(float deltaTime)
        {
            if (itemstack == null) return;

            api.Render.Render2DTexturePremultipliedAlpha(textureId, Bounds, 1000);

            double offset = (int)scaled(30 + ItemStackSize/2);

            api.Render.RenderItemstackToGui(
                itemstack,
                (int)Bounds.renderX + offset,
                (int)Bounds.renderY + offset + (int)scaled(MarginTop), 
                1000 + scaled(GuiElementPassiveItemSlot.unscaledItemSize) * 2, 
                (float)scaled(ItemStackSize), 
                ColorUtil.WhiteArgb,
                true,
                true,
                false
            );
        }



        
        


        public ItemStack GetItemStack()
        {
            return itemstack;
        }

        public void SetItemstack(ItemStack itemstack)
        {
            bool recompose = this.itemstack == null || !this.itemstack.Equals(itemstack);

            this.itemstack = itemstack;

            if (itemstack == null)
            {
                Bounds.fixedHeight = 0;
            } else
            {
                // Whats this good for? This makes overlays weirdly squeezed
               /* bounds.fixedHeight = Math.Max(MinBoxHeight, 
                    MarginTop + descriptionElement.GetMultilineTextHeight(OnRequireInfoText(itemstack), bounds.fixedWidth - 30) / ClientSettingsApi.GUIScale
                );*/
                //Console.WriteLine("set height to {0}", bounds.fixedHeight);
            }

            if (recompose) Recompose();
        }

    }
}