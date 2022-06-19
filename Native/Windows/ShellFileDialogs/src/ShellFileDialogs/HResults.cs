using System;

namespace ShellFileDialogs
{
    internal static class HResults
    {
        public static readonly HResult Cancelled = CreateWin32(code: Win32ErrorCodes.ErrorCancelled);

        /// <summary>Creates a Win32 <see cref="HResult"/> value.</summary>
        public static HResult CreateWin32(Win32ErrorCodes code)
        {
            return Create(isFailure: code != Win32ErrorCodes.Success, isCustomer: false, facility: HResultFacility.Win32, code: (ushort)Win32ErrorCodes.ErrorCancelled);
        }

        // Bytes:                                     333333333    222222222    111111111    000000000
        private const uint _codeBitMask = 0b____0000_0000____0000_0000____1111_1111____1111_1111; // Lower 16 bits
        private const uint _facilityBitMask = 0b____0000_0111____1111_1111____0000_0000____0000_0000; // Next 11 bits
        private const uint _reserveXBitMask = 0b____0000_1000____0000_0000____0000_0000____0000_0000; // Next 1 bit
        private const uint _ntStatusBitMask = 0b____0001_0000____0000_0000____0000_0000____0000_0000; // Next 1 bit
        private const uint _customerBitMask = 0b____0010_0000____0000_0000____0000_0000____0000_0000; // Next 1 bit
        private const uint _reservedBitMask = 0b____0100_0000____0000_0000____0000_0000____0000_0000; // Highest bit
        private const uint _severityBitMask = 0b____1000_0000____0000_0000____0000_0000____0000_0000; // Highest bit

        /// <summary>Creates a non-NTSTATUS <see cref="HResult"/> value.</summary>
        public static HResult Create(
            bool isFailure,
            bool isCustomer,
            HResultFacility facility,
            ushort code
        )
        {
            if (facility >= HResultFacility.MAX_VALUE_EXCLUSIVE)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(facility), actualValue: facility, message: "Value must be in the range 0 through 2047 (inclusive). The HRESULT Facility code is an 11-bit number.");
            }

            uint value = 0;

            if (isFailure)
            {
                value |= _severityBitMask;
            }

            if (isCustomer)
            {
                value |= _customerBitMask;
            }

            {
                uint facilityBits = (uint)facility;
                facilityBits <<= 16;

                value |= facilityBits;
            }

            value |= code;

            return (HResult)value;
        }

        public static HResultSeverity GetSeverity(this HResult hr)
        {
            uint severityBits = (uint)hr & _severityBitMask;
            return (severityBits == 0) ? HResultSeverity.Success : HResultSeverity.Failure;
        }

        public static HResultCustomer GetCustomer(this HResult hr)
        {
            uint customerBits = (uint)hr & _customerBitMask;
            return (customerBits == 0) ? HResultCustomer.MicrosoftDefined : HResultCustomer.CustomerDefined;
        }

        public static HResultFacility GetFacility(this HResult hr)
        {
            uint facilityBits = (uint)hr & _facilityBitMask;
            uint facilityBitsOnly = facilityBits >> 16;
            ushort facilityBitsOnlyAsU16 = (ushort)facilityBitsOnly;
            return (HResultFacility)facilityBitsOnlyAsU16;
        }

        public static ushort GetCode(this HResult hr)
        {
            uint codeBits = (uint)hr & _codeBitMask;
            return (ushort)codeBits;
        }

        /// <summary>Indicates if the required zeros for reserved bits are indeed zero - otherwise <paramref name="hr"/> may be an <c>NTSTATUS</c> value or some other 32-bit value.</summary>
        public static bool IsValidHResult(this HResult hr)
        {
            uint ntstatusBits = (uint)hr & _ntStatusBitMask;
            if (ntstatusBits != 0)
            {
                // The NTSTATUS bit is set, so this is not a HRESULT.
                return false;
            }

            // If the NTSTATUS (`N`) bit is clear, then the Reserved (`R`) bit must also be clear:
            // > Reserved. If the N bit is clear, this bit MUST be set to 0. If the N bit is set, this bit is defined by the NTSTATUS numbering space (as specified in section 2.3).

            uint reservedBits = (uint)hr & _reservedBitMask;
            if (reservedBits != 0)
            {
                // Invalid HRESULT: `R` must be 0 if `N` is 0.
                return false;
            }

            uint reservedXBits = (uint)hr & _reserveXBitMask;
            if (reservedXBits != 0)
            {
                // The `X` bit should always be false. (Though "should" - implying it *COULD* be non-zero... but how should this function interpret that language in the spec?)
                return false;
            }

            return true;
        }

        public static bool TryGetWin32ErrorCode(this HResult hr, out Win32ErrorCodes win32Code)
        {
            // Set `win32Code` anyway, just in case the HRESULT's Customer and Facility codes are wrong:
            ushort codeBits = GetCode(hr);
            win32Code = (Win32ErrorCodes)codeBits;

            if (IsValidHResult(hr))
            {
                // But only return true or false if the flag bits are correct:
                if (GetCustomer(hr) == HResultCustomer.MicrosoftDefined)
                {
                    if (GetFacility(hr) == HResultFacility.Win32)
                    {
                        bool hresultSeverityMatchesWin32Code =
                            (GetSeverity(hr) == HResultSeverity.Success && win32Code == Win32ErrorCodes.Success)
                            ||
                            (GetSeverity(hr) == HResultSeverity.Failure && win32Code != Win32ErrorCodes.Success);

                        if (hresultSeverityMatchesWin32Code)
                        {
                            return true;
                        }
                    }
                    if (0 <= hr)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    internal enum HResultSeverity
    {
        Success = 0,
        Failure = 1
    }

    internal enum HResultCustomer
    {
        MicrosoftDefined = 0,
        CustomerDefined = 1
    }

    internal enum HResultFacility : ushort
    {
        /// <summary>FACILITY_NULL - The default facility code.</summary>
        Null = 0,

        /// <summary>FACILITY_RPC - The source of the error code is an RPC subsystem.</summary>
        Rpc = 1,

        /// <summary>FACILITY_WIN32 - This region is reserved to map undecorated error codes into HRESULTs.</summary>
        Win32 = 7,

        /// <summary>FACILITY_WINDOWS - The source of the error code is the Windows subsystem.</summary>
        Windows = 8,

        MAX_VALUE_INCLUSIVE = 2047,
        MAX_VALUE_EXCLUSIVE = 2048,
    }
}
