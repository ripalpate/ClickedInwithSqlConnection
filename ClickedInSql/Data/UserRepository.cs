﻿using ClickedInSql.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ClickedInSql.Data
{
    public class UserRepository
    {
        const string ConnectionString = "Server = localhost; Database = ClinckedIn; Trusted_Connection = True;";

        public User AddUser(string name, DateTime releaseDate, int age, bool isPrisoner)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var insertUserCommand = connection.CreateCommand();
                insertUserCommand.CommandText = @"Insert into users (name, releaseDate, age, isPrisoner)
                                              Output inserted.*
                                              Values(@name, @releaseDate, @age, @isPrisoner )";
                insertUserCommand.Parameters.AddWithValue("name", name);
                insertUserCommand.Parameters.AddWithValue("releaseDate", releaseDate);
                insertUserCommand.Parameters.AddWithValue("age", age);
                insertUserCommand.Parameters.AddWithValue("isPrisoner", isPrisoner);

                var reader = insertUserCommand.ExecuteReader();

                if (reader.Read())
                {
                    var insertedName = reader["name"].ToString();
                    var insertedReleaseDate = (DateTime)reader["releaseDate"];
                    var insertedAge = (int)reader["age"];
                    var insertedIsPrisoner = (bool)reader["isPrisoner"];
                    var insertedId = (int)reader["id"];
                    var newUser = new User(insertedName, insertedReleaseDate, insertedAge, insertedIsPrisoner) { Id = insertedId };

                    connection.Close();

                    return newUser;
                }
            }
            throw new Exception("No user found");
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var getAllUsersCommand = connection.CreateCommand();
            getAllUsersCommand.CommandText = "select * from users";

            var reader = getAllUsersCommand.ExecuteReader();

            while (reader.Read())
            {
                var id = (int)reader["Id"];
                var name = reader["Name"].ToString();
                var releaseDate = (DateTime)reader["ReleaseDate"];
                var age = (int)reader["Age"];
                var isPrisoner = (bool)reader["IsPrisoner"];

                var user = new User(name, releaseDate, age, isPrisoner) { Id = id };

                users.Add(user);
            }

            connection.Close();

            foreach (User user in users)
            {
                user.Services = (GetServices(user.Id));
            }

            foreach (User user in users)
            {
                user.Interests = (GetInterests(user.Id));
            }

            return users;
        }


        public List<string> GetInterests(int userId)
        {
            var interests = new List<string>();
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            var getUserWithInterestsCommand = connection.CreateCommand();
            getUserWithInterestsCommand.CommandText = @"Select i.Name InterestName
                                                        From Interests i
														Join UsersInterests ui
														On ui.InterestId = i.Id
                                                        Where ui.UserId = @userId"; 

            getUserWithInterestsCommand.Parameters.AddWithValue("@userId", userId);
            var reader = getUserWithInterestsCommand.ExecuteReader();

            while (reader.Read())
            {
                var interest = reader["InterestName"].ToString();

                interests.Add(interest);
            }

            connection.Close();

            return interests;
        }

       public List<string> GetServices(int userId)
        {
            var services = new List<string>();

            var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var getUserWithServicesCommand = connection.CreateCommand();
            getUserWithServicesCommand.Parameters.AddWithValue("@userId", userId);
            getUserWithServicesCommand.CommandText = @"Select s.Name ServiceName
                                                        From Services s
														Join UsersService us
														On us.ServiceId = s.Id
                                                        Where us.UserId = @userId";

            var reader = getUserWithServicesCommand.ExecuteReader();

            while (reader.Read())
            {
                var service = reader["ServiceName"].ToString();

                services.Add(service);
            }

            connection.Close();

            return services;
        }

        public void DeleteUser(int id)
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var deleteUserCommand = connection.CreateCommand();
            deleteUserCommand.CommandText = @"Delete
                                               From Users
                                               Where id = @id";
            deleteUserCommand.Parameters.AddWithValue("id", id);
            var reader = deleteUserCommand.ExecuteNonQuery();

            connection.Close();
        }

        public bool UpdateUser(int id, string name, DateTime releaseDate, int age, bool isPrisoner)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var updateUserCommand = connection.CreateCommand();
                updateUserCommand.Parameters.AddWithValue("@id", id);
                updateUserCommand.CommandText = @"Update Users
                                                  Set Name = @name,
                                                  ReleaseDate = @releaseDate,
                                                  Age = @age,
                                                  IsPrisoner = @isPrisoner
                                                  Where id = @id";
                updateUserCommand.Parameters.AddWithValue("name", name);
                updateUserCommand.Parameters.AddWithValue("releaseDate", releaseDate);
                updateUserCommand.Parameters.AddWithValue("age", age);
                updateUserCommand.Parameters.AddWithValue("isPrisoner", isPrisoner);

                var numberOfRowsUpdated = updateUserCommand.ExecuteNonQuery();
                connection.Close();

                if (numberOfRowsUpdated > 0)
                {
                    return true;
                }
                return false;
            }
            throw new Exception("User is not updated.");
        }

        public List<object> GetOtherUsersWithSameInterest(int userId, string InterestName)
        {
            var UsersWithSameInterest = new List<object>();

            var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var getOtherUsersWithSameInterestCommand = connection.CreateCommand();
            getOtherUsersWithSameInterestCommand.CommandText = @"Select u.Name as UserName, i.Name as InterestName 
                                                                from UsersInterests as ui
                                                                Join Interests as i
                                                                On ui.InterestId = i.Id And i.Name = @InterestName
                                                                Join Users as u 
                                                                On ui.UserId = u.Id And u.Id != @userId;";
            getOtherUsersWithSameInterestCommand.Parameters.AddWithValue("@InterestName", InterestName);
            getOtherUsersWithSameInterestCommand.Parameters.AddWithValue("@userId", userId);

            var reader = getOtherUsersWithSameInterestCommand.ExecuteReader();

            while (reader.Read())
            {
                var istName = reader["InterestName"].ToString();
                var userName = reader["UserName"].ToString();
                var userWithInterest = new { UserName = userName, InterestName= istName };

                UsersWithSameInterest.Add(userWithInterest);
            }

            connection.Close();

            return UsersWithSameInterest;
        }
    }
}

