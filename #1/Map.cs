using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class Map
{
    public static int[ , ] Generate_cells(int map_width  , int map_height ,int fill_percent = 50 ,  string save  = " ")
    {
        int[,] map_grid = new int[map_width, map_height];

        // pseudo_random
        System.Random pseudo_random = new System.Random(save.GetHashCode());
        //

        for(int y = 0;  y < map_height; y += 1)
        {
            for (int x = 0; x < map_width; x += 1)
            {
                int n = (  pseudo_random.Next(0, 100) < fill_percent) ? 1 : 0;                     // wall..........1
                map_grid[x, y] = n;              
            }
        }

        for(int  i = 0; i < 3; i += 1)
        {
            Smooth(map_grid);
        }
        //Border_grid_Apply();
        int[,] new_map_grid = border_grid(map_grid, 5);
        return new_map_grid;
    }

    //Neighbour
    static int number_of_neighbours(int x_ , int y_ ,  int[ , ] map_grid)
    {
        int sum = 0;

        for(int y = y_  - 1; y  <= y_+ 1; y += 1)
        {
            for (int x = x_ - 1; x <= x_ + 1; x += 1)
            {
                bool own = (x == x_ && y == y_); // own.....
                if (!own)
                {
                    int n = map_grid[x, y];
                    sum += n;
                }
            }
        }

        //Debug.Log( sum);
        return sum;
    }

    //Smooth
    static void Smooth(int[ , ] map_grid)
    {
        int x_L = map_grid.GetLength(0);
        int y_L = map_grid.GetLength(1);

        for(int y = 0;  y < y_L ; y++)
        {
            for (int x = 0; x < x_L ; x++)
            {
                        bool Corner = (x == 0 || x == x_L - 1 || y == 0 || y == y_L - 1);
                        if (Corner)
                        {
                            map_grid[x, y] = 1;
                        }
                        else//   Play_Smoothing
                        {
                            int num = number_of_neighbours(x, y, map_grid);
                          //
                            if(num > 4)
                            {
                                map_grid[x, y] = 1;
                                //Debug.Log("B");
                            }
                            else if(num < 4)
                            {
                                map_grid[x, y] = 0;
                                //Debug.Log("A");
                            }
                            //
                         }
            }
        }

    }


    // Border_grid
    static int[ , ] border_grid(int[ , ] map_grid , int b)
    {
        int x_L = map_grid.GetLength(0);
        int y_L = map_grid.GetLength(1);

        int[,] new_map_grid = new int[x_L + 2 * b, y_L + 2 * b];

        for(int y = 0;  y < y_L  + 2 * b; y += 1)
        {
            for (int x = 0; x < x_L + 2 * b; x += 1)
            {
                            if (  x < b  || x > x_L + b - 1          ||        y < b  ||  y > y_L + b - 1)  //      ......   or|| ........
                            {
                                new_map_grid[x, y] = 1;
                            }
                            else
                            {
                                new_map_grid[x, y] = map_grid[x  - b, y  -  b];
                            }
            }
        }

        return new_map_grid;
    }
}
