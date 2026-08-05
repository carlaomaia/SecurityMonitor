using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace SecurityMonitor
{
    public static class SecurityCheckEngine
    {
        public static async Task<List<SecurityRequirement>> CheckAllRequirementsAsync()
        {
            var requirements = new List<SecurityRequirement>();

            var osResult = CheckOSVersion();
            requirements.Add(new SecurityRequirement { Title = "Windows 11 25H2 ou posterior", Status = osResult.Status, Details = osResult.Details });

            var secureBootResult = CheckSecureBoot();
            requirements.Add(new SecurityRequirement { Title = "UEFI Secure Boot", Status = secureBootResult.Status, Details = secureBootResult.Details });

            var tpmResult = CheckTpm20();
            requirements.Add(new SecurityRequirement { Title = "TPM 2.0", Status = tpmResult.Status, Details = tpmResult.Details });

            var deviceGuardResult = CheckVbsAndHvci();
            requirements.Add(new SecurityRequirement
            {
                Title = "VBS",
                Status = deviceGuardResult.Status == SecurityCheckStatus.Error || deviceGuardResult.Status == SecurityCheckStatus.Unknown
                    ? deviceGuardResult.Status
                    : (deviceGuardResult.vbs ? SecurityCheckStatus.Active : SecurityCheckStatus.Inactive),
                Details = deviceGuardResult.Details
            });

            requirements.Add(new SecurityRequirement
            {
                Title = "HVCI",
                Status = deviceGuardResult.Status == SecurityCheckStatus.Error || deviceGuardResult.Status == SecurityCheckStatus.Unknown
                    ? deviceGuardResult.Status
                    : (deviceGuardResult.hvci ? SecurityCheckStatus.Active : SecurityCheckStatus.Inactive),
                Details = deviceGuardResult.Details
            });

            var iommuResult = CheckIommu();
            requirements.Add(new SecurityRequirement { Title = "IOMMU", Status = iommuResult.Status, Details = iommuResult.Details });

            await Task.Yield();
            return requirements;
        }

        private static (SecurityCheckStatus Status, string Details) CheckOSVersion()
        {
            try
            {
                var build = Environment.OSVersion.Version.Build;
                return build >= 22621
                    ? (SecurityCheckStatus.Active, $"Build {build} detectada no sistema atual.")
                    : (SecurityCheckStatus.Inactive, $"Build {build} detectada; o requisito Windows 11 25H2+ não foi atendido.");
            }
            catch (Exception ex)
            {
                return (SecurityCheckStatus.Error, $"Não foi possível determinar a versão do SO: {ex.Message}");
            }
        }

        private static (SecurityCheckStatus Status, string Details) CheckSecureBoot()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\SecureBoot\State");
                if (key != null)
                {
                    var val = key.GetValue("UEFISecureBootEnabled");
                    if (val is int enabled)
                    {
                        return enabled == 1
                            ? (SecurityCheckStatus.Active, "Secure Boot está habilitado.")
                            : (SecurityCheckStatus.Inactive, "Secure Boot está desabilitado.");
                    }
                }

                return (SecurityCheckStatus.Unknown, "Não foi possível localizar o estado do Secure Boot.");
            }
            catch (Exception ex)
            {
                return (SecurityCheckStatus.Error, $"Erro ao consultar Secure Boot: {ex.Message}");
            }
        }

        private static (SecurityCheckStatus Status, string Details) CheckTpm20()
        {
            try
            {
                var scope = new ManagementScope(@"\\.\root\CIMV2\Security\MicrosoftTpm");
                var query = new ObjectQuery("SELECT SpecVersion FROM Win32_Tpm");
                using var searcher = new ManagementObjectSearcher(scope, query);
                foreach (var obj in searcher.Get())
                {
                    var version = obj["SpecVersion"]?.ToString();
                    if (!string.IsNullOrEmpty(version) && version.StartsWith("2.0"))
                    {
                        return (SecurityCheckStatus.Active, $"TPM encontrado com versão {version}.");
                    }

                    if (!string.IsNullOrEmpty(version))
                    {
                        return (SecurityCheckStatus.Inactive, $"TPM encontrado, mas a versão {version} não atende ao requisito.");
                    }
                }

                return (SecurityCheckStatus.Unknown, "TPM não foi encontrado ou não está disponível nesta sessão.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return (SecurityCheckStatus.Error, $"Acesso negado ao TPM. Execute como administrador. {ex.Message}");
            }
            catch (ManagementException ex)
            {
                return (SecurityCheckStatus.Error, $"Erro WMI ao consultar TPM: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (SecurityCheckStatus.Error, $"Erro ao consultar TPM: {ex.Message}");
            }
        }

        private static (SecurityCheckStatus Status, string Details, bool vbs, bool hvci) CheckVbsAndHvci()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT VirtualizationBasedSecurityStatus, SecurityServicesRunning FROM Win32_DeviceGuard");
                foreach (var obj in searcher.Get())
                {
                    var vbsStatus = Convert.ToInt32(obj["VirtualizationBasedSecurityStatus"]);
                    var services = obj["SecurityServicesRunning"] as int[];
                    var vbs = vbsStatus == 2;
                    var hvci = services != null && services.Contains(1);

                    return (vbs ? SecurityCheckStatus.Active : SecurityCheckStatus.Inactive,
                        $"VBS {(vbs ? "ativo" : "inativo")} e HVCI {(hvci ? "ativo" : "inativo")}",
                        vbs,
                        hvci);
                }

                return (SecurityCheckStatus.Unknown, "Não foi possível localizar dados do Device Guard.", false, false);
            }
            catch (UnauthorizedAccessException ex)
            {
                return (SecurityCheckStatus.Error, $"Acesso negado ao consultar VBS/HVCI: {ex.Message}", false, false);
            }
            catch (ManagementException ex)
            {
                return (SecurityCheckStatus.Error, $"Erro WMI ao consultar VBS/HVCI: {ex.Message}", false, false);
            }
            catch (Exception ex)
            {
                return (SecurityCheckStatus.Error, $"Erro ao consultar VBS/HVCI: {ex.Message}", false, false);
            }
        }

        private static (SecurityCheckStatus Status, string Details) CheckIommu()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\DmaSecurity\Default");
                if (key != null)
                {
                    var val = key.GetValue("KernelDmaProtection");
                    if (val is int enabled)
                    {
                        return enabled == 1
                            ? (SecurityCheckStatus.Active, "Proteção DMA está habilitada.")
                            : (SecurityCheckStatus.Inactive, "Proteção DMA está desabilitada.");
                    }
                }

                return (SecurityCheckStatus.Unknown, "Não foi possível localizar o estado de IOMMU.");
            }
            catch (Exception ex)
            {
                return (SecurityCheckStatus.Error, $"Erro ao consultar IOMMU: {ex.Message}");
            }
        }
    }
}
