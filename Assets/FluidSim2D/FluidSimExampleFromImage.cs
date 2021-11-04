using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FluidSim2DProject//REQUIRES Fluid Core
{
    public class FluidSimExampleFromImage : MonoBehaviour
    {
        public Texture2D texture;
        public float impulseDensity = 1.0f;
        public float impluseRadius = 0.1f;
        public float mouseImpluseRadius = 0.05f;

        FluidSimCore fluid;
        const int READ = 0;
        const int WRITE = 1;

        // Start is called before the first frame update
        void Start()
        {
            fluid = GetComponent<FluidSimCore>();
            fluid.Init(texture.width, texture.height);
            RenderTexture.active = fluid.m_densityTex[READ];
            Graphics.Blit(texture, fluid.m_densityTex[READ]);
        }

        /*[ImageEffectOpaque]
        //based on this site https://github.com/SebLague/Ray-Marching/blob/master/Assets/Scripts/SDF/Master.cs
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(fluid.m_result, destination);

        }*/

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 pos = Input.mousePosition;

                pos.x -= fluid.m_rect.xMin;
                pos.y -= fluid.m_rect.yMin;

                pos.x /= fluid.m_rect.width;
                pos.y /= fluid.m_rect.height;

                //Color c = new Color(0, 0, 0, impulseDensity);
                Vector4 velocity = new Vector4(Mathf.Cos(Time.realtimeSinceStartup) * 1000, Mathf.Sin(Time.realtimeSinceStartup) * 1000, 0, 0);

                //fluid.ApplyImpulse(fluid.m_densityTex, pos, mouseImpluseRadius, c);//alpha used as density other rgb are color
                fluid.ApplyImpulse(fluid.m_velocityTex, pos, mouseImpluseRadius / 3, velocity);//add velocity
            }
            fluid.Generate();
        }

        private void OnGUI()
        {
            GUI.DrawTexture(fluid.m_rect, fluid.m_result);
        }
    }
}