using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Terra.Source.Util {
    public static class BlurUtils {
        public static float[,] BoxBlur(int[,] img, int radius) {
            int kSize = radius;
            if (kSize % 2 == 0) kSize++;

            int width = img.GetLength(0);
            int height = img.GetLength(1);

            float[,] hblur = Clone(img);
            float avg = (float) 1 / kSize;

            for (int j = 0; j < width; j++) {
                float hSum = 0f;
                for (int x = 0; x < kSize; x++) {
                    hSum += img[x, j];
                }

                float iAvg = hSum * avg;
                for (int i = 0; i < width; i++) {
                    if (i - kSize / 2 >= 0 && i + 1 + kSize / 2 < width) {
                        hSum -= img[i - kSize / 2, j];
                        hSum += img[i + 1 + kSize / 2, j];
                        iAvg = hSum * avg;
                    }

                    hblur[i, j] = iAvg;
                }
            }

            float[,] total = Clone(hblur);
            for (int i = 0; i < width; i++) {
                float tSum = 0f;
                for (int y = 0; y < kSize; y++) {
                    tSum += hblur[i, y];
                }

                float iAvg = tSum * avg;

                for (int j = 0; j < height; j++) {
                    if (j - kSize / 2 >= 0 && j + 1 + kSize / 2 < height) {
                        tSum -= hblur[i, j - kSize / 2];
                        tSum += hblur[i, j + 1 + kSize / 2];
                        iAvg = tSum * avg;
                    }

                    total[i, j] = iAvg;
                }
            }

            return total;
        }

        #region Gaussian Blur

        public static float[,] GaussianConvolution(int[,] matrix, float deviation) {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            float[,] fmatrix = new float[width, height];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    fmatrix[x, y] = matrix[x, y];
                }
            }

            return GaussianConvolution(fmatrix, deviation);
        }

        public static float[,] GaussianConvolution(float[,] matrix, float deviation) {
            float[,] kernel = CalculateNormalized1DSampleKernel(deviation);
            float[,] res1 = new float[matrix.GetLength(0), matrix.GetLength(1)];
            float[,] res2 = new float[matrix.GetLength(0), matrix.GetLength(1)];
            //x-direction
            for (int i = 0; i < matrix.GetLength(0); i++) {
                for (int j = 0; j < matrix.GetLength(1); j++)
                    res1[i, j] = ProcessPoint(matrix, i, j, kernel, 0);
            }

            //y-direction
            for (int i = 0; i < matrix.GetLength(0); i++) {
                for (int j = 0; j < matrix.GetLength(1); j++)
                    res2[i, j] = ProcessPoint(res1, i, j, kernel, 1);
            }

            return res2;
        }

        public static int GetGaussianKernelSize(float deviation) {
            return (int) Math.Ceiling(deviation * 3) * 2 + 1;
        }

        private static float ProcessPoint(float[,] matrix, int x, int y, float[,] kernel, int direction) {
            float res = 0;
            int half = kernel.GetLength(0) / 2;
            for (int i = 0; i < kernel.GetLength(0); i++) {
                int cox = direction == 0 ? x + i - half : x;
                int coy = direction == 1 ? y + i - half : y;
                if (cox >= 0 && cox < matrix.GetLength(0) && coy >= 0 && coy < matrix.GetLength(1)) {
                    res += matrix[cox, coy] * kernel[i, 0];
                }
            }

            return res;
        }

        private static float[,] Calculate1DSampleKernel(float deviation, int size) {
            float[,] ret = new float[size, 1];
            float sum = 0;
            int half = size / 2;
            for (int i = 0; i < size; i++) {
                ret[i, 0] = 1 / (Mathf.Sqrt(2 * Mathf.PI) * deviation) *
                            Mathf.Exp(-(i - half) * (i - half) / (2 * deviation * deviation));
                sum += ret[i, 0];
            }

            return ret;
        }

        private static float[,] CalculateNormalized1DSampleKernel(float deviation) {
            int size = GetGaussianKernelSize(deviation);
            float[,] matrix = Calculate1DSampleKernel(deviation, size);
            float[,] ret = new float[matrix.GetLength(0), matrix.GetLength(1)];
            float sum = 0;
            for (int i = 0; i < ret.GetLength(0); i++) {
                for (int j = 0; j < ret.GetLength(1); j++)
                    sum += matrix[i, j];
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (sum != 0) {
                for (int i = 0; i < ret.GetLength(0); i++) {
                    for (int j = 0; j < ret.GetLength(1); j++)
                        ret[i, j] = matrix[i, j] / sum;
                }
            }

            return ret;
        }

        #endregion

        #region Utility

        private static float[,] Clone(float[,] input) {
            int width = input.GetLength(0);
            int height = input.GetLength(1);
            float[,] result = new float[width, height];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    result[x, y] = input[x, y];
                }
            }

            return result;
        }

        private static float[,] Clone(int[,] input) {
            int width = input.GetLength(0);
            int height = input.GetLength(1);
            float[,] result = new float[width, height];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    result[x, y] = input[x, y];
                }
            }

            return result;
        }

        #endregion
    }
}