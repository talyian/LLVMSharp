using System;
using System.Runtime.InteropServices;

namespace LLVMSharp
{
    public unsafe partial struct LLVMOrcJITStackRef : IEquatable<LLVMOrcJITStackRef>, IDisposable
    {
        public static bool operator ==(LLVMOrcJITStackRef left, LLVMOrcJITStackRef right) => left.Pointer == right.Pointer;

        public static bool operator !=(LLVMOrcJITStackRef left, LLVMOrcJITStackRef right) => !(left == right);

        public override bool Equals(object obj) => obj is LLVMOrcJITStackRef other && Equals(other);

        public bool Equals(LLVMOrcJITStackRef other) => Pointer == other.Pointer;

        public override int GetHashCode() => Pointer.GetHashCode();

        public void Dispose() {
            if (Pointer != IntPtr.Zero) {
                LLVM.OrcDisposeInstance(this);
                Pointer = IntPtr.Zero;
            }
        }

        public string ErrorMsg {
            get {
                if (Pointer == IntPtr.Zero)
                {
                    return "";
                }

                var pErrorStr = LLVM.OrcGetErrorMsg(this);
                if (pErrorStr == null)
                {
                    return "";
                }

                var span = new ReadOnlySpan<byte>(pErrorStr, int.MaxValue);
                return span.Slice(0, span.IndexOf((byte)'\0')).AsString();
            }
        }

        public string OrcGetMangledSymbol(string Symbol) {
            if (Pointer == IntPtr.Zero)
            {
                return "";
            }

            sbyte* mangledSymbol;

            using(var marshaledSymbol = new MarshaledString(Symbol))
            {
                LLVM.OrcGetMangledSymbol(this, &mangledSymbol, marshaledSymbol);
            }

            if (mangledSymbol == null)
            {
                return "";
            }
            var span = new ReadOnlySpan<byte>(mangledSymbol, int.MaxValue);
            return span.Slice(0, span.IndexOf((byte)'\0')).AsString();
        }

        void OrcDisposeMangledSymbol(string MangledSymbol) {
            // TODO
        }

        public void AddEagerlyCompiledIR(LLVMModuleRef module, LLVMOrcSymbolResolverFn resolver, void* context)
        {
  // // convert F# function to delegate which is turned into a native pointer
  //             let resolverfn = LLVMOrcSymbolResolverFn resolve_symbol
  //                   let resolver = M.GetFunctionPointerForDelegate resolverfn
  //                 let context = OrcContext(LLVMOrcJITStackRef.op_Implicit orc, ll_module, 0uL)
  //                   let context_ptr = NativeInterop.NativePtr.toVoidPtr &&context
  //                 let orc_module = orc.AddEagerlyCompiledIR(ll_module, resolver, context_ptr)
  //                 LLVM.OrcAddEagerlyCompiledIR(
  //                                              LLVMOrcJITStackRef.op_Implicit orc,
  //                                              &&orc_module,
  //                                              ll_module_ptr,
  //                                              resolver,
  //                                              context_ptr) |> assert_zero
            ulong orcModule;
            var result = LLVM.OrcAddEagerlyCompiledIR(
                this,
                &orcModule,
                module,
                Marshal.GetFunctionPointerForDelegate(resolver),
                context);
            var error = LLVM.GetErrorMessage(result);
            if (error == null)
            {
                Console.WriteLine("null error");
            }
            else if (error[0] == (byte)'\0')
                Console.WriteLine("empty error");
            else {
                var span = new ReadOnlySpan<byte>(error, Int32.MaxValue);
                var errorString = span.Slice(0, span.IndexOf((byte)'\0')).AsString();
                LLVM.DisposeMessage(error);
                throw new ExternalException(errorString);
            }
        }

        public DelegateType GetFunction<DelegateType> (string name) {
            return Marshal.GetDelegateForFunctionPointer<DelegateType>(GetSymbolAddress(name));
        }
        public IntPtr GetSymbolAddress(string main) {
            if (!TryGetSymbolAddress(main, out var addressPointer, out var Error))
            {
                throw new ExternalException(Error);
            }
            else
            {
                return addressPointer;
            }
        }
        
        public bool TryGetSymbolAddress(string name, out IntPtr addressPointer, out string Error)
        {
            ulong address;
            using(var marshaledName = new MarshaledString(name))
            {
                var error = LLVM.OrcGetSymbolAddress(this, &address, marshaledName);
                if (error == null)
                {
                    addressPointer = (IntPtr)address;
                    Error = "";
                    return true;
                }
                else
                {
                    sbyte* error_bytes = LLVM.GetErrorMessage(error);
                    addressPointer = IntPtr.Zero;
                    var span = new ReadOnlySpan<byte>(error_bytes, Int32.MaxValue);
                    Error = span.Slice(0, span.IndexOf((byte)'\0')).AsString();
                    LLVM.DisposeMessage(error_bytes);
                    return false;
                }
            }
        }
    }
}
