using System;

namespace LLVMSharp
{
    public unsafe partial struct LLVMTargetMachineRef : IEquatable<LLVMTargetMachineRef>, IDisposable
    {
        public LLVMOrcJITStackRef OrcCreateInstance()
        {
            // OrcCreateInstance takes ownership over TargetMachineRef
            // and will dispose it when the OrcJitStackRef is disposed.
            IsDisposed = true;
            return LLVM.OrcCreateInstance(this);
        }

        public static bool operator ==(LLVMTargetMachineRef left, LLVMTargetMachineRef right) => left.Pointer == right.Pointer;

        public static bool operator !=(LLVMTargetMachineRef left, LLVMTargetMachineRef right) => !(left == right);

        public override bool Equals(object obj) => obj is LLVMTargetMachineRef other && Equals(other);

        public bool Equals(LLVMTargetMachineRef other) => Pointer == other.Pointer;

        public override int GetHashCode() => Pointer.GetHashCode();

        public void Dispose() {
            if (!IsDisposed && Pointer != IntPtr.Zero) {
                LLVM.DisposeTargetMachine(this);
                IsDisposed = true;
                Pointer = IntPtr.Zero;
            }
        }
    }
}
