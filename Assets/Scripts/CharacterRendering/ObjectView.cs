using System.Collections;
using UnityEngine;

namespace RogueLike.CharacterRendering
{
    // Renders a 3D GameObject through a dedicated off-screen camera into a
    // RenderTexture, then displays that texture on this GameObject's own
    // flat renderer (SpriteRenderer or MeshRenderer2D) — so a rigged 3D
    // model can sit in a 2D scene looking like an ordinary sprite.
    //
    // Ported from solhwi/SebamoGameClient's ObjectView. Adapted: the source
    // object is a plain prefab reference instead of an
    // AssetReferenceGameObject/Addressables.InstantiateAsync pair — this
    // project wires prefabs directly everywhere else, and CharacterDemo's
    // own DemoScene.cs placed its Character.prefab the same way rather
    // than loading it through Addressables (only the parts *inside* it are
    // Addressable). Dropped the RawImage/UI-icon branch, which nothing
    // here needs; see SebamoGameClient's original if that's wanted later.
    public class ObjectView : MonoBehaviour
    {
        [SerializeField] private GameObject originPrefab;

        [SerializeField] protected Vector3 spawnLocalPos = new Vector3(0f, -0.5f, 0f);
        [SerializeField] protected Vector3 spawnLocalRot = new Vector3(0f, 180f, 0f);

        [SerializeField] private bool isFixedPosition = false;
        [SerializeField] private bool isFixedRotation = false;

        [SerializeField] private bool isOrthoSize = true;
        [SerializeField] private float fov = 1.5f;

        [SerializeField] private int height = 512;
        [SerializeField] private int width = 512;
        [SerializeField] private int depth = 32;

        protected Camera cam;
        protected Transform cameraArm;

        public GameObject originObj;

        protected SpriteRenderer spriteView;
        protected MeshRenderer2D meshView;

        private RenderTexture renderTexture;
        private Texture2D texture;
        private Rect rect;

        private bool isInitialized;
        private Coroutine prepareRoutine;

        public bool isVisible;

        public virtual void Initialize()
        {
            if (!isInitialized)
            {
                spriteView = GetComponent<SpriteRenderer>();
                if (spriteView != null)
                {
                    spriteView.color = Color.clear;
                }

                meshView = GetComponent<MeshRenderer2D>();
                if (meshView != null)
                {
                    meshView.SetColor(Color.clear);
                }

                renderTexture = new RenderTexture(width, height, depth);
                rect = new Rect(0, 0, width, height);
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

                cam = ObjectCameraManager.Instance.MakeCamera(isOrthoSize, fov);
                cameraArm = cam.transform.GetChild(0);
                cam.targetTexture = renderTexture;

                isInitialized = true;
            }

            gameObject.SetActive(true);

            if (originObj == null)
            {
                if (prepareRoutine != null)
                {
                    StopCoroutine(prepareRoutine);
                }

                prepareRoutine = StartCoroutine(OnPrepareRendering());
            }
        }

        protected virtual void OnCreateObject(GameObject obj)
        {
            originObj = obj;
            originObj.SetActive(true);

            originObj.transform.localPosition = spawnLocalPos;
            originObj.transform.localEulerAngles = spawnLocalRot;
        }

        protected virtual void OnEnable()
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(true);
            }
        }

        protected virtual void OnDisable()
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(false);
            }
        }

        protected virtual IEnumerator OnPrepareRendering()
        {
            if (originPrefab != null)
            {
                OnCreateObject(Instantiate(originPrefab, cameraArm));
            }

            // Rendering only reflects the newly parented object starting
            // next frame.
            yield return null;
        }

        protected virtual void Update()
        {
            if (originObj == null || cam == null)
            {
                return;
            }

            if (cam.gameObject.activeSelf != isVisible)
            {
                cam.gameObject.SetActive(isVisible);
                OnChangeVisibleObject(isVisible);
            }

            if (!isVisible)
            {
                return;
            }

            if (spriteView != null)
            {
                spriteView.color = Color.white;
                spriteView.sprite = GetScreenShotSprite();
            }
            else if (meshView != null)
            {
                meshView.SetColor(Color.white);
                meshView.SetTexture(GetScreenShotTexture());
            }
        }

        protected virtual void OnBecameVisible()
        {
            isVisible = true;
        }

        protected virtual void OnChangeVisibleObject(bool visible)
        {
        }

        protected virtual void OnBecameInvisible()
        {
            isVisible = false;
        }

        private void LateUpdate()
        {
            if (originObj == null)
            {
                return;
            }

            if (isFixedPosition)
            {
                originObj.transform.localPosition = spawnLocalPos;
            }
            if (isFixedRotation)
            {
                originObj.transform.localEulerAngles = spawnLocalRot;
            }
        }

        private Texture2D GetScreenShotTexture()
        {
            RenderTexture.active = renderTexture;
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();
            return texture;
        }

        // Allocates a new Sprite every call — avoid in a hot path (kept
        // only for the SpriteRenderer branch; prefer MeshRenderer2D).
        private Sprite GetScreenShotSprite()
        {
            var tex = GetScreenShotTexture();
            return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
        }
    }
}
