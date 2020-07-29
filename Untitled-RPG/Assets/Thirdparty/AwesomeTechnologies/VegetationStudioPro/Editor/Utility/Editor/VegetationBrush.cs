using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    internal class VegetationBrush
    {
        private float[] _strength;
        public int Size;
        private Texture2D _brush;
        private Texture2D _preview;
        private Projector _brushProjector;

        public bool Load(Texture2D brushTex, int size)
        {
            if (_brushProjector != null && _preview != null)
            {
                _brushProjector.material.mainTexture = _preview;
            }
            bool result;
            if (_brush == brushTex && size == Size && _strength != null)
            {
                result = true;
            }
            else if (brushTex != null)
            {
                float num = size;
                Size = size;
                _strength = new float[Size * Size];
                if (Size > 3)
                {
                    for (int i = 0; i < Size; i++)
                    {
                        for (int j = 0; j < Size; j++)
                        {
                            _strength[i * Size + j] = brushTex.GetPixelBilinear((j + 0.5f) / num, i / num).a;
                        }
                    }
                }
                else
                {
                    for (int k = 0; k < _strength.Length; k++)
                    {
                        _strength[k] = 1f;
                    }
                }
                Object.DestroyImmediate(_preview);
                _preview =
                    new Texture2D(Size, Size, TextureFormat.RGBA32, false)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                        wrapMode = TextureWrapMode.Repeat,
                        filterMode = FilterMode.Point
                    };
                Color[] array = new Color[Size * Size];
                for (int l = 0; l < array.Length; l++)
                {
                    array[l] = new Color(1f, 1f, 1f, _strength[l]);
                }
                _preview.SetPixels(0, 0, Size, Size, array, 0);
                _preview.Apply();
                if (_brushProjector == null)
                {
                    CreatePreviewBrush();
                }
                _brushProjector.material.mainTexture = _preview;
                _brush = brushTex;
                result = true;
            }
            else
            {
                _strength = new float[1];
                _strength[0] = 1f;
                Size = 1;
                result = false;
            }
            return result;
        }

        public float GetStrengthInt(int ix, int iy)
        {
            ix = Mathf.Clamp(ix, 0, Size - 1);
            iy = Mathf.Clamp(iy, 0, Size - 1);
            return _strength[iy * Size + ix];
        }

        public void Dispose()
        {
            if (_brushProjector)
            {
                Object.DestroyImmediate(_brushProjector.gameObject);
                _brushProjector = null;
            }
            Object.DestroyImmediate(_preview);
            _preview = null;
        }

        public Projector GetPreviewProjector()
        {
            return _brushProjector;
        }

        private void CreatePreviewBrush()
        {
            GameObject gameObject = EditorUtility.CreateGameObjectWithHideFlags("VegetationBrushPreview", HideFlags.HideAndDontSave, typeof(Projector));
            _brushProjector = (gameObject.GetComponent(typeof(Projector)) as Projector);
            var mBrushProjector = this._brushProjector;
            if (mBrushProjector != null)
            {
                mBrushProjector.enabled = true;
                mBrushProjector.nearClipPlane = -1000f;
                mBrushProjector.farClipPlane = 1000f;
                mBrushProjector.orthographic = true;
                mBrushProjector.orthographicSize = 10f;
                mBrushProjector.transform.Rotate(90f, 0f, 0f);
                Material material = EditorGUIUtility.LoadRequired("SceneView/TerrainBrushMaterial.mat") as Material;
                mBrushProjector.material = material;
            }
        }
    }

}
