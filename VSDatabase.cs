using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace PvPCommands
{
    public class VSDatabase // Thanks Scavenger for his Essentials' esSQL.cs
    {
        private static IDbConnection db;
        private static string savepath = Path.Combine(TShock.SavePath, "VSSystem", (TShock.Config.StorageType.ToLower() == "mysql" ? "VSDatabase.db" : "VSDatabase.sqlite"));

        #region Setup Database
        public static void SetupDB()
        {
            switch (TShock.Config.StorageType.ToLower())
            {
                case "mysql":
                    string[] host = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection()
                    {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                        host[0],
                        host.Length == 1 ? "3306" : host[1],
                        TShock.Config.MySqlDbName,
                        TShock.Config.MySqlUsername,
                        TShock.Config.MySqlPassword)
                    };
                    break;
                case "sqlite":
                    string sql = savepath;
                    db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                    break;
            }
            SqlTableCreator sqlcreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            sqlcreator.EnsureExists(new SqlTable("VSPlayers",
                new SqlColumn("UserID", MySqlDbType.Int32),
                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("Commands", MySqlDbType.Text)));
            sqlcreator.EnsureExists(new SqlTable("VSCommands",
                new SqlColumn("UserID", MySqlDbType.Int32),
                new SqlColumn("CmdAlias", MySqlDbType.Text),
                new SqlColumn("Count", MySqlDbType.Int32)));
        }
        #endregion

        #region VSPlayerExists (By UserID, true if UserID is registered)
        public static bool VSPlayerExists(int UserID)
        {
            String query = "SELECT Name FROM VSPlayers WHERE UserID=@0;";
            List<string> usr = new List<string>();
            using (var reader = db.QueryReader(query, UserID))
            {
                while (reader.Read())
                {
                    usr.Add(reader.Get<string>("Name"));
                }
            }

            if (usr.Count < 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region Pull Player
        public static VSPlayer PullPlayer(int UserID)
        {
            String query = "SELECT Name FROM VSPlayers WHERE UserID=@0;";
            VSPlayer player;

            try
            {
                using (var reader = db.QueryReader(query, UserID))
                {
                    while (reader.Read())
                    {
                        player = new VSPlayer(reader.Get<string>("Name"));
                        return player;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
        #endregion

        #region Get Cooldowns (As Dictionary <CmdAlias,Count>)
        public static Dictionary<string,int> GetCooldowns(int UserID)
        {
            String query = "SELECT CmdAlias FROM VSCommands WHERE UserID=@0;";
            String query2 = "SELECT Count FROM VSCommands WHERE UserID=@0;";
            List<string> cmdalias = new List<string>();
            List<int> count = new List<int>();
            Dictionary<string,int> cooldowns = new Dictionary<string,int>();

            using (var reader = db.QueryReader(query, UserID))
            {
                while (reader.Read())
                {
                    cmdalias.Add(reader.Get<string>("CmdAlias"));
                }
            }
            using (var reader = db.QueryReader(query2, UserID))
            {
                while (reader.Read())
                {
                    count.Add(reader.Get<int>("Count"));
                }
            }
            for (int i = 0; i < cmdalias.Count; i++)
            {
                cooldowns.Add(cmdalias[i], count[i]);
            }
            return cooldowns;
        }
        #endregion

        #region Add VSPlayer
        public static bool AddVSPlayer(int UserID, string Name)
        {
            String query = "INSERT INTO VSPlayers (UserID, Name) VALUES (@0, @1);";

            if (db.Query(query, UserID, Name) != 1)
            {
                Log.ConsoleError("[PvP Commands DB] Creating a user's VSPlayer has failed!");
                return false;
            }
            return true;
        }
        #endregion

        #region Add Cooldown
        public static bool AddCooldown(int UserID, string CmdAlias, int Count)
        {
            String query = "INSERT INTO VSCommands (UserID, CmdAlias, Count) VALUES (@0, @1, @2);";

            if (db.Query(query, UserID, CmdAlias, Count) != 1)
            {
                Log.ConsoleError("[PvP Commands DB] Creating an user's cooldowns has failed!");
                return false;
            }
            return true;
        }
        #endregion

        #region Save Cooldown
        public static bool SaveCooldown(int UserID, string CmdAlias, int Count)
        {
            String query = "UPDATE VSCommands SET Count=@2 WHERE UserID=@0 AND CmdAlias=@1;";

            if (db.Query(query, UserID, CmdAlias, Count) != 1)
            {
                Log.ConsoleError("[PvP Commands DB] Saving an user's cooldowns has failed!");
                return false;
            }
            return true;
        }
        #endregion
    }
}