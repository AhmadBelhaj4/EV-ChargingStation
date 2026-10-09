using UnityEngine;
namespace Weston.SampleWelding
{
    public class Paintable : MonoBehaviour
    {
        const int TEXTURE_SIZE = 1024;

        public float extendsIslandOffset = 1;

        public RenderTexture extendIslandsRenderTexture;
        public RenderTexture uvIslandsRenderTexture;
        public RenderTexture maskRenderTexture;
        public RenderTexture supportTexture;

        public RenderTexture weldTexture;

        public RenderTexture extendIslandsRenderTexture1;
        public RenderTexture uvIslandsRenderTexture1;
        public RenderTexture supportTexture1;

        Renderer rend;

        int maskTextureID = Shader.PropertyToID("_Mask");
        int weldTextureID = Shader.PropertyToID("_WelTexture");

        public RenderTexture getMask() => maskRenderTexture;
        public RenderTexture getUVIslands() => uvIslandsRenderTexture;
        public RenderTexture getExtend() => extendIslandsRenderTexture;
        public RenderTexture getSupport() => supportTexture;
        public Renderer getRenderer() => rend;
        public RenderTexture getWeld() => weldTexture;

        public RenderTexture getUVIslands1() => uvIslandsRenderTexture1;
        public RenderTexture getSupport1() => supportTexture1;
        public RenderTexture getExtend1() => extendIslandsRenderTexture1;

        void Start()
        {
            maskRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            maskRenderTexture.filterMode = FilterMode.Bilinear;

            extendIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            extendIslandsRenderTexture.filterMode = FilterMode.Bilinear;

            uvIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            uvIslandsRenderTexture.filterMode = FilterMode.Bilinear;

            supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            supportTexture.filterMode = FilterMode.Bilinear;
            uvIslandsRenderTexture1 = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            uvIslandsRenderTexture1.filterMode = FilterMode.Bilinear;

            supportTexture1 = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            supportTexture1.filterMode = FilterMode.Bilinear;

            extendIslandsRenderTexture1 = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            extendIslandsRenderTexture1.filterMode = FilterMode.Bilinear;

            weldTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            weldTexture.filterMode = FilterMode.Bilinear;

            rend = GetComponent<Renderer>();
            rend.material.SetTexture(maskTextureID, extendIslandsRenderTexture);
            rend.material.SetTexture(weldTextureID, extendIslandsRenderTexture1);
            PaintManager.instance.initTextures(this);
        }

        void OnDisable()
        {
            maskRenderTexture.Release();
            uvIslandsRenderTexture.Release();
            extendIslandsRenderTexture.Release();
            supportTexture.Release();
            weldTexture.Release();

            uvIslandsRenderTexture1.Release();
            supportTexture1.Release();
        }
    }
}