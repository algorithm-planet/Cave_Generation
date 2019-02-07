using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave : MonoBehaviour
{
    int[,] map_grid;
    int[,] New_map_grid;

    //Bridge
    List<Vector3> Cave_Verts;
    List<int> Cave_tri_index;
    List<List<int>> OutLines;

    List<Vector3> Wall_Verts;
    List<int>          Wall_tri_index;

    // Connect
    List<Vector3> Room_centers;

    [Range(20 , 120)]
    public int map_width = 80;
    [Range(20, 120)]
    public int map_height = 40;
    [Range(0.1f  , 1f)]
    public float s = 1f;
    [Range(40, 60)]
    public int Fill_percent = 50;
    public string save_at = "";
    public bool random_string = false;

    [Header("Mesh")]
    public MeshFilter Cave_Top;
    public MeshFilter Cave_Wall;
    public MeshCollider Cave_top_collider;
    public MeshCollider Cave_wall_collider;

    [Header("steps_test")]
    [Range(-10 , 10)]
    public int u = 1;
    [Range(-10, 10)]
    public int v = 1;

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Generate_Cave();
            //
            Generate_Mesh();
            //
        }

    }

    void Generate_Cave()
    {
        #region Map_......._Connect
        string seed = random_string ? Random_string(5) : save_at;
        map_grid = Map.Generate_cells(map_width, map_height, Fill_percent, seed);

        Room_Connect room_Connect = new Room_Connect(map_grid);
        room_Connect.Link_();
        New_map_grid = room_Connect.New_2d_int;
        #endregion

        #region Bridge
        Bridge bridge = new Bridge(New_map_grid, s);
        bridge.Loop_through_all_squares();

        Cave_Verts = bridge.Verts_;
        Cave_tri_index = bridge.Tri_index;
        OutLines = bridge.Outlines;

        #endregion
       //
    }

    void Generate_Mesh()
    {
        #region Cave_Top
        Mesh mesh_01 = new Mesh();
        mesh_01.vertices = Cave_Verts.ToArray();
        mesh_01.triangles = Cave_tri_index.ToArray();
        mesh_01.RecalculateNormals();

        Cave_top_collider.sharedMesh = mesh_01;//Collider_top
        Cave_Top.mesh = mesh_01;
        #endregion

        #region The_Wall_

        #region Verts_
        Wall_Square_Grids wall_Square_Grids = new Wall_Square_Grids(OutLines, Cave_Verts, 5);
        List<n_Node> n_Nodes = wall_Square_Grids.Points_;
        Wall_Verts = new List<Vector3>();

        for (int i = 0; i < n_Nodes.Count; i += 1)
        {           
            Vector3 v = n_Nodes[i].pos_;
            Debug.DrawRay(n_Nodes[i].pos_   , n_Nodes[i].dir_ * 0.5f  ,  Color.yellow    ,   1f);
            //
            Wall_Verts.Add(v);
        }
        #endregion

        Wall_tri_index = wall_Square_Grids.Tri_index;

        Mesh mesh_02 = new Mesh();
        mesh_02.vertices = Wall_Verts.ToArray();
        mesh_02.triangles = Wall_tri_index.ToArray();
        mesh_02.RecalculateNormals();

        Cave_Wall.mesh = mesh_02;
        Cave_wall_collider.sharedMesh = mesh_02;
        #endregion
    }

   
    Vector3 position_(int x, int y)
    {
        Vector3 Center_ = new Vector3((map_width - 1) / 2f, 0f, (map_height - 1) / 2f);
        Vector3 fromCorner_ = new Vector3(x * s, 0f, y * s);

        Vector3 p_ = fromCorner_ - Center_;
        return p_;
    }


    #endregion

    #region Methods_
    // Random_string
    string Random_string(int Length)
    {
        string x = "";
        string Construct = "!@#$%^&*(){}[]<>?|";

        for(int i = 0;  i < Length; i += 1)
        {
            int random_index = Random.Range(0, Construct.Length);
            x += Construct[random_index];
        }
        Debug.Log(x);
        return x;
    }
    #endregion

}
