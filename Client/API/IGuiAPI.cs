﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Client
{
    public interface IGuiAPI
    {
        /// <summary>
        /// List of all registered guis
        /// </summary>
        List<GuiDialog> LoadedGuis { get; }

        /// <summary>
        /// List of all currently opened guis
        /// </summary>
        List<GuiDialog> OpenedGuis { get; }


        /// <summary>
        /// A utility class that does text texture generation for you
        /// </summary>
        TextUtil Text { get; }

        /// <summary>
        /// A utlity class that helps you figure out the dimensions of text
        /// </summary>
        TextSizeProber TextSizeProber { get; }

        /// <summary>
        /// A utility class that contains a bunch of hardcoded icons
        /// </summary>
        IconUtil Icons { get; }
        

        /// <summary>
        /// Creates a new gui composition
        /// </summary>
        /// <param name="dialogName"></param>
        /// <param name="bounds"></param>
        /// <param name="doCache"></param>
        /// <param name="setWindowBounds"></param>
        /// <returns></returns>
        GuiComposer CreateCompo(string dialogName, ElementBounds bounds, bool doCache = true, bool setWindowBounds = true);


        /// <summary>
        /// Register given dialog(s) to the gui/input event listening chain. You only need to call this if your dialog has to listen to events 
        /// even while closed. The method GuiDialog.TryOpen() also does the register if not registered already.
        /// </summary>
        /// <param name="guiClassName"></param>
        /// <param name="parameters"></param>
        void RegisterDialog(params GuiDialog[] dialogs);

        /// <summary>
        /// Removes given texture from graphics card memory
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        void DeleteTexture(int textureid);


        /// <summary>
        /// Load the contents of a cairo surface into a opengl texture. Returns the texture id
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="linearMag"></param>
        /// <returns></returns>
        int LoadCairoTexture(Cairo.ImageSurface surface, bool linearMag);


        /// <summary>
        /// Retrieve the saved dialog position from settings
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Vec2i GetDialogPosition(string key);

        /// <summary>
        /// Remember the dialog position for given dialog key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pos"></param>
        void SetDialogPosition(string key, Vec2i pos);

        /// <summary>
        /// Plays a sound, non location specific
        /// </summary>
        /// <param name="soundname"></param>
        void PlaySound(string soundname, bool randomizePitch = false);

        void PlaySound(AssetLocation soundname, bool randomizePitch = false);

        void RequestFocus(GuiDialog guiDialog);
        void TriggerDialogOpened(GuiDialog guiDialog);
        void TriggerDialogClosed(GuiDialog guiDialog);
    }
}