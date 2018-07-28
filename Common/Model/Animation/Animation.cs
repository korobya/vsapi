﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Common
{
    public enum EnumEntityActivityStoppedHandling
    {
        PlayTillEnd = 0,
        Rewind = 1,
        Stop = 2,
        EaseOut = 3
    }

    public enum EnumEntityAnimationEndHandling
    {
        Repeat = 0,
        Hold = 1,
        Stop = 2
    }




    /// <summary>
    /// Represents a shape animation and can calculate the transformation matrices for each frame to be sent to the shader
    /// Process
    /// 1. For each frame, for each root element, calculate the transformation matrix. Curent model matrix is identy mat.
    /// 1.1. Get previous and next key frame. Apply translation, rotation and scale to model matrix.
    /// 1.2. Store this matrix as animationmatrix in list
    /// 1.3. For each child element
    /// 1.3.1. Multiply local transformation matrix with the animation matrix. This matrix is now the curent model matrix. Go to 1 with child elements as root elems
    /// 
    /// 2. For each frame, for each joint
    /// 2.1. Calculate the inverse model matrix 
    /// 2.2. Multiply stored animationmatrix with the inverse model matrix
    /// 
    /// 3. done
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Animation
    {
        [JsonProperty]
        public int QuantityFrames;
    
        [JsonProperty]
        public string Name;

        [JsonProperty]
        public string Code;

        [JsonProperty]
        public AnimationKeyFrame[] KeyFrames;

        [JsonProperty]
        public EnumEntityActivityStoppedHandling OnActivityStopped = EnumEntityActivityStoppedHandling.Rewind;

        [JsonProperty]
        public EnumEntityAnimationEndHandling OnAnimationEnd = EnumEntityAnimationEndHandling.Repeat;

        public uint CodeCrc32;

        public AnimationFrame[] AllFrames;
        public AnimationKeyFrame[][] PrevNextKeyFrameByFrame;
        protected HashSet<int> jointsDone = new HashSet<int>();


        /// <summary>
        /// Compiles the animation into a bunch of matrices, 31 matrices per frame.
        /// </summary>
        public void GenerateAllFrames(ShapeElement[] rootElements, Dictionary<int, AnimationJoint> jointsById)
        {
            CodeCrc32 = GameMath.Crc32(Code.ToLowerInvariant());

            AllFrames = new AnimationFrame[QuantityFrames];

            for (int i = 0; i < AllFrames.Length; i++)
            {
                AllFrames[i] = new AnimationFrame();
            }

            if (KeyFrames.Length == 0) return;

            for (int i = 0; i < AllFrames.Length; i++)
            {
                jointsDone.Clear();
                GenerateFrame(i, rootElements, jointsById, Mat4f.Create(), AllFrames[i].RootElementTransforms);
            }

            for (int i = 0; i < AllFrames.Length; i++)
            {
                AllFrames[i].FinalizeMatrices(jointsById);
            }
        }



        protected void GenerateFrame(int frameNumber, ShapeElement[] elements, Dictionary<int, AnimationJoint> jointsById, float[] modelMatrix, List<ElementPose> transforms)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                ShapeElement element = elements[i];
                element.CacheInverseTransformMatrix();

                ElementPose animTransform = new ElementPose();
                animTransform.ForElement = element;

                GenerateFrameForElement(frameNumber, element, ref animTransform);
                transforms.Add(animTransform);


                float[] animModelMatrix = Mat4f.CloneIt(modelMatrix);
                Mat4f.Mul(animModelMatrix, animModelMatrix, element.GetLocalTransformMatrix(null, animTransform));

                if (element.JointId > 0 && !jointsDone.Contains(element.JointId))
                {
                    AllFrames[frameNumber].SetTransform(element.JointId, animModelMatrix);
                    jointsDone.Add(element.JointId);
                }

                if (element.Children != null)
                {
                    GenerateFrame(frameNumber, element.Children, jointsById, animModelMatrix, animTransform.ChildElementPoses);
                }

                
            }
        }


        protected void GenerateFrameForElement(int frameNumber, ShapeElement element, ref ElementPose transform)
        {
            for (int flag = 0; flag < 3; flag++)
            {
                AnimationKeyFrameElement curKelem, nextKelem;

                getTwoKeyFramesElementForFlag(frameNumber, element, flag, out curKelem, out nextKelem);

                if (curKelem == null) continue;

                float t;

                if (nextKelem == null || curKelem == nextKelem)
                {
                    nextKelem = curKelem;
                    t = 0;
                }
                else
                {
                    if (nextKelem.Frame < curKelem.Frame)
                    {
                        int quantity = nextKelem.Frame + (QuantityFrames - curKelem.Frame);
                        int framePos = GameMath.Mod(frameNumber - curKelem.Frame, QuantityFrames);
                        
                        t = (float)framePos / quantity;
                    }
                    else
                    {
                        t = (float)(frameNumber - curKelem.Frame) / (nextKelem.Frame - curKelem.Frame);
                    }
                }


                lerpKeyFrameElement(curKelem, nextKelem, flag, t, ref transform);
            }
        }


        protected void lerpKeyFrameElement(AnimationKeyFrameElement prev, AnimationKeyFrameElement next, int forFlag, float t, ref ElementPose transform)
        {
            if (prev == null && next == null) return;

            int jointId = prev.ForElement.JointId;
            ShapeElement elem = prev.ForElement;

            // Applies the transforms in model space
            if (forFlag == 0)
            {
                transform.translateX = GameMath.Lerp((float)prev.OffsetX / 16f, (float)next.OffsetX / 16f, t);
                transform.translateY = GameMath.Lerp((float)prev.OffsetY / 16f, (float)next.OffsetY / 16f, t);
                transform.translateZ = GameMath.Lerp((float)prev.OffsetZ / 16f, (float)next.OffsetZ / 16f, t);
            }
            else if (forFlag == 1)
            {
                transform.degX = GameMath.Lerp((float)prev.RotationX, (float)next.RotationX, t);
                transform.degY = GameMath.Lerp((float)prev.RotationY, (float)next.RotationY, t);
                transform.degZ = GameMath.Lerp((float)prev.RotationZ, (float)next.RotationZ, t);
            }
            else
            {
                transform.scaleX = GameMath.Lerp((float)prev.StretchX, (float)next.StretchX, t);
                transform.scaleY = GameMath.Lerp((float)prev.StretchY, (float)next.StretchY, t);
                transform.scaleZ = GameMath.Lerp((float)prev.StretchZ, (float)next.StretchZ, t);
            }
        }



        protected void getTwoKeyFramesElementForFlag(int frameNumber, ShapeElement forElement, int forFlag, out AnimationKeyFrameElement left, out AnimationKeyFrameElement right)
        {
            left = null;
            right = null;

            // Go left of frameNumber until we hit the first keyframe
            int keyframeIndex = KeyFrames.Length - 1;
            bool loopAround = false;

            while (keyframeIndex >= -1)
            {
                AnimationKeyFrame keyframe = KeyFrames[GameMath.Mod(keyframeIndex, KeyFrames.Length)];
                keyframeIndex--;

                if (keyframe.Frame <= frameNumber || loopAround)
                {
                    AnimationKeyFrameElement kelem = keyframe.GetKeyFrameElement(forElement);
                    if (kelem != null && kelem.IsSet(forFlag))
                    {
                        left = kelem;
                        break;
                    }
                }

                if (keyframeIndex == -1) loopAround = true;
            }
        

            keyframeIndex+=2;
            int tries = KeyFrames.Length;

            while (tries-- > 0)
            {
                AnimationKeyFrame nextkeyframe = KeyFrames[GameMath.Mod(keyframeIndex, KeyFrames.Length)];

                AnimationKeyFrameElement kelem = nextkeyframe.GetKeyFrameElement(forElement);
                if (kelem != null && kelem.IsSet(forFlag))
                {
                    right = kelem;
                    return;
                }

                keyframeIndex++;
            }
        }
    }
}