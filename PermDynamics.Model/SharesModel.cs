using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace PermDynamics.Model
{
    public static class SharesModel
    {
        private static string _connectionString = ConfigurationManager.ConnectionStrings["SharesConnection"].ConnectionString;

        public static bool CreateTables()
        {
            var result = false;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var userCreate = "if not exists (select name from sysobjects where name = 'Users') CREATE TABLE Users (Id int NOT NULL PRIMARY KEY, Name nvarchar(100) NOT NULL, Money decimal NOT NULL, ShareCount DECIMAL(20, 0) NOT NULL)";
                    var operationCreate = "if not exists (select name from sysobjects where name = 'Operation') CREATE TABLE Operation (Id int NOT NULL PRIMARY KEY, Count DECIMAL(20,0) NOT NULL, IsBuying BIT NOT NULL, Price decimal NOT NULL, UserId int NOT NULL, CONSTRAINT FK_Operation_Users FOREIGN KEY (UserId) References Users (Id) ON DELETE CASCADE ON UPDATE CASCADE)";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = userCreate;
                    var r1 = cmd.ExecuteNonQuery();
                    cmd = connection.CreateCommand();
                    cmd.CommandText = operationCreate;
                    var r2 = cmd.ExecuteNonQuery();
                    result = true;
                }
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }

        public static bool AddUser(User user)
        {
            var result = false;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var addUser = $"INSERT INTO Users (Id, Name, Money, ShareCount) VALUES ({user.Id}, '{user.Name}', {user.Money.ToString().Replace(',', '.')}, {user.ShareCount})";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = addUser;
                    cmd.ExecuteNonQuery();
                    result = true;
                }
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }

        public static bool AddOperation(Operation operation)
        {
            var result = false;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var addOperation = $"INSERT INTO Operation (Id, Count, IsBuying, Price, UserId) VALUES ({operation.Id}, {operation.Count}, {(operation.IsBuying?1:0)}, {operation.Price.ToString().Replace(',','.')}, {operation.UserId})";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = addOperation;
                    cmd.ExecuteNonQuery();
                    result = true;
                }
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }

        public static bool UpdateUser(User user)
        {
            var result = false;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var updateUser = $"UPDATE Users SET Name='{user.Name}', Money={user.Money.ToString().Replace(',', '.')}, ShareCount={user.ShareCount} WHERE Id={user.Id}";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = updateUser;
                    cmd.ExecuteNonQuery();
                    result = true;
                }
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }

        public static List<User> GetUsers()
        {
            var result = new List<User>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var getUsers = $"SELECT * FROM Users";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = getUsers;
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            var name = reader.GetString(1);
                            var money = reader.GetDecimal(2);
                            var shareCount = (ulong)reader.GetDecimal(3);
                            var user = new User{Id = id, Name = name, Money = money, ShareCount = shareCount};
                            result.Add(user);
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            return result;
        }

        public static List<Operation> GetOperations()
        {
            var result = new List<Operation>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var getOperations = $"SELECT * FROM Operation";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = getOperations;
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            var count = (ulong)reader.GetDecimal(1);
                            var isBuying = reader.GetBoolean(2);
                            var price = reader.GetDecimal(3);
                            var userId = reader.GetInt32(4);
                            var operation = new Operation(){Id = id, Count = count, IsBuying = isBuying, Price = price, UserId = userId};
                            result.Add(operation);
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            return result;
        }

        public static void DeleteTables()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var dropUser = $"DROP TABLE Users";
                    var dropOperation = "DROP TABLE Operation";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = dropOperation;
                    cmd.ExecuteNonQuery();
                    cmd = connection.CreateCommand();
                    cmd.CommandText = dropUser;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}