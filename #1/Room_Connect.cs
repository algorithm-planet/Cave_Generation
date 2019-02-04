using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room_Connect
{
    public int[,] New_2d_int;
    public List<Vector3> mid_points_In_rooms;

    int[,] map_grid;
    int map_width;
    int map_height;
    float s = 1f;

    List<Room> Survival_Rooms;
    List<Coord> From;
    List<Coord> To;

    public Room_Connect(int[ ,] Old_2d_int )
    {
        int x_L = Old_2d_int.GetLength(0);
        int y_L = Old_2d_int.GetLength(1);
        map_grid = new int[x_L, y_L];

        map_width = x_L;
        map_height = y_L;
        map_grid = Old_2d_int;
    }


    public void Link_()
    {
        //Modify_and_Set;
        Min_Remove_and_Add_Survived_Rooms();   //  01

        //Connect_passages   
        Find_Main_Max_Room();       // 02 

        From = new List<Coord>();
        To = new List<Coord>();
        Connection_Flow();                   // 03

        //Make_Lines_and_Clear_Path(radius : 1);                // 04 
        //

        New_2d_int = map_grid;
        //

    }

    #region Flood_flow
    List<Coord> Flood_flow(int startX, int startY)
    {
        List<Coord> _Unchecked = new List<Coord>();
        List<Coord> _Checked = new List<Coord>();

        int[,] map_Flags = new int[map_width, map_height];
        int tile_Type = map_grid[startX, startY];

        Coord start_at = new Coord(startX, startY);
        _Unchecked.Add(start_at);
        map_Flags[start_at.X, start_at.Y] = 1;

        while(_Unchecked.Count > 0)
        {
            Coord current_at = remove_first_and_return(_Unchecked);
            _Checked.Add(current_at);//_Checked

            for(int y = current_at.Y - 1; y <= current_at.Y + 1; y += 1)
            {
                for (int x = current_at.X - 1; x <= current_at.X + 1; x += 1)
                {   
                                if( In_map_range(x, y))       // In_range   
                                {
                                    if (x == current_at.X || y == current_at.Y)             // non_Diagonal
                                   {
                                        // tile_Type && map_flags
                                        if (map_grid[x, y] == tile_Type && map_Flags[x, y] == 0)
                                        {
                                            Coord new_coord = new Coord(x, y);
                                            _Unchecked.Add(new_coord);
                                            map_Flags[x, y] = 1;
                                        }
                                        //
                                    }
                                }
                }
            }

        }
        return _Checked;
    }

    Coord remove_first_and_return(List<Coord> List)
    {
        Coord coord = List[0];
        List.RemoveAt(0);
        return coord;
    }

    bool In_map_range(int x , int y)
    {
        return (x >= 0 && x < map_width     &&   y >= 0 && y < map_height);
    }
    #endregion

    #region Region_flow
    List<List<Coord>> Regions_(int tile_Type)
    {
        List<List<Coord>> regions_of_type = new List<List<Coord>>();

        int[,] map_Flags = new int[map_width, map_height];

        for (int y = 0; y < map_height; y += 1)
        {
            for (int x = 0; x < map_width; x += 1)
            {
                            //  tile_Type  &&  map_Flags  //
                            if (map_grid[x, y] == tile_Type && map_Flags[x, y] == 0)
                            {
                                List<Coord> new_region = new List<Coord>();
                                new_region = Flood_flow(x, y);
                                regions_of_type.Add(new_region);
                                // set_Flags
                                foreach (Coord c in new_region)
                                {
                                    map_Flags[c.X, c.Y] = 1;
                                }
                                //
                            }
            }
        }
        return regions_of_type;
    }
    #endregion

    Vector3 position_(int x , int y)
    {
        Vector3 Center_ = new Vector3((map_width - 1) / 2f, 0f, (map_height - 1) / 2f);
        Vector3 fromCorner_ = new Vector3(x * s, 0f, y * s);

        Vector3 p_ = fromCorner_    -    Center_;
        return p_;
    }

    #region Min_Remove_and_Add_Survived_Rooms
    void Min_Remove_and_Add_Survived_Rooms()
    {
        //
        List<List<Coord>> Walls_ = Regions_(1);
        int wall_isolated_min = 17;
        foreach (List<Coord> coords in Walls_)
        {
            if (coords.Count <= wall_isolated_min)
            {
                //Debug.Log("Wall   :  " + coords.Count);
                foreach (Coord c in coords)
                {
                    map_grid[c.X, c.Y] = 0;
                }
            }
        }
        //
        List<List<Coord>> Rooms_ = Regions_(0);
        int empty_isolated_min = 40;
        Survival_Rooms = new List<Room>();                          // Initiate_Survived_Rooms
        foreach (List<Coord> coords in Rooms_)
        {
            if (coords.Count <= empty_isolated_min)
            {
                //Debug.Log("Room  :  " + coords.Count);
                foreach (Coord c in coords)
                {
                    map_grid[c.X, c.Y] = 1;
                }
            }
            else
            {
                Room new_room = new Room(coords, map_grid);
                //Debug.Log(new_room.Mid_point);
                Survival_Rooms.Add(new_room);
            }
        }
        //
    }
    #endregion

    #region Find_Main_Max_Room
    void Find_Main_Max_Room()
    {
        int  max_Coords = 0;
        int index = 0;
        for(int i = 0; i < Survival_Rooms.Count; i += 1)
        {
            //
            if(Survival_Rooms[i].Total_tiles >= max_Coords)
            {
                max_Coords = Survival_Rooms[i].Total_tiles;
                index = i;
            }
            //
        }
        // Max_Coord_Room
        Survival_Rooms[index].Access_M = true;
        //Debug.Log(max_Coords + " // " + Survival_Rooms[index].Edge_tiles.Count   + "//" + index); 
        
    }
    #endregion
 

    //_CONNECT_

    #region Connection_Flow
    void Connection_Flow( )
    {
        #region Initialize
        Coord best_Coord_A = new Coord();
        Coord best_Coord_B = new Coord();

        Room best_Room_A = new Room();
        Room best_Room_B = new Room();
        float t1 = 1f;
       
        int best_dist = 0;
        bool possible_connection_found = false;
        List<Room> All_Rooms = Survival_Rooms;
        #endregion

        foreach(Room _From_room in All_Rooms)  
        {
            possible_connection_found = false;
            if (_From_room.Connected_rooms.Count > 0) 
            {
                continue;
            }
            foreach (Room _To_room in All_Rooms)
            {
                if (_From_room  ==  _To_room)  
                {
                    continue;
                }
                
                #region Edge_tiles_dist
                for (int y = 0; y < _From_room.Edge_tiles.Count; y += 1)
                {
                    for (int x = 0; x < _To_room.Edge_tiles.Count; x += 1)
                    {
                        Coord tile_01 = _From_room.Edge_tiles[y];
                        Coord tile_02 = _To_room.Edge_tiles[x];
                        int dist_ = (int)(Mathf.Pow(tile_02.X - tile_01.X, 2f) + Mathf.Pow(tile_02.Y - tile_01.Y, 2f));
                        if (dist_ < best_dist || !possible_connection_found)
                        {
                            //
                            possible_connection_found = true;
                            best_dist = dist_;
                            //
                            best_Coord_A = tile_01;
                            best_Coord_B = tile_02;
                            best_Room_A = _From_room;
                            best_Room_B = _To_room;
                        }
                    }
                }
                #endregion
            }
            #region Possible_connection_found
            if (possible_connection_found )  
            {
                Debug.DrawLine(position_(best_Coord_A.X, best_Coord_A.Y), position_(best_Coord_B.X, best_Coord_B.Y), Color.red, t1);
                Passage_Create(best_Coord_A, best_Coord_B, best_Room_A, best_Room_B);
                t1 += 0.2f;
            }
            #endregion
        }
        //
        Connection_Flow_for_Big_Room();
    }
    #endregion

    float t2 = 3f;
    #region Connection_Flow_for_Big_Room
    void Connection_Flow_for_Big_Room(  )
    {
        #region Initialize
        Coord best_Coord_A = new Coord();
        Coord best_Coord_B = new Coord();

        Room best_Room_A = new Room();
        Room best_Room_B = new Room();
       
        int best_dist = 0;
        bool Begin = true;
        #endregion

        #region List_of_Acceseble_&&_Un_Acceseble
        List<Room> Acceseble = new List<Room>();
        List<Room> Un_Acceseble = new List<Room>();

        foreach (Room room in Survival_Rooms)
        {
            if (room.Access_M)
            {
                Acceseble.Add(room);
            }
            else
            {
                Un_Acceseble.Add(room);
            }
        }
        //Debug.Log(Acceseble.Count + "//" + Un_Acceseble.Count);
        #endregion

        foreach (Room un_accesable_r in Un_Acceseble) 
        {
            foreach (Room accesable_r in Acceseble)
            {
                #region Edge_tiles_dist
                for (int y_ = 0; y_ < un_accesable_r.Edge_tiles.Count; y_ += 1)
                {
                    for (int x_ = 0; x_ < accesable_r.Edge_tiles.Count; x_ += 1)
                    {
                        Coord tile_01 = un_accesable_r.Edge_tiles[y_];
                        Coord tile_02 = accesable_r.Edge_tiles[x_];
                        int dist_ = (int)(Mathf.Pow(tile_02.X - tile_01.X, 2f) + Mathf.Pow(tile_02.Y - tile_01.Y, 2f));
                        if (dist_ < best_dist       ||       Begin)
                        {
                            best_dist = dist_;
                            Begin = false;
                            //
                            best_Coord_A = tile_01;
                            best_Coord_B = tile_02;
                            best_Room_A = un_accesable_r;
                            best_Room_B = accesable_r;
                        }
                    }
                }
                #endregion
            }
        }

        #region Debug_Line
        t2 += 0.2f;
        float x = t2;
        Debug.DrawLine(position_(best_Coord_A.X, best_Coord_A.Y), position_(best_Coord_B.X, best_Coord_B.Y), Color.green, x);
        #endregion
        
        Passage_Create(best_Coord_A, best_Coord_B, best_Room_A, best_Room_B);
        
        #region sum_loop
        int sum = 0;
        foreach (Room A_Room in Survival_Rooms)
        {
            if  (!A_Room.Access_M   )
            {
                sum += 1;
            }
        }
        //Loop
        if(sum > 0)
        {
            Connection_Flow_for_Big_Room();
        }
        #endregion      
    }
    #endregion

    #region Passage_Create
    void Passage_Create(Coord from , Coord to , Room from_room , Room to_room)
    {
        Room.Connect_eachOther(from_room, to_room);
        //
        From.Add(from);
        To.Add(to);
        //

        #region Debug_line
        /*      
             float t1 = 5f;// time
             float t2 = t1 / 2f;
        if (!loop_ability)
        {
            //Debug.DrawLine(position_(from.X, from.Y), position_(to.X, to.Y), Color.red, t1);
        }
        else
        {
            //Debug.DrawLine(position_(from.X, from.Y), position_(to.X, to.Y), Color.green, t2);
        }
        */
        #endregion
    }
    #endregion

    //_Clear_Path_
    void Make_Lines_and_Clear_Path(int radius = 1)
    {
        List<Orthogonal_steps> steps = new List<Orthogonal_steps>();
        for(int i = 0; i < From.Count; i += 1)  // From< >_...Coord 
        {
            Orthogonal_steps new_steps = new Orthogonal_steps(From[i], To[i]);
            steps.Add(new_steps);
        }

        #region Clear_path
        foreach (Orthogonal_steps steps_path in steps)
        {
            foreach (Coord c in steps_path.Points)
            {
                Circle new_circle = new Circle(c, radius);//radius
                foreach (Coord circle_coord in new_circle.region_Bound)
                {
                    map_grid[circle_coord.X, circle_coord.Y] = 0;
                }
            }
        }
        #endregion
    }

}
