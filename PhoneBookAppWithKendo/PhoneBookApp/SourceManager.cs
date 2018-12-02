using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using System.Web;
using PhoneBookApp.Models;

namespace PhoneBookApp
{
    public static class SourceManager
    {
        public static SqlConnection connection = null;
        public static void Open()
        {
            if (connection == null)
            {
                try
                {
                    connection = new SqlConnection();
                    connection.ConnectionString = "Integrated Security=SSPI;" +
                                                  "Initial Catalog=PhoneBook;" +
                                                  "Data Source=.\\SQLEXPRESS;";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                throw new Exception("Połączenie jest otwarte");
            }

        }

        public static void Close()
        {
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
            else
            {
                throw new Exception("Nie można zamknąć");
            }
        }
        
        public static List<PersonModel> Get(string search, int page)
        {
            int start = 0;
            int take = 0;
            if (page == 1)
            {
                start = 0;
                take = 10;
            }
            else if (page != 1)
            {
                start = page * 10 - 9;
                take = 10;
            }
            List<PersonModel> people = new List<PersonModel>();
            Open();
            
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = "SELECT * FROM [PhoneBook].[dbo].[People] WHERE [LastName] LIKE @search "+
                                     "ORDER BY ID " +
                                    "OFFSET @start ROWS FETCH NEXT @take ROWS ONLY";
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Connection = connection;

            SqlParameter parameter = new SqlParameter()
            {
                ParameterName = "@search",
                Value = $"{search}%",
                DbType = DbType.String,
            };
            SqlParameter parameter1 = new SqlParameter()
            {
                ParameterName = "@start",
                Value = $"{start}",
                DbType = DbType.Int32,
            };
            SqlParameter parameter2 = new SqlParameter()
            {
                ParameterName = "@take",
                Value = $"{take}",
                DbType = DbType.Int32,
            };
            sqlCommand.Parameters.Add(parameter);
            sqlCommand.Parameters.Add(parameter1);
            sqlCommand.Parameters.Add(parameter2);
            connection.Open();
            try
            {
                SqlDataReader sqlReader = sqlCommand.ExecuteReader();
                while (sqlReader.Read())
                {
                    people.Add(new PersonModel(Convert.ToInt32(sqlReader.GetValue(0)),
                        Convert.ToString(sqlReader.GetValue(1)),
                        Convert.ToString(sqlReader.GetValue(2)),
                        Convert.ToInt32(sqlReader.GetValue(3)),
                        Convert.ToString(sqlReader.GetValue(4)),
                        Convert.ToDateTime(sqlReader.GetValue(5)),
                        Convert.ToDateTime(sqlReader.GetValue(6) as DateTime?)
                    ));

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            Close();
            return people;
        }
        
        public static void Add(PersonModel personModel)
        {
            Open();
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            SqlCommand cmd = new SqlCommand()
            {
                CommandText = "INSERT INTO [PhoneBook].[dbo].[People] ( [Name], [LastName], [Phone], [Email], [Created], [Updated] ) " +
                              "VALUES ( @Name, @LastName, @Phone, @Email, @Created, @Updated ); " +
                              "SELECT SCOPE_IDENTITY(); ",
                CommandType = CommandType.Text,
                Connection = connection,
                Transaction = transaction
            };
            SqlParameter Name = new SqlParameter()
            {
                ParameterName = "@Name",
                Value = personModel.Name,
                DbType = DbType.String
            };
            SqlParameter LastName = new SqlParameter()
            {
                ParameterName = "@LastName",
                Value = personModel.LastName,
                DbType = DbType.String
            };
            SqlParameter Phone = new SqlParameter()
            {
                ParameterName = "@Phone",
                Value = personModel.Phone,
                DbType = DbType.Int32
            };
            SqlParameter Email = new SqlParameter()
            {
                ParameterName = "@Email",
                Value = personModel.Email,
                DbType = DbType.String
            };
            SqlParameter Created = new SqlParameter()
            {
                ParameterName = "@Created",
                Value = DateTime.Now,
                DbType = DbType.DateTime
            };
            SqlParameter Updated = new SqlParameter()
            {
                ParameterName = "@Updated",
                Value = (object)personModel.Updated ?? DBNull.Value,
                DbType = DbType.DateTime
            };

            cmd.Parameters.Add(Name);
            cmd.Parameters.Add(LastName);
            cmd.Parameters.Add(Phone);
            cmd.Parameters.Add(Email);
            cmd.Parameters.Add(Created);
            cmd.Parameters.Add(Updated);
            cmd.ExecuteNonQuery();
            transaction.Commit();
            Close();
        }
        public static PersonModel GetById(int ID)
        {

            PersonModel people = new PersonModel();
            Open();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT * FROM[PhoneBook].[dbo].[People] " +
                                     "WHERE ID = @ID";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            SqlParameter parameter = new SqlParameter()
            {
                ParameterName = "@ID",
                Value = ID,
                DbType = DbType.Int32,
                Direction = ParameterDirection.Input,
            };
            cmd.Parameters.Add(parameter);
            connection.Open();
            try
            {
                SqlDataReader sqlReader = cmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    people = new PersonModel(Convert.ToInt32(sqlReader.GetValue(0)),
                        Convert.ToString(sqlReader.GetValue(1)),
                        Convert.ToString(sqlReader.GetValue(2)),
                        Convert.ToInt32(sqlReader.GetValue(3)),
                        Convert.ToString(sqlReader.GetValue(4)),
                        Convert.ToDateTime(sqlReader.GetValue(5)),
                        Convert.ToDateTime(sqlReader.GetValue(6) as DateTime?)
                    );

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            

            Close();
            return people;
        }
        public static void Remove(int ID)
        {
            Open();
            connection.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "DELETE FROM [PhoneBook].[dbo].[People] " +
                              "WHERE ID = @ID";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            try
            {
                SqlParameter parameter = new SqlParameter()
                {
                    ParameterName = "@ID",
                    Value = ID,
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Input,
                };
                cmd.Parameters.Add(parameter);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            Close();
        }

        public static void Edit(PersonModel personModel)
        {
            Open();
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            SqlCommand cmd = new SqlCommand()
            {
                CommandText = "UPDATE [PhoneBook].[dbo].[People] " +
                              "SET [Name] = @Name, [LastName] = @LastName, [Phone] = @Phone, [Email] = @Email, [Updated] = @Updated " +
                              "WHERE [ID] = @ID;",
                CommandType = CommandType.Text,
                Connection = connection,
                Transaction = transaction
            };
            SqlParameter ID = new SqlParameter()
            {
                ParameterName = "@ID",
                Value = personModel.ID,
                DbType = DbType.Int32
            };
            SqlParameter Name = new SqlParameter()
            {
                ParameterName = "@Name",
                Value = personModel.Name,
                DbType = DbType.String
            };
            SqlParameter LastName = new SqlParameter()
            {
                ParameterName = "@LastName",
                Value = personModel.LastName,
                DbType = DbType.String
            };
            SqlParameter Phone = new SqlParameter()
            {
                ParameterName = "@Phone",
                Value = personModel.Phone,
                DbType = DbType.Int32
            };
            SqlParameter Email = new SqlParameter()
            {
                ParameterName = "@Email",
                Value = personModel.Email,
                DbType = DbType.String
            };
            SqlParameter Updated = new SqlParameter()
            {
                ParameterName = "@Updated",
                Value = DateTime.Now,
                DbType = DbType.DateTime
            };
            cmd.Parameters.Add(ID);
            cmd.Parameters.Add(Name);
            cmd.Parameters.Add(LastName);
            cmd.Parameters.Add(Phone);
            cmd.Parameters.Add(Email);
            cmd.Parameters.Add(Updated);
            cmd.ExecuteNonQuery();
            transaction.Commit();
            Close();
        }
    }
}