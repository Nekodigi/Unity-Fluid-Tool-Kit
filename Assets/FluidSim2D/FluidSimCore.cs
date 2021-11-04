using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FluidSim2DProject
{

    public class FluidSimCore : MonoBehaviour
    {
        //environment setting
        //public Color m_fluidColor = Color.red;
        public Color m_obstacleColor = Color.white;//for debug..
        public Texture2D gradTex;//gradient texture for visualize

        //convection
        public float m_temperatureDissipation = 0.99f;
        public float m_smokeBuoyancy = 1.0f;
        public float m_smokeWeight = 0.05f;
        public float m_ambientTemperature = 0.0f;
        //diffussions
        public float m_velocityDissipation = 0.99f;
        public float m_densityDissipation = 0.9999f;
        //silumation setting
        public float m_cellSize = 1.0f;
        public float m_gradientScale = 1.0f;
        //resolution setting
        public int m_numJacobiIterations = 50;
        //if enable black will transparent
        public bool BLEND_FROM_RGB = true;

        Vector2 m_inverseSize;

        //RESULT
        [HideInInspector]
        public RenderTexture m_result;
        [HideInInspector]
        public Rect m_rect;

        //variable
        Material m_guiMat, m_advectMat, m_buoyancyMat, m_divergenceMat, m_jacobiMat, m_impluseMat, m_impluseVecMat, m_gradientMat, m_obstaclesMat;

        RenderTexture m_divergenceTex, m_obstaclesTex;
        [HideInInspector]
        public RenderTexture[] m_velocityTex, m_densityTex, m_pressureTex, m_temperatureTex;

        
        int m_width, m_height;
        float aspect;
        //constant
        const int READ = 0;
        const int WRITE = 1;

        public void Init(int width, int height)
        {
            m_width = width;// Camera.main.pixelWidth;
            m_height = height;// Camera.main.pixelHeight;
            //m_width = Camera.main.pixelWidth;
            //m_height = Camera.main.pixelHeight;
            aspect = (float)(m_width) / m_height;

            #region setMaterial
            m_guiMat = Resources.Load("Materials/GUI") as Material;//need to fix
            m_advectMat = Resources.Load("Materials/Advect") as Material;
            m_buoyancyMat = Resources.Load("Materials/Buoyancy") as Material;
            m_divergenceMat = Resources.Load("Materials/Divergence") as Material;
            m_jacobiMat = Resources.Load("Materials/Jacobi") as Material;
            m_impluseMat = Resources.Load("Materials/Impluse") as Material;
            m_impluseVecMat = Resources.Load("Materials/ImpluseVec") as Material;
            m_gradientMat = Resources.Load("Materials/SubtractGradient") as Material;
            m_obstaclesMat = Resources.Load("Materials/Obstacle") as Material;
            #endregion

            if (BLEND_FROM_RGB) m_guiMat.EnableKeyword("BLEND_FROM_RGB");
            else m_guiMat.DisableKeyword("BLEND_FROM_RGB");
            if (gradTex != null) m_guiMat.EnableKeyword("DENSITY_GRADIENT");
            else m_guiMat.DisableKeyword("DENSITY_GRADIENT");

            Vector2 size = new Vector2(m_width, m_height);
            Vector2 pos = new Vector2(Screen.width / 2, Screen.height / 2) - size * 0.5f;
            m_rect = new Rect(pos, size);

            m_inverseSize = new Vector2(1.0f / m_width, 1.0f / m_height);

            m_velocityTex = new RenderTexture[2];
            m_densityTex = new RenderTexture[2];
            m_temperatureTex = new RenderTexture[2];
            m_pressureTex = new RenderTexture[2];

            CreateSurface(m_velocityTex, RenderTextureFormat.RGFloat, FilterMode.Bilinear);
            CreateSurface(m_densityTex, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);
            CreateSurface(m_temperatureTex, RenderTextureFormat.RFloat, FilterMode.Bilinear);
            CreateSurface(m_pressureTex, RenderTextureFormat.RFloat, FilterMode.Point);

            m_result = new RenderTexture(m_width, m_height, 0, RenderTextureFormat.ARGB32);
            m_result.filterMode = FilterMode.Bilinear;
            m_result.wrapMode = TextureWrapMode.Clamp;
            m_result.Create();

            m_divergenceTex = new RenderTexture(m_width, m_height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            m_divergenceTex.filterMode = FilterMode.Point;
            m_divergenceTex.wrapMode = TextureWrapMode.Clamp;
            m_divergenceTex.Create();

            m_obstaclesTex = new RenderTexture(m_width, m_height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            m_obstaclesTex.filterMode = FilterMode.Point;
            m_obstaclesTex.wrapMode = TextureWrapMode.Clamp;
            m_obstaclesTex.Create();
        }

        /*[ImageEffectOpaque]
        void OnGUI()
        {
            GUI.DrawTexture(m_rect, m_result);
        }*/

        void CreateSurface(RenderTexture[] surface, RenderTextureFormat format, FilterMode filter)
        {
            surface[0] = new RenderTexture(m_width, m_height, 0, format, RenderTextureReadWrite.Linear);
            surface[0].filterMode = filter;
            surface[0].wrapMode = TextureWrapMode.Clamp;
            surface[0].Create();

            surface[1] = new RenderTexture(m_width, m_height, 0, format, RenderTextureReadWrite.Linear);
            surface[1].filterMode = filter;
            surface[1].wrapMode = TextureWrapMode.Clamp;
            surface[1].Create();
        }

        void Advect(RenderTexture velocity, RenderTexture source, RenderTexture dest, float dissipation, float timeStep)
        {
            m_advectMat.SetVector("_InverseSize", m_inverseSize);
            m_advectMat.SetFloat("_TimeStep", timeStep);
            m_advectMat.SetFloat("_Dissipation", dissipation);
            m_advectMat.SetTexture("_Velocity", velocity);
            m_advectMat.SetTexture("_Source", source);
            m_advectMat.SetTexture("_Obstacles", m_obstaclesTex);

            Graphics.Blit(null, dest, m_advectMat);
        }

        void ApplyBuoyancy(RenderTexture velocity, RenderTexture temperature, RenderTexture density, RenderTexture dest, float timeStep)
        {
            m_buoyancyMat.SetTexture("_Velocity", velocity);
            m_buoyancyMat.SetTexture("_Temperature", temperature);
            m_buoyancyMat.SetTexture("_Density", density);
            m_buoyancyMat.SetFloat("_AmbientTemperature", m_ambientTemperature);
            m_buoyancyMat.SetFloat("_TimeStep", timeStep);
            m_buoyancyMat.SetFloat("_Sigma", m_smokeBuoyancy);
            m_buoyancyMat.SetFloat("_Kappa", m_smokeWeight);

            Graphics.Blit(null, dest, m_buoyancyMat);
        }

        public void ApplyImpulse(RenderTexture[] target, Vector2 pos, float radius, float val)
        {
            ApplyImpulse(target, pos, radius, new Vector4(val, val, val, val));
            //m_impluseMat.SetVector("_Point", pos);
            //m_impluseMat.SetFloat("_Radius", radius);
            //m_impluseMat.SetFloat("_Fill", val);
            //m_impluseMat.SetTexture("_Source", source);
            //m_impluseMat.SetFloat("_Aspect", aspect);

            //Graphics.Blit(null, dest, m_impluseMat);
        }

        public void ApplyImpulse(RenderTexture[] target, Vector2 pos, float radius, Vector4 val)//assume read to write
        {
            m_impluseVecMat.SetVector("_Point", pos);
            m_impluseVecMat.SetFloat("_Radius", radius);
            m_impluseVecMat.SetColor("_Fill", val);
            m_impluseVecMat.SetTexture("_Source", target[READ]);
            m_impluseVecMat.SetFloat("_Aspect", aspect);

            Graphics.Blit(null, target[WRITE], m_impluseVecMat);
            Swap(target);
        }

        void ComputeDivergence(RenderTexture velocity, RenderTexture dest)
        {
            m_divergenceMat.SetFloat("_HalfInverseCellSize", 0.5f / m_cellSize);
            m_divergenceMat.SetTexture("_Velocity", velocity);
            m_divergenceMat.SetVector("_InverseSize", m_inverseSize);
            m_divergenceMat.SetTexture("_Obstacles", m_obstaclesTex);

            Graphics.Blit(null, dest, m_divergenceMat);
        }

        void Jacobi(RenderTexture pressure, RenderTexture divergence, RenderTexture dest)
        {

            m_jacobiMat.SetTexture("_Pressure", pressure);
            m_jacobiMat.SetTexture("_Divergence", divergence);
            m_jacobiMat.SetVector("_InverseSize", m_inverseSize);
            m_jacobiMat.SetFloat("_Alpha", -m_cellSize * m_cellSize);
            m_jacobiMat.SetFloat("_InverseBeta", 0.25f);
            m_jacobiMat.SetTexture("_Obstacles", m_obstaclesTex);

            Graphics.Blit(null, dest, m_jacobiMat);
        }

        void SubtractGradient(RenderTexture velocity, RenderTexture pressure, RenderTexture dest)
        {
            m_gradientMat.SetTexture("_Velocity", velocity);
            m_gradientMat.SetTexture("_Pressure", pressure);
            m_gradientMat.SetFloat("_GradientScale", m_gradientScale);
            m_gradientMat.SetVector("_InverseSize", m_inverseSize);
            m_gradientMat.SetTexture("_Obstacles", m_obstaclesTex);

            Graphics.Blit(null, dest, m_gradientMat);
        }

        public void AddObstacles(Vector2 obstaclePos, float obstacleRadius)//NEED TO MOVE
        {
            m_obstaclesMat.SetVector("_InverseSize", m_inverseSize);
            m_obstaclesMat.SetVector("_Point", obstaclePos);
            m_obstaclesMat.SetFloat("_Radius", obstacleRadius);
            m_obstaclesMat.SetFloat("_Aspect", aspect);

            Graphics.Blit(null, m_obstaclesTex, m_obstaclesMat);
        }

        void ClearSurface(RenderTexture surface)
        {
            Graphics.SetRenderTarget(surface);
            GL.Clear(false, true, new Color(0, 0, 0, 0));
            Graphics.SetRenderTarget(null);
        }

        void Swap(RenderTexture[] texs)
        {
            RenderTexture temp = texs[0];
            texs[0] = texs[1];
            texs[1] = temp;
        }

        public RenderTexture Generate()//should not call in onGUI
        {
            

            //Set the density field and obstacle color.
            //m_guiMat.SetColor("_FluidColor", m_fluidColor);
            m_guiMat.SetColor("_ObstacleColor", m_obstacleColor);
            float dt = 0.125f;

            //Advect velocity against its self
            Advect(m_velocityTex[READ], m_velocityTex[READ], m_velocityTex[WRITE], m_velocityDissipation, dt);
            //Advect temperature against velocity
            Advect(m_velocityTex[READ], m_temperatureTex[READ], m_temperatureTex[WRITE], m_temperatureDissipation, dt);
            //Advect density against velocity
            Advect(m_velocityTex[READ], m_densityTex[READ], m_densityTex[WRITE], m_densityDissipation, dt);

            Swap(m_velocityTex);
            Swap(m_temperatureTex);
            Swap(m_densityTex);

            //Determine how the flow of the fluid changes the velocity
            ApplyBuoyancy(m_velocityTex[READ], m_temperatureTex[READ], m_densityTex[READ], m_velocityTex[WRITE], dt);

            Swap(m_velocityTex);

            //Calculates how divergent the velocity is
            ComputeDivergence(m_velocityTex[READ], m_divergenceTex);

            ClearSurface(m_pressureTex[READ]);

            int i = 0;
            for (i = 0; i < m_numJacobiIterations; ++i)
            {
                Jacobi(m_pressureTex[READ], m_divergenceTex, m_pressureTex[WRITE]);
                Swap(m_pressureTex);
            }

            //Use the pressure tex that was last rendered into. This computes divergence free velocity
            SubtractGradient(m_velocityTex[READ], m_pressureTex[READ], m_velocityTex[WRITE]);

            Swap(m_velocityTex);

            //Render the tex you want to see into gui tex. Will only use the red channel
            m_guiMat.SetTexture("_Obstacles", m_obstaclesTex);
            m_guiMat.SetTexture("_Gradient", gradTex);
            Graphics.Blit(m_densityTex[READ], m_result, m_guiMat);
            return m_result;
        }
    }

}
