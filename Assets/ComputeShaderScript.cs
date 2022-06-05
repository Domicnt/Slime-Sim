using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Agent {
    public Vector2 position;
    public Vector2 velocity;
}

public class ComputeShaderScript : MonoBehaviour
{
    public ComputeShader computeShader;
    ComputeBuffer agentsBuffer;

    public RenderTexture renderTexture;

    private Agent[] agents;
    public int agentCount;

    public float trailDiffuseStrength;
    public float trailFollowSpeed;
    public Vector4 color;

    private void Start() {
        renderTexture = new RenderTexture(640, 360, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        agents = new Agent[agentCount];
        for(int i = 0; i < agentCount; i++){
            //agents[i].position = new Vector2(renderTexture.width / 2, renderTexture.height / 2);
            agents[i].position = new Vector2(Random.Range(0, renderTexture.width), Random.Range(0, renderTexture.height));
            
            //uniform circular distribution
            agents[i].velocity = new Vector2(
                Mathf.Cos(((float) i / (float) agentCount) * 2.0f * Mathf.PI), 
                Mathf.Sin(((float) i / (float) agentCount) * 2.0f * Mathf.PI)
            );
            
            //random circular distribution
            //agents[i].velocity = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
            //agents[i].velocity.Normalize();
        }

        agentsBuffer = new ComputeBuffer(agents.Length, sizeof(float) * 4);
    }

    private void FixedUpdate() {
        agentsBuffer.SetData(agents);

        computeShader.SetTexture(1, "image", renderTexture);
        computeShader.SetFloat("diffuseStrength", trailDiffuseStrength);
        computeShader.Dispatch(1, renderTexture.width, renderTexture.height, 1);

        computeShader.SetTexture(0, "image", renderTexture);
        computeShader.SetBuffer(0, "agents", agentsBuffer);
        computeShader.SetFloats("resolution", renderTexture.width, renderTexture.height);
        computeShader.SetFloats("agentColor", color.x, color.y, color.z, color.w);        
        computeShader.SetFloat("trailFollowSpeed", trailFollowSpeed);
        computeShader.Dispatch(0, agents.Length / 100, 1, 1);

        agentsBuffer.GetData(agents);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(renderTexture, dest);
    }

    private void OnDestroy() {
        agentsBuffer.Dispose();
    }
}
