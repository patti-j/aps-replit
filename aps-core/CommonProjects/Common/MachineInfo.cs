using System.Runtime.Intrinsics.X86;

namespace PT.Common;

public class MachineInfo
{
    public static string GetProcessorId()
    {
        if (!X86Base.IsSupported)
            throw new PlatformNotSupportedException("CPUID not supported on this CPU.");

        // Call CPUID with EAX=1 (processor info and feature bits)
        var regs = X86Base.CpuId(1, 0);

        // On Intel/AMD, ProcessorId = EDX:EAX
        return $"{regs.Edx:X8}{regs.Eax:X8}";
    }
}