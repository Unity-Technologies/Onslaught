using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

public class HapticAbstraction
{
    public static void BuzzBothHands(float seconds)
    {
        BuzzNode(seconds, XRNode.LeftHand);
        BuzzNode(seconds, XRNode.RightHand);
    }

    public static void BuzzNode(float seconds, XRNode node)
    {
        HapticCapabilities caps = new HapticCapabilities();

        if (!InputDevices.GetDeviceAtXRNode(node).TryGetHapticCapabilities(out caps))
            return;

        if (caps.supportsImpulse)
        {
            InputDevices.GetDeviceAtXRNode(node).SendHapticImpulse(0, 1, seconds);
        }
        else if (caps.supportsBuffer)
        {
            byte[] clip = {};
            if (GenerateBuzzClip(seconds, node, ref clip))
            {
                InputDevices.GetDeviceAtXRNode(node).SendHapticBuffer(0, clip);
            }
        }
    }

    public static bool GenerateBuzzClip(float seconds, XRNode node, ref byte[] clip)
    {
        HapticCapabilities caps = new HapticCapabilities();

        if (!InputDevices.GetDeviceAtXRNode(node).TryGetHapticCapabilities(out caps))
            return false;

        int clipCount = (int)(caps.bufferFrequencyHz * seconds);
        clip = new byte[clipCount];
        for (int i = 0; i < clipCount; i++)
        {
            clip[i] = byte.MaxValue;
        }

        return true;
    }
}
