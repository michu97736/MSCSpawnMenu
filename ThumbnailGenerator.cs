using System.Collections;
using System.Runtime.CompilerServices;
using MSCLoader;
using UnityEngine;
using UnityEngine.UI;

namespace MSCSpawnMenu
{
    public class ThumbnailGenerator : MonoBehaviour
    {
        private static Camera captureCamera;
        private static GameObject GenPivot;
        private static RenderTexture genTexture = new RenderTexture(256, 256, 0);
        public static RawImage iconComp;
        public static ThumbnailGenerator instance;

        private void Start()
        {
            captureCamera = gameObject.transform.Find("Camera").GetComponent<Camera>();
            GenPivot = gameObject.transform.Find("ObjectPivot").gameObject;
            captureCamera.targetTexture = genTexture;
            instance = this;
        }

        public void GenerateImage(GameObject prefab, RawImage iconRawImage)
        {
            iconComp = iconRawImage;
            // Activate the camera
            captureCamera.enabled = true;

            // Set the camera's target texture to the Render Texture
            captureCamera.targetTexture = genTexture;

            // Spawn and position the object you want to capture
            GameObject spawnedObject = Instantiate<GameObject>(prefab);
            spawnedObject.transform.SetParent(GenPivot.transform, false);

            // Wait for a frame to ensure object is rendered
            StartCoroutine(CaptureImage());

            // Deactivate the camera
            captureCamera.targetTexture = null;
            captureCamera.enabled = false;

            // Destroy the spawned object (if needed)
            Destroy(spawnedObject);
        }

        private IEnumerator CaptureImage()
        {
            // Wait for end of frame
            yield return new WaitForEndOfFrame();

            // Capture the image from Render Texture
            RenderTexture.active = genTexture;
            Texture2D image = new Texture2D(genTexture.width, genTexture.height);
            image.ReadPixels(new Rect(0, 0, genTexture.width, genTexture.height), 0, 0);
            image.Apply();

            // Assign the captured image to the UI Image
            iconComp.texture = (Sprite.Create(image, new Rect(0, 0, genTexture.width, genTexture.height), Vector2.zero)).texture;
        }

        private void Update()
        {

        }
    }
}