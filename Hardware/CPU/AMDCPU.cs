/*
  
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
  
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
 	
*/

namespace OpenHardwareMonitor.Hardware.CPU {

  internal abstract class AMDCPU : GenericCPU {

    private const byte PCI_BUS = 0;
    private const byte PCI_BASE_DEVICE = 0x18;
    private const byte DEVICE_VENDOR_ID_REGISTER = 0;
    private const ushort AMD_VENDOR_ID = 0x1022;

    public AMDCPU(int processorIndex, CPUID[][] cpuid, ISettings settings)
      : base(processorIndex, cpuid, settings) { }

    protected uint GetPciAddress(byte function, ushort deviceId) {
      
      // assemble the pci address
      uint address = Ring0.GetPciAddress(PCI_BUS,
        (byte)(PCI_BASE_DEVICE + processorIndex), function);

      // verify that we have the correct bus, device and function
      uint deviceVendor;
      if (!Ring0.ReadPciConfig(
        address, DEVICE_VENDOR_ID_REGISTER, out deviceVendor))
        return Ring0.InvalidPciAddress;
      
      if (deviceVendor != (deviceId << 16 | AMD_VENDOR_ID))
        return Ring0.InvalidPciAddress;

      return address;
    }

    // 추가된 코드: AMD Ryzen 9 9600X의 온도를 읽어오는 메서드
    protected float ReadTemperature() {
      // 예시 코드, 실제 온도 읽기 로직을 구현해야 함
      uint address = GetPciAddress(0, 0x1437); // AMD Family 19h Model 21h (Zen 3) CPU
      if (address == Ring0.InvalidPciAddress)
        return float.NaN;

      uint value;
      if (!Ring0.ReadPciConfig(address, 0xA4, out value)) // Temperature Register
        return float.NaN;

      // 온도를 계산하는 로직
      float temperature = ((value >> 21) & 0x7FF) / 8.0f;
      return temperature;
    }
  }
}
