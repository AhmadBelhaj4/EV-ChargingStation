using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Weston.SampleWelding
{
    public class WritingManager : MonoBehaviour
    {
        public static WritingManager Instance;
        public Material paintMaterial;
        int prepareUVID = Shader.PropertyToID("_PrepareUV");
        int colorID = Shader.PropertyToID("_PainterColor");
        int baseColorID = Shader.PropertyToID("_BaseColor");
        int p1ID = Shader.PropertyToID("_P1");
        int p2ID = Shader.PropertyToID("_P2");
        int p3ID = Shader.PropertyToID("_P3");
        int hardnessID = Shader.PropertyToID("_Hardness");
        int strengthID = Shader.PropertyToID("_Strength");
        int radiusID = Shader.PropertyToID("_Radius");
        int textureID = Shader.PropertyToID("_MainTex");


        CommandBuffer command;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            command = new CommandBuffer();
            command.name = "CommmandBuffer - " + gameObject.name;
        }

        /// <summary>
        /// 画出三角形
        /// </summary>
        /// <param name="paintable"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="hardness"></param>
        /// <param name="strength"></param>
        /// <param name="speed"></param>
        /// <param name="color"></param>
        public void paint(Writable paintable, Vector3 p1,Vector3 p2,Vector3 p3, Color? color = null)
        {
            Renderer rend = paintable.getRenderer();
            rend.material.SetColor(baseColorID, color ?? Color.gray);



            RenderTexture mask = paintable.getMask();

            RenderTexture support = paintable.getSupport();

            paintMaterial.SetFloat(prepareUVID, 0);
            paintMaterial.SetVector(p1ID, p1);
            paintMaterial.SetVector(p2ID, p2);
            paintMaterial.SetVector(p3ID, p3);

            // paintMaterial.SetFloat(hardnessID, hardness);
            // paintMaterial.SetFloat(strengthID, strength);
            // paintMaterial.SetFloat(radiusID, radius);
            paintMaterial.SetTexture(textureID, support);
            paintMaterial.SetColor(colorID, Color.white);




            command.SetRenderTarget(mask);
            command.DrawRenderer(rend, paintMaterial, 0);

            command.SetRenderTarget(support);
            command.Blit(mask, support);


            Graphics.ExecuteCommandBuffer(command);
            command.Clear();

        }

    public void initTextures(Writable paintable)
        {
            RenderTexture mask = paintable.getMask();

            RenderTexture support = paintable.getSupport();



    

            Renderer rend = paintable.getRenderer();


            paintMaterial.SetFloat(prepareUVID, 1);
            command.DrawRenderer(rend, paintMaterial, 0);

            Graphics.ExecuteCommandBuffer(command);
            command.Clear();
        }
    }

    
}