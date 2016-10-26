using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

namespace yjpark.alpha1316.editor {
    public static class Alpha1316Packer {
        /*
        * this is a quite hacky way, require that the generated texture's import setting:
        *
        * Do NOT check `Generate Mip Maps`
        * If not using Point filter mode, make sure the `Aniso Level` is `0`
        *
        */
        public static float DEBUG_CHECK_ALPHA_DIFF_THRESHOLD = 0.01f;

        public static int SRC_SIZE_STEP = 13;
        public static int DEST_SIZE_STEP = 16;
        public static float SRC_2_DEST_RATE = 0.8125f;
        public static float ALPHA_RATE = 0.1875f;

        private static void WriteSrcData(Color32[] srcPixels, int srcSize, Color32[] destPixels, int destSize) {
            for (int row = 0; row < srcSize; row++) {
                for (int col = 0; col < srcSize; col++) {
                    Color32 srcColor = srcPixels[row * srcSize + col];
                    if (srcColor.a > 0) {
                        destPixels[row * destSize + col] = new Color32(srcColor.r, srcColor.g, srcColor.b, 255);
                    } else {
                        destPixels[row * destSize + col] = new Color32(0, 0, 0, 255);
                    }
                }
            }
        }

        private static void WriteCornerData(Color32[] srcPixels, int srcSize, Color32[] destPixels, int destSize) {
            for (int row = srcSize; row < destSize; row++) {
                for (int col = srcSize; col < destSize; col++) {
                    destPixels[row * destSize + col] = new Color32(0, 0, 0, 255);
                }
            }
        }

        private static void GetAlphaRowCol(int srcSize, int row, int col, KeyValuePair<int, int> segment,
                                        out int alphaRow, out int alphaCol) {
            int stepCount = srcSize / SRC_SIZE_STEP;
            alphaRow = row;
            alphaCol = srcSize + stepCount * 3 / 2 + (col - segment.Value);

            if (segment.Key >= 3) {
                alphaRow = alphaCol;
                alphaCol = row;
            }
        }

        private static Color32 GetAlphaColor(Color32 alphaColor, Color32 srcColor, KeyValuePair<int, int> segment) {
            switch (segment.Key % 3) {
            case 0:
                return new Color32(srcColor.a, alphaColor.g, alphaColor.b, (byte)(alphaColor.a + 1));
            case 1:
                return new Color32(alphaColor.r, srcColor.a, alphaColor.b, (byte)(alphaColor.a + 1));
            case 2:
                return new Color32(alphaColor.r, alphaColor.g, srcColor.a, (byte)(alphaColor.a + 1));
            }
            return alphaColor;
        }

        private static void WriteAlphaToSegment(int srcSize, Color32[] destPixels, int destSize,
                                                int row, int col, KeyValuePair<int, int> segment, Color32 srcColor) {
            int alphaRow, alphaCol;
            GetAlphaRowCol(srcSize, row, col, segment, out alphaRow, out alphaCol);

            Color32 alphaColor = destPixels[alphaRow * destSize + alphaCol];
            destPixels[alphaRow * destSize + alphaCol] = GetAlphaColor(alphaColor, srcColor, segment);
        }

        private static void WriteAlphaData(Color32[] srcPixels, int srcSize, Color32[] destPixels, int destSize) {
            for (int row = 0; row < srcSize; row++) {
                for (int col = 0; col < srcSize; col++) {
                    Color32 srcColor = srcPixels[row * srcSize + col];
                    List<KeyValuePair<int, int>> segments = GetSegments(srcSize, col);
                    foreach (KeyValuePair<int, int> segment in segments) {
                        WriteAlphaToSegment(srcSize, destPixels, destSize, row, col, segment, srcColor);
                    }
                }
            }
            for (int row = 0; row < destSize; row++) {
                for (int col = 0; col < destSize; col++) {
                    if (row < srcSize && col < srcSize) continue;
                    if (row >= srcSize && col >= srcSize) continue;

                    Color32 alphaColor = destPixels[row * destSize + col];
                    if (alphaColor.a == 3 || alphaColor.a == 2) {
                        destPixels[row * destSize + col] = new Color32(alphaColor.r, alphaColor.g, alphaColor.b, 255);
                    } else {
                        Debug.LogError(string.Format("Invalid Alpha Data: {0}, {1} -> {2}", row, col, alphaColor));
                    }
                }
            }
        }

        private static Vector2 GetCoord(int row, int col, int size) {
            return new Vector2((float)col / (float)size, (float)row / (float)size);
        }

        private static int GetCenterFrom16(int stepCount, float center_16) {
            return Mathf.RoundToInt(stepCount * center_16 * SRC_2_DEST_RATE);
        }

        private static bool TryAddSegment(int stepCount, List<KeyValuePair<int, int>> segments,
                                        int col, int segment, float center_16, int length) {
            int centerCol = GetCenterFrom16(stepCount, center_16);
            int startCol = centerCol - length / 2;
            if (col >= startCol && col < startCol + length) {
                segments.Add(new KeyValuePair<int, int>(segment, centerCol));
                return true;
            }
            return false;
        }

        private static List<KeyValuePair<int, int>> GetSegments(int size, int col) {
            List<KeyValuePair<int, int>> segments = new List<KeyValuePair<int, int>>();

            int stepCount = size / SRC_SIZE_STEP;
            int length = stepCount * (DEST_SIZE_STEP - SRC_SIZE_STEP);

            for (int i = 0; i < 6; i++) {
                TryAddSegment(stepCount, segments, col, i, 1.0f + i * 2.8f, length);
            }

            return segments;
        }

        private static void CheckSegmentValues(int size) {
            int stepCount = size / SRC_SIZE_STEP;
            int length = stepCount * (DEST_SIZE_STEP - SRC_SIZE_STEP);

            for (int i = 0; i < 6; i++) {
                float center_16 = 1.0f + i * 2.8f;
                int centerCol = GetCenterFrom16(stepCount, center_16);
                int startCol = centerCol - length / 2;
                Debug.Log(string.Format("Segment: {0}, Center: {1}, CenterCol: {2}, StartCol: {3}, EndCol: {4}",
                                        i, center_16 / 16.0f, centerCol, startCol, startCol + length - 1));
            }
        }


        private static KeyValuePair<int, int> GetSegment(int srcSize, float x) {
            int stepCount = srcSize / SRC_SIZE_STEP;

            x *= DEST_SIZE_STEP;

            int segment = (int)((x + 0.4) / 2.8f);
            int center = GetCenterFrom16(stepCount, 1.0f + segment * 2.8f);
            return new KeyValuePair<int, int>(segment, center);
        }

        private static KeyValuePair<int, int> GetAlphaSegmentCoord(int srcSize, Vector2 texCoord, out Vector2 alphaCoord) {
            KeyValuePair<int, int> segment = GetSegment(srcSize, texCoord.x);

            float x = (float)SRC_2_DEST_RATE + ALPHA_RATE / 2.0f + (texCoord.x - (float)segment.Value / srcSize) * SRC_2_DEST_RATE;
            float y = texCoord.y * SRC_SIZE_STEP / DEST_SIZE_STEP;

            if (segment.Key >= 3) {
                float tmp = y;
                y = x;
                x = tmp;
            }

            alphaCoord = new Vector2(x, y);
            return segment;
        }

        private static void GetRowCol(Vector2 coord, int size,
                                    out int row, out int col, int type = 0) {
            float _row = coord.y * size;
            float _col = coord.x * size;
            if (type == 0) {
                row = Mathf.RoundToInt(_row);
                col = Mathf.RoundToInt(_col);
            } else if (type < 0) {
                row = Mathf.FloorToInt(_row);
                col = Mathf.FloorToInt(_col);
            } else {
                row = Mathf.CeilToInt(_row);
                col = Mathf.CeilToInt(_col);
            }
            if (row >= size) row = size - 1;
            if (col >= size) col = size - 1;
        }

        private static byte GetAlpha(Color32[] destPixels, int destSize, KeyValuePair<int, int> segment, Vector2 alphaCoord, out int alphaRow, out int alphaCol) {
            GetRowCol(alphaCoord, destSize, out alphaRow, out alphaCol);
            Color32 alphaColor = destPixels[alphaRow * destSize + alphaCol];
            switch (segment.Key % 3) {
            case 0:
                return alphaColor.r;
            case 1:
                return alphaColor.g;
            case 2:
                return alphaColor.b;
            }
            return 0;
        }

        private static void WriteCheckAlphaData(Color32[] srcPixels, int srcSize, Color32[] destPixels, int destSize) {
            for (int row = 0; row < srcSize; row++) {
                for (int col = 0; col < srcSize; col++) {
                    Vector2 texCoord = GetCoord(row, col, srcSize);
                    Vector2 alphaCoord;
                    KeyValuePair<int, int> segment = GetAlphaSegmentCoord(srcSize, texCoord, out alphaCoord);
                    int alphaRow, alphaCol;
                    byte alpha = GetAlpha(destPixels, destSize, segment, alphaCoord, out alphaRow, out alphaCol);
                    byte srcAlpha = srcPixels[row * srcSize + col].a;
                    float alphaDiff = Mathf.Abs((float)alpha - (float)srcAlpha);

                    if (alphaDiff > DEBUG_CHECK_ALPHA_DIFF_THRESHOLD * 255.0f) {
                        Debug.Log(string.Format("srcRow = {0}, sorCol = {1}, texCoord = {2}, {3}, alphaCoord = {4}, {5}, " +
                                                "alphaRow = {6}, alphaCol = {7}, srcAlpah = {8}, destAlpha = {9}, segment = {10}, alphaColor = {11}",
                                                row, col, texCoord.x, texCoord.y, alphaCoord.x, alphaCoord.y,
                                                alphaRow, alphaCol, srcAlpha, alpha, segment,
                                                destPixels[alphaRow * destSize + alphaCol]));
                    }

                    List<KeyValuePair<int, int>> segments = GetSegments(srcSize, col);

                    Color32 alphaColor = new Color32(
                        //color.r, color.g, color.b, alpha);
                        alpha,
                        (byte)(segment.Key % 3 * 16 + 64 * segments.Count),
                        (byte)(alphaDiff > DEBUG_CHECK_ALPHA_DIFF_THRESHOLD * 255.0f ? 255 : 0),
                        255);
                    destPixels[row * destSize + col] = alphaColor;
                }
            }
        }

        private static int GetDestSize(int srcSize) {
            return srcSize / SRC_SIZE_STEP * DEST_SIZE_STEP;
        }

        private static void CheckCoordConversion(int srcSize, int destSize) {
            int failedCount = 0;
            for (int row = 0; row < srcSize; row++) {
                for (int col = 0; col < srcSize; col++) {
                    Vector2 texCoord = GetCoord(row, col, srcSize);
                    Vector2 colorCoord = texCoord * SRC_SIZE_STEP / DEST_SIZE_STEP;
                    Vector2 alphaCoord;
                    KeyValuePair<int, int> segment = GetAlphaSegmentCoord(srcSize, texCoord, out alphaCoord);
                    int alphaRow, alphaCol;
                    GetRowCol(alphaCoord, destSize, out alphaRow, out alphaCol);

                    bool found = false;
                    List<KeyValuePair<int, int>> segments = GetSegments(srcSize, col);
                    foreach (KeyValuePair<int, int> _segment in segments) {
                        int _alphaRow, _alphaCol;
                        GetAlphaRowCol(srcSize, row, col, _segment, out _alphaRow, out _alphaCol);
                        if (alphaRow == _alphaRow && alphaCol == _alphaCol) {
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        Debug.Log(string.Format("CheckCoordConversion: {0}, {1}, tex: {2}, {3}, color: {4}, {5}, alpha: {6}, {7}, got: {8}, {9}, [{10}]",
                                                row, col, texCoord.x, texCoord.y, colorCoord.x, colorCoord.y, alphaCoord.x, alphaCoord.y, alphaRow, alphaCol, segment));
                        foreach (KeyValuePair<int, int> _segment in segments) {
                            int _alphaRow, _alphaCol;
                            GetAlphaRowCol(srcSize, row, col, _segment, out _alphaRow, out _alphaCol);
                            Debug.Log(string.Format("\t{0}, {1}, [{2}]", _alphaRow, _alphaCol, _segment));
                        }
                        Debug.Log("-------------------------------------------------------");
                        if (failedCount++ > 20) {
                            return;
                        }
                    }
                }
            }
        }

        private static void CreateAlphaTexture(Color32[] srcPixels, int srcSize, Color32[] destPixels, int destSize) {
            Debug.Log("Clear Old Data ===================================================================");
            for (int row = 0; row < destSize; row++) {
                for (int col = 0; col < destSize; col++) {
                    destPixels[row * destSize + col] = new Color32(0, 0, 0, 0);
                }
            }

            Debug.Log("Writing Corner Color ================================================================");
            WriteCornerData(srcPixels, srcSize, destPixels, destSize);

            Debug.Log("Writing Alpha ====================================================================");
            WriteAlphaData(srcPixels, srcSize, destPixels, destSize);
        }

        private static void WriteDestToFile(Color32[] destPixels, int destSize, string destPath) {
            Texture2D destTex = new Texture2D(destSize, destSize);
            destTex.SetPixels32(destPixels);
            destTex.Apply();

            byte[] destBytes = destTex.EncodeToPNG();
            Debug.Log(string.Format("Create Alpha 13/16 Texture: {0}", destPath));
            System.IO.File.WriteAllBytes(destPath, destBytes);
        }

        public static bool IsValidAlpha1316Source(Texture2D srcTex) {
            return srcTex.width == srcTex.height && srcTex.width % Alpha1316Packer.SRC_SIZE_STEP == 0;
        }

        public static string PackAlpha1316(Texture2D srcTex, string destPath, bool debugMode) {
            if (!IsValidAlpha1316Source(srcTex)) {
                return "The width and height of the texture should be same, and can be divided by 13!";
            }

            int srcSize = srcTex.width;
            int destSize = GetDestSize(srcSize);

            Color32[] srcPixels = srcTex.GetPixels32();
            Color32[] destPixels = new Color32[destSize * destSize];

            Debug.Log("CheckSegmentValues =============================================================");
            CheckSegmentValues(srcSize);

            Debug.Log("CheckCoordConversion =============================================================");
            CheckCoordConversion(srcSize, destSize);

            CreateAlphaTexture(srcPixels, srcSize, destPixels, destSize);

            if (debugMode) {
                Debug.Log("Writing Debug Texture ================================================================");
                WriteCheckAlphaData(srcPixels, srcSize, destPixels, destSize);
            } else {
                Debug.Log("Writing Src Color ================================================================");
                WriteSrcData(srcPixels, srcSize, destPixels, destSize);
            }

            WriteDestToFile(destPixels, destSize, destPath);

            AssetDatabase.Refresh();
            Object asset = AssetDatabase.LoadAssetAtPath(destPath, typeof(Texture2D));
            if (asset) {
                EditorUtility.SetDirty(asset);
            } else {
                return "A1316 Texture Asset Not Created!";
            }
            return null;
        }
    }
}
