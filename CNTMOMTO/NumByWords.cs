
    using System;
    using Intermech;
    using Intermech.DataFormats;
    using Intermech.Interfaces;
    using Intermech.Interfaces.Plugins;
    using Intermech.Interfaces.Client;
    using Intermech.Interfaces.Configuration;
    using Intermech.Kernel.Search;
    using Intermech.Navigator;

    namespace CNTMOMTO
    {
    /// <summary>
    /// Класс, который позволяет создать новый объект
    /// </summary>
    public class NumByWords
    {

        public string RurPhrase(decimal money)
        {
            return CurPhrase(money, "рубль", "рубля", "рублей", true, "копейка", "копейки", "копеек", false);
        }

        public string UsdPhrase(decimal money)
        {
            return CurPhrase(money, "доллар США", "доллара США", "долларов США", true, "цент", "цента", "центов", true);
        }

        public string NumPhrase(ulong Value, bool IsMale)
        {
            if (Value == 0UL) return "Ноль";
            string[] Dek1 = { "", " од", " дв", " три", " четыре", " пять", " шесть", " семь", " восемь", " девять", " десять", " одиннадцать", " двенадцать", " тринадцать", " четырнадцать", " пятнадцать", " шестнадцать", " семнадцать", " восемнадцать", " девятнадцать" };
            string[] Dek2 = { "", "", " двадцать", " тридцать", " сорок", " пятьдесят", " шестьдесят", " семьдесят", " восемьдесят", " девяносто" };
            string[] Dek3 = { "", " сто", " двести", " триста", " четыреста", " пятьсот", " шестьсот", " семьсот", " восемьсот", " девятьсот" };
            string[] Th = { "", "", " тысяч", " миллион", " миллиард", " триллион", " квадрилион", " квинтилион" };
            string str = "";
            for (byte th = 1; Value > 0; th++)
            {
                ushort gr = (ushort)(Value % 1000);
                Value = (Value - gr) / 1000;
                if (gr > 0)
                {
                    byte d3 = (byte)((gr - gr % 100) / 100);
                    byte d1 = (byte)(gr % 10);
                    byte d2 = (byte)((gr - d3 * 100 - d1) / 10);
                    if (d2 == 1) d1 += (byte)10;
                    bool ismale = (th > 2) || ((th == 1) && IsMale);
                    // str = Dek3[d3] + Dek2[d2] + Dek1[d1] + EndDek1(d1, ismale) + Th[th] + EndTh(th, d1) + str;
                    str = Dek3[d3] + Dek2[d2] + Dek1[d1] + EndDek1(d1, ismale) + Th[th] + EndTh(th, d1) + str;
                };
            };
            str = str.Substring(1, 1).ToUpper() + str.Substring(2);
            return str;
        }

        private string CurPhrase
            (
            decimal money,
            string word1, string word234, string wordmore, bool IsMale,
            string sword1, string sword234, string swordmore, bool sIsMale
            )
        {
            money = decimal.Round(money, 2);
            decimal decintpart = decimal.Truncate(money);
            ulong intpart = decimal.ToUInt64(decintpart);
            string str = NumPhrase(intpart, IsMale) + " ";
            byte endpart = (byte)(intpart % 100UL);
            if (endpart > 19) endpart = (byte)(endpart % 10);
            byte fracpart = decimal.ToByte((money - decintpart) * 100M);
            //  str += "и " + ((fracpart < 10) ? "0" : "") + fracpart.ToString() + "/100 ";

            switch (endpart)
            {
                case 1: str += word1; break;
                case 2:
                case 3:
                case 4: str += word234; break;
                default: str += wordmore; break;
            }
            str += " и " + fracpart.ToString() + " коп.";
            return str;
        }

        private static string EndTh(byte ThNum, byte Dek)
        {
            bool In234 = ((Dek >= 2) && (Dek <= 4));
            bool More4 = ((Dek > 4) || (Dek == 0));
            if (((ThNum > 2) && In234) || ((ThNum == 2) && (Dek == 1))) return "а";
            else if ((ThNum > 2) && More4) return "ов";
            else if ((ThNum == 2) && In234) return "и";
            else return "";
        }

        private static string EndDek1(byte Dek, bool IsMale)
        {
            if ((Dek > 2) || (Dek == 0)) return "";
            else if (Dek == 1)
            {
                if (IsMale) return "ин";
                else return "на";
            }
            else
            {
                if (IsMale) return "а";
                else return "е";
            }
        }

    }
}

