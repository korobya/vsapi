﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vintagestory.API.MathTools
{
    /// <summary>
    /// Many utility methods and fields for working with colors
    /// </summary>
    public class ColorUtil
    {
        /// <summary>
        /// Amount of bits per block that are available to store the hue value
        /// </summary>
        public const int HueBits = 6;
        /// <summary>
        /// Amount of bits per block that are available to store the saturation value
        /// </summary>
        public const int SatBits = 3;
        /// <summary>
        /// Amount of bits per block that are available to store the brightness value
        /// </summary>
        public const int BrightBits = 5;

        public const int HueMul = 4;
        public const int SatMul = 32;
        public const int BrightMul = 8;

        public static int HueQuantities = (int)Math.Pow(2, HueBits);
        public static int SatQuantities = (int)Math.Pow(2, SatBits);
        public static int BrightQuantities = (int)Math.Pow(2, BrightBits);

        /// <summary>
        /// 255 &lt;&lt; 24
        /// </summary>
        public const int OpaqueAlpha = 255 << 24;
        /// <summary>
        /// ~(255 &lt;&lt; 24)
        /// </summary>
        public const int ClearAlpha = ~(255 << 24);

        /// <summary>
        /// White opaque color as normalized float values (0..1)
        /// </summary>
        public static readonly Vec3f WhiteRgbVec = new Vec3f(1, 1, 1);
        /// <summary>
        /// White opaque color as normalized float values (0..1)
        /// </summary>
        public static readonly Vec4f WhiteArgbVec = new Vec4f(1, 1, 1, 1);
        /// <summary>
        /// White opaque color as normalized float values (0..1)
        /// </summary>
        public static readonly float[] WhiteArgbFloat = new float[] { 1, 1, 1, 1 };
        /// <summary>
        /// White opaque color as normalized float values (0..1)
        /// </summary>
        public static readonly double[] WhiteArgbDouble = new double[] { 1, 1, 1, 1 };
        /// <summary>
        /// White opaque argb color as bytes (0..255)
        /// </summary>
        public static readonly byte[] WhiteArgbBytes = new byte[] { 255, 255, 255, 255 };
        /// <summary>
        /// White opaque ahsv color as bytes (0..255)
        /// </summary>
        public static readonly byte[] WhiteAhsvBytes = new byte[] { 255, 0, 0, 255 };

        /// <summary>
        /// White opaque argb color
        /// </summary>
        public static readonly int WhiteArgb = ColorFromArgb(255, 255, 255, 255);
        /// <summary>
        /// White opaque AHSV color
        /// </summary>
        public static readonly int WhiteAhsl = ColorFromArgb(255, 255, 0, 0);
        /// <summary>
        /// Black opaque rgb color
        /// </summary>
        public static readonly int BlackArgb = ColorFromArgb(255, 0, 0, 0);
        /// <summary>
        /// Black opaque rgb color
        /// </summary>
        public static readonly Vec3f BlackRgbVec = new Vec3f(0, 0, 0);
        /// <summary>
        /// Black opaque rgb color
        /// </summary>
        public static readonly Vec4f BlackArgbVec = new Vec4f(0, 0, 0, 255);
        /// <summary>
        /// White opaque color as normalized float values (0..1)
        /// </summary>
        public static readonly double[] BlackArgbDouble = new double[] { 0,0,0, 1 };


        public static byte[] ReverseColorBytes(byte[] color)
        {
            return new byte[]
            {
                color[2],
                color[1],
                color[0],
                color[3]
            };
        }

        public static int ReverseColorBytes(int color)
        {
            return
                (int)(color & 0xff000000) |
                (((color >> 16) & 0xff) << 0) |
                (((color >> 8) & 0xff) << 8) |
                ((color & 0xff) << 16)
            ;
        }
        
        /// <summary>
        /// Splits up a 32bit int color into 4 1 byte components, in BGRA order (Alpha channel at the highest 8 bits)
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static byte[] ToBGRABytes(int color)
        {
            return new byte[]
            {
                (byte)((color >> 0) & 0xff),
                (byte)((color >> 8) & 0xff),
                (byte)((color >> 16) & 0xff),
                (byte)((color >> 24) & 0xff)
            };
        }

        /// <summary>
        /// Splits up a 32bit int color into 4 1 byte components, in RGBA order
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static byte[] ToRGBABytes(int color)
        {
            return new byte[]
            {
                (byte)((color >> 16) & 0xff),
                (byte)((color >> 8) & 0xff),
                (byte)((color >> 0) & 0xff),
                (byte)((color >> 24) & 0xff)
            };
        }

        /// <summary>
        /// Returns a 4 element rgb float with values between 0..1
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static float[] ToRGBAFloats(int color)
        {
            return new float[]
            {
                ((color >> 16) & 0xff) / 255f,
                ((color >> 8) & 0xff) / 255f,
                ((color >> 0) & 0xff) / 255f,
                ((color >> 24) & 0xff) / 255f
            };
        }


        /// <summary>
        /// Returns a 4 element rgb float with values between 0..1
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Vec3f ToRGBVec3f(int color, ref Vec3f outVal)
        {
            return outVal.Set(
                ((color >> 16) & 0xff) / 255f,
                ((color >> 8) & 0xff) / 255f,
                ((color >> 0) & 0xff) / 255f
            );
        }

        public static Vec4f ToRGBAVec4f(int color, ref Vec4f outVal)
        {
            return outVal.Set(
                ((color >> 16) & 0xff) / 255f,
                ((color >> 8) & 0xff) / 255f,
                ((color >> 0) & 0xff) / 255f,
                ((color >> 24) & 0xff) / 255f
            );
        }


        /// <summary>
        /// Returns a 4 element rgb double with values between 0..1
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static double[] ToRGBADoubles(int color)
        {
            return new double[]
            {
                ((color >> 16) & 0xff) / 255.0,
                ((color >> 8) & 0xff) / 255.0,
                ((color >> 0) & 0xff) / 255.0,
                ((color >> 24) & 0xff) / 255.0
            };
        }


        /// <summary>
        /// Multiplies two colors together: c=(a*b)/255
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        public static byte[] ColorMultiply(byte[] color1, byte[] color2)
        {
            return new byte[]
            {
                (byte)((color1[0] * color2[0]) / 255),
                (byte)((color1[1] * color2[1]) / 255),
                (byte)((color1[2] * color2[2]) / 255)
            };
        }


        /// <summary>
        /// Multiplies two colors together c=(a*b)/255
        /// </summary>
        /// <param name="color"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        public static int ColorMultiplyEach(int color, int color2)
        {
            // yaaaay bracket party (o) )o( o// \o/
            // could probably save on some bit masking
            return
                (((((color >> 24) & 0xff) * ((color2 >> 24) & 0xff)) / 255) << 24) |
                (((((color >> 16) & 0xff) * ((color2 >> 16) & 0xff)) / 255) << 16) |
                (((((color >> 8) & 0xff) * ((color2 >> 8) & 0xff)) / 255) << 8) |
                (((color & 0xff) * ((color2 >> 0) & 0xff)) / 255)
            ;
        }


        /// <summary>
        /// Multiplies a float value to the rgb color channels, leaves alpha channel unchanged
        /// </summary>
        /// <param name="color"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static int ColorMultiply3(int color, float multiplier)
        {
            return
                (int)(color & 0xff000000) |
                ((int)(((color >> 16) & 0xff) * multiplier) << 16) |
                ((int)(((color >> 8) & 0xff) * multiplier) << 8) |
                (int)((color & 0xff) * multiplier)
            ;
        }
        



        /// <summary>
        /// Multiplies a float value to every color channel including the alpha component.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static int ColorMultiply4(int color, float multiplier)
        {
            return
                ((int)(((color >> 24) & 0xff) * multiplier) << 24) |
                ((int)(((color >> 16) & 0xff) * multiplier) << 16) |
                ((int)(((color >> 8) & 0xff) * multiplier) << 8) |
                (int)((color & 0xff) * multiplier)
            ;
        }


        /// <summary>
        /// Averages several colors together in RGB space
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int ColorAverage(int[] colors, float[] weights)
        {
            int r = 0;
            int g = 0;
            int b = 0;

            for (int i = 0; i < colors.Length; i++)
            {
                r += (int)(weights[i] * ((colors[i] >> 16) & 0xff));
                g += (int)(weights[i] * ((colors[i] >> 8) & 0xff));
                b += (int)(weights[i] * (colors[i] & 0xff));
            }

            return
                (Math.Min(255, r) << 16) + (Math.Min(255, g) << 8) + Math.Min(255, b);
            ;
        }

        /// <summary>
        /// Overlays rgb2 over rgb1
        /// When c2weight = 0 resulting color is color1, when c2weight = 1 then resulting color is color2
        /// Resulting color alpha value is 100% color1 alpha
        /// </summary>
        /// <param name="rgb1"></param>
        /// <param name="rgb2"></param>
        /// <param name="c2weight"></param>
        /// <returns></returns>
        public static int ColorOverlay(int rgb1, int rgb2, float c2weight)
        {
            return
                (((rgb1 >> 24) & 0xff) << 24) |
                ((int)(((rgb1 >> 16) & 0xff) * (1 - c2weight) + c2weight * ((rgb2 >> 16) & 0xff)) << 16) |
                ((int)(((rgb1 >> 8) & 0xff) * (1 - c2weight) + c2weight * ((rgb2 >> 8) & 0xff)) << 8) |
                ((int)((rgb1 & 0xff) * (1 - c2weight) + c2weight * ((rgb2 >> 0) & 0xff)))
            ;
        }




        /// <summary>
        /// Overlays rgb1 on top of rgb2, based on their alph values
        /// </summary>
        /// <param name="rgb1"></param>
        /// <param name="rgb2"></param>
        /// <returns></returns>
        public static int ColorOver(int rgb1, int rgb2)
        {
            float a1 = ((rgb1 >> 24) & 0xff) / 255f;
            float a2 = ((rgb2 >> 24) & 0xff) / 255f;

            if (a1 == 0 && a2 == 0) return 0;

            return
                ((int)(255*(a1 + a2 * (1 - a1))) << 24) |
                (ValueOverlay((rgb1 >> 16) & 0xff, a1, (rgb2 >> 16) & 0xff, a2) << 16) |
                (ValueOverlay((rgb1 >> 8) & 0xff, a1, (rgb2 >> 8) & 0xff, a2) << 8) |
                ValueOverlay(rgb1 & 0xff, a1, rgb2 & 0xff, a2)
            ;
        }

        public static int ValueOverlay(int c1, float a1, int c2, float a2)
        {
            return (int) (
                (c1 * a1 + c2 * a2 * (1 - a1)) /
                (a1 + a2 * (1 - a1))
            );
        }

        /// <summary>
        /// Combines two HSV colors by converting them to rgb then back to hsv. Uses the brightness as a weighting factor. Also leaves the brightness at the max of both hsv colors.
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="s1"></param>
        /// <param name="v1"></param>
        /// <param name="h2"></param>
        /// <param name="s2"></param>
        /// <param name="v2"></param>
        /// <returns>Combined HSV Color</returns>
        public static int[] ColorCombineHSV(int h1, int s1, int v1, int h2, int s2, int v2)
        {
            int[] rgb1 = HSV2RGBInts(h1, s1, v1);
            int[] rgb2 = HSV2RGBInts(h2, s2, v2);

            float leftweight = (float)v1 / (v1 + v2);
            float rightweight = 1 - leftweight;

            int[] hsv = RGB2HSVInts(
                (int)(rgb1[0] * leftweight + rgb2[0] * rightweight),
                (int)(rgb1[1] * leftweight + rgb2[1] * rightweight),
                (int)(rgb1[2] * leftweight + rgb2[2] * rightweight)
            );

            hsv[2] = Math.Max(v1, v2);

            return hsv;
        }


        /// <summary>
        /// Removes HSV2 from HSV1 by converting them to rgb then back to hsv. Uses the brightness as a weighting factor. Leaves brightness unchanged.
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="s1"></param>
        /// <param name="v1"></param>
        /// <param name="h2"></param>
        /// <param name="s2"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static int[] ColorSubstractHSV(int h1, int s1, int v1, int h2, int s2, int v2)
        {
            int[] rgb1 = HSV2RGBInts(h1, s1, v1);
            int[] rgb2 = HSV2RGBInts(h2, s2, v2);

            float leftweight = (float)(v1 + v2) / v1;
            float rightweight = 1 - (float)v1 / (v1 + v2);

            int[] hsv = RGB2HSVInts(
                (int)(rgb1[0] * leftweight - rgb2[0] * rightweight),
                (int)(rgb1[1] * leftweight - rgb2[1] * rightweight),
                (int)(rgb1[2] * leftweight - rgb2[2] * rightweight)
            );

            hsv[2] = v1;

            return hsv;
        }

        /// <summary>
        /// Pack the 4 color components into a single ARGB 32bit int
        /// </summary>
        /// <param name="a"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int ColorFromArgb(int a, int r, int g, int b)
        {
            int iCol = (a << 24) | (r << 16) | (g << 8) | b;
            return iCol;
        }

        /// <summary>
        /// Returns alpha value of given color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static int ColorA(int color)
        {
            byte a = (byte)(color >> 24);
            return a;
        }

        /// <summary>
        /// Returns red value of given color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static int ColorR(int color)
        {
            byte r = (byte)(color >> 16);
            return r;
        }

        /// <summary>
        /// Returns green value of given color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static int ColorG(int color)
        {
            byte g = (byte)(color >> 8);
            return g;
        }

        /// <summary>
        /// Returns blue value of given color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static int ColorB(int color)
        {
            byte b = (byte)(color);
            return b;
        }

        /// <summary>
        /// Returns human a readable string of given color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ColorToString(int color)
        {
            return ColorR(color) + ", " + ColorG(color) + ", " + ColorB(color) + ", " + ColorA(color);
        }


        internal static int Hex2Int(string hex)
        {
            return int.Parse(hex.Substring(1), System.Globalization.NumberStyles.HexNumber);
        }
        
        /// <summary>
        /// Converts given RGB values into it's respective HSV Representation (all values in range of 0-255)
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int RGB2HSV(int r, int g, int b)
        {
            float K = 0f;

            if (g < b)
            {
                int tmp = g; g = b; b = tmp;
                K = -1f;
            }

            if (r < g)
            {
                int tmp = r; r = g; g = tmp;
                K = -2f / 6f - K;
            }

            float chroma = r - Math.Min(g, b);
            return
                255 * (int)Math.Abs(K + (g - b) / (6f * chroma + 1e-20f)) << 16 |
                255 * (int)(chroma / (r + 1e-20f)) << 8 |
                r
            ;
        }


        /// <summary>
        /// Converts given RGB value into it's respective HSV Representation (all values in range of 0-255)
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static int RGB2HSV(int rgb)
        {
            float K = 0f;

            int r = (rgb >> 16) & 0xff;
            int g = (rgb >> 8) & 0xff;
            int b = rgb & 0xff;

            if (g < b)
            {
                int tmp = g; g = b; b = tmp;
                K = -1f;
            }

            if (r < g)
            {
                int tmp = r; r = g; g = tmp;
                K = -2f / 6f - K;
            }

            float chroma = r - Math.Min(g, b);
            return
                (int)(255 * Math.Abs(K + (g - b) / (6.0 * chroma + 1.0e-20))) << 16 |
                (int)(255 * chroma / (r + 1.0e-20)) << 8 |
                r
            ;
        }


        /// <summary>
        /// Converts given RGB values into it's respective HSV Representation (all values in range of 0-255)
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int[] RGB2HSVInts(int r, int g, int b)
        {
            float K = 0f;

            if (g < b)
            {
                int tmp = g; g = b; b = tmp;
                K = -1f;
            }

            if (r < g)
            {
                int tmp = r; r = g; g = tmp;
                K = -2f / 6f - K;
            }

            float chroma = r - Math.Min(g, b);
            return new int[] {
                (int)(255 * Math.Abs(K + (g - b) / (6.0 * chroma + 1.0e-20))),
                (int)(255 * chroma / (r + 1.0e-20)),
                r
            };
        }


        /// <summary>
        /// Converts given HSV value into it's respective RGB Representation (all values in range of 0-255)
        /// </summary>
        /// <param name="hsv"></param>
        /// <returns></returns>
        public static int HSV2RGB(int hsv)
        {
            int region, p, q, t;
            int remainder;

            int h = (hsv >> 16) & 0xff;
            int s = (hsv >> 8) & 0xff;
            int v = hsv & 0xff;

            if (s == 0)
            {
                return v << 16 | v << 8 | v;
            }

            region = h / 43;
            remainder = (h - (region * 43)) * 6;

            p = (v * (255 - s)) >> 8;
            q = (v * (255 - ((s * remainder) >> 8))) >> 8;
            t = (v * (255 - ((s * (255 - remainder)) >> 8))) >> 8;

            switch (region)
            {
                case 0:
                    return v << 16 | t << 8 | p;
                case 1:
                    return q << 16 | v << 8 | p;
                case 2:
                    return p << 16 | v << 8 | t;
                case 3:
                    return p << 16 | q << 8 | v;
                case 4:
                    return t << 16 | p << 8 | v;
                default:
                    return v << 16 | p << 8 | q;
            }
        }

        /// <summary>
        /// Converts given HSV values into it's respective RGB Representation (all values in range of 0-255)
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static int HSV2RGB(int h, int s, int v)
        {
            int region, p, q, t;
            int remainder;

            if (s == 0)
            {
                return v << 16 | v << 8 | v;
            }

            region = h / 43;
            remainder = (h - (region * 43)) * 6;

            p = (v * (255 - s)) >> 8;
            q = (v * (255 - ((s * remainder) >> 8))) >> 8;
            t = (v * (255 - ((s * (255 - remainder)) >> 8))) >> 8;

            switch (region)
            {
                case 0:
                    return v << 16 | t << 8 | p;
                case 1:
                    return q << 16 | v << 8 | p;
                case 2:
                    return p << 16 | v << 8 | t;
                case 3:
                    return p << 16 | q << 8 | v;
                case 4:
                    return t << 16 | p << 8 | v;
                default:
                    return v << 16 | p << 8 | q;
            }
        }

        /// <summary>
        /// Returns a fully opaque gray color with given brightness
        /// </summary>
        /// <param name="brightness"></param>
        /// <returns></returns>
        public static int GrayscaleColor(byte brightness)
        {
            return OpaqueAlpha | brightness << 16 | brightness << 8 | brightness;
        }

        /// <summary>
        /// Converts given HSB values into it's respective ARGB Representation (all values in range of 0-255, alpha always 255)
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static int HSV2ARGB(int h, int s, int v)
        {
            return HSV2ARGB(h, s, v, OpaqueAlpha);
        }

        /// <summary>
        /// Converts given HSB values into it's respective ARGB Representation (all values in range of 0-255)
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static int HSV2ARGB(int h, int s, int v, int a)
        {
            int region, p, q, t;
            int remainder;

            if (s == 0)
            {
                return a << 24 | v << 16 | v << 8 | v;
            }

            region = h / 43;
            remainder = (h - (region * 43)) * 6;

            p = (v * (255 - s)) >> 8;
            q = (v * (255 - ((s * remainder) >> 8))) >> 8;
            t = (v * (255 - ((s * (255 - remainder)) >> 8))) >> 8;

            switch (region)
            {
                case 0:
                    return a << 24 | p << 16 | t << 8 | v;
                case 1:
                    return a << 24 | p << 16 | v << 8 | q;
                case 2:
                    return a << 24 | t << 16 | v << 8 | p;
                case 3:
                    return a << 24 | v << 16 | q << 8 | p;
                case 4:
                    return a << 24 | v << 16 | p << 8 | t;
                default:
                    return a << 24 | q << 16 | p << 8 | v;
            }
        }

        /// <summary>
        /// Converts given HSB values into it's respective HSV Representation (all values in range of 0-255)
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static int[] HSV2RGBInts(int h, int s, int v)
        {
            int region, p, q, t;
            int remainder;

            if (s == 0)
            {
                return new int[] { v, v, v };
            }

            region = h / 43;
            remainder = (h - (region * 43)) * 6;

            p = (v * (255 - s)) >> 8;
            q = (v * (255 - ((s * remainder) >> 8))) >> 8;
            t = (v * (255 - ((s * (255 - remainder)) >> 8))) >> 8;

            switch (region)
            {
                case 0:
                    return new int[] { v, t, p };
                case 1:
                    return new int[] { q, v, p };
                case 2:
                    return new int[] { p, v, t };
                case 3:
                    return new int[] { p, q, v };
                case 4:
                    return new int[] { t, p, v };
                default:
                    return new int[] { v, p, q };
            }
        }

        /// <summary>
        /// Converts given HSVA values into it's respective RGBA Representation (all values in range of 0-255)
        /// </summary>
        /// <param name="hsva"></param>
        /// <returns></returns>
        public static byte[] HSVa2RGBaBytes(byte[] hsva)
        {
            int region;
            int p, q, t;
            int remainder;

            int h = hsva[0];
            int s = hsva[1];
            int v = hsva[2];


            if (v == 0)
            {
                return new byte[] { hsva[2], hsva[2], hsva[2], hsva[3] };
            }

            region = h / 43;
            remainder = (hsva[0] - (region * 43)) * 6;

            p = (v * (255 - s)) >> 8;
            q = (v * (255 - ((s * remainder) >> 8))) >> 8;
            t = (v * (255 - ((s * (255 - remainder)) >> 8))) >> 8;

            switch (region)
            {
                case 0:
                    return new byte[] { hsva[2], (byte)t, (byte)p, hsva[3] };
                case 1:
                    return new byte[] { (byte)q, hsva[2], (byte)p, hsva[3] };
                case 2:
                    return new byte[] { (byte)p, hsva[2], (byte)t, hsva[3] };
                case 3:
                    return new byte[] { (byte)p, (byte)q, hsva[2], hsva[3] };
                case 4:
                    return new byte[] { (byte)t, (byte)p, hsva[2], hsva[3] };
                default:
                    return new byte[] { hsva[2], (byte)p, (byte)q, hsva[3] };
            }
        }



        // According to https://en.wikipedia.org/wiki/Incandescence
        public static int[] getIncandescenceColor(int temperature)
        {
            if (temperature < 520) return new int[] { 0, 0, 0, 0 };

            return new int[]{
                Math.Max(0, Math.Min(255, ((temperature - 500) * 255) / 400)),
                Math.Max(0, Math.Min(255, ((temperature - 900) * 255) / 200)),
                Math.Max(0, Math.Min(255, ((temperature - 1100) * 255) / 200)),
                Math.Max(0, Math.Min(96, (temperature - 525) / 2))
            };
        }

        public static float[] getIncandescenceColorAsColor4f(int temperature)
        {
            if (temperature < 500) return new float[] { 0f, 0f, 0f, 0f };

            return new float[]{
                Math.Max(0f, Math.Min(1, (temperature - 500) / 400f)),
                Math.Max(0f, Math.Min(1, (temperature - 900) / 200f)),
                Math.Max(0f, Math.Min(1, (temperature - 1100)/ 200f)),
                Math.Max(0f, Math.Min(0.38f, (temperature - 525) / 2f))
            };
        }

    }
}
