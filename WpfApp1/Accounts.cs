
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

using System.Windows.Controls;

using System.Windows.Media.Imaging;



namespace WpfApp1
{



    class Accounts : INotifyPropertyChanged
    {
        public List<Account> list;
        public List<Account> List
        {
            get { return list; }
            set
            {
                list = value;
                OnPropertyChanged("List");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }



    public class Account
    {
        [Required]
        [RegularExpression("[0-9]+", ErrorMessage = "Номер счета пользователя указан неверно")]
        public int Number { get; set; }
        [Required(ErrorMessage = "Тип счета пользователя не указан", AllowEmptyStrings = false)]
        public string Type { get; set; }
        [Required]
        [RegularExpression("[0-9]+", ErrorMessage = "Баланс пользователя указан неверно")]
        public double Balance { get; set; }
        [Required]
        [NameFamilyVal(ErrorMessage = "Неправильно указано имя")]
        public string Name { get; set; }
        [Required]
        [NameFamilyVal(ErrorMessage = "Неправильно указана фамилия")]
        public string Family { get; set; }
        [Required]
        [PasportVal]
        public string Passport { get; set; }
        [Required]
        [RegularExpression(@"^(0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.](19|20)\d\d$", ErrorMessage = "Дата указана неверно")]
        public string Date { get; set; }
        public Image SimpleImage { get; set; }


        public Account()
        {

        }
        public Account(int number, string type, double balance, string date, string name, string family, string passport, System.Drawing.Bitmap BT)
        {
            this.Number = number;
            this.Type = type;
            this.Balance = balance;
            this.Date = date;
            this.Name = name;
            this.Family = family;
            this.Passport = passport;
            SimpleImage = new Image();
            if (BT != null)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    BT.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();
                    SimpleImage.Source = bitmapimage;
                }
            }


        }
    }




    public class PasportValAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value != null)
            {
                if (((string)value).Length != 10) return false;
                string Value = value.ToString();
                Regex regex = new Regex("[0-9A-Z]");
                if (regex.IsMatch(Value))
                    return true;
                else
                    this.ErrorMessage = "Неверно указан номер паспорта";
            }
            return false;
        }
    }
    public class NameFamilyValAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value != null)
            {
                string Value = value.ToString();
                Regex regex = new Regex("^[a-zA-Z]{1,20}$");
                if (regex.IsMatch(Value))
                    return true;

            }
            return false;
        }
    }

}