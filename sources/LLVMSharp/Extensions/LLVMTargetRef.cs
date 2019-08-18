using System;
using System.Collections.Generic;

namespace LLVMSharp
{
    public unsafe partial struct LLVMTargetRef : IEquatable<LLVMTargetRef>
    {
        public static LLVMTargetRef GetFromTriple(string triple)
        {
            LLVMTarget* target;
            sbyte* error;
            var marsh = new MarshaledString(triple);
            int result = LLVM.GetTargetFromTriple(marsh.Value, &target, &error);
            return target;
        }

        public static string DefaultTriple
        {
            get
            {
                var pDefaultTriple = LLVM.GetDefaultTargetTriple();

                if (pDefaultTriple is null)
                {
                    return string.Empty;
                }

                var span = new ReadOnlySpan<byte>(pDefaultTriple, int.MaxValue);
                return span.Slice(0, span.IndexOf((byte)'\0')).AsString();
            }
        }

        public static LLVMTargetRef Default
        {
            get
            {
                var triple = LLVM.GetDefaultTargetTriple();
                if (triple is null)
                {
                    // TODO?
                    return null;
                }
                LLVMTarget* target;
                sbyte* error;
                int result = LLVM.GetTargetFromTriple(triple, &target, &error);
                if (result > 0) {
                    // TODO
                }
                return target;

            }
        }

        public LLVMTargetMachineRef CreateMachine(
            string triple, string CPU, string features,
            LLVMCodeGenOptLevel optLevel,
            LLVMRelocMode relocMode,
            LLVMCodeModel codeModel) {
            var _triple = new MarshaledString(triple);
            var _CPU = new MarshaledString(CPU);
            var _features = new MarshaledString(features);
            return LLVM.CreateTargetMachine(
                this, _triple.Value, _CPU.Value, _features.Value,
                optLevel, relocMode, codeModel);
        }
        public static LLVMTargetRef First => LLVM.GetFirstTarget();

        public static IEnumerable<LLVMTargetRef> Targets
        {
            get
            {
                var target = First;

                while (target != null)
                {
                    yield return target;
                    target = target.GetNext();
                }
            }
        }

        public string Name
        {
            get
            {
                if (Pointer == IntPtr.Zero)
                {
                    return string.Empty;
                }

                var pName = LLVM.GetTargetName(this);

                if (pName is null)
                {
                    return string.Empty;
                }

                var span = new ReadOnlySpan<byte>(pName, int.MaxValue);
                return span.Slice(0, span.IndexOf((byte)'\0')).AsString();
            }
        }

        public static bool operator ==(LLVMTargetRef left, LLVMTargetRef right) => left.Pointer == right.Pointer;

        public static bool operator !=(LLVMTargetRef left, LLVMTargetRef right) => !(left == right);

        public override bool Equals(object obj) => obj is LLVMTargetRef other && Equals(other);

        public bool Equals(LLVMTargetRef other) => Pointer == other.Pointer;

        public override int GetHashCode() => Pointer.GetHashCode();

        public LLVMTargetRef GetNext() => LLVM.GetNextTarget(this);
    }
}
