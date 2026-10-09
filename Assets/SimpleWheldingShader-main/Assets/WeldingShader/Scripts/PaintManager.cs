using UnityEngine;
using UnityEngine.Rendering;
namespace Weston.SampleWelding
{
    public class PaintManager : MonoBehaviour
    {

        public static PaintManager instance;
        public Shader texturePaint;
        public Shader extendIslands;

        int prepareUVID = Shader.PropertyToID("_PrepareUV");
        int positionID = Shader.PropertyToID("_PainterPosition");
        int hardnessID = Shader.PropertyToID("_Hardness");
        int strengthID = Shader.PropertyToID("_Strength");
        int radiusID = Shader.PropertyToID("_Radius");
        int blendOpID = Shader.PropertyToID("_BlendOp");
        int colorID = Shader.PropertyToID("_PainterColor");
        int textureID = Shader.PropertyToID("_MainTex");
        int uvOffsetID = Shader.PropertyToID("_OffsetUV");
        int uvIslandsID = Shader.PropertyToID("_UVIslands");

        int baseColorID = Shader.PropertyToID("_BaseColor");
        //Welding setting
        int wRadiusID = Shader.PropertyToID("_Radius");
        int wHardnessID = Shader.PropertyToID("_Hardness");
        int wStrengthID = Shader.PropertyToID("_Strength");
        int wSpeedID = Shader.PropertyToID("_Speed");

        public Material paintMaterial;
        public Material extendMaterial;

        public Material weldMaterial;

        CommandBuffer command;

        public void Awake()
        {

            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            paintMaterial = new Material(texturePaint);
            extendMaterial = new Material(extendIslands);
            command = new CommandBuffer();
            command.name = "CommmandBuffer - " + gameObject.name;
        }

        public void initTextures(Paintable paintable)
        {
            RenderTexture mask = paintable.getMask();
            RenderTexture uvIslands = paintable.getUVIslands();
            RenderTexture extend = paintable.getExtend();
            RenderTexture support = paintable.getSupport();

            RenderTexture uvIslands1 = paintable.getUVIslands();
            RenderTexture extend1 = paintable.getExtend1();
            RenderTexture support1 = paintable.getSupport();

            RenderTexture weld = paintable.getWeld();

            Renderer rend = paintable.getRenderer();

            command.SetRenderTarget(mask);
            command.SetRenderTarget(extend);
            command.SetRenderTarget(support);

            command.SetRenderTarget(weld);
            command.SetRenderTarget(extend1);
            command.SetRenderTarget(support1);
            command.SetRenderTarget(uvIslands1);

            paintMaterial.SetFloat(prepareUVID, 1);
            weldMaterial.SetFloat(prepareUVID, 1);
            command.SetRenderTarget(uvIslands);
            command.DrawRenderer(rend, paintMaterial, 0);

            Graphics.ExecuteCommandBuffer(command);
            command.Clear();
        }


        public void paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f, float speed = -2, Color? color = null)
        {

            Renderer rend = paintable.getRenderer();
            rend.material.SetColor(baseColorID, color ?? Color.gray);



            RenderTexture mask = paintable.getMask();
            RenderTexture uvIslands = paintable.getUVIslands();
            RenderTexture extend = paintable.getExtend();
            RenderTexture support = paintable.getSupport();

            //生成Weldtexture纹理
            RenderTexture weld = paintable.getWeld();
            RenderTexture uvIslands1 = paintable.getUVIslands1();
            RenderTexture extend1 = paintable.getExtend1();
            RenderTexture support1 = paintable.getSupport1();




            weldMaterial.SetFloat(wRadiusID, radius);
            weldMaterial.SetFloat(wHardnessID, hardness);
            weldMaterial.SetFloat(wStrengthID, strength);
            weldMaterial.SetFloat(wSpeedID, speed);

            weldMaterial.SetVector(positionID, pos);
            weldMaterial.SetFloat(prepareUVID, 0);
            weldMaterial.SetTexture(textureID, support1);


            paintMaterial.SetFloat(prepareUVID, 0);
            paintMaterial.SetVector(positionID, pos);
            paintMaterial.SetFloat(hardnessID, hardness);
            paintMaterial.SetFloat(strengthID, strength);
            paintMaterial.SetFloat(radiusID, radius);
            paintMaterial.SetTexture(textureID, support);
            paintMaterial.SetColor(colorID, Color.white);

            extendMaterial.SetFloat(uvOffsetID, paintable.extendsIslandOffset);
            extendMaterial.SetTexture(uvIslandsID, uvIslands);

            //生成波纹
            command.SetRenderTarget(weld);
            command.DrawRenderer(rend, weldMaterial, 0);

            command.SetRenderTarget(support1);
            command.Blit(weld, support1);

            command.SetRenderTarget(extend1);
            command.Blit(weld, extend1, extendMaterial);



            command.SetRenderTarget(mask);
            command.DrawRenderer(rend, paintMaterial, 0);

            command.SetRenderTarget(support);
            command.Blit(mask, support);

            command.SetRenderTarget(extend);
            command.Blit(mask, extend, extendMaterial);

            Graphics.ExecuteCommandBuffer(command);
            command.Clear();
        }

    }
}
