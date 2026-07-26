using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SecurityMonitor
{
    public class SecurityRequirement : INotifyPropertyChanged
    {
        private string _title;
        private bool _isMet;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsMet
        {
            get => _isMet;
            set
            {
                if (SetProperty(ref _isMet, value))
                {
                    // Se o status mudar, avisa a interface para atualizar o ícone e a cor
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconGlyph)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconBrush)));
                }
            }
        }

        // LÓGICA DO ÍCONE:
        // Retorna o código do "Check" (E73E) se verdadeiro, ou o "X" (E711) se falso.
        public string IconGlyph => IsMet ? "\xE73E" : "\xE711";

        // LÓGICA DA COR:
        // Retorna Verde (#4CAF50) se verdadeiro, Vermelho (#F44336) se falso.
        public SolidColorBrush IconBrush => new SolidColorBrush(
            IsMet
            ? ColorHelper.FromArgb(255, 76, 175, 80)
            : ColorHelper.FromArgb(255, 244, 67, 54));

        // --- Implementação da Interface INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }
    }
}