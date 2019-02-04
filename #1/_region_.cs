using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Coord
{
    public int X;
    public int Y;

    public Vector3 position_;

    public Coord(int x , int y )
    {
        X = x;
        Y = y;
    }
    public Coord()
    {

    }
}


#region Orthogonal steps

public class Orthogonal_steps
{
    public List<Coord> Points;
    
    public Orthogonal_steps(Coord Origin , Coord End)
    {
        Points = new List<Coord>();

        int dx = End.X - Origin.X;
        int dy = End.Y - Origin.Y;

        int dx_n = (int)Mathf.Sign(dx);     int dy_n = (int)Mathf.Sign(dy); // dir_n
        int dx_m = Mathf.Abs(dx);             int dy_m = Mathf.Abs(dy); ;       // mag_n

        float slope_ = (float)dy_m / dx_m;
        // Origin
        Coord p = Origin;
        Points.Add(p);
        //
        for(int iy = 0 , ix = 0; iy < dy_m  ||  ix < dx_m;)//.................................till          ix   < dx_m      or      iy < dy_m
        {
            float lines_to_origin_slope = (iy + 0.5f) / (ix + 0.5f);
            if (lines_to_origin_slope > slope_)
            {
                //next point is Horizontal
                p.X += dx_n;
                ix += 1;
            }
            else
            {
                //next point is Vertical
                p.Y += dy_n;
                iy += 1;
            }
            Coord new_coord = new Coord(p.X, p.Y);
            Points.Add(new_coord);
        }

    }
}

#endregion

#region Supercover lines
public class Supercover_lines
{
    public List<Coord> Points;

    public Supercover_lines(Coord Origin, Coord End)
    {
        Points = new List<Coord>();

        int dx = End.X - Origin.X;
        int dy = End.Y - Origin.Y;

        int dx_n = (int)Mathf.Sign(dx);
        int dy_n = (int)Mathf.Sign(dy);

        int dx_m = Mathf.Abs(dx);
        int dy_m = Mathf.Abs(dy);
        float slope = (float)dy_m / dx_m;

        Coord p = Origin;
        Points.Add(p);

        for (int iy = 0, ix = 0; iy < dy_m || ix < dx_m;)
        {
            float lines_to_origin_slope = (iy + 0.5f) / (ix + 0.5f);
            //
             if (lines_to_origin_slope == slope)
            {
                //next point is Diagonal
                p.X += dx_n;
                p.Y += dy_n;
                ix += 1;
                iy += 1;
            }
            else if (lines_to_origin_slope > slope)
            {
                //next point is Horizontal
                p.X += dx_n;
                ix += 1;
            }
            else
            {
                //next point is Vertical
                p.Y += dy_n;
                iy += 1;
            }
            Coord new_coord = new Coord(p.X, p.Y);
            Points.Add(new_coord);
        }

    }

}
#endregion


#region Circle
public class Circle
{
    public List<Coord> region_Bound;

    public Circle(Coord point , int r = 1)
    {
        //
        region_Bound = new List<Coord>();
        for(int y = -r     ; y  <=  r; y += 1)
        {
            for (int x = -r    ; x <= r; x += 1)
            {
                            bool Inside_ = (x * x + y * y <= r * r);
                            if (Inside_)
                            {
                                Coord new_coord = new Coord(point.X + x, point.Y + y);
                                region_Bound.Add(new_coord);
                            }
            }
        }
        //
    }
}
#endregion


public class Room
{
    public List<Coord> region_;
    public List<Coord> Edge_tiles;
    public int Total_tiles;

    public bool Access_M;

    public Vector3 Mid_point;
    int[,] map_grid_2D;

    public List<Room> Connected_rooms;

    public Room()
    {
    }
    public Room(List<Coord> region , int[ , ] map_grid)
    {
        region_ = new List<Coord>();
        Edge_tiles = new List<Coord>();
        Connected_rooms = new List<Room>();

        region_ = region;
        Total_tiles = region.Count;       
        #region Edge_tiles
        for (int i = 0; i < region.Count; i += 1)
        {
            //
            for (int y = region[i].Y - 1; y <= region[i].Y + 1; y += 1)
            {
                for (int x = region[i].X - 1; x <= region[i].X + 1; x += 1)
                {
                            if (x == region[i].X || y == region[i].Y)      // Non_Diagonal
                            {
                                if (map_grid[x, y] == 1)
                                {
                                    Coord new_coord = region[i];
                                    Edge_tiles.Add(new_coord);
                                }
                            }
                }
            }
            //
        }
        #endregion

        #region Mid_point
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < region.Count; i += 1)
        {
            Coord c = region[i];
            Vector3 p_ = position_(c.X, c.Y , map_grid);
            sum += p_;
        }//
        Mid_point = sum/region.Count;
        #endregion
    }
    #region position_
    Vector3 position_(int x, int y, int[,] map_grid)
    {
        int map_width = map_grid.GetLength(0);
        int map_height = map_grid.GetLength(1);
        float s = 1f;

        Vector3 Center_ = new Vector3((map_width - 1) / 2f, 0f, (map_height - 1) / 2f);
        Vector3 fromCorner_ = new Vector3(x * s, 0f, y * s);

        Vector3 p_ = fromCorner_ - Center_;       
        return p_;
    }
    #endregion

    //
    #region Connect_
    public static void Connect_eachOther(Room A, Room B)
    {
        //
        A.Connected_rooms.Add(B);
        B.Connected_rooms.Add(A);
        //
        if (B.Access_M)
        {
            A.Access_M = true;
            List<Room> linked_rooms_01 = A.Connected_rooms;
            linked_rooms_01.Remove(B);
            foreach(Room room_from_A in linked_rooms_01)
            {               
                Flow(room_from_A, A);
            }
        }
        else if (A.Access_M)
        {
            B.Access_M = true;
            List<Room> linked_rooms_01 = B.Connected_rooms;
            linked_rooms_01.Remove(A);
            foreach (Room room_from_B in linked_rooms_01)
            {
                Flow(room_from_B, B);
            }
        }
    }

    static void Flow(Room current_room, Room from_room)
    {
        current_room.Access_M = true;
        
        string x = "";
        List<Room> linked_rooms = current_room.Connected_rooms;

        x += linked_rooms.Count.ToString();
        linked_rooms.Remove(from_room);
        x += "  //  " + linked_rooms.Count.ToString();
        Debug.Log(x);

        if (linked_rooms.Count > 0)
        {
            foreach (Room sub_connected_Room in linked_rooms)
            {
                Flow(sub_connected_Room, current_room);
            }
            //
        }
    }
    #endregion

}


