using System;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class UnityTerrain
    {
        //public struct ProcessSplatMapRuleJob : IJobParallelForBatch
        //{
        //    public void Execute(int startIndex, int count)
        //    {

        //    }
        //}

        [BurstCompile(CompileSynchronously = true)]
        public struct GenerateDefaultBiomeBlendMaskJob : IJobParallelFor
        {
            public NativeArray<float> BlendMask;

            public void Execute(int index)
            {
                BlendMask[index] = 1;
            }
        }


        [BurstCompile(CompileSynchronously = true)]
        public struct GenerateBlendMaskJob : IJobParallelFor
        {
            public NativeArray<float> BlendMask;
            [ReadOnly] public NativeArray<Vector2> PolygonArray;
            [ReadOnly] public NativeArray<LineSegment2D> SegmentArray;
            [ReadOnly] public NativeArray<float> CurveArray;

            public int Width;
            public int Height;
            public Vector3 TerrainSize;
            public Vector4 TerrainPosition;

            public Rect PolygonRect;

            public bool UseNoise;
            public float NoiseScale;
            public float BlendDistance;
            public bool Include;

            public void Execute(int index)
            {
                float currentBlend = BlendMask[index];
                float originalBlend = currentBlend;

                int y = Mathf.FloorToInt((float) index / (float) Width);
                int x = index - (y * Width);

                float xPosition = TerrainPosition.x + (TerrainSize.x / Width) * x;
                float yPosition = TerrainPosition.z + (TerrainSize.z / Height) * y;
                
                float xNoisePosition = TerrainPosition.x + (TerrainSize.x / Width) * y;
                float yNoisePosition = TerrainPosition.z - (TerrainSize.z / Height) * -x;


                Vector2 point = new Vector2(xPosition, yPosition);
                if (!PolygonRect.Contains(point)) return;

                if (IsInPolygon(point))
                {
                    currentBlend = math.max(currentBlend, 1);

                    float distanceToEdge = DistanceToEdge(point);
                    if (distanceToEdge < BlendDistance)
                    {
                       // float perlinNoise = math.@select(1,
                       //     Mathf.PerlinNoise(point.x / NoiseScale, point.y / NoiseScale), UseNoise);
                        //perlinNoise = math.@select(perlinNoise, 0, !Include && !UseNoise);
                        
                        float perlinNoise = math.@select(1,
                            Mathf.PerlinNoise(xNoisePosition / NoiseScale, yNoisePosition / NoiseScale), UseNoise);
                        perlinNoise = math.@select(perlinNoise, 0, !Include && !UseNoise);
                        currentBlend =
                            math.@select(
                                math.max((1 - SampleCurveArray(distanceToEdge / BlendDistance)) * (1 - perlinNoise),
                                    currentBlend),
                                math.min(SampleCurveArray(distanceToEdge / BlendDistance) * perlinNoise, currentBlend),
                                Include);

                        currentBlend = math.max(originalBlend, currentBlend);
                    }


                    //ADD new code


                    BlendMask[index] = currentBlend;
                }
            }

            private float SampleCurveArray(float value)
            {
                if (CurveArray.Length == 0) return 0f;
                int index = Mathf.RoundToInt((value) * CurveArray.Length);
                index = Mathf.Clamp(index, 0, CurveArray.Length - 1);
                //return CurveArray[index];
                
                if (index == CurveArray.Length - 1)
                {
                    return CurveArray[index];
                }

                float floatIndex = (math.clamp(value,0,1) * (CurveArray.Length - 1));
                float lowerValue = CurveArray[index];
                float higherValue = CurveArray[index + 1];
                return math.lerp(lowerValue, higherValue, math.frac(floatIndex));                
            }

            float DistanceToEdge(Vector2 point)
            {
                float distance = float.MaxValue;
                for (int i = 0; i < SegmentArray.Length; i++)
                {
                    if ((SegmentArray[i].DisableEdge == 0))
                    {
                        distance = math.min(distance, SegmentArray[i].DistanceToPoint(point));
                    }                   
                }

                return distance;
            }

            bool IsInPolygon(Vector2 p)
            {
                bool inside = false;

                if (PolygonArray.Length < 3)
                {
                    return false;
                }

                var oldPoint = new Vector2(
                    PolygonArray[PolygonArray.Length - 1].x, PolygonArray[PolygonArray.Length - 1].y);

                for (int i = 0; i < PolygonArray.Length; i++)
                {
                    var newPoint = new Vector2(PolygonArray[i].x, PolygonArray[i].y);

                    Vector2 p1;
                    Vector2 p2;
                    if (newPoint.x > oldPoint.x)
                    {
                        p1 = oldPoint;
                        p2 = newPoint;
                    }
                    else
                    {
                        p1 = newPoint;
                        p2 = oldPoint;
                    }

                    if ((newPoint.x < p.x) == (p.x <= oldPoint.x)
                        && (p.y - (long) p1.y) * (p2.x - p1.x)
                        < (p2.y - (long) p1.y) * (p.x - p1.x))
                    {
                        inside = !inside;
                    }

                    oldPoint = newPoint;
                }

                return inside;
            }
        }

        private JobHandle _splatMapHandle;
        private NativeArray<HeightMapSample> _heightMapSamples;
        private NativeArray<float> _currentSplatmapArray;
        private readonly List<NativeArray<float>> _nativeArrayFloatList = new List<NativeArray<float>>();


        public bool NeedsSplatMapUpdate(Bounds updateBounds)
        {
            return updateBounds.Intersects(TerrainBounds);
        }

        public void PrepareSplatmapGeneration(bool clearLockedTextures)
        {
            LoadHeightData();


            int width = Terrain.terrainData.alphamapWidth;
            int height = Terrain.terrainData.alphamapHeight;
            int layers = Terrain.terrainData.alphamapLayers;
            int heightMapLength = width * height;

            if (_heightMapSamples.IsCreated) _heightMapSamples.Dispose();

            _heightMapSamples = new NativeArray<HeightMapSample>(heightMapLength, Allocator.TempJob);
            SampleHeightMapJob sampleHeightMapJob = new SampleHeightMapJob
            {
                HeightMapSamples = _heightMapSamples,
                InputHeights = Heights,
                HeightMapScale = _heightmapScale,
                HeightmapHeight = _heightmapHeight,
                HeightmapWidth = _heightmapWidth,
                Scale = _scale,
                Size = _size,
                Width = width,
                Height = height
            };
            _splatMapHandle = sampleHeightMapJob.Schedule(heightMapLength, 32);


            if (_currentSplatmapArray.IsCreated) _currentSplatmapArray.Dispose();

            _currentSplatmapArray = new NativeArray<float>(width * height * layers, Allocator.TempJob);

            if (!clearLockedTextures)
            {
                float[,,] spltmapArray = Terrain.terrainData.GetAlphamaps(0,0,width,height);
                _currentSplatmapArray.CopyFromFast(spltmapArray);
            }

            //Read original splatdata to a original splat map array 
        }

        public void GenerateSplatMapBiome(Bounds updateBounds, BiomeType biomeType,
            List<PolygonBiomeMask> polygonBiomeMaskList, List<TerrainTextureSettings> terrainTextureSettingsList,
            float heightCurveSampleHeight, float worldSpaceSeaLevel,bool clearLockedTextures)
        {
            int width = Terrain.terrainData.alphamapWidth;
            int height = Terrain.terrainData.alphamapHeight;
            int layers = Terrain.terrainData.alphamapLayers;

            int blendMaskLength = width * height;
            NativeArray<float> blendMask = new NativeArray<float>(blendMaskLength, Allocator.TempJob);
            NativeArray<float> splatmapArray = new NativeArray<float>(width * height * layers, Allocator.TempJob);

            if (biomeType == BiomeType.Default)
            {
                GenerateDefaultBiomeBlendMaskJob generateDefaultBiomeBlendMaskJob =
                    new GenerateDefaultBiomeBlendMaskJob {BlendMask = blendMask};
                _splatMapHandle = generateDefaultBiomeBlendMaskJob.Schedule(blendMaskLength, 32, _splatMapHandle);
            }
            else
            {
                for (int i = 0; i <= polygonBiomeMaskList.Count - 1; i++)
                {
                    GenerateBlendMaskJob generateBlendMaskJob = new GenerateBlendMaskJob
                    {
                        Width = width,
                        Height = height,
                        TerrainSize = _size,
                        TerrainPosition = TerrainPosition,
                        BlendMask = blendMask,
                        PolygonArray = polygonBiomeMaskList[i].PolygonArray,
                        SegmentArray = polygonBiomeMaskList[i].SegmentArray,
                        CurveArray = polygonBiomeMaskList[i].TextureCurveArray,
                        UseNoise = polygonBiomeMaskList[i].UseNoise,
                        NoiseScale = polygonBiomeMaskList[i].NoiseScale,
                        BlendDistance = polygonBiomeMaskList[i].BlendDistance,
                        PolygonRect = RectExtension.CreateRectFromBounds(polygonBiomeMaskList[i].MaskBounds),
                        Include = true
                    };

                    _splatMapHandle = generateBlendMaskJob.Schedule(blendMaskLength, 32, _splatMapHandle);
                }
            }

            for (int i = 0; i <= terrainTextureSettingsList.Count - 1; i++)
            {
                if (i >= layers) continue;
                if (terrainTextureSettingsList[i].Enabled)
                {
                    ProcessSplatMapJob processSplatMap = new ProcessSplatMapJob
                    {
                        Height = height,
                        Width = width,
                        Layers = layers,
                        SplatMapArray = splatmapArray,
                        BlendMask = blendMask,
                        HeightMap = _heightMapSamples,
                        Heights = Heights,
                        TextureIndex = i,
                        TextureUseNoise = terrainTextureSettingsList[i].UseNoise,
                        TextureNoiseScale = terrainTextureSettingsList[i].NoiseScale,
                        TextureWeight = terrainTextureSettingsList[i].TextureWeight,
                        TextureNoiseOffset = terrainTextureSettingsList[i].NoiseOffset,
                        InverseTextureNoise = terrainTextureSettingsList[i].InverseNoise,
                        HeightCurve = terrainTextureSettingsList[i].HeightCurveArray,
                        SteepnessCurve = terrainTextureSettingsList[i].SteepnessCurveArray,
                        TerrainHeight = heightCurveSampleHeight,
                        TerrainYPosition = TerrainPosition.y,
                        WorldspaceSeaLevel = worldSpaceSeaLevel,
                        HeightMapScale = _heightmapScale,
                        HeightmapHeight = _heightmapHeight,
                        HeightmapWidth = _heightmapWidth,
                        ConcaveEnable = terrainTextureSettingsList[i].ConcaveEnable,
                        ConvexEnable = terrainTextureSettingsList[i].ConvexEnable,
                        ConcaveAverage = terrainTextureSettingsList[i].ConcaveAverage,
                        ConcaveMinHeightDifference = terrainTextureSettingsList[i].ConcaveMinHeightDifference,
                        ConcaveDistance = terrainTextureSettingsList[i].ConcaveDistance,
                        ConcaveMode = (int) terrainTextureSettingsList[i].ConcaveMode,
                        TerrainSize = _size,
                        TerrainPosition = TerrainPosition                        
                    };                    
                    _splatMapHandle = processSplatMap.Schedule(width * height * layers, 32, _splatMapHandle);

                    
                }
                else
                {
                    if (!clearLockedTextures && terrainTextureSettingsList[i].LockTexture )
                    {
                        CopyLockedDataJob copyLockedDataJobJob = new CopyLockedDataJob
                        {
                            Height = height,
                            Width = width,
                            Layers = layers,
                            SplatMapArray = splatmapArray,
                            CurrentSplatMapArray = _currentSplatmapArray,
                            TextureIndex = i,
                        };
                        _splatMapHandle = copyLockedDataJobJob.Schedule(width * height * layers, 32, _splatMapHandle);
                    }                  
                }
            }


            int firstEnabledIndex = 0;
            for (int i = 0; i <= terrainTextureSettingsList.Count - 1; i++)
            {
                if (terrainTextureSettingsList[i].Enabled)
                {
                    firstEnabledIndex = i;
                    break;
                }
            }
            
            if (!clearLockedTextures)
            {
                NativeArray<int> lockedTextureArray = new NativeArray<int>(terrainTextureSettingsList.Count,Allocator.TempJob);
                NativeArray<int> automaticGenerationArray = new NativeArray<int>(terrainTextureSettingsList.Count,Allocator.TempJob);
                for (int i = 0; i <= terrainTextureSettingsList.Count - 1; i++)
                {

                    if (terrainTextureSettingsList[i].Enabled)
                    {
                        automaticGenerationArray[i] = 1;
                    }else if (terrainTextureSettingsList[i].LockTexture)
                    {
                        lockedTextureArray[i] = 1;
                    }
                    
                  
                }     
                NormalizeSplatMapKeepLockedDataJob normalizeSplatMapJob = new NormalizeSplatMapKeepLockedDataJob
                {
                    SplatMapArray = splatmapArray, 
                    FirstEnabledIndex = firstEnabledIndex,
                    AutomaticGenerationArray = automaticGenerationArray,
                    LockedTextureArray = lockedTextureArray
                };
                _splatMapHandle = normalizeSplatMapJob.ScheduleBatch(width * height * layers, layers, _splatMapHandle); 
            }
            else
            {
                 NormalizeSplatMapJob normalizeSplatMapJob = new NormalizeSplatMapJob
                {
                    SplatMapArray = splatmapArray, 
                    FirstEnabledIndex = firstEnabledIndex
                };
                _splatMapHandle = normalizeSplatMapJob.ScheduleBatch(width * height * layers, layers, _splatMapHandle);  
            }
           
            //blend biome splatmap against current splatmap
            BlendSplatMapJob blendSplatMapJob = new BlendSplatMapJob
            {
                CurrentSplatMapArray = _currentSplatmapArray,
                SplatMapArray = splatmapArray,
                BlendMask = blendMask,
                Height = height,
                Width = width,
                Layers = layers
            };
            _splatMapHandle = blendSplatMapJob.Schedule(width * height * layers, 32, _splatMapHandle);

            _nativeArrayFloatList.Add(splatmapArray);
            _nativeArrayFloatList.Add(blendMask);
        }

        public void CompleteSplatmapGeneration()
        {
            _splatMapHandle.Complete();

            int width = Terrain.terrainData.alphamapWidth;
            int height = Terrain.terrainData.alphamapHeight;
            int layers = Terrain.terrainData.alphamapLayers;

            //float[] splatmap1DArray = new float[width * height * layers];
            //NativeToManagedCopyMemory(splatmap1DArray, _currentSplatmapArray);            
            //_terrain.terrainData.SetAlphamaps(0, 0, To3DArray(splatmap1DArray, width, height, layers));

            float[,,] splatmapArray = new float[width, height, layers];
            _currentSplatmapArray.CopyToFast(splatmapArray);
            Terrain.terrainData.SetAlphamaps(0, 0, splatmapArray);

            if (_heightMapSamples.IsCreated) _heightMapSamples.Dispose();
            if (_currentSplatmapArray.IsCreated) _currentSplatmapArray.Dispose();

            for (int i = 0; i <= _nativeArrayFloatList.Count - 1; i++)
            {
                if (_nativeArrayFloatList[i].IsCreated) _nativeArrayFloatList[i].Dispose();
            }


            //splatmapArray.CopyTo(splatmap1DArray);

            _nativeArrayFloatList.Clear();
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct CopyLockedDataJob : IJobParallelFor
        {
            public NativeArray<float> SplatMapArray;
            [ReadOnly]
            public NativeArray<float> CurrentSplatMapArray;

            public int Width;
            public int Height;
            public int Layers;
            public int TextureIndex;
            
            public void Execute(int index)
            {
                //int y;
                int z;
                Math.DivRem(index, Layers, out z);
                //int yQuotient = Math.DivRem(zQuotient, Height, out y);
                //var x = yQuotient % Width;

                if (z == TextureIndex)
                {
                    SplatMapArray[index] = CurrentSplatMapArray[index];
                }
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct ProcessSplatMapJob : IJobParallelFor
        {
            public NativeArray<float> SplatMapArray;
            [ReadOnly] public NativeArray<float> BlendMask;
            [ReadOnly] public NativeArray<HeightMapSample> HeightMap;
            [ReadOnly] public NativeArray<float> Heights;
            [ReadOnly] public NativeArray<float> HeightCurve;
            [ReadOnly] public NativeArray<float> SteepnessCurve;

            public int Width;
            public int Height;
            public int Layers;
            public int TextureIndex;
            public bool TextureUseNoise;
            public float TextureNoiseScale;
            public float TextureWeight;
            public Vector2 TextureNoiseOffset;
            public float NoiceCellResolutionFactor;
            public bool InverseTextureNoise;
            public float TerrainHeight;
            public float TerrainYPosition;
            public float WorldspaceSeaLevel;

            public int HeightmapWidth;
            public int HeightmapHeight;
            public Vector3 HeightMapScale;

            public bool ConcaveEnable;
            public bool ConvexEnable;
            public bool ConcaveAverage;
            public float ConcaveMinHeightDifference;
            public float ConcaveDistance;
            public int ConcaveMode;

            public Vector3 TerrainSize;
            public Vector4 TerrainPosition;
            
            public void Execute(int index)
            {
                int y;
                int z;
                int zQuotient = Math.DivRem(index, Layers, out z);
                int yQuotient = Math.DivRem(zQuotient, Height, out y);
                var x = yQuotient % Width;
                
                               
                float xNoisePosition = TerrainPosition.x + (TerrainSize.x / Width) * y;
                float yNoisePosition = TerrainPosition.z - (TerrainSize.z / Height) * -x;

                //float pixelBlend = BlendMask[y + x * Width];
                float height = HeightMap[y + x * Width].Height + TerrainYPosition;
                height -= WorldspaceSeaLevel;
                float steepness = HeightMap[y + x * Width].Steepness;

                if (z == TextureIndex)
                {
                    //float noise = math.@select(1,
                    //    Mathf.PerlinNoise(((x + TextureNoiseOffset.x ) / TextureNoiseScale) + NoiseTerrainOffset.z ,
                    //        ((y + TextureNoiseOffset.y ) / TextureNoiseScale)+ NoiseTerrainOffset.x), TextureUseNoise);
                    
                    float noise = math.@select(1,
                        Mathf.PerlinNoise(((xNoisePosition + TextureNoiseOffset.x ) / TextureNoiseScale),
                            ((yNoisePosition + TextureNoiseOffset.y ) / TextureNoiseScale)), TextureUseNoise);
                    
                    noise = math.@select(noise, 1 - noise, InverseTextureNoise && TextureUseNoise);

                    float calculatedCurveWeight = SampleCurveArray(SteepnessCurve, steepness / 90f) *
                                                  SampleCurveArray(HeightCurve, height / TerrainHeight) * noise *
                                                  TextureWeight;

                    if (ConcaveEnable || ConvexEnable)
                    {
                        float scale = (float) Width / HeightmapWidth;
                        float2 heightmapPosition = new float2(y / scale, x / scale);
                        float calculatedConcaveConvexWeight = SampleConcaveFactor(heightmapPosition);

                        if (ConcaveMode == 0)
                        {
                            calculatedConcaveConvexWeight = calculatedConcaveConvexWeight * noise * TextureWeight;
                            SplatMapArray[index] = math.max(calculatedConcaveConvexWeight, calculatedCurveWeight);
                        }
                        else
                        {
                            SplatMapArray[index] = calculatedConcaveConvexWeight * calculatedCurveWeight;
                        }
                    }
                    else
                    {
                        SplatMapArray[index] = calculatedCurveWeight;
                    }
                }
            }

            private float SampleCurveArray(NativeArray<float> curve, float value)
            {
                if (curve.Length == 0) return 0f;
                                                
                int index = Mathf.RoundToInt((value) * curve.Length);
                index = Mathf.Clamp(index, 0, curve.Length - 1);
                //return curve[index];
                
                if (index == curve.Length - 1)
                {
                    return curve[index];
                }

                float floatIndex = (math.clamp(value,0,1) * (curve.Length - 1));
                float lowerValue = curve[index];
                float higherValue = curve[index + 1];
                return math.lerp(lowerValue, higherValue, math.frac(floatIndex));                                                 
            }

            float SampleConcaveFactor(float2 heightmapPosition)
            {
                int x = Mathf.RoundToInt(heightmapPosition.x);
                int z = Mathf.RoundToInt(heightmapPosition.y);


                //bool Average = true;
                //float MinHeightDifference = 5f;

                int sampleDistance = Mathf.RoundToInt(ConcaveDistance / HeightMapScale.x);
                float centerHeight = GetHeight(x, z);

                float height1 = GetHeight(x - sampleDistance, z - sampleDistance);
                float height2 = GetHeight(x, z - sampleDistance);
                float height3 = GetHeight(x + sampleDistance, z - sampleDistance);

                float height4 = GetHeight(x - sampleDistance, z);
                float height5 = GetHeight(x + sampleDistance, z);

                float height6 = GetHeight(x - sampleDistance, z + sampleDistance);
                float height7 = GetHeight(x, z + sampleDistance);
                float height8 = GetHeight(x + sampleDistance, z + sampleDistance);

                float edgeHeight;

                if (ConcaveAverage)
                {
                    edgeHeight = (height1 + height2 + height3 + height4 + height5 + height6 + height7 + height8) / 8f;
                }
                else
                {
                    edgeHeight = GetMinimumHeight(height1, height2, height3, height4, height5, height6, height7,
                        height8);
                }

                //inverse
                float convex = math.clamp((centerHeight - edgeHeight) / ConcaveMinHeightDifference, 0, 1);
                //normal
                float concave = math.clamp((edgeHeight - centerHeight) / ConcaveMinHeightDifference, 0, 1);


                if (ConvexEnable && ConcaveEnable)
                {
                    return math.max(convex, concave);
                }
                else if (ConcaveEnable)
                {
                    return concave;
                }

                return convex;
            }

            float GetMinimumHeight(float height1, float height2, float height3, float height4, float height5,
                float height6,
                float height7, float height8)
            {
                float minHeight = math.min(height1, height2);
                minHeight = math.min(minHeight, height3);
                minHeight = math.min(minHeight, height4);
                minHeight = math.min(minHeight, height5);
                minHeight = math.min(minHeight, height6);
                minHeight = math.min(minHeight, height7);
                minHeight = math.min(minHeight, height8);
                return minHeight;
            }

            float GetHeight(int x, int y)
            {
                x = math.clamp(x, 0, HeightmapWidth - 1);
                y = math.clamp(y, 0, HeightmapHeight - 1);
                return Heights[y * HeightmapWidth + x] * HeightMapScale.y;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct BlendSplatMapJob : IJobParallelFor
        {
            public NativeArray<float> CurrentSplatMapArray;
            [ReadOnly] public NativeArray<float> SplatMapArray;
            [ReadOnly] public NativeArray<float> BlendMask;

            public int Width;
            public int Height;
            public int Layers;

            public void Execute(int index)
            {
                int y;
                int z;
                int zQuotient = Math.DivRem(index, Layers, out z);
                int yQuotient = Math.DivRem(zQuotient, Height, out y);
                var x = yQuotient % Width;
                float pixelBlend = BlendMask[y + x * Width];
                CurrentSplatMapArray[index] =
                    CurrentSplatMapArray[index] * (1 - pixelBlend) + SplatMapArray[index] * pixelBlend;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct NormalizeSplatMapJob : IJobParallelForBatch
        {
            public NativeArray<float> SplatMapArray;
            public int FirstEnabledIndex;

            public void Execute(int startIndex, int count)
            {
                float totalValue = 0;

                for (int i = 0; i <= count - 1; i++)
                {
                    totalValue += SplatMapArray[startIndex + i];
                }

                if (Math.Abs(totalValue) > 0.0001f)
                {
                    for (int i = 0; i <= count - 1; i++)
                    {
                        SplatMapArray[startIndex + i] /= totalValue;
                    }
                }
                else
                {
                    for (int i = 0; i <= count - 1; i++)
                    {
                        SplatMapArray[startIndex + i] = 0;
                    }

                    SplatMapArray[startIndex + FirstEnabledIndex] = 1;
                }
            }
        }
        
        [BurstCompile(CompileSynchronously = true)]
        public struct NormalizeSplatMapKeepLockedDataJob : IJobParallelForBatch
        {
            public NativeArray<float> SplatMapArray;
            public int FirstEnabledIndex;            
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<int> AutomaticGenerationArray;
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<int> LockedTextureArray;

            public void Execute(int startIndex, int count)
            {

                float lockedValue = 0;                    
                for (int i = 0; i <= count - 1; i++)
                {
                    if (LockedTextureArray[i] == 1)
                    {
                        lockedValue += SplatMapArray[startIndex + i];
                    }
                }
                
                float totalValue = 0;
                for (int i = 0; i <= count - 1; i++)
                {
                    if (AutomaticGenerationArray[i] == 1)
                    {
                        totalValue += SplatMapArray[startIndex + i]; 
                    }
                }

                totalValue = totalValue / (1 - lockedValue);
                
                if (Math.Abs(totalValue + lockedValue) > float.Epsilon)
                {
                    for (int i = 0; i <= count - 1; i++)
                    {
                        if (AutomaticGenerationArray[i] == 1)
                        {
                            SplatMapArray[startIndex + i] /= totalValue;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i <= count - 1; i++)
                    {
                        SplatMapArray[startIndex + i] = 0;
                    }

                    SplatMapArray[startIndex + FirstEnabledIndex] = 1;
                }
            }
        }


        //public static float[,,] To3DArray(float[] source, int width, int height, int depth)
        //{
        //    float[,,] result = new float[width, height, depth];
        //    Buffer.BlockCopy(source, 0, result, 0, source.Length * sizeof(float));
        //    return result;
        //}

        //public static float[] To1DArray(float[,,] source)
        //{
        //    float[] result = new float[source.GetLength(0) * source.GetLength(1) * source.GetLength(2)];
        //    Buffer.BlockCopy(source, 0, result, 0, result.Length * sizeof(float));
        //    return result;
        //}

        //private static byte[] ToByteArray<T>(T[] source) where T : struct
        //{
        //    GCHandle handle = GCHandle.Alloc(source, GCHandleType.Pinned);
        //    try
        //    {
        //        IntPtr pointer = handle.AddrOfPinnedObject();
        //        byte[] destination = new byte[source.Length * Marshal.SizeOf(typeof(T))];
        //        Marshal.Copy(pointer, destination, 0, destination.Length);
        //        return destination;
        //    }
        //    finally
        //    {
        //        if (handle.IsAllocated)
        //            handle.Free();
        //    }
        //}
        //private static Matrix4x4[] FromByteArray<T>(Matrix4x4[] destination, byte[] source)
        //{
        //    GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        //    try
        //    {
        //        IntPtr pointer = handle.AddrOfPinnedObject();
        //        Marshal.Copy(source, 0, pointer, source.Length);
        //        return destination;
        //    }
        //    finally
        //    {
        //        if (handle.IsAllocated)
        //            handle.Free();
        //    }
        //}

        //public static unsafe void NativeToManagedCopyMemory(float[] targetArray, NativeArray<float> sourceNativeArray)
        //{
        //    //NativeSlice<float> slice = new NativeSlice<float>(sourceNativeArray);           
        //    //void* memoryPointer = slice.GetUnsafeReadOnlyPtr();
        //    void* memoryPointer = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(sourceNativeArray);
        //    Marshal.Copy((IntPtr)memoryPointer, targetArray, 0, sourceNativeArray.Length);
        //}
        //public static unsafe Matrix4x4[] NativeToManagedMatrix4X4(byte[] tempByteArray, Matrix4x4[] targetMatrixArray, NativeArray<Matrix4x4> sourceNativeArray)
        //{
        //    void* memoryPointer = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(sourceNativeArray);
        //    Marshal.Copy((IntPtr)memoryPointer, tempByteArray, 0, sourceNativeArray.Length * 16 * 4);
        //    return FromByteArray<Matrix4x4>(targetMatrixArray, tempByteArray); ;
        //}

        //private static T[] FromByteArray<T>(byte[] source) where T : struct
        //{
        //    T[] destination = new T[source.Length / Marshal.SizeOf(typeof(T))];
        //    GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        //    try
        //    {
        //        IntPtr pointer = handle.AddrOfPinnedObject();
        //        Marshal.Copy(source, 0, pointer, source.Length);
        //        return destination;
        //    }
        //    finally
        //    {
        //        if (handle.IsAllocated)
        //            handle.Free();
        //    }
        //}
    }

    [Serializable]
    public struct HeightMapSample
    {
        public float Height;

        //public Vector3 Normal;
        public float Steepness;
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct SampleHeightMapJob : IJobParallelFor
    {
        public NativeArray<HeightMapSample> HeightMapSamples;
        [ReadOnly] public NativeArray<float> InputHeights;
        public int Width;
        public int Height;

        public int HeightmapWidth;
        public int HeightmapHeight;
        public Vector3 Scale;
        public Vector3 Size;
        public Vector3 HeightMapScale;

        public void Execute(int index)
        {
            int y = Mathf.FloorToInt((float) index / (float) Width);
            int x = index - (y * Width);

            float interpolatedX = (float) x / Width;
            float interpolatedY = (float) y / Height;

            float3 normal = GetInterpolatedNormal(interpolatedX, interpolatedY);
            HeightMapSample heightMapSample =
                new HeightMapSample
                {
                    Height = GetTriangleInterpolatedHeight(interpolatedX, interpolatedY)
                };

            var slopeCos = math.dot(normal, new float3(0, 1, 0));
            heightMapSample.Steepness = math.acos(slopeCos) * Mathf.Rad2Deg;
            HeightMapSamples[index] = heightMapSample;
        }

        float GetTriangleInterpolatedHeight(float x, float y)
        {
            float fx = x * (HeightmapWidth - 1);
            float fy = y * (HeightmapHeight - 1);
            int lx = (int) fx;
            int ly = (int) fy;

            float u = fx - lx;
            float v = fy - ly;
            if (u > v)
            {
                float z00 = GetHeight(lx + 0, ly + 0);
                float z01 = GetHeight(lx + 1, ly + 0);
                float z11 = GetHeight(lx + 1, ly + 1);
                return z00 + (z01 - z00) * u + (z11 - z01) * v;
            }
            else
            {
                float z00 = GetHeight(lx + 0, ly + 0);
                float z10 = GetHeight(lx + 0, ly + 1);
                float z11 = GetHeight(lx + 1, ly + 1);
                return z00 + (z11 - z10) * u + (z10 - z00) * v;
            }
        }

        float GetHeight(int x, int y)
        {
            x = math.clamp(x, 0, HeightmapWidth - 1);
            y = math.clamp(y, 0, HeightmapHeight - 1);
            return InputHeights[y * HeightmapWidth + x] * HeightMapScale.y;
        }

        public float3 GetInterpolatedNormal(float x, float y)
        {
            float fx = x * (HeightmapWidth - 1);
            float fy = y * (HeightmapHeight - 1);
            int lx = (int) fx;
            int ly = (int) fy;

            float3 n00 = CalculateNormalSobel(lx + 0, ly + 0);
            float3 n10 = CalculateNormalSobel(lx + 1, ly + 0);
            float3 n01 = CalculateNormalSobel(lx + 0, ly + 1);
            float3 n11 = CalculateNormalSobel(lx + 1, ly + 1);

            float u = fx - lx;
            float v = fy - ly;

            float3 s = math.lerp(n00, n10, u);
            float3 t = math.lerp(n01, n11, u);
            float3 value = math.lerp(s, t, v);
            return math.normalize(value);
        }

        float3 CalculateNormalSobel(int x, int y)
        {
            float3 normal;
            var dX = GetHeight(x - 1, y - 1) * -1.0F;
            dX += GetHeight(x - 1, y) * -2.0F;
            dX += GetHeight(x - 1, y + 1) * -1.0F;
            dX += GetHeight(x + 1, y - 1) * 1.0F;
            dX += GetHeight(x + 1, y) * 2.0F;
            dX += GetHeight(x + 1, y + 1) * 1.0F;

            dX /= Scale.x;

            var dY = GetHeight(x - 1, y - 1) * -1.0F;
            dY += GetHeight(x, y - 1) * -2.0F;
            dY += GetHeight(x + 1, y - 1) * -1.0F;
            dY += GetHeight(x - 1, y + 1) * 1.0F;
            dY += GetHeight(x, y + 1) * 2.0F;
            dY += GetHeight(x + 1, y + 1) * 1.0F;
            dY /= Scale.z;

            normal.x = -dX;
            normal.y = 8;
            normal.z = -dY;
            return math.normalize(normal);
        }
    }
}