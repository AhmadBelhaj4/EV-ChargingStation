using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weston.SampleWelding
{
    public class Writable : MonoBehaviour
    {
        const int TEXTURE_SIZE = 1024;

        public RenderTexture maskRenderTexture;
        public RenderTexture supportTexture;
        Renderer rend;

        public RenderTexture getMask() => maskRenderTexture;

        public RenderTexture getSupport() => supportTexture;
        public Renderer getRenderer() => rend;

        int maskTextureID = Shader.PropertyToID("_Mask");


        // Start is called before the first frame update
        void Start()
        {
            maskRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            maskRenderTexture.filterMode = FilterMode.Bilinear;


            supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            supportTexture.filterMode = FilterMode.Bilinear;

            rend = GetComponent<Renderer>();
            rend.material.SetTexture(maskTextureID, supportTexture);


            WritingManager.Instance.initTextures(this);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnDisable()
        {
            maskRenderTexture.Release();
            supportTexture.Release();
        }
    }
}