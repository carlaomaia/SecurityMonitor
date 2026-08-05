using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SecurityMonitor
{
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<SecurityRequirement> RequirementResults { get; } = new ObservableCollection<SecurityRequirement>();

        public MainWindow()
        {
            this.InitializeComponent();
            RequirementsList.ItemsSource = RequirementResults;
            RefreshButton.Click += async (_, __) => await CarregarEVerificarTelemetriaAsync();
            _ = CarregarEVerificarTelemetriaAsync();
        }

        private async Task CarregarEVerificarTelemetriaAsync()
        {
            RequirementResults.Clear();
            RequirementResults.Add(new SecurityRequirement
            {
                Title = "Verificando recursos",
                Status = SecurityCheckStatus.Unknown,
                Details = "Aguarde enquanto os dados são consultados no sistema."
            });

            try
            {
                var results = await SecurityCheckEngine.CheckAllRequirementsAsync();
                RequirementResults.Clear();
                foreach (var req in results)
                {
                    RequirementResults.Add(req);
                }
            }
            catch (Exception ex)
            {
                RequirementResults.Clear();
                RequirementResults.Add(new SecurityRequirement
                {
                    Title = "Falha ao verificar",
                    Status = SecurityCheckStatus.Error,
                    Details = ex.Message
                });
            }
        }
    }
}