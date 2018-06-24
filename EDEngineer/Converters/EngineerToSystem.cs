using System;
using System.Globalization;
using System.Windows.Data;
using EDEngineer.Localization;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class EngineerToSystem : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var engineer = (string) value;

            switch (engineer)
            {
                case "Zacariah Nemo": return "Yoru (Yoru 4, Nemo Cyber Party Base)";
                case "Bill Turner": return "Alioth (Alioth 4 a, Turner Metallics Inc)";
                case "Ram Tah": return "Meene (Meene AB 5 d, Phoenix Base)";
                case "Lori Jameson": return "Shinrarta Dezhra, (Shinrarta Dezhra A 1, Jameson Base)";
                case "Liz Ryder": return "Eurybia (Makalu, Demolition Unlimited)";
                case "Selene Jean": return "Kuk (Kuk B 3, Prospector's Rest)";
                case "Tod McQuinn": return "Wolf 397 (Trus Madi, Trophy Camp)";
                case "The Sarge": return "Beta-3 Tucani (Beta-3 Tucani 2 b a, The Beach)";
                case "Felicity Farseer": return "Deciat (Deciat 6 A, Farseer Inc)";
                case "Elvira Martuuk": return "Khun (Khun 5, Long Sight Base)";
                case "Professor Palin": return "Maia (Maia A 3 a, Palin Research Centre)";
                case "Juri Ishmaak": return "Giryak (Giryak 2 a, Pater's Memorial)";
                case "Marco Qwent": return "Sirius (Lucifer, Qwent Research Base)";
                case "Hera Tani": return "Kuwemaki (Kuwemaki A 3 A, The Jet's Hole)";
                case "Colonel Bris Dekker": return "Sol (Iapetus, Dekker's Yard)";
                case "Tiana Fortune": return "Achenar (Achenar 4A, Fortune's Loss)";
                case "Lei Cheung": return "Laksak (Laksak A 1 , Trader's Rest)";
                case "Didi Vatermann": return "Leesti (Leesti 1 A, Leesti)";
                case "The Dweller": return "Wyrd (Wyrd A 2, Black Hide)";
                case "Broo Tarquin": return "Muang (Muang 5 a, Broo's Legacy)";
                default: return "N/A";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}