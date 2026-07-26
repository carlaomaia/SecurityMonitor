using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecurityMonitor
{
    public sealed partial class MainWindow : Window
    {
        // Coleção observável que atualizará a interface automaticamente
        public ObservableCollection<SecurityRequirement> RequirementResults { get; } = new ObservableCollection<SecurityRequirement>();

        public MainWindow()
        {
            this.InitializeComponent();

            // Define o contexto de dados para que o ListView possa fazer o binding
            RequirementsList.ItemsSource = RequirementResults;

            // Inicia a verificação assim que a janela é carregada
            CarregarEVerificarTelemetria();
        }

        private async void CarregarEVerificarTelemetria()
        {
            // O motor assíncrono varre o sistema
            var results = await SecurityCheckEngine.CheckAllRequirementsAsync();

            // Limpa os dados de 'placeholder' e preenche com os reais
            RequirementResults.Clear();
            foreach (var req in results)
            {
                RequirementResults.Add(req);
            }
        }
    }
}