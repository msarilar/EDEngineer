using System.ComponentModel;

namespace EDEngineer.Models
{
    public interface ILanguage
    {
        string Translate(string text);
        event PropertyChangedEventHandler PropertyChanged;
    }
}