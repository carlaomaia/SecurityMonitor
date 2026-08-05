using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SecurityMonitor
{
    public enum SecurityCheckStatus
    {
        Active,
        Inactive,
        Unknown,
        Error
    }

    public class SecurityRequirement : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private SecurityCheckStatus _status = SecurityCheckStatus.Unknown;
        private string _details = string.Empty;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public SecurityCheckStatus Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMet)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconGlyph)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconBrush)));
                }
            }
        }

        public string Details
        {
            get => _details;
            set => SetProperty(ref _details, value);
        }

        public bool IsMet => Status == SecurityCheckStatus.Active;

        public string StatusText => Status switch
        {
            SecurityCheckStatus.Active => "Ativo",
            SecurityCheckStatus.Inactive => "Inativo",
            SecurityCheckStatus.Error => "Erro na verificação",
            _ => "Indeterminado"
        };

        public string IconGlyph => Status switch
        {
            SecurityCheckStatus.Active => "\uE73E",
            SecurityCheckStatus.Inactive => "\uE711",
            SecurityCheckStatus.Error => "\uE783",
            _ => "\uE9CE"
        };

        public SolidColorBrush IconBrush => new SolidColorBrush(
            Status switch
            {
                SecurityCheckStatus.Active => ColorHelper.FromArgb(255, 76, 175, 80),
                SecurityCheckStatus.Inactive => ColorHelper.FromArgb(255, 244, 67, 54),
                SecurityCheckStatus.Error => ColorHelper.FromArgb(255, 255, 152, 0),
                _ => ColorHelper.FromArgb(255, 158, 158, 158)
            });

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