using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.IO;
using System.Configuration;

namespace WpfApp1
{
    class Database
    {
        string connectionString;
        public Database(string con)
        {
            connectionString = con;
        }


        public List<Account> GetDate()
        {
            List<Account> list = new List<Account>();
            string sql = "select* from Account a inner join OwnerTable o on a.Owner = o.Passport";
            list=Select(sql);
            return list;
        }

        public List<Account> Select(string sql)
        {
            SqlConnection connection = null;
            SqlDataAdapter adapter;
            List<Account> list = new List<Account>();
            try
            {
                connection = new SqlConnection(connectionString); //С помощью объекта Connection происходит установка подключения к источнику данных. 
                adapter = new SqlDataAdapter(sql, connection);  //Объект DataAdapter является посредником между DataSet и источником данных. 
                DataTable dataTable = new DataTable(); //DataTable - таблица
                connection.Open();
                adapter.Fill(dataTable);
                for (int index = 0; index < dataTable.Rows.Count; index++)
                {
                    DataRow promoRow = dataTable.Rows[index]; //DataRow - строка таблицы



                    System.Drawing.Bitmap BT = null;
                   MemoryStream memoryStream = new MemoryStream();
                    if ((promoRow.Field<byte[]>("Picture")) != null)
                    {
                        memoryStream.Write(promoRow.Field<byte[]>("Picture"), 0, (promoRow.Field<byte[]>("Picture")).Length);

                        BT = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
                      
                    }



                    Account acc = new Account(promoRow.Field<int>("Number"), promoRow.Field<string>("Type"), promoRow.Field<double>("Balance"), promoRow.Field<DateTime>("Date").ToString("d"), promoRow.Field<string>("Name"), promoRow.Field<string>("Family"), promoRow.Field<string>("Passport"), BT);
                    list.Add(acc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
            return list;
        }

        public List<Account> GetSortDate(string sortStr)
        {
            List<Account> list = new List<Account>();
            string sql = "select* from Account a inner join OwnerTable o on a.Owner = o.Passport " + sortStr;
            list = Select(sql);
            return list;
        }

        public void Save(List<Account> list)
        {
            Delete();
            foreach (Account i in list)
            {
                InsertOwner(i);
                InsertAccount(i);
            }
        }
         public void InsertAccount(Account elem)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlExpression = "insert into Account(Number,Type,Balance,Date,Owner) values(" + elem.Number + "," + "\'" + elem.Type + "\', " + elem.Balance + "," + "\'" + elem.Date + "\', \'" + elem.Passport + "\')";
                    SqlCommand command = new SqlCommand(sqlExpression, connection);  //SqlCommand
                    command.ExecuteNonQuery(); //ExecuteNonQuery операторов без результатов
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Delete()
        {
            string sqlExpression = "delete Account  Delete OwnerTable";
            Query(sqlExpression);
        }
         public void InsertOwner(Account elem)
         {
             using (SqlConnection connection = new SqlConnection(connectionString))
             {
                 connection.Open();
                 SqlCommand command = new SqlCommand("AddOwner", connection);
                 command.CommandType = CommandType.StoredProcedure;
                 command.Parameters.AddWithValue("name", elem.Name);
                 command.Parameters.AddWithValue("family", elem.Family);
                 command.Parameters.AddWithValue("passport", elem.Passport);
                 command.ExecuteNonQuery();
            }


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand sqlCommand2 = new SqlCommand("update  OwnerTable set Picture  = @img  where passport=" + "\'" + elem.Passport + "\'", connection);
                SqlParameter sqlParameter = new SqlParameter("@img", SqlDbType.VarBinary); //SqlParameter
            

                MemoryStream memoryStream = new MemoryStream();

                    Bitmap bt = BitmapImage2Bitmap(((System.Windows.Media.Imaging.BitmapImage)elem.SimpleImage.Source));
                    bt.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

                    sqlParameter.Value = memoryStream.ToArray();
                    sqlCommand2.Parameters.Add(sqlParameter); //используем SqlParameter
                sqlCommand2.ExecuteNonQuery();
                    memoryStream.Dispose();
            }
        }

        private Bitmap BitmapImage2Bitmap(System.Windows.Media.Imaging.BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                System.Windows.Media.Imaging.BitmapEncoder enc = new System.Windows.Media.Imaging.BmpBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        public bool Query(string sqlExpression)
        {
            bool ret = false;
            string connect = ConfigurationManager.ConnectionStrings["DefaultFilesName"].ConnectionString; ;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader(); //SqlDataReader
                ret = reader.FieldCount > 0;
                reader.Close();
            }
            return ret;
        }
        public bool Query1(string sqlExpression)
        {
            bool ret = false;
            string connect = ConfigurationManager.ConnectionStrings["MasterConnection"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();
                ret = reader.FieldCount > 0;
                reader.Close();
            }
            return ret;
        }
        public void Check()
        {
            if (Query1("if db_id('OOP7') is not null select 1 ")) return;

            string sqlExpression = @"CREATE DATABASE [OOP7]";
            Query1(sqlExpression);
            Query("USE OOP7; CREATE TABLE [dbo].[OwnerTable]( [Name] [nvarchar](50) NOT NULL, [Family] [nvarchar](50) NOT NULL, [Passport] [char](10) NOT NULL, [Picture] [image] NULL, CONSTRAINT [PK_OwnerTable_1] PRIMARY KEY CLUSTERED ( [Passport] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] SET ANSI_PADDING OFF");
            Query("USE OOP7; CREATE TABLE [dbo].[Account]( [Number] [int] NOT NULL, [Type] [nvarchar](50) NOT NULL, [Balance] [float] NOT NULL, [Date] [date] NOT NULL, [Owner] [char](10) NOT NULL, CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED ( [Number] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY]  ALTER TABLE [dbo].[Account] WITH CHECK ADD CONSTRAINT [FK_Account_OwnerTable] FOREIGN KEY([Owner]) REFERENCES [dbo].[OwnerTable] ([Passport])  ALTER TABLE [dbo].[Account] CHECK CONSTRAINT [FK_Account_OwnerTable]");
            Query("USE OOP7; CREATE TABLE [dbo].[FilesName]( [id] [int] IDENTITY(1,1) NOT NULL, [FileName] [nchar](50) NOT NULL, [DateTime] [datetime] NOT NULL, CONSTRAINT [PK_FilesName] PRIMARY KEY CLUSTERED ( [id] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY]  ALTER TABLE [dbo].[FilesName] ADD  CONSTRAINT [DF_FilesName_DateTime]  DEFAULT (getdate()) FOR [DateTime]");
            Query("CREATE PROCEDURE [dbo].[AddOwner] @name nvarchar(50), @family nvarchar(50), @passport char(10) AS DECLARE @c int if(select count(*) from OwnerTable t where t.Passport=@passport)=0 INSERT INTO OwnerTable(Name,Family,Passport) VALUES (@name, @family,@passport) ");
        }


    }
}
