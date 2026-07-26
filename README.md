# Security Status Monitor (WinUI 3)

Uma ferramenta moderna, leve e eficiente desenvolvida em **C# + WinUI 3** para monitorar em tempo real o status de recursos essenciais de segurança do Windows (Versão do SO, UEFI Secure Boot, TPM 2.0, VBS, HVCI e IOMMU).

---

## 🚀 Recursos Monitorados

1. **Versão do Sistema Operacional**: Validação da build e versão do Windows instalada.
2. **UEFI Secure Boot**: Status de inicialização segura do firmware UEFI.
3. **TPM 2.0**: Detecção do Trusted Platform Module (Versão 2.0 ou superior).
4. **VBS (Virtualization-based Security)**: Status do recurso de segurança baseado em virtualização.
5. **HVCI (Hypervisor-enforced Code Integrity)**: Integridade de código protegida por hipervisor (Integridade de Memória).
6. **IOMMU / Kernel DMA Protection**: Proteção contra ataques de acesso direto à memória (DMA).

---

## 🎨 Interface Gráfica
* Desenvolvido com **WinUI 3** e **Fluent Design**.
* Interface adaptativa baseada em cartões escuros com indicadores dinâmicos em tempo real:
  * 🟢 **Check Verde (`\uE73E`)**: Recurso ativo e operacional.
  * 🔴 **X Vermelho (`\uE711`)**: Recurso inativo ou desabilitado.

---

## 🛠️ Requisitos de Sistema e Desenvolvimento

* **Visual Studio 2026** (ou versão recente compatível).
* **Carga de trabalho instalada:** *Desenvolvimento para Computador Desktop com .NET* e o pacote de ferramentas do *WinUI 3*.
* **Privilégios:** O aplicativo requer execução como **Administrador** para conseguir consultar com precisão os dados de hardware profundos (como TPM e Device Guard) via WMI.

---

## 📦 Passo a Passo para Instalação e Execução

### 1. Criar o Projeto no Visual Studio
1. Abra o **Visual Studio**.
2. Selecione **Create a new project**.
3. Busque e escolha o template **Blank App, Packaged (WinUI 3 in Desktop)**.
4. Nomeie o projeto como `SecurityMonitor` e clique em *Create*.

### 2. Adicionar Dependência WMI
1. No *Solution Explorer* (Gerenciador de Soluções) à direita, clique com o botão direito no seu projeto e vá em **Manage NuGet Packages...**.
2. Vá na aba *Browse*, digite **`System.Management`** e instale a versão mais recente oficial da Microsoft.

### 3. Estrutura de Arquivos
Substitua ou crie os arquivos principais do seu projeto com os códigos correspondentes:

* **`SecurityRequirement.cs`**: Modelo de dados com notificação de alterações para a interface (`INotifyPropertyChanged`).
* **`SecurityCheckEngine.cs`**: Motor responsável por varrer o Registro do Windows e consultas WMI.
* **`MainWindow.xaml`**: Estrutura visual em XAML dos cartões de segurança.
* **`MainWindow.xaml.cs`**: Inicializador que aciona o motor de varredura assíncrona.

### 4. Executando o Projeto
1. Como algumas consultas (TPM e VBS) exigem permissões elevadas, abra o **Visual Studio como Administrador** caso queira ler o status real da sua máquina.
2. Na barra superior do Visual Studio, defina a configuração para **Debug** ou **Release**.
3. Pressione **`F5`** (ou clique no botão verde *Play*) para compilar e rodar a aplicação.
