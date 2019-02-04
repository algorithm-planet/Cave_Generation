using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge
{
    public List<Vector3> Verts_;
    public List<int> Tri_index;
    public List<List<int>> Outlines;

    //
    Square_Grid square_Grid;   
    public Bridge(int[ , ] int_2D , float square_Dimension) 
    {
        square_Grid = new Square_Grid(int_2D, square_Dimension);
    }

    public void Loop_through_all_squares()                       
    {
        #region Initialize
        Verts_ = new List<Vector3>();
        Tri_index = new List<int>();
        Outlines = new List<List<int>>();

        triangle_Dictionary = new Dictionary<int, List<Triangle>>();
        checked_vertex = new HashSet<int>();
        #endregion

        Square[,] squares = square_Grid.squares;
        int x_s = squares.GetLength(0);
        int y_s = squares.GetLength(1);

        #region foreach_ sq
        for (int y = 0;  y < y_s;  y += 1)
        {
            for (int x = 0; x < x_s; x += 1)
            {              
                Square sq = squares[x, y];
                In_a_square(sq);//
            }
        }
        #endregion
        //
        Create_Outlines();
    }

    Dictionary<int, List<Triangle>> triangle_Dictionary;
    HashSet<int> checked_vertex;

    
    #region sq

    #region     // Anti_Clockwise//  sq
    void In_a_square(Square sq)
    {
        int switch_on = sq.sum;
        Control_Node[] c = sq.control_Nodes;
        Node[] n = sq.nodes;

        switch (switch_on)
        {
            // One_point
            case 8:
                Make_Flow(n[0], n[3], c[0]);
                break;
            case 4:
                Make_Flow(n[1], n[0], c[1]);                                  //  Isolated_Cave_region
                break;
            case 2:
                Make_Flow(n[2], n[1], c[2]);
                break;
            case 1:
                Make_Flow(n[3], n[2], c[3]);
                break;
            // Three_point
            case 13:
                Make_Flow(n[1], n[2], c[3], c[0], c[1]);
                break;
            case 14:
                Make_Flow(n[2], n[3], c[0], c[1], c[2]);                //  Isolated_Cave_region
                break;
            case 7:
                Make_Flow(n[3], n[0], c[1], c[2], c[3]);
                break;
            case 11:
                Make_Flow(n[0], n[1], c[2], c[3], c[0]);              //    Cave
                break;
            // Two_point....1
            case 12:
                Make_Flow(n[1], n[3], c[0], c[1]);
                break;
            case 6:
                Make_Flow(n[2], n[0], c[1], c[2]);
                break;
            case 3:
                Make_Flow(n[3], n[1], c[2], c[3]);
                break;
            case 9:
                Make_Flow(n[0], n[2], c[3], c[0]);
                break;
            //Two_point....2
            case 5:
                Make_Flow(c[1], n[1], n[2], c[3], n[3], n[0]);
                break;
            case 10:
                Make_Flow(c[2], n[2], n[3], c[0], n[0], n[1]);
                break;
            //Four_point
            case 15:
                Make_Flow(c[0], c[1], c[2], c[3]);
                checked_vertex.Add(c[0].tri_index);        // Optimize
                checked_vertex.Add(c[1].tri_index);
                checked_vertex.Add(c[2].tri_index);
                checked_vertex.Add(c[3].tri_index);
                break;
            // empty
            case 0:
                break;
        }

    }
    #endregion

    #region Make_Flow
    void Make_Flow(params Node[]  nodes_to_consider)
    {
        //
        Assign_index_to_Nodes(nodes_to_consider);

        Make_triangle_(nodes_to_consider);
    }
    #endregion

    #region Create_Bridge................Verts <-> tri_index
    void Assign_index_to_Nodes(Node[] nodes)                             
    {
        for(int i = 0;  i < nodes.Length; i += 1)
        {
            bool empty_ = (nodes[i].tri_index == -1);
            if (empty_)
            {
                nodes[i].tri_index = Verts_.Count;                                                    // Bridge
                Verts_.Add(nodes[i].pos_);              
            }
            //
        }
    }

    #endregion

    #region Make_Triangle_....<v>
    void Make_triangle_(Node[] N_)
    {
        #region index_
        int[] index_ = new int[N_.Length];
        for (int n = 0; n < index_.Length; n += 1)
        {
            index_[n] = N_[n].tri_index;
        }
        #endregion

        #region Make_triangle
        int num_of_triangles = (N_.Length - 2);
        for (int i = 0; i < num_of_triangles; i += 1)
        {
            Tri_index.Add(index_[0]);
            Tri_index.Add(index_[i + 1]);
            Tri_index.Add(index_[i + 2]);

            Triangle new_triangle = new Triangle(index_[0], index_[i + 1], index_[i + 2]);

            Add_to_Dictionary_of_triangle(index_[0], new_triangle);
            Add_to_Dictionary_of_triangle(index_[i  + 1], new_triangle);
            Add_to_Dictionary_of_triangle(index_[i  +  2], new_triangle);
        }
        #endregion
    }

    #region add_to_triangle_dictionary
    // 
    void Add_to_Dictionary_of_triangle(int i, Triangle triangle)
    {
        if (triangle_Dictionary.ContainsKey(i))
        {
            triangle_Dictionary[i].Add(triangle);
        }
        else
        {
            List<Triangle> new_list = new List<Triangle>();
            new_list.Add(triangle);
            triangle_Dictionary.Add(i, new_list);
        }
    }
    #endregion
    #endregion

    #endregion
    

    //
    #region OUTLINES

    #region Create_OutLine..
    void Create_Outlines()
    {
        for(int vertex_index = 0    ;    vertex_index < Verts_.Count      ;       vertex_index += 1)
        {
            int find_index = Find_next_index(vertex_index);
            bool Found_nothing = (find_index == -10);
            if (!Found_nothing)
            {
                int start_index = vertex_index;
                List<int> New_Outline = new List<int>();

                #region Create_new_Out_Line
                New_Outline.Add(vertex_index);
                checked_vertex.Add(start_index);

                int next_index = Find_next_index(start_index);
                //
                keep_Going_outline(next_index, New_Outline);
                //
                #endregion

                New_Outline.Add(start_index);

                Outlines.Add(New_Outline);
            }
        }
    }
    #region Keep_going
    void keep_Going_outline(int current_index , List<int> Current_outline)
    {
        Current_outline.Add(current_index);
        //CHAIN
        checked_vertex.Add(current_index);
        int next_index = Find_next_index(current_index);
        //
        bool both_forward_and_previous_are_checked = (next_index == -10);
        // Loop
        if (!both_forward_and_previous_are_checked)
        {
            keep_Going_outline(next_index, Current_outline);
        }
        //
    }
    #endregion
    #endregion

    #region  bool................Find_next
    //next_int
    int Find_next_index(int a)
    {
        List<Triangle> triangles_a = triangle_Dictionary[a];
        foreach (Triangle _triangle_a in triangles_a)
        {
            foreach (int v in _triangle_a.vertex)
            {
                //
                if (!checked_vertex.Contains(v)) // Dont_check_previous_One...or(forward_at_Last).................Keep_Going
                {
                    if (Is_an_outline_edge(a, v))
                    {
                        return v;
                    }
                }
                //
            }
        }
        return -10;
    }

    // Bool
    bool Is_an_outline_edge(int a, int b)
    {
        List<Triangle> triangles_a = triangle_Dictionary[a];

        int sum = 0;
        foreach (Triangle _triangle_a in triangles_a)
        {
            if (_triangle_a.contains_vertex_(b))
            {
                sum += 1;
            }
        }

        return (sum == 1);
    }
    #endregion

    #endregion

}

