// Copyright (c) 2025 CriticalVFX
// This script is part of the Stylized Projectile FX Kit Mini Vol1 package.
// For licensing information, please refer to the included LICENSE file.

namespace CVFX_SPFXKMINIV2
{
    using UnityEngine;

    public class RandomRotation : MonoBehaviour
    {
        private void OnEnable() 
        {
            transform.rotation = Random.rotation;
        }
    }
}
