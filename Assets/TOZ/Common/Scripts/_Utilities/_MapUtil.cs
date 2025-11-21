using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _MapUtil {
		//Get the grayscale texture from float map
		public static Texture2D GetTextureFromGrayscaleMap(float[,] map, float alpha = 1.0f) {
			Texture2D tex = new Texture2D(map.GetLength(0)-1, map.GetLength(1)-1, TextureFormat.RGBA32, true);
			int size = tex.width * tex.height;
			Color[] col = new Color[size];
			for(int y=0; y < tex.height; y++) {
				for(int x=0; x < tex.width; x++) {
					col[y * tex.width + x] = new Color(map[x,y], map[x,y], map[x,y], alpha);
				}
			}
			tex.SetPixels(col);
			tex.Apply();
			return tex;
		}

		//Get the grayscale float map
		public static float[] _GetGrayscaleMapFromTexture1(Texture2D tex) {
			int size = tex.width * tex.height;
			float[] map = new float[size];
			for(int y=0; y < tex.height; y++) {
				for(int x=0; x < tex.width; x++) {
					map[y * tex.width + x] = tex.GetPixel(x, y).grayscale;
				}
			}
			return map;
		}

		public static float[,] _GetGrayscaleMapFromTexture(Texture2D tex) {
			float[,] map = new float[tex.width, tex.height];
			for(int y=0; y < tex.height; y++) {
				for(int x=0; x < tex.width; x++) {
					map[x,y] = tex.GetPixel(x, y).grayscale;
				}
			}
			return map;
		}

		//Get the grayscale float map
		public static float[,] _GetGrayscaleMapFromTerrainData(TerrainData data) {
			return data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);
		}

		//Erode map
		public static void _Erode(float[,] map, float sedimentCapacityFactor = 4f, int brushRadius = 2) {
			int dropletCount = 100000;
			int maxLife = 50;
			float initialSpeed = 1.0f;
			float initialWaterVolume = 1.0f;
			float inertia = 0.05f;
			float gravity = 9.8f;
			float evaporationSpeed = 0.1f;
			float erodeSpeed = 0.1f;
			float depositSpeed = 0.1f;
			float minSedimentCapacity = 0.01f;

			int brushDiameter = (brushRadius * 2) + 1;
			float[,] brush = new float[brushDiameter, brushDiameter];
			float weightSum = 0;
			for(int y = -brushRadius; y <= brushRadius; y++) {
				for(int x = -brushRadius; x <= brushRadius; x++) {
					float sqrDst = x * x + y * y;
					if(sqrDst < brushRadius * brushRadius) {
						float weight = 1f - Mathf.Sqrt(sqrDst) / brushRadius;
						weightSum += weight;
						brush[x + brushRadius, y + brushRadius] = weight;
					}
				}
			}
			for(int y = -brushRadius; y <= brushRadius; y++) {
				for(int x = -brushRadius; x <= brushRadius; x++) {
					brush[x + brushRadius, y + brushRadius] /= weightSum;
				}
			}

			for(int i = 0; i < dropletCount; i++) {
				// Create water droplet at random point on map
				int mapWidth = map.GetLength(0)-1;
				int mapHeight = map.GetLength(1)-1;
				Vector2 pos = new Vector2(_RandUtil._GetRange((float)brushRadius, (float)(mapWidth-brushRadius)), _RandUtil._GetRange((float)brushRadius, (float)(mapHeight-brushRadius)));
				Vector2 dir = Vector2.zero;
				float speed = initialSpeed;
				float waterVolume = initialWaterVolume;
				float sediment = 0;

				//while(waterVolume > 0.0001f) {
				for(int life = 0; life < maxLife; life++) {
					Vector2Int node = new Vector2Int((int)pos.x, (int)pos.y);
					// Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
					Vector2 offs = pos - node;
					// Calculate droplet's height and direction of flow with bilinear interpolation of surrounding heights
					Vector3 hn0 = _GetHeightAndNormals(map, pos.x, pos.y);
					// Update the droplet's direction and position (move position 1 unit regardless of speed)
					dir.x = (dir.x * inertia - hn0.x * (1 - inertia));
					dir.y = (dir.y * inertia - hn0.y * (1 - inertia));
					// Normalize direction
					dir.Normalize();
					pos += dir;

					// Stop simulating droplet if it's not moving or has flowed over edge of map
					if((dir.magnitude < 0.0001f) || pos.x < brushRadius || pos.x > mapWidth-brushRadius || pos.y < brushRadius || pos.y > mapHeight-brushRadius) {
						break;
					}

					// Find the droplet's new height and calculate the deltaHeight
					Vector3 hn1 = _GetHeightAndNormals(map, pos.x, pos.y);
					float heightDiff = hn1.z - hn0.z;

					// Calculate the droplet's sediment capacity (higher when moving fast down a slope and contains lots of water)
					float sedimentCapacity = Mathf.Max(-heightDiff * speed * waterVolume * sedimentCapacityFactor, minSedimentCapacity);

					// If carrying more sediment than capacity, or if flowing uphill:
					if(sediment > sedimentCapacity || heightDiff > 0) {
						// If moving uphill (deltaHeight > 0) try fill up to the current height, otherwise deposit a fraction of the excess sediment
						float depositionAmount = (heightDiff > 0) ? Mathf.Min(heightDiff, sediment) : (sediment - sedimentCapacity) * depositSpeed;
						sediment -= depositionAmount;
						// Add the sediment to the four nodes of the current cell using bilinear interpolation
						// Deposition is not distributed over a radius (like erosion) so that it can fill small pits
						map[node.x, node.y] += depositionAmount * (1f-offs.x) * (1f-offs.y);
						map[node.x+1, node.y] += depositionAmount * offs.x * (1f-offs.y);
						map[node.x, node.y+1] += depositionAmount * (1f-offs.x) * offs.y;
						map[node.x+1, node.y+1] += depositionAmount * offs.x * offs.y;

					}
					else {
						// Erode a fraction of the droplet's current carry capacity.
						// Clamp the erosion to the change in height so that it doesn't dig a hole in the terrain behind the droplet
						float erosionAmount = Mathf.Min((sedimentCapacity - sediment) * erodeSpeed, -heightDiff);
						// Use erosion brush to erode from all nodes inside the droplet's erosion radius
						for(int y = -brushRadius; y <= brushRadius; y++) {
							for(int x = -brushRadius; x <= brushRadius; x++) {
								//Debug.Log((node.x + x) + " " + (node.y + y));
								float weighedErosionAmount = erosionAmount * brush[x + brushRadius, y + brushRadius];
								float deltaSediment = (map[node.x + x, node.y + y] < weighedErosionAmount) ? map[node.x + x, node.y + y] : weighedErosionAmount;
								map[node.x + x, node.y + y] -= deltaSediment;
								sediment += deltaSediment;
							}
						}
					}

					// Update droplet's speed and waterVolume
					speed = Mathf.Sqrt(speed * speed + heightDiff * gravity);
					waterVolume *= (1 - evaporationSpeed);
				}
			}
		}

		//Calculate height and normals of a position from the map position (xy is normals, z is height)
		public static Vector3 _GetHeightAndNormals(float[,] map, float x, float y) {
			int cx = (int)x;
			int cy = (int)y;
			//Calculate offset inside the cell (0,0) = at NW node, (1,1) = at SE node
			float ox = x - cx;
			float oy = y - cy;
			// Calculate heights of the four nodes of the droplet's cell
			float nw = map[cx, cy];
			float ne = map[cx+1, cy];
			float sw = map[cx, cy+1];
			float se = map[cx+1, cy+1];
			// Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
			float nx = (ne - nw) * (1f-oy) + (se - sw) * oy;
			float ny = (sw - nw) * (1f-ox) + (se - ne) * ox;
			// Calculate height with bilinear interpolation of the heights of the nodes of the cell
			float h = (nw * (1f-ox) * (1f-oy)) + (ne * ox * (1f-oy)) + (sw * (1f-ox) * oy) + (se * ox * oy);

			return new Vector3(nx, ny, h);
		}

	}

}