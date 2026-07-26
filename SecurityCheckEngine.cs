using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityMonitor
{
    // Motor para verificar todos os pré-requisitos de segurança.
    // Falha graciosamente se rodado sem privilégios de administrador.
    public static class SecurityCheckEngine
    {
        public static async Task<List<SecurityRequirement>> CheckAllRequirementsAsync()
        {
            var requirements = new List<SecurityRequirement>();

            requirements.Add(new SecurityRequirement { Title = "Windows 11 25H2 ou posterior", IsMet = CheckOSVersion() });
            requirements.Add(new SecurityRequirement { Title = "UEFI Secure Boot", IsMet = CheckSecureBoot() });
            requirements.Add(new SecurityRequirement { Title = "TPM 2.0", IsMet = CheckTpm20() });

            var (vbs, hvci) = CheckVbsAndHvci();
            requirements.Add(new SecurityRequirement { Title = "VBS", IsMet = vbs });
            requirements.Add(new SecurityRequirement { Title = "HVCI", IsMet = hvci });

            requirements.Add(new SecurityRequirement { Title = "IOMMU", IsMet = CheckIommu() });

            // Simula um delay para testes de interface (remova em produção)
            await Task.Delay(100);

            return requirements;
        }

        private static bool CheckOSVersion()
        {
            try { return Environment.OSVersion.Version.Build >= 22621; } // Exemplo: 22621 é Windows 11 22H2
            catch { return false; }
        }

        private static bool CheckSecureBoot()
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\SecureBoot\State");
            if (key != null)
            {
                var val = key.GetValue("UEFISecureBootEnabled");
                return val != null && (int)val == 1;
            }
            return false;
        }

        private static bool CheckTpm20()
        {
            try
            {
                // Requer Administrador. Falhará silenciosamente e retornará false caso o usuário não seja admin.
                ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2\Security\MicrosoftTpm");
                ObjectQuery query = new ObjectQuery("SELECT SpecVersion FROM Win32_Tpm");
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                foreach (ManagementObject obj in searcher.Get())
                {
                    string version = obj["SpecVersion"]?.ToString();
                    if (!string.IsNullOrEmpty(version) && version.StartsWith("2.0")) return true;
                }
            }
            catch { }
            return false;
        }

        private static (bool vbs, bool hvci) CheckVbsAndHvci()
        {
            bool vbs = false, hvci = false;
            try
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT VirtualizationBasedSecurityStatus, SecurityServicesRunning FROM Win32_DeviceGuard");
                foreach (ManagementObject obj in searcher.Get())
                {
                    vbs = Convert.ToInt32(obj["VirtualizationBasedSecurityStatus"]) == 2;
                    int[] services = obj["SecurityServicesRunning"] as int[];
                    if (services != null && services.Contains(1)) hvci = true; // 1 = HVCI
                }
            }
            catch { }
            return (vbs, hvci);
        }

        private static bool CheckIommu()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\DmaSecurity\Default");
                if (key != null)
                {
                    var val = key.GetValue("KernelDmaProtection");
                    return val != null && (int)val == 1;
                }
            }
            catch { }
            return false;
        }
    }
}
