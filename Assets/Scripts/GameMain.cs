using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell
{
    public int x;
    public int y;
    public bool isAliveNow;
    public bool isAliveNext;
    public Image imageCom;
    public List<Cell> neighbors;
    public Cell(int x, int y, bool isAlive)
    {
        this.x = x;
        this.y = y;
        isAliveNow = isAlive;
        isAliveNext = false;
        imageCom = null;
        neighbors = new List<Cell>();
    }

    public void ShowNow()
    {
        if (imageCom == null)
            return;
        imageCom.color = isAliveNow ? Color.black : Color.white;
    }

    public void UpdateNext()
    {
        isAliveNow = isAliveNext;
        isAliveNext = false;
    }

    public void SetNext(bool b)
    {
        isAliveNext = b;
    }

    public int GetAliveNeighborNum()
    {
        int res = 0;
        foreach(Cell neighbor in neighbors)
        {
            if (neighbor != null && neighbor.isAliveNow)
                res++;
        }
        return res;
    }
}

public class GameMain : MonoBehaviour {

    [SerializeField]
    private Texture2D inputTex;
    private int GameWidth;
    private int GameHeight;


    private const int SIZE_OF_CELL = 2;

    private List<Cell> cells;

	void Start () {
        InitByInputTex();
        InitMatrix();
	}

    void Update()
    {
        if (inputTex == null) return;
        foreach(Cell cell in cells)
        {
            int num = cell.GetAliveNeighborNum();
            if (cell.isAliveNow)
            {
                if (num < 2)
                    cell.SetNext(false);
                else if (num < 4)
                    cell.SetNext(cell.isAliveNow);
                else
                    cell.SetNext(false);
            }
            else if (num == 3)
                cell.SetNext(true);
        }
        foreach(Cell cell in cells)
        {
            cell.ShowNow();
            cell.UpdateNext();
        }
    }

    private void InitByInputTex()
    {
        if (inputTex == null)
            return;
        GameWidth = inputTex.width;
        GameHeight = inputTex.height;
        cells = new List<Cell>();
        for(int y = 0; y < GameHeight; y++)
            for (int x = 0; x < GameWidth; x++)
            {
                Color c = inputTex.GetPixel(x, y);
                cells.Add(new Cell(x, y, c.a > 0));
            }
    }

    private void InitMatrix()
    {
        Transform templet = transform.Find("CellTemplet");
        if (templet == null)
            return;
        for (int y = 0; y < GameHeight; y++)
            for (int x = 0; x < GameWidth; x++)
            {
                GameObject cell = Object.Instantiate<GameObject>(templet.gameObject);
                cell.transform.parent = transform;
                cell.transform.localPosition = new Vector3((x - (GameWidth >> 1)) * SIZE_OF_CELL, (y - (GameHeight >> 1)) * SIZE_OF_CELL, 0);
                cell.SetActive(true);
                cells[x + y * GameWidth].imageCom = cell.GetComponent<Image>();
            }
        foreach (Cell cell in cells)
        {
            cell.neighbors.Add(GetCell(cell.x - 1, cell.y - 1));
            cell.neighbors.Add(GetCell(cell.x - 1, cell.y));
            cell.neighbors.Add(GetCell(cell.x - 1, cell.y + 1));
            cell.neighbors.Add(GetCell(cell.x, cell.y - 1));
            cell.neighbors.Add(GetCell(cell.x, cell.y + 1));
            cell.neighbors.Add(GetCell(cell.x + 1, cell.y - 1));
            cell.neighbors.Add(GetCell(cell.x + 1, cell.y));
            cell.neighbors.Add(GetCell(cell.x + 1, cell.y + 1));
        }
    }

    private Cell GetCell(int x, int y)
    {
        if (x < 0 || x >= GameWidth || y < 0 || y >= GameHeight)
            return null;
        return cells[x + y * GameWidth];
    }

}
