using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace AwesomeTechnologies.Utility
{
    //public unsafe class ManagedNativeFloatArray
    //{
    //    public NativeSlice<float> NativeArray;
    //    public AtomicSafetyHandle SafetyHandle;
    //    public GCHandle GcHandle;

    //    public ManagedNativeFloatArray(float[,] managedArray)
    //    {
    //        SafetyHandle = new AtomicSafetyHandle();
    //        GcHandle = GCHandle.Alloc(managedArray, GCHandleType.Pinned);
    //        int length = managedArray.GetLength(0) * managedArray.GetLength(1);
    //        NativeArray =
    //            NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<float>((void*) GcHandle.AddrOfPinnedObject(),4,length);
    //        NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref NativeArray, SafetyHandle);
    //    }

    //    public void Dispose()
    //    {                 
    //        AtomicSafetyHandle.Release(SafetyHandle);
    //        GcHandle.Free();
    //    }
    //}

    
    //private static readonly Matrix4x4 SizeMatrix = Matrix4x4.identity;
        //public static readonly int SizeOfMatrix4X4 = Marshal.SizeOf(SizeMatrix);

        //private static readonly Vector4 SizeVector4 = Vector4.zero;
        //public static readonly int SizeOffVector4 = Marshal.SizeOf(SizeVector4);

        //public static unsafe void NativeToManagedMatrix(Matrix4x4[] targetMatrixArray, NativeSlice<Matrix4x4> sourceSlice)
        //{
        //    void* memoryPointer = sourceSlice.GetUnsafeReadOnlyPtr(); //NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(sourceNativeArray);
        //    GCHandle handle = GCHandle.Alloc(targetMatrixArray, GCHandleType.Pinned);
        //    try
        //    {
        //        IntPtr pointer = handle.AddrOfPinnedObject();
        //        CopyMemory(pointer,(IntPtr)memoryPointer, sourceSlice.Length * 16 * 4);

        //        //Buffer.MemoryCopy(memoryPointer, (void*)pointer, sourceSlice.Length * 16 * 4, sourceSlice.Length * 16 * 4);
        //    }
        //    finally
        //    {
        //        if (handle.IsAllocated)
        //            handle.Free();
        //    }
        //}

//#if PLATFORM_STANDALONE_WIN
//        [DllImport("msvcrt.dll", EntryPoint = "memcpy")]
//        public static extern void CopyMemory(IntPtr pDest, IntPtr pSrc, int length);
//#endif

        //public static unsafe void CopyNativeToManagedMatrixArray(byte[] tempByteArray, Matrix4x4[] targetMatrixArray,
        //    NativeArray<Matrix4x4> sourceNativeArray)
        //{
        //    void* memoryPointer = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(sourceNativeArray);
        //    Marshal.Copy((IntPtr) memoryPointer, tempByteArray, 0, sourceNativeArray.Length * 16 * 4);
        //    Matrix4X4FromByteArray(targetMatrixArray, tempByteArray);
        //}

        //public static unsafe void CopyNativeToManagedMatrixArray(byte[] tempByteArray, Matrix4x4[] targetMatrixArray,
        //    NativeSlice<Matrix4x4> sourceSlice)
        //{
        //    void* memoryPointer = sourceSlice.GetUnsafeReadOnlyPtr();
        //    Marshal.Copy((IntPtr)memoryPointer, tempByteArray, 0, sourceSlice.Length * 16 * 4);
        //    Matrix4X4FromByteArray(targetMatrixArray, tempByteArray);
        //}

        //public static unsafe void CopyNativeToManagedFloatArray(byte[] tempByteArray, float[] targetFloatArray,
        //    NativeSlice<float> sourceSlice)
        //{
        //    void* memoryPointer = sourceSlice.GetUnsafeReadOnlyPtr();
        //    Marshal.Copy((IntPtr)memoryPointer, tempByteArray, 0, sourceSlice.Length * 4);
        //    FloatFromByteArray(targetFloatArray, tempByteArray);
        //}

        //public static unsafe void CopyNativeToManagedVector4Array(byte[] tempByteArray, Vector4[] targetVector4Array,
        //    NativeSlice<Vector4> sourceSlice)
        //{
        //    void* memoryPointer = sourceSlice.GetUnsafeReadOnlyPtr();
        //    Marshal.Copy((IntPtr)memoryPointer, tempByteArray, 0, sourceSlice.Length * 4 * 4);
        //    Vector4FromByteArray(targetVector4Array, tempByteArray);
        //}

        //private static void Matrix4X4FromByteArray(Matrix4x4[] destination, byte[] source)
        //{
        //    GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        //    try
        //    {
        //        IntPtr pointer = handle.AddrOfPinnedObject();
        //        Marshal.Copy(source, 0, pointer, source.Length);
        //    }
        //    finally
        //    {
        //        if (handle.IsAllocated)
        //            handle.Free();
        //    }
        //}

        //private static void FloatFromByteArray(float[] destination, byte[] source)
        //{
        //    GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        //    try
        //    {
        //        IntPtr pointer = handle.AddrOfPinnedObject();
        //        Marshal.Copy(source, 0, pointer, source.Length);
        //    }
        //    finally
        //    {
        //        if (handle.IsAllocated)
        //            handle.Free();
        //    }
        //}

        //private static void Vector4FromByteArray(Vector4[] destination, byte[] source)
        //{
        //    GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        //    try
        //    {
        //        IntPtr pointer = handle.AddrOfPinnedObject();
        //        Marshal.Copy(source, 0, pointer, source.Length);
        //    }
        //    finally
        //    {
        //        if (handle.IsAllocated)
        //            handle.Free();
        //    }
        //}

        //public static unsafe NativeArray<float> CreateNativeArrayFromManagedArray(float[,] array)
        //{
        //    void* managedBuffer = UnsafeUtility.AddressOf(ref array[0, 0]);
        //    int length = array.GetLength(0) * array.GetLength(1);
        //    return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(managedBuffer, length, Allocator.None);
        //}
    //}

    public static class NativeListExtentions
    {
        public static unsafe void ClearMemory<T>(
            this NativeList<T> nativeList)
            where T : struct
        {
            UnsafeUtility.MemClear(nativeList.GetUnsafePtr(), nativeList.Length * UnsafeUtility.SizeOf<T>()); 
        }
        
        public static unsafe void CompactMemory<T>(
            this NativeList<T> nativeList)
            where T : struct
        {
            nativeList.Clear();
            nativeList.Capacity = 0;
        }               
    }




    public static class NativeArrayExtensions
    {               
        public static unsafe void CopyToFast<T>(
            this NativeArray<T> nativeArray,
            T[] array)
            where T : struct
        {
            if (array == null)
            {
                throw new NullReferenceException(nameof(array) + " is null");
            }

            int nativeArrayLength = nativeArray.Length;
            if (array.Length < nativeArrayLength)
            {
                throw new IndexOutOfRangeException(
                    nameof(array) + " is shorter than " + nameof(nativeArray));
            }

            int byteLength = nativeArray.Length * UnsafeUtility.SizeOf<T>();
            void* managedBuffer = UnsafeUtility.AddressOf(ref array[0]);
            void* nativeBuffer = nativeArray.GetUnsafePtr();
            UnsafeUtility.MemCpy(managedBuffer, nativeBuffer, byteLength);
        }

        public static unsafe void CopyToFast<T>(
            this NativeSlice<T> nativeSlice,
            T[] array)
            where T : struct
        {         
            if (array == null)
            {
                throw new NullReferenceException(nameof(array) + " is null");
            }            
            int nativeArrayLength = nativeSlice.Length;
            if (array.Length < nativeArrayLength)
            {
                throw new IndexOutOfRangeException(
                    nameof(array) + " is shorter than " + nameof(nativeSlice));
            }
            int byteLength = nativeSlice.Length * UnsafeUtility.SizeOf<T>();
            void* managedBuffer = UnsafeUtility.AddressOf(ref array[0]);
            void* nativeBuffer = nativeSlice.GetUnsafePtr();
            UnsafeUtility.MemCpy(managedBuffer, nativeBuffer, byteLength);
        }


        public static unsafe void CopyToFast<T>(
            this NativeArray<T> nativeArray,
            T[,,] array)
            where T : struct
        {
            if (array == null)
            {
                throw new NullReferenceException(nameof(array) + " is null");
            }

            int nativeArrayLength = nativeArray.Length;
            int managedArrayLength = array.GetLength(0) * array.GetLength(1) * array.GetLength(2);
            if (managedArrayLength < nativeArrayLength)
            {
                throw new IndexOutOfRangeException(
                    nameof(array) + " is shorter than " + nameof(nativeArray));
            }

            int byteLength = nativeArray.Length * UnsafeUtility.SizeOf<T>();
            void* managedBuffer = UnsafeUtility.AddressOf(ref array[0,0,0]);
            void* nativeBuffer = nativeArray.GetUnsafePtr();
            UnsafeUtility.MemCpy(managedBuffer, nativeBuffer, byteLength);
        }

        public static unsafe void CopyFromFast<T>(
            this NativeArray<T> nativeArray,
            T[,] array)
            where T : struct
        {
            if (array == null)
            {
                throw new NullReferenceException(nameof(array) + " is null");
            }

            int nativeArrayLength = nativeArray.Length;
            int managedArrayLength = array.GetLength(0) * array.GetLength(1);
            if (managedArrayLength > nativeArrayLength)
            {
                throw new IndexOutOfRangeException(
                    nameof(nativeArray) + " is shorter than " + nameof(array));
            }

            int byteLength = managedArrayLength * UnsafeUtility.SizeOf<T>();
            void* managedBuffer = UnsafeUtility.AddressOf(ref array[0, 0]);
            void* nativeBuffer = nativeArray.GetUnsafePtr();
            UnsafeUtility.MemCpy(nativeBuffer,managedBuffer, byteLength);
        }
        
        public static unsafe void CopyFromFast<T>(
            this NativeArray<T> nativeArray,
            T[,,] array)
            where T : struct
        {
            if (array == null)
            {
                throw new NullReferenceException(nameof(array) + " is null");
            }

            int nativeArrayLength = nativeArray.Length;
            int managedArrayLength = array.GetLength(0) * array.GetLength(1) * array.GetLength(2);
            if (managedArrayLength > nativeArrayLength)
            {
                throw new IndexOutOfRangeException(
                    nameof(nativeArray) + " is shorter than " + nameof(array));
            }

            int byteLength = managedArrayLength * UnsafeUtility.SizeOf<T>();
            void* managedBuffer = UnsafeUtility.AddressOf(ref array[0, 0, 0]);
            void* nativeBuffer = nativeArray.GetUnsafePtr();
            UnsafeUtility.MemCpy(nativeBuffer,managedBuffer, byteLength);
        }

        public static unsafe void CopyFromFast<T>(
            this NativeArray<T> nativeArray,
            T[] array)
            where T : struct
        {
            if (array == null)
            {
                throw new NullReferenceException(nameof(array) + " is null");
            }

            int nativeArrayLength = nativeArray.Length;
            int managedArrayLength = array.GetLength(0);
            if (managedArrayLength > nativeArrayLength)
            {
                throw new IndexOutOfRangeException(
                    nameof(nativeArray) + " is shorter than " + nameof(array));
            }

            int byteLength = managedArrayLength * UnsafeUtility.SizeOf<T>();
            void* managedBuffer = UnsafeUtility.AddressOf(ref array[0]);
            void* nativeBuffer = nativeArray.GetUnsafePtr();

            UnsafeUtility.MemCpy(nativeBuffer,managedBuffer, byteLength);
        }       
        
        public static unsafe void CopyFromFast<T>(
            this NativeArray<T> nativeArray,
            List<T> managedList)
            where T : struct
        {
            if (managedList == null)
            {
                throw new NullReferenceException(nameof(managedList) + " is null");
            }

            int nativeArrayLength = nativeArray.Length;
            int managedListLength = managedList.Count;
            T[] managedInternalArray = managedList.GetInternalArray();
            
            if (managedListLength > nativeArrayLength)
            {
                throw new IndexOutOfRangeException(
                    nameof(nativeArray) + " is shorter than " + nameof(managedInternalArray));
            }

            int byteLength = managedListLength * UnsafeUtility.SizeOf<T>();
            void* managedBuffer = UnsafeUtility.AddressOf(ref managedInternalArray[0]);
            void* nativeBuffer = nativeArray.GetUnsafePtr();

            UnsafeUtility.MemCpy(nativeBuffer,managedBuffer, byteLength);
        }
    }
}
