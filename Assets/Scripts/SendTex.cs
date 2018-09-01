using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GPUCell
{
    public Vector4 xynn;
    public GPUCell(float x, float y, float z, float w)
    {
        xynn = new Vector4(x, y, z, w);
    }
}

public class SendTex : MonoBehaviour {
    public ComputeShader GoLComputeShader;
    public Texture InputTex;
    public Color AliveColor;
    public Color DeadColor;
    public Renderer Shower;
    [SerializeField]
    private RenderTexture GameTex;
    private ComputeBuffer CellBuffer;
    private bool needRendering = false;
    int kernelIdx;
    int updateIdx;

    #region Mono
    void OnGUI()
    {
        if (GUILayout.Button("开始生命游戏"))
        {
            Init();            
            needRendering = true;
        }
    }
	
	void Update () {
        if (!needRendering)
            return;
       
        //运算
        GoLComputeShader.Dispatch(kernelIdx, Mathf.CeilToInt(InputTex.width / 32f), Mathf.CeilToInt(InputTex.height / 32f), 1);
        GoLComputeShader.Dispatch(updateIdx, Mathf.CeilToInt(InputTex.width / 32f), Mathf.CeilToInt(InputTex.height / 32f), 1);
       
	}
    #endregion

    #region Logic
    private void Init()
    {
        //找到核心地址
        kernelIdx = GoLComputeShader.FindKernel("GameOfLifeUpdate");
        updateIdx = GoLComputeShader.FindKernel("UpdateNext");
        InitTexture();
        InitColor();
        InitBuffer();
        Shower.material.mainTexture = GameTex;
    }

    private void InitColor()
    {
        GoLComputeShader.SetVector("AliveColor", AliveColor);
        GoLComputeShader.SetVector("DeadColor", DeadColor);
    }

    private void InitTexture()
    {
        //传入展示和逻辑用纹理
        GameTex = new RenderTexture(InputTex.width, InputTex.height, 24);
        GameTex.enableRandomWrite = true;
        GameTex.filterMode = FilterMode.Point;
        GameTex.Create();
        GoLComputeShader.SetTexture(kernelIdx, "GameTex", GameTex);
        //传入边界
        GoLComputeShader.SetVector("BorderVec", new Vector4(0, InputTex.width, 0, InputTex.height));
    }

    private void InitBuffer()
    {
        int count = InputTex.width * InputTex.height;
        CellBuffer = new ComputeBuffer(count, 16);
        GPUCell[] values = new GPUCell[count];
        for (int y = 0 ; y < InputTex.height; y++)
            for (int x = 0; x < InputTex.width; x++)
            {
                Color c = ((Texture2D)InputTex).GetPixel(x, y);
                GPUCell cell = new GPUCell(x, y,
                    c.a != 0 ? 1 : 0, c.a != 0 ? 1 : 0);
                values[x + y * InputTex.width] = cell;
            }
        //装填数据
        CellBuffer.SetData(values);
        GoLComputeShader.SetBuffer(kernelIdx, "Cells", CellBuffer);
        GoLComputeShader.SetBuffer(updateIdx, "Cells", CellBuffer);
    }
    #endregion
}
