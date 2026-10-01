using UnityEngine;
using RogueLike.Core;

namespace RogueLike.CharacterRendering
{
    // Hands out off-screen cameras for ObjectView to render 3D content
    // into a RenderTexture. Adapted from solhwi/SebamoGameClient's version:
    // that one instantiated an Addressable camera prefab and pooled
    // cameras for a roster of simultaneous previews; this project only
    // ever needs one character at a time, so it just builds a plain Camera
    // and gives each one a unique position far from the playable world —
    // simplest way to guarantee the main game camera never sees it (same
    // trick the original used to keep multiple preview rigs from seeing
    // each other).
    public class ObjectCameraManager : Singleton<ObjectCameraManager>
    {
        private const float BaseOffset = 10000f;
        private const float PerCameraSpacing = 100f;

        private int madeCount;

        public Camera MakeCamera(bool isOrtho, float fov)
        {
            var cameraGo = new GameObject("ObjectPreviewCamera");
            cameraGo.transform.SetParent(transform, false);

            var armGo = new GameObject("CameraArm");
            armGo.transform.SetParent(cameraGo.transform, false);

            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.orthographic = isOrtho;
            if (isOrtho)
            {
                camera.orthographicSize = fov;
            }
            else
            {
                camera.fieldOfView = fov;
            }

            float offset = BaseOffset + madeCount * PerCameraSpacing;
            cameraGo.transform.position = new Vector3(offset, offset, 0f);
            madeCount++;

            return camera;
        }
    }
}
