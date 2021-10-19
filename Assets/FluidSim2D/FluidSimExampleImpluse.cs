using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FluidSim2DProject//REQUIRES Fluid Core
{
    public class FluidSimExampleImpluse : MonoBehaviour
    {
        FluidSimCore fluid;

        //impluse
        Vector2 implusePos = new Vector2(0.5f, 0.0f);
        public float impulseTemperature = 10.0f;
        public float impulseDensity = 1.0f;
        public float impluseRadius = 0.1f;
        public float mouseImpluseRadius = 0.05f;
        //obstacle
        Vector2 obstaclePos = new Vector2(0.5f, 0.5f);
        public float obstacleRadius = 0.1f;



        // Start is called before the first frame update
        void Start()
        {
            fluid = GetComponent<FluidSimCore>();
            Camera cam = Camera.main;
            fluid.Init(cam.pixelWidth, cam.pixelHeight);
        }

        // Update is called once per frame
        void Update()
        {
            //obstacle
            fluid.AddObstacles(obstaclePos, obstacleRadius);//Obstacles only need to be added once unless changed.
            //impluse
            fluid.ApplyImpulse(fluid.m_temperatureTex, implusePos, impluseRadius, impulseTemperature);//temperature
            fluid.ApplyImpulse(fluid.m_densityTex, implusePos, impluseRadius, new Vector4(1, 1, 1, impulseDensity));//density

            if (Input.GetMouseButton(0))
            {
                Vector2 pos = Input.mousePosition;

                pos.x -= fluid.m_rect.xMin;
                pos.y -= fluid.m_rect.yMin;

                pos.x /= fluid.m_rect.width;
                pos.y /= fluid.m_rect.height;

                Color c = Color.HSVToRGB((Time.realtimeSinceStartup/3f)%1, 1, 1)*impulseDensity;//color
                c.a = impulseDensity;//simulation density
                Vector4 velocity = new Vector4(Mathf.Cos(Time.realtimeSinceStartup) * 100000, Mathf.Sin(Time.realtimeSinceStartup) * 100000, 0, 0);

                fluid.ApplyImpulse(fluid.m_densityTex, pos, mouseImpluseRadius, c);//alpha used as density other rgb are color
                
                fluid.ApplyImpulse(fluid.m_velocityTex, pos, mouseImpluseRadius / 10, velocity);//add velocity
            }
            //generate
            fluid.Generate();
        }

        private void OnGUI()
        {
            GUI.DrawTexture(fluid.m_rect, fluid.m_result);
        }
    }
}