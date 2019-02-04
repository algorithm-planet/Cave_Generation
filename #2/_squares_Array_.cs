using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square_Grid
{
    public Square[,] squares;

    public Square_Grid(int[ , ] map_grid , float s)
    {
        int x_L = map_grid.GetLength(0);
        int y_L = map_grid.GetLength(1);
    
        #region Control_Node[ , ]
        //control_nodes
        Vector3 centre_ = new Vector3((x_L - 1) * s / 2f, 0f, (y_L - 1) * s / 2f);
        Control_Node[,] C_ = new Control_Node[x_L, y_L];
        for(int y = 0;  y < y_L; y += 1)
        {
            for (int x = 0; x < x_L; x += 1)
            {
                Vector3 from_corner = new Vector3(s * x, 0f, s * y);//
                Vector3 p_ = from_corner  -  centre_;//

                bool On_ = (map_grid[x, y] == 1);
                C_[x, y] = new Control_Node(p_, s, On_);
            }
        }
        #endregion

        #region Square[ , ]
        //squares
        squares = new Square[x_L - 1    ,    y_L - 1];
        for (int y = 0; y < y_L - 1; y += 1)
        {
            for (int x = 0; x < x_L - 1; x += 1)
            {             
                squares[x, y] = new Square(C_[x, y + 1]     ,   C_[x + 1, y + 1]    ,   C_[x + 1, y]    ,   C_[x, y]);
            }
        }
        #endregion
       
    }
}

public class Square
{
    public Control_Node[] control_Nodes;
    public Node[] nodes;
    //
    public int sum;

    public Square(Control_Node a , Control_Node b , Control_Node c , Control_Node d)
    {
        control_Nodes = new Control_Node[4]
        {
            a , b , c , d
        };
        nodes = new Node[4]
        {
            a.right ,c.top , d.right , d.top
        };

        #region sum
        sum = 0;
        //
        if (a.Active)
            sum += 8;
        if (b.Active)
            sum += 4;
        if (c.Active)
            sum +=2;
        if (d.Active)
            sum += 1;
        //
        #endregion

    }
}

public class Node
{
    public Vector3 pos_;
    public int tri_index = -1;

    public Node(Vector3 pos)
    {
        pos_ = pos;
    }
}

public class Control_Node : Node
{
    public bool Active;
    public Node top, right;

    public Control_Node(Vector3 pos , float s , bool Active_ ) : base(pos)
    {
        top = new Node(pos + Vector3.forward * s / 2f);
        right = new Node(pos + Vector3.right * s / 2f);

        Active = Active_;
    }
}

//Triangle
public struct Triangle // struct...    <v>..........[ _ _ _ _ ]
{
    public int[] vertex;

    public Triangle(int a , int b , int c)
    {
        vertex = new int[3]
        {
            a , b , c
        };
    }

    public bool contains_vertex_(int i)
    {
        return (i == vertex[0] || i == vertex[1] || i == vertex[2]);
    }
}


public class n_Node
{
    public Vector3 pos_;
    public int tri_index = -1;
    public Vector3 dir_;

    public n_Node(Vector3 position  , Vector3 normal_direction)
    {
        pos_ = position;
        dir_ = normal_direction;
    }
}

public class n_Square
{
    public n_Node[] n_nodes;

    public n_Square(n_Node a , n_Node b , n_Node c , n_Node d)
    {
        n_nodes = new n_Node[4]
        {
            a , b , c , d
        };
    }
}


public class Wall_Square_Grids
{
    public List<n_Node> Points_;
    public List<int> Tri_index;

    List<n_Node> wall_spacing_points;

    public Wall_Square_Grids(List<List<int>> Outlines  ,  List<Vector3> Verts_     , int r_ =  10 , float w_ = 5f)
    {
        #region Initialize
        Points_ = new List<n_Node>();
        Tri_index = new List<int>();
        #endregion

        #region List<n_Node[,]>
        List<n_Node[,]> N_ = new List<n_Node[,]>();

        float s = w_ / (r_ - 1);

        for (int u = 0; u < Outlines.Count; u += 1)
        {
            List<int> index_ = Outlines[u];
            int x_L = Outlines[u].Count;
            n_Node[,] new_2D_n_Node_Array = new n_Node[x_L, r_];

            for (int x = 0; x < x_L; x += 1)
            {
                Vector3 dir_ = Vector3.zero;
                Vector3 v_ = Verts_[index_[x]];
                #region dir_ && v
                if (x <= x_L - 2)
                {
                    Vector3 a = Verts_[index_[x + 1]] - Verts_[index_[x]];
                    Vector3 b = Vector3.down;

                    dir_ = -Vector3.Cross(a, b);
                }
                else
                {  // Last_vert
                    Vector3 a = Verts_[index_[1]] - Verts_[index_[0]];
                    Vector3 b = Vector3.down;

                    dir_ = -Vector3.Cross(a, b);
                }
                #endregion

                for (int y = 0; y <=  r_ - 1    ; y += 1)
                {
                    Vector3 p_ = v_ + (Vector3.down * (s) * y);
                    new_2D_n_Node_Array[x, y] = new n_Node(p_, dir_);
                }
            }
            N_.Add(new_2D_n_Node_Array);
        }
        #endregion

        #region points_on_wall
        wall_spacing_points = new List<n_Node>();
        for(int i = 0; i <  N_.Count;  i += 1)
        {
            int V_spacing = r_/2;
            int H_spacing = 10;
            int loop_t = N_[i].GetLength(0) / H_spacing;

            int y = V_spacing;
            for(int  x = 0;   x < loop_t   ;  x += H_spacing)
            {
                n_Node new_n_Node = N_[i][x, y];
                wall_spacing_points.Add(new_n_Node);
            }
        }
        //Debug.Log(wall_spacing_points.Count);
        #endregion

        #region List<n_Square>
        List<n_Square> n_squares = new List<n_Square>();

        for(int u = 0;  u  <  Outlines.Count; u  += 1)
        {
            int x_L = Outlines[u].Count;
            n_Node[,] _n_ = N_[u];

            for(int y = 0;  y  <=  r_ - 2; y += 1)
            {
                for(int x = 0;  x <=  x_L - 2; x += 1)
                {
                    n_Square new_n_sq = new n_Square(_n_[x, y], _n_[x, y + 1], _n_[x + 1, y + 1], _n_[x + 1, y]);
                    n_squares.Add(new_n_sq);
                }
            }
        }
        #endregion

        #region Loop_through_all     n_sq in ...........List
        for (int i = 0; i < n_squares.Count; i += 1)
        {
            n_Square sq = n_squares[i];
            // Create_Bridge
            Create_Bridge(sq);
            //Make Triangle
            Make_Triangle(sq.n_nodes);
        }
        #endregion

    }
    #region Create_Bridge................verts <-> tri_index
    void Create_Bridge(n_Square sq)
    {
        n_Node[] nodes = sq.n_nodes;
        for (int n = 0; n < nodes.Length; n += 1)
        {
            if (nodes[n].tri_index == -1)
            {
                nodes[n].tri_index = Points_.Count;
                Points_.Add(nodes[n]);
            }
        }
        //
    }
    #endregion

    #region Make_Triangle
    void Make_Triangle(n_Node[] nodes)
    {
        for(int i = 0; i < 2; i += 1)
        {
            Tri_index.Add(nodes[0].tri_index);
            Tri_index.Add(nodes[i + 1].tri_index);
            Tri_index.Add(nodes[i + 2].tri_index);
        }
    }
    #endregion

}


