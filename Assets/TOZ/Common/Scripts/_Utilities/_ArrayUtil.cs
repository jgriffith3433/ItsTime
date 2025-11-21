/**********************************NOTES********************************************
USAGE:
int[] numbers = new int[8]{1, 2, 3, 4, 5, 6, 7, 8};
int[,] test1;
int[] test2;
test1 = ArrayUtil.MonoToDuo(nos, 2, 4);
test2 = ArrayUtil.DuoToMono(test1);
**************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace TOZ {

	public static class _ArrayUtil {
		//1D array to 2D array
		public static T[,] _MonoToDuo<T>(T[] array, int xSize, int ySize) {
			Assert.AreEqual(array.Length, (xSize * ySize), "Array length is invalid.");

			T[,] result = new T[xSize, ySize];
			int i = 0;
			for(int y = 0; y < ySize; y++) {
				for(int x = 0; x < xSize; x++) {
					result[x,y] = array[i++];
				}
			}
			return result;
		}

		//1D array to 3D array
		public static T[,,] _MonoToTrio<T>(T[] array, int xSize, int ySize, int zSize) {
			Assert.AreEqual(array.Length, (xSize * ySize * zSize), "Array length is invalid.");

			T[,,] result = new T[xSize, ySize, zSize];
			int i = 0;
			for(int z = 0; z < zSize; z++) {
				for(int y = 0; y < ySize; y++) {
					for(int x = 0; x < xSize; x++) {
						result[x,y,z] = array[i++];
					}
				}
			}
			return result;
		}

		//2D array to 1D array
		public static T[] _DuoToMono<T>(T[,] array) {
			int xSize = array.GetLength(0);
			int ySize = array.GetLength(1);

			T[] result = new T[xSize * ySize];
			int i = 0;
			for(int y = 0; y < ySize; y++) {
				for(int x = 0; x < xSize; x++) {
					result[i++] = array[x,y];
				}
			}
			return result;
		}

		//3D array to 1D array
		public static T[] _TrioToMono<T>(T[,,] array) {
			int xSize = array.GetLength(0);
			int ySize = array.GetLength(1);
			int zSize = array.GetLength(2);

			T[] result = new T[xSize * ySize * zSize];
			int i = 0;
			for(int z = 0; z < zSize; z++) {
				for(int y = 0; y < ySize; y++) {
					for(int x = 0; x < xSize; x++) {
						result[i++] = array[x,y,z];
					}
				}
			}
			return result;
		}
	}

}