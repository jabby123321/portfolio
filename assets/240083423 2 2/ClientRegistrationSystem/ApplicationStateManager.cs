using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Security.Cryptography;


namespace ClientRegistrationSystem
{

    /// <summary>
    /// Keeps track of application state - everything which the application uses to operate in a consistent state eg the database connection, which user is logged in and the state of the client db
    /// </summary>
    public class ApplicationStateManager
    {
        public Staff User;

        private MySqlConnection DatabaseConnection;


        public List<Client> clientData;

        public List<Client> ClientData
        {
            get { FillClientList(); return clientData; }
            set { clientData = value; }
        }


        public bool LoggedIn { get; private set; }


        public ApplicationStateManager()
        {
           clientData = new List<Client>();
        }

        /// <summary>
        /// Called to allow the user to login... attempts to connect to the database and if successfull checks the user credentials on the database
        /// If the user credentials are correct then it updates the user in the ASMs
        /// </summary>
        /// <param name="username">Username to attempt to login as</param>
        /// <param name="password">Password of login user</param>
        /// <param name="connectionString">Connection string for the database</param>
        public void Login(string username, string password, string connectionString)
        {
            // hash the password
            password = Staff.hash(password);

            // initialize the database collection
            DatabaseConnection = new MySqlConnection(connectionString);

            // retreive username and password hash of login user
            var query = "SELECT PasswordHash, PermissionLevel FROM Staff WHERE Username=@uname";

            var cmd = new MySqlCommand(query, DatabaseConnection);
            cmd.Parameters.AddWithValue("@uname", username);
            DatabaseConnection.Open();
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                var ex = new Exception("No Rows returned");
                throw ex;
            }
            var correctPassword = $"{reader["PasswordHash"]}";
            var permissionLevelString = $"{reader["PermissionLevel"]}";
            DatabaseConnection.Close();

            // Set user to be the login user if the passwords match
            if (correctPassword == password)
            {
                if (permissionLevelString == "admin")
                {
                    User = new Staff(username, password, PermissionLevel.Admin);
                    LoggedIn = true;
                    return;
                }
                if (permissionLevelString == "staff")
                {
                    User = new Staff(username, password, PermissionLevel.Staff);
                    LoggedIn = true;
                    return;
                }
            }

            throw new Exception("Authentication Failed");

        }

        // adds a client to the database
        public void AddClient(Client client)
        {
            // prepares an sql query
            var query = "CALL insert_client (@name, @address, @phone, @email, @product);";

            var cmd = new MySqlCommand(query, DatabaseConnection);
            cmd.Parameters.AddWithValue("@name", client.Name);
            cmd.Parameters.AddWithValue("@address", client.Address);
            cmd.Parameters.AddWithValue("@email", client.Email);
            cmd.Parameters.AddWithValue("@phone", client.PhoneNum);
            var product = client.ProductType.ToString();
            cmd.Parameters.AddWithValue("@product", product);

            // opens the database connection, inserts the client and then closes the connection
            DatabaseConnection.Open();
            cmd.ExecuteNonQuery();
            DatabaseConnection.Close();
        }

        // Modifies client in database
        public void ModifyClient(Client client, string ID)
        {
            // prepares an sql query
            var query = "UPDATE Client SET Name = @name, Address = @address, PhoneNumber = @phone, Email = @email, ProductType = @product WHERE ClientID = @ID";
           
            var cmd = new MySqlCommand(query, DatabaseConnection);
            cmd.Parameters.AddWithValue("@name", client.Name);
            cmd.Parameters.AddWithValue("@address", client.Address);
            cmd.Parameters.AddWithValue("@email", client.Email);
            cmd.Parameters.AddWithValue("@phone", client.PhoneNum);
            var product = client.ProductType.ToString();
            cmd.Parameters.AddWithValue("@product", product);
            cmd.Parameters.AddWithValue("@ID", int.Parse(ID));

            // opens the database connection, updates the client and then closes the connection
            DatabaseConnection.Open();
            cmd.ExecuteNonQuery();
            DatabaseConnection.Close();
        }

        // Deletes client from database
        public void DeleteClient(string ID)
        {
            // prepares an sql query
            var query = "DELETE FROM Client WHERE ClientID = @ID";

            var cmd = new MySqlCommand(query, DatabaseConnection);
            cmd.Parameters.AddWithValue("@id", ID);

            // opens the database connection, deletes the client and then closes the connection
            DatabaseConnection.Open();
            cmd.ExecuteNonQuery();
            DatabaseConnection.Close();
        }

        // retreives details about a specified client from the database and returns them in a Client object
        public Client GetClient(string ID)
        {
            // prepare sql query
            var query = "SELECT * FROM Client WHERE ClientID=@ID";
            var cmd = new MySqlCommand(query, DatabaseConnection);
            cmd.Parameters.AddWithValue("@ID", ID);

            // execute query
            DatabaseConnection.Open();
            var reader = cmd.ExecuteReader();
            

            // construct a client from the result
            Client client;
            while (reader.Read())
            {
                Console.WriteLine($"ClientID: {reader["ClientID"]}\tName: {reader["Name"]}");
                Product productType;
                switch (reader["ProductType"].ToString())
                {
                    case "Software":
                        productType = Product.Software;
                        break;
                    case "LaptopsPCs":
                        productType = Product.LaptopsPCs;
                        break;
                    case "Games":
                        productType = Product.Games;
                        break;
                    case "Office":
                        productType = Product.Office;
                        break;
                    case "Accessories":
                        productType = Product.Accessories;
                        break;

                    default:
                        throw new Exception("unknown product type");
                }
                client = new Client(
                    int.Parse(reader["ClientID"].ToString()),
                    reader["Name"].ToString(),
                    reader["Address"].ToString(),
                    reader["PhoneNumber"].ToString(),
                    reader["Email"].ToString(),
                    productType);

                // close database and return client
                DatabaseConnection.Close();
                return client;
            }

            throw new Exception();
        }

        // Function populates client list from databse
        public void FillClientList()
        {
            // retreive data from database
            var query = "SELECT * FROM Client";

            var cmd = new MySqlCommand(query, DatabaseConnection);

            var _clientData = new List<Client>();
            DatabaseConnection.Open();
            var reader = cmd.ExecuteReader();

            // iterate over result forming clients from each row and appending them to the temperary _clientData List
            while (reader.Read())
            {
                Console.WriteLine($"ClientID: {reader["ClientID"]}\tName: {reader["Name"]}");
                Product productType;
                switch (reader["ProductType"].ToString())
                {
                    case "Software":
                        productType = Product.Software;
                        break;
                    case "LaptopsPCs":
                        productType = Product.LaptopsPCs;
                        break;
                    case "Games":
                        productType = Product.Games;
                        break;
                    case "Office":
                        productType = Product.Office;
                        break;
                    case "Accessories":
                        productType = Product.Accessories;
                        break;

                    default:
                        throw new Exception("unknown product type");
                }
                var client = new Client(
                    int.Parse(reader["ClientID"].ToString()),
                    reader["Name"].ToString(),
                    reader["Address"].ToString(),
                    reader["PhoneNumber"].ToString(),
                    reader["Email"].ToString(),
                    productType);
                _clientData.Add(client);
            }

            // update list and close database connection
            DatabaseConnection.Close();
            ClientData = _clientData;
        }

// JSON save function
        public void SaveAsJson(string filePath)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(ClientData, Newtonsoft.Json.Formatting.Indented);
            string encrypted = EncryptionHelper.Encrypt(json);
            File.WriteAllText(filePath, encrypted);
        }

// JSON load function
        public void LoadFromJson(string filePath)
        {
            string encrypted = File.ReadAllText(filePath);
            string json = EncryptionHelper.Decrypt(encrypted);

            List<Client> importList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Client>>(json);

            DbImport(importList);

        }

        // CSV save function
        public void SaveAsCsv(string filePath)
        {
            var sb = new StringBuilder();

            foreach(var c in ClientData)
            {
                sb.AppendLine($"{c.ID}|{c.Name}|{c.Address}|{c.PhoneNum}|{c.Email}|{c.ProductType}");
            }

            string encrypted = EncryptionHelper.Encrypt(sb.ToString());
            File.WriteAllText(filePath, encrypted);
        }


// CSV load function
        public void LoadFromCsv(string filePath)
        {
            string encrypted = File.ReadAllText(filePath);
            string csv = EncryptionHelper.Decrypt(encrypted);

            var importList = new List<Client>();

            foreach (var line in csv.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('|');

                importList.Add(new Client(
                    int.Parse(parts[0]),
                    parts[1],
                    parts[2],
                    parts[3],
                    parts[4],
                    (Product)Enum.Parse(typeof(Product), parts[5].Trim())));
            }

            DbImport(importList);


        }

        private void DbImport(List<Client> importList)
        {
            clientData = new List<Client>();
            FillClientList();

            var insertList = new List<Client>();

            foreach (var i in importList)
            {
                var broken = false;
                foreach (var c in clientData)
                {
                    if (c.ID == i.ID) {  broken = true; break; }
                }
                if (!broken)
                    insertList.Add(i);
            }

            InsertClientsWithSpecifiedIDs(insertList);
        }

        private void InsertClientsWithSpecifiedIDs(List<Client> insertList)
        {
            var command = new MySqlCommand("INSERT INTO `Client` (ClientID, Name, Address, PhoneNumber, Email, ProductType) VALUES (@id, @name, @address, @phone, @email, @product);", DatabaseConnection);

            command.Parameters.Add("@id", MySqlDbType.Int64);
            command.Parameters.Add("@name", MySqlDbType.VarChar);
            command.Parameters.Add("@address", MySqlDbType.VarChar);
            command.Parameters.Add("@phone", MySqlDbType.VarChar);
            command.Parameters.Add("@email", MySqlDbType.VarChar);
            command.Parameters.Add("@product", MySqlDbType.Enum);


            DatabaseConnection.Open();
            foreach (var c in insertList) {
                command.Parameters["@id"].Value = c.ID;
                command.Parameters["@name"].Value = c.Name;
                command.Parameters["@address"].Value = c.Address;
                command.Parameters["@phone"].Value = c.PhoneNum;
                command.Parameters["@email"].Value = c.Email;
                command.Parameters["@product"].Value = c.ProductType;

                command.ExecuteNonQuery();
            }
            DatabaseConnection.Close();
        }
    }
}
