#define TARGET_64BIT
//Warning: this whole system including corlib is for 64bit
//A minimum corlib to make c# work

using Internal.Runtime;
using Internal.Runtime.CompilerHelpers;
using Internal.Runtime.CompilerServices;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region A couple very basic things
namespace System
{
    public unsafe class Object
    {
#pragma warning disable 169
        // The layout of object is a contract with the compiler.
        internal EEType* EEType;
#pragma warning restore 169

        public virtual bool Equals(object b) 
        {
            object a = this;
            return Unsafe.As<object, ulong>(ref a) == Unsafe.As<object, ulong>(ref b);
        }

        public virtual int GetHashCode()
            => 0;

        public virtual string ToString() => "System.Object";

        public virtual void Dispose()
        {
            var obj = this;
            Allocator.Free((void*)Unsafe.As<object, ulong>(ref obj));
        }
    }
    public struct Void { }

    // The layout of primitive types is special cased because it would be recursive.
    // These really don't need any fields to work.
    public struct Boolean { }
    public struct Char { }
    public struct SByte { }
    public struct Byte { }
    public struct Int16 { }
    public struct UInt16 { }
    public struct Int32 { }
    public struct UInt32 { }
    public struct Int64 { }
    public struct UInt64 { }
    public struct IntPtr { }
    public struct UIntPtr { }
    public struct Single { }
    public struct Double { }

    public unsafe struct EETypePtr
    {
        public EEType* Value;

        [Intrinsic]
        internal extern static EETypePtr EETypePtrOf<T>();
    }

    public abstract class ValueType { }
    public abstract class Enum : ValueType
    {
        [Intrinsic]
        public extern bool HasFlag(Enum flag);
    }

    public struct Nullable<T> where T : struct { }

    public sealed partial class String
    {
        // The layout of the string type is a contract with the compiler.
        public int Length;
        internal char FirstChar;

        public unsafe char this[int index]
        {
            [Intrinsic]
            get => Unsafe.Add(ref FirstChar, index);

            set
            {
                fixed (char* p = &FirstChar)
                {
                    p[index] = value;
                }
            }
        }
    }
    
    public abstract class Array 
    {
        public int Length;
    }
    
    public abstract class Delegate { }
    public abstract class MulticastDelegate : Delegate { }

    public struct RuntimeTypeHandle { }
    public struct RuntimeMethodHandle { }
    public struct RuntimeFieldHandle { }

    public class Attribute { }

    public sealed class FlagsAttribute : Attribute { }

    public sealed class ParamArrayAttribute : Attribute
    {
        public ParamArrayAttribute() { }
    }

    public enum AttributeTargets { }

    public sealed class AttributeUsageAttribute : Attribute
    {
        public AttributeUsageAttribute(AttributeTargets validOn) { }
        public bool AllowMultiple { get; set; }
        public bool Inherited { get; set; }
    }

    public class AppContext
    {
        public static void SetData(string s, object o) { }
    }

    namespace Reflection
    {
        public sealed class DefaultMemberAttribute : Attribute
        {
            // You must provide the name of the member, this is required
            public DefaultMemberAttribute(string memberName)
            {
                MemberName = memberName;
            }

            // A get accessor to return the name from the attribute.
            // NOTE: There is no setter because the name must be provided
            //    to the constructor.  The name is not optional.
            public string MemberName { get; }
        }
    }

    namespace Runtime.CompilerServices
    {
        public sealed class ExtensionAttribute : Attribute { }

        public static class IsVolatile
        {
        }

        public class RuntimeHelpers
        {
            public static unsafe int OffsetToStringData => sizeof(IntPtr) + sizeof(int);
        }

        public static class RuntimeFeature
        {
            public const string UnmanagedSignatureCallingConvention = nameof(UnmanagedSignatureCallingConvention);
        }

        internal sealed class IntrinsicAttribute : Attribute { }

        public sealed class MethodImplAttribute : Attribute
        {
            public MethodImplAttribute(MethodImplOptions methodImplOptions) { }
        }

        public enum MethodImplOptions
        {
            Unmanaged = 0x0004,
            NoInlining = 0x0008,
            NoOptimization = 0x0040,
            AggressiveInlining = 0x0100,
            AggressiveOptimization = 0x200,
            InternalCall = 0x1000,
        }
    }
}

namespace System.Runtime.InteropServices
{
    public class UnmanagedType { }

    sealed class StructLayoutAttribute : Attribute
    {
        public StructLayoutAttribute(LayoutKind layoutKind)
        {
            Value = layoutKind;
        }

        public StructLayoutAttribute(short layoutKind)
        {
            Value = (LayoutKind)layoutKind;
        }

        public LayoutKind Value { get; }

        public int Pack;
        public int Size;
        public CharSet CharSet;
    }

    public sealed class FieldOffsetAttribute : Attribute
    {
        public FieldOffsetAttribute(int offset)
        {
            Value = offset;
        }

        public int Value { get; }
    }

    public sealed class DllImportAttribute : Attribute
    {
        public CallingConvention CallingConvention;

        public string EntryPoint;

        public DllImportAttribute(string dllName)
        {
        }
    }

    internal enum LayoutKind
    {
        Sequential = 0, // 0x00000008,
        Explicit = 2, // 0x00000010,
        Auto = 3, // 0x00000000,
    }

    internal enum CharSet
    {
        None = 1,       // User didn't specify how to marshal strings.
        Ansi = 2,       // Strings should be marshalled as ANSI 1 byte chars.
        Unicode = 3,    // Strings should be marshalled as Unicode 2 byte chars.
        Auto = 4,       // Marshal Strings in the right way for the target system.
    }

    public enum CallingConvention
    {
        Winapi = 1,
        Cdecl = 2,
        StdCall = 3,
        ThisCall = 4,
        FastCall = 5,
    }
}
#endregion

#region Things needed by ILC
namespace System
{
    internal sealed partial class RuntimeType
    {
    }

    namespace Runtime
    {
        internal sealed class RuntimeExportAttribute : Attribute
        {
            public RuntimeExportAttribute(string entry) { }
        }
    }

    class Array<T> : Array { }
}

namespace Internal.Runtime
{
    namespace CompilerServices
    {
        public static unsafe class Unsafe
        {
            [Intrinsic]
            public static extern ref T Add<T>(ref T source, int elementOffset);

            [Intrinsic]
            public static extern ref TTo As<TFrom, TTo>(ref TFrom source);

            [Intrinsic]
            public static extern T As<T>(object value) where T : class;

            [Intrinsic]
            public static extern void* AsPointer<T>(ref T value);

            [Intrinsic]
            public static extern ref T AsRef<T>(void* pointer);

            public static ref T AsRef<T>(ulong pointer)
                => ref AsRef<T>((void*)pointer);

            [Intrinsic]
            public static extern int SizeOf<T>();

            [Intrinsic]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref T AddByteOffset<T>(ref T source, ulong byteOffset)
            {
                for (; ; );
            }

            [Intrinsic]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ref T AddByteOffset<T>(ref T source, nuint byteOffset)
            {
                return ref AddByteOffset(ref source, (ulong)(void*)byteOffset);
            }
        }
    }

    namespace CompilerHelpers
    {
        using Internal.Runtime.CompilerServices;
        using System.Runtime;

        public static class ThrowHelpers
        {
            public static void ThrowInvalidProgramException(int id) { }
            public static void ThrowInvalidProgramExceptionWithArgument(int id, string methodName) { }
            public static void ThrowOverflowException() { }
            public static void ThrowIndexOutOfRangeException() { }
            public static void ThrowTypeLoadException(int id, string className, string typeName) { }
        }

        // A class that the compiler looks for that has helpers to initialize the
        // process. The compiler can gracefully handle the helpers not being present,
        // but the class itself being absent is unhandled. Let's add an empty class.
        class StartupCodeHelpers
        {
            // A couple symbols the generated code will need we park them in this class
            // for no particular reason. These aid in transitioning to/from managed code.
            // Since we don't have a GC, the transition is a no-op.
            [RuntimeExport("__fail_fast")]
            static void __fail_fast() { while (true) ; }
            [RuntimeExport("RhpReversePInvoke")]
            static void RhpReversePInvoke(IntPtr frame) { }
            [RuntimeExport("RhpReversePInvokeReturn")]
            static void RhpReversePInvokeReturn(IntPtr frame) { }
            [RuntimeExport("RhpReversePInvoke2")]
            static void RhpReversePInvoke2(IntPtr frame) { }
            [RuntimeExport("RhpReversePInvokeReturn2")]
            static void RhpReversePInvokeReturn2(IntPtr frame) { }
            [RuntimeExport("RhpPInvoke")]
            static void RhpPInvoke(IntPtr frame) { }
            [RuntimeExport("RhpPInvokeReturn")]
            static void RhpPInvokeReturn(IntPtr frame) { }

            [RuntimeExport("RhpFallbackFailFast")]
            static void RhpFallbackFailFast() { while (true) ; }

            [RuntimeExport("RhpNewFast")]
            internal static unsafe object RhpNewFast(EEType* pEEType)
            {
                var size = pEEType->BaseSize;

                if (size % 8 > 0)
                    size = ((size / 8) + 1) * 8;

                var data = (ulong)Allocator.Allocate(size);
                var obj = Unsafe.As<ulong, object>(ref data);
                Allocator.ZeroFill((byte*)data, size);
                *(ulong*)data = (ulong)pEEType;

                return obj;
            }

            [RuntimeExport("RhpNewArray")]
            internal static unsafe object RhpNewArray(EEType* pEEType, int length)
            {
                var size = pEEType->BaseSize + (ulong)length * pEEType->ComponentSize;

                if (size % 8 > 0)
                    size = ((size / 8) + 1) * 8;

                var data = (ulong)Allocator.Allocate(size);
                var obj = Unsafe.As<ulong, object>(ref data);
                Allocator.ZeroFill((byte*)data, size);
                *(ulong*)data = (ulong)pEEType;

                var b = (byte*)data;
                b += sizeof(IntPtr);
                Allocator.MemCpy(b, (byte*)(&length), sizeof(int));

                return obj;
            }

            [RuntimeExport("RhpAssignRef")]
            static unsafe void RhpAssignRef(void** address, void* obj)
            {
                *address = obj;
            }

            [RuntimeExport("RhpByRefAssignRef")]
            static unsafe void RhpByRefAssignRef(void** address, void* obj)
            {
                *address = obj;
            }

            [RuntimeExport("RhpCheckedAssignRef")]
            static unsafe void RhpCheckedAssignRef(void** address, void* obj)
            {
                *address = obj;
            }

            [RuntimeExport("RhpStelemRef")]
            static unsafe void RhpStelemRef(Array array, int index, object obj)
            {
                fixed (int* n = &array.Length)
                {
                    var ptr = (byte*)n;
                    ptr += sizeof(void*);   // Array length is padded to 8 bytes on 64-bit
                    ptr += index * array.EEType->ComponentSize;  // Component size should always be 8, seeing as it's a pointer...
                    var pp = (IntPtr*)ptr;
                    *pp = Unsafe.As<object, IntPtr>(ref obj);
                }
            }

            [RuntimeExport("RhTypeCast_IsInstanceOfClass")]
            public static unsafe object RhTypeCast_IsInstanceOfClass(EEType* pTargetType, object obj)
            {
                if (obj == null)
                    return null;

                if (pTargetType == obj.EEType)
                    return obj;

                var bt = obj.EEType->RelatedType.BaseType;

                while (true)
                {
                    if (bt == null)
                        return null;

                    if (pTargetType == bt)
                        return obj;

                    bt = bt->RelatedType.BaseType;
                }
            }

            internal static unsafe void InitializeModules(void* Modules)
            {
                var header = *((ulong*)Modules);
                var sections = (ulong*)((ulong)header + 16);

                if (*(uint*)header != 0x00525452) return;

                ushort numSec = *(ushort*)((ulong)header + 12);
                for (int k = 0; k < numSec; k++)
                {
                    var section = ((ulong)header + 16 + (ulong)(k * 24));

                    if (*(int*)section == 201)
                    {
                        ulong* start = (ulong*)*(ulong*)(section + 8);
                        ulong* end = (ulong*)*(ulong*)(section + 16);
                        for (ulong* block = start; block < end; block++)
                        {
                            var pBlock = (ulong*)*block;
                            var blockAddr = (long)(*pBlock);

                            if ((blockAddr & 0x01) == 0x01)
                            {
                                var obj = StartupCodeHelpers.RhpNewFast((EEType*)(blockAddr & ~0x03));

                                var handle = (ulong)Allocator.Allocate((ulong)sizeof(ulong));
                                *(ulong*)handle = Unsafe.As<object, ulong>(ref obj);
                                *pBlock = handle;
                            }
                        }
                    }
                }
            }
        }
    }

    #region EEType
    internal enum EETypeElementType
    {
        // Primitive
        Unknown = 0x00,
        Void = 0x01,
        Boolean = 0x02,
        Char = 0x03,
        SByte = 0x04,
        Byte = 0x05,
        Int16 = 0x06,
        UInt16 = 0x07,
        Int32 = 0x08,
        UInt32 = 0x09,
        Int64 = 0x0A,
        UInt64 = 0x0B,
        IntPtr = 0x0C,
        UIntPtr = 0x0D,
        Single = 0x0E,
        Double = 0x0F,

        ValueType = 0x10,
        // Enum = 0x11, // EETypes store enums as their underlying type
        Nullable = 0x12,
        // Unused 0x13,

        Class = 0x14,
        Interface = 0x15,

        SystemArray = 0x16, // System.Array type

        Array = 0x17,
        SzArray = 0x18,
        ByRef = 0x19,
        Pointer = 0x1A,
    }

    [Flags]
    internal enum EETypeFlags : ushort
    {
        /// <summary>
        /// There are four kinds of EETypes, defined in <c>Kinds</c>.
        /// </summary>
        EETypeKindMask = 0x0003,

        /// <summary>
        /// This flag is set when m_RelatedType is in a different module.  In that case, _pRelatedType
        /// actually points to an IAT slot in this module, which then points to the desired EEType in the
        /// other module.  In other words, there is an extra indirection through m_RelatedType to get to 
        /// the related type in the other module.  When this flag is set, it is expected that you use the 
        /// "_ppXxxxViaIAT" member of the RelatedTypeUnion for the particular related type you're 
        /// accessing.
        /// </summary>
        RelatedTypeViaIATFlag = 0x0004,

        /// <summary>
        /// This type was dynamically allocated at runtime.
        /// </summary>
        IsDynamicTypeFlag = 0x0008,

        /// <summary>
        /// This EEType represents a type which requires finalization.
        /// </summary>
        HasFinalizerFlag = 0x0010,

        /// <summary>
        /// This type contain GC pointers.
        /// </summary>
        HasPointersFlag = 0x0020,

        /// <summary>
        /// Type implements ICastable to allow dynamic resolution of interface casts.
        /// </summary>
        ICastableFlag = 0x0040,

        /// <summary>
        /// This type is generic and one or more of its type parameters is co- or contra-variant. This
        /// only applies to interface and delegate types.
        /// </summary>
        GenericVarianceFlag = 0x0080,

        /// <summary>
        /// This type has optional fields present.
        /// </summary>
        OptionalFieldsFlag = 0x0100,

        // Unused = 0x0200,

        /// <summary>
        /// This type is generic.
        /// </summary>
        IsGenericFlag = 0x0400,

        /// <summary>
        /// We are storing a EETypeElementType in the upper bits for unboxing enums.
        /// </summary>
        ElementTypeMask = 0xf800,
        ElementTypeShift = 11,

        /// <summary>
        /// Single mark to check TypeKind and two flags. When non-zero, casting is more complicated.
        /// </summary>
        ComplexCastingMask = EETypeKindMask | RelatedTypeViaIATFlag | GenericVarianceFlag
    };

    internal enum EETypeKind : ushort
    {
        /// <summary>
        /// Represents a standard ECMA type
        /// </summary>
        CanonicalEEType = 0x0000,

        /// <summary>
        /// Represents a type cloned from another EEType
        /// </summary>
        ClonedEEType = 0x0001,

        /// <summary>
        /// Represents a parameterized type. For example a single dimensional array or pointer type
        /// </summary>
        ParameterizedEEType = 0x0002,

        /// <summary>
        /// Represents an uninstantiated generic type definition
        /// </summary>
        GenericTypeDefEEType = 0x0003,
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct ObjHeader
    {
        // Contents of the object header
        private IntPtr _objHeaderContents;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EEType
    {
        private const int POINTER_SIZE = 8;
        private const int PADDING = 1; // _numComponents is padded by one Int32 to make the first element pointer-aligned
        internal const int SZARRAY_BASE_SIZE = POINTER_SIZE + POINTER_SIZE + (1 + PADDING) * 4;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe struct RelatedTypeUnion
        {
            // Kinds.CanonicalEEType
            [FieldOffset(0)]
            public EEType* BaseType;
            [FieldOffset(0)]
            public EEType** BaseTypeViaIAT;

            // Kinds.ClonedEEType
            [FieldOffset(0)]
            public EEType* CanonicalType;
            [FieldOffset(0)]
            public EEType** CanonicalTypeViaIAT;

            // Kinds.ArrayEEType
            [FieldOffset(0)]
            public EEType* RelatedParameterType;
            [FieldOffset(0)]
            public EEType** RelatedParameterTypeViaIAT;
        }

        internal ushort ComponentSize;
        internal EETypeFlags Flags;
        internal uint BaseSize;
        internal RelatedTypeUnion RelatedType;
        internal ushort NumVtableSlots;
        internal ushort NumInterfaces;
        internal uint HashCode;

        // vtable follows

        // These masks and paddings have been chosen so that the ValueTypePadding field can always fit in a byte of data.
        // if the alignment is 8 bytes or less. If the alignment is higher then there may be a need for more bits to hold
        // the rest of the padding data.
        // If paddings of greater than 7 bytes are necessary, then the high bits of the field represent that padding
        private const uint ValueTypePaddingLowMask = 0x7;
        private const uint ValueTypePaddingHighMask = 0xFFFFFF00;
        private const uint ValueTypePaddingMax = 0x07FFFFFF;
        private const int ValueTypePaddingHighShift = 8;
        private const uint ValueTypePaddingAlignmentMask = 0xF8;
        private const int ValueTypePaddingAlignmentShift = 3;
    }
    #endregion
}
#endregion