﻿using Cairo;
using Vintagestory.API.Client;

namespace Vintagestory.API.Client
{
    public class GuiElementChatInput : GuiElementEditableTextBase
    {
        int highlightTextureId;
        ElementBounds highlightBounds;

        public GuiElementChatInput(ICoreClientAPI capi, ElementBounds bounds, API.Common.Action<string> OnTextChanged) : base(capi, null, bounds)
        {
            this.OnTextChanged = OnTextChanged;
            this.caretColor = new float[] { 1, 1, 1, 1 };
            this.Font = CairoFont.WhiteSmallText();
        }

        public override void ComposeTextElements(Context ctx, ImageSurface surface)
        {
            ctx.LineWidth = 1;

            // Vertical seperator line above the bounds
            ctx.NewPath();
            ctx.MoveTo(Bounds.drawX + 1, Bounds.drawY);
            ctx.LineTo(Bounds.drawX + 1 + Bounds.InnerWidth, Bounds.drawY);
            ctx.ClosePath();
            ctx.SetSourceRGBA(1, 1, 1, 0.7);
            ctx.Stroke();


            ctx.NewPath();
            ctx.MoveTo(Bounds.drawX + 1, Bounds.drawY + 1);
            ctx.LineTo(Bounds.drawX + 1 + Bounds.InnerWidth, Bounds.drawY + 1);
            ctx.ClosePath();
            ctx.SetSourceRGBA(0, 0, 0, 0.7);
            ctx.Stroke();

            ImageSurface surfaceHighlight = new ImageSurface(Format.Argb32, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight);
            Context ctxHighlight = genContext(surfaceHighlight);

            ctxHighlight.SetSourceRGBA(0, 0, 0, 0);
            ctxHighlight.Paint();

            ctxHighlight.SetSourceRGBA(1, 1, 1, 0.1);
            ctxHighlight.Paint();

            generateTexture(surfaceHighlight, ref highlightTextureId);

            ctxHighlight.Dispose();
            surfaceHighlight.Dispose();

            highlightBounds = Bounds.CopyOffsetedSibling().WithFixedPadding(0, 0).FixedGrow(2 * Bounds.absPaddingX, 2 * Bounds.absPaddingY);
            highlightBounds.CalcWorldBounds();

            RecomposeText();
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            if (hasFocus)
            {
                api.Render.GlToggleBlend(true, EnumBlendMode.Standard);
                api.Render.Render2DTexturePremultipliedAlpha(highlightTextureId, highlightBounds);
                api.Render.GlToggleBlend(true, EnumBlendMode.Standard);
            }

            api.Render.GlScissor((int)(Bounds.renderX), (int)(api.Render.FrameHeight - Bounds.renderY - Bounds.InnerHeight), Bounds.OuterWidthInt + 1 - (int)rightSpacing, Bounds.OuterHeightInt + 1 - (int)bottomSpacing);
            api.Render.GlScissorFlag(true);
            api.Render.Render2DTexturePremultipliedAlpha(textTextureId, Bounds.renderX - renderLeftOffset, Bounds.renderY, textSize.X, textSize.Y);
            api.Render.GlScissorFlag(false);
            
            base.RenderInteractiveElements(deltaTime);
        }
    }



    public static partial class GuiComposerHelpers
    {
        public static GuiComposer AddChatInput(this GuiComposer composer, ElementBounds bounds, API.Common.Action<string> OnTextChanged, string key = null)
        {
            if (!composer.composed)
            {
                composer.AddInteractiveElement(new GuiElementChatInput(composer.Api, bounds, OnTextChanged), key);
            }

            return composer;
        }

        public static GuiElementChatInput GetChatInput(this GuiComposer composer, string key)
        {
            return (GuiElementChatInput)composer.GetElement(key);
        }



    }
}