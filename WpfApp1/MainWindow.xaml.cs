using System;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;

using System.IO;
using System.Configuration;

using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations;

//1)
//ADO.NET предоставляет собой технологию работы с данными, которая основана на платформе .NET Framework. 
//Эта технология представляет нам набор классов, через которые мы можем отправлять запросы к базам данных, устанавливать подключения, получать ответ от базы данных и производить ряд других операций.
//Существует 3 режима:
//1.Подключенный режим - подключение открывается и закрывается по завершению работы.
//2.Автономный режим - данные копируются клиенту, а подключение не остаётся открытым.
//3.С помощью технологии Entity Framework

//2) 3) 5) 7) 8)
//С помощью объекта SqlConnection происходит установка подключения к источнику данных. 
//Объект SqlCommand позволяет выполнять операции с данными из БД. 
//Объект SqlParameter представляет параметр для объекта SqlCommand
//Объект SqlDataReader считывает полученные в результате запроса данные. 
//Объект SqlDataSet предназначен для хранения данных из БД и позволяет работать с ними независимо от БД. 
//Объект SqlDataAdapter является посредником между DataSet и источником данных. 
//Главным образом, через эти объекты и будет идти работа с базой данных.

//4)
//SqlException это исключение, которое возникает, когда SQL Server возвращает предупреждение или ошибку.

//6)
//ExecuteScalar обычно используется, когда запрос возвращает одно значение. Если он возвращает больше, то результатом будет первый столбец первой строки.
//ExecuteReader используется для любого набора результатов с несколькими строками/столбцами.
//ExecuteNonQuery обычно используется для операторов SQL без результатов.

//9)
//Чтобы все операции с объектом SqlCommand выполнялись как одна транзакция, надо присвоить объект транзакции его свойству Transaction.
//Для завершения всех операции после их выполнения вызывается метод Commit() объекта SqlTransaction.
//Если в ходе выполнения произошла ошибка, то мы можем откатить транзакцию, вызвав метод Rollback().

//10)
//Объект DataSet предназначен для хранения данных из БД и позволяет работать с ними независимо от БД.
//Объект DataSet содержит таблицы, которые представлены типом DataTable. Таблица, в свою очередь, состоит из столбцов и строк. 
//Каждый столбец представляет объект DataColumn, а строка - объект DataRow. 
//Все данные строки хранятся в свойстве ItemArray, который представляет массив объектов - значений отдельных ячеек строки.
//В наборе DataSet с несколькими объектами DataTable можно использовать объекты DataRelation для связи таблиц друг с другом, 
//для перехода по таблицам, а также для возвращения дочерних или родительских строк из связанной таблицы.

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Database database;

        public MainWindow()
        {
            InitializeComponent();
            string configuratonStrig = ConfigurationManager.ConnectionStrings["DefaultFilesName"].ConnectionString;
            database = new Database(configuratonStrig);
            database.Check();
           
        }

        private void ButtonGetDateClick(object sender, RoutedEventArgs e)
        {

            Accounts ac = (Accounts)App.Current.Resources["accountResource"];
            ac.List = database.GetDate();
        }

        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            Accounts ac = (Accounts)App.Current.Resources["accountResource"];
            switch (comboBoxSort.SelectedIndex)
            {
                case 0:
                    ac.List= database.GetSortDate("order by o.Name ");
                    break;
                case 1:
                    ac.List = database.GetSortDate("order by o.Family ");
                    break;
                case 2:
                    ac.List = database.GetSortDate("order by a.Balance ");
                    break;
            }

        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            Accounts ac = (Accounts)App.Current.Resources["accountResource"];
           if( ValidationAccounts(ac))
            database.Save(ac.List);
        }
        private bool ValidationAccounts(Accounts ac)
        {
            bool ret = true;
            if (ac.List == null) return false;
            foreach (Account i in ac.List)
            {
                ret= ValidationAccount(i);
            }
            return ret;
        }


        private bool ValidationAccount(Account a)
        {
            bool ret = true;

            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new ValidationContext(a);
            if (!Validator.TryValidateObject(a, context, results, true))
            {
                foreach (var error in results)
                {
                    MessageBox.Show(error.ErrorMessage);
                    ret = false;
                }
            }
            return ret;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Accounts ac = (Accounts)App.Current.Resources["accountResource"];
            foreach (Account i in ac.List)
            {
                if (i.SimpleImage.Source == ((Image)sender).Source)
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        System.Drawing.Bitmap bt = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(@"C:\Users\Lenovo\Desktop\first.jpg");
                        bt.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                        memory.Position = 0;
                        BitmapImage bitmapimage = new BitmapImage();
                        bitmapimage.BeginInit();
                        bitmapimage.StreamSource = memory;
                        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapimage.EndInit();
                        i.SimpleImage.Source = bitmapimage;
                    }
                }
            }
        }

        private void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            Accounts ac = (Accounts)App.Current.Resources["accountResource"];
            System.Drawing.Bitmap bt = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(@"C:\Users\Lenovo\Desktop\audi.png");
            List<Account> list = new List<Account>();

            foreach (Account i in ac.List)
            {
                list.Add(i);
            }
            list.Add(new Account(1, "type", 0, DateTime.Now.ToString("d"), "Name", "Surname", "0123456789", bt));
            ac.List = list;
        }
    }

    

}
