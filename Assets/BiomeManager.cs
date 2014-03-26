using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BiomeManager {

	public static int[] Fill(int tile, int size) {

		int[] map = new int[size * size];

		for (int i=0; i<map.Length; i++) {
				map[i] = tile;
		}
		return map;
	}
	

	public static IEnumerable<Vector2> GetPointsOnLine(int x0, int y0, int x1, int y1)
	{
		bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
		if (steep)
		{
			int t;
			t = x0; // swap x0 and y0
			x0 = y0;
			y0 = t;
			t = x1; // swap x1 and y1
			x1 = y1;
			y1 = t;
		}
		if (x0 > x1)
		{
			int t;
			t = x0; // swap x0 and x1
			x0 = x1;
			x1 = t;
			t = y0; // swap y0 and y1
			y0 = y1;
			y1 = t;
		}
		int dx = x1 - x0;
		int dy = Mathf.Abs(y1 - y0);
		int error = dx / 2;
		int ystep = (y0 < y1) ? 1 : -1;
		int y = y0;
		for (int x = x0; x <= x1; x++)
		{
			yield return new Vector2((steep ? y : x), (steep ? x : y));
			error = error - dy;
			if (error < 0)
			{
				y += ystep;
				error += dx;
			}

		}
		yield break;
	}

    public static int[] generateBiomeHills(int width, int height, float smooth,int type, int line_biome, int under_biome, int empty,bool genSpawns=false)
    {
        int[] ret = new int[width * height];
        int y_start = height/2;

        for (int x = 0; x < width; x++)
        {
            float a = (180 * ((float)x / (float)width)) * Mathf.Deg2Rad;
            float yy = 0;

            yy = Mathf.Sin(a * type) * smooth;


            ret[x + (y_start + (int)yy) * width] = line_biome;

            if (genSpawns)
            {
                if (x == width / 2)
                {
                    OmniWorld.SpawnPoints.Add(new Vector2(x, (y_start + (int)yy)));
                }
            }

            bool flag = false;
            for (int y = 0; y < height; y++)
            {
                int id = x + y * width;
                if (ret[id] == line_biome)
                    flag = true;
                else if (!flag)
                    ret[id] = under_biome;
                else if (flag)
                    ret[id] = empty;

            }
        }


        return ret;
    }
	
	public static int[] generateHills(int width, int height, float smooth,int empty, int hill_line_item,int matGrass) {

		int[] map = new int[width * height];
		for (int i=0; i<map.Length; i++)
						map [i] = empty;

		float slope = 1;
		float dx = height/2;

		Vector2[] heights = new Vector2[(int)smooth];
		for (int i=0; i<smooth; i++) {
			heights[i] = new Vector2(width * (float)((1/smooth)*i),1+(int)(Random.value * (int)(height/2)));
		}
		for (int i=0; i<smooth-1; i++) {
			IEnumerable<Vector2> points;
			float a = (float)i * ((float)width/(float)smooth);
			points = GetPointsOnLine((int)heights[i].x,(int)heights[i].y,(int)heights[i+1].x,(int)heights[i+1].y);
			foreach(Vector2 p in points) {
				int index = (int)(p.x + p.y * width);
				map [index] = hill_line_item;
			}
		}
		IEnumerable<Vector2> p2 = GetPointsOnLine((int)heights[(int)smooth-1].x,(int)heights[(int)smooth-1].y,width,1);
		foreach(Vector2 p in p2) {
			int index = (int)(p.x + p.y * width);
			map [index] = hill_line_item;
		}
		for (int x = 0; x<width; x++) {
			bool flag = false;
			for (int y = height-1; y>=0; y--) {
			int i = x + y * width;
			if(flag)
					map[i] = matGrass;

			if(map[i] == hill_line_item)
					flag = true;

			}
		}
		return map;
	}

	public static int[] generateCorner(int size, int dir, int top_tile, int under_tile, int empty) {
		int[] ret = new int[size * size];
		for(int i=0; i<ret.Length;i++)
			ret[i] = empty; 

		IEnumerable<Vector2> points;

		if (dir > 0) {
						points = GetPointsOnLine (0, size/2, size-1, size-1);
				} else {
						points = GetPointsOnLine (size-1, size/2, 0, size-1);
		}

		foreach (Vector2 p in points) {
			ret[(int)p.x + (int)p.y * size] = top_tile;
		}

		for (int x = 0; x < size; x++)
			for (int y=0; y < size; y++) {
					if(ret[x+y*size] == top_tile)
						break;
			ret[x+y*size] = under_tile;
		}
		return ret;
	}


	public static int[] generateCaves(int width, int height, int main_tile, int smoothness, int percent) {
		
		int ii,jj;
		
		
		int[] map1 = new int[width*height];
		int[] map2 = new int[width*height];

		int[] ret = new int[width * height];
		for (int i=0; i<ret.Length; i++) {
						ret [i] = -1;
			map1[i] = -1;
			map2[i] = -1;
				}

		
		for(int x=0; x<width; x++) {
			for(int y=0; y<height; y++) {
				int i = x + y * width;
				if(Random.value <= percent) {

					map1[i] = main_tile;
				}
				map2[i] = main_tile;
			}
		}
		
		
		
		for(int i=0; i<smoothness; i++) {
			
			for(int x=1; x<width-1; x++) {
				for(int y=1; y<height-1; y++) {
					
					int pass = 0;
					int pass2 = 0;
					int index = x + y * width;
					for(ii=-1; ii<=1; ii++)
						for(jj=-1; jj<=1; jj++)
					{
						int index2 = (x+ii) + (y+jj) * width;
						if(map1[index2] > 0)
							pass++;
					}
					for(ii=x-2; ii<=x+2; ii++)
						for(jj=y-2; jj<=y+2; jj++)
					{
						int index2 = (ii) + (jj) * width;
						if(Mathf.Abs(ii-x)==2 && Mathf.Abs(jj-y)==2)
							continue;
						if(ii<0 || jj<0 || ii>=width || jj>=height)
							continue;
						if(map1[index2] > 0)
							pass2++;
					}
					if(pass >= 5 || pass2 <= 3) {
						map2[index] = main_tile;
						
					}
					else {
						map2[index] = 0;
					}
				}
			}
			for(int x2=0; x2<width; x2++)
				for(int y2=0; y2<height; y2++) {
				int index = (x2) + (y2) * width;
					map1[index] = map2[index];
			}
		}
		for (int x2=0; x2<width; x2++)
			for (int y2=0; y2<height; y2++) {
				int index = x2 + y2 * width;
				ret[index] = map1[index];
		}
		
		return ret;
		
	}
}

