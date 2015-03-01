﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace TimeCard.Models
{
    public class WorkdayModel
    {
        public UserModel User { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartIn { get; set; }
        public DateTime LunchOut { get; set; }
        public DateTime LunchIn { get; set; }
        public DateTime EndOut { get; set; }
        public bool IsPaidTimeOff { get; set; }
        public bool IsHoliday { get; set; }

        private const string LOAD_QUERY = @"select * from workday join user on workday.userid = user.id where userId = @id and day = @day";

        public static WorkdayModel Load(SQLiteConnection conn, int userId, DateTime date)
        {
            SQLiteCommand query = null;
            SQLiteDataReader reader = null;
            try
            {
                bool prepareStatement = false;

                if (prepareStatement)
                {
                    // this method isn't returning results for some reason; workaround below just 
                    // concatenates a string instead.
                    query = new SQLiteCommand(LOAD_QUERY, conn);
                    SQLiteParameter idParam = new SQLiteParameter("@id", SqlDbType.Int);
                    SQLiteParameter dateParam = new SQLiteParameter("@day", SqlDbType.Date);
                    idParam.Value = userId;
                    dateParam.Value = date.Date;
                    query.Parameters.Add(idParam);
                    query.Parameters.Add(dateParam);
                    query.Prepare();
                    reader = query.ExecuteReader();
                }
                else
                {
                    string queryString = LOAD_QUERY.Replace("@id", userId.ToString()).Replace("@day", String.Format("'{0:yyyy-MM-dd}'", date));
                    query = new SQLiteCommand(queryString, conn);
                    reader = query.ExecuteReader();
                }


                if (reader.Read())
                {
                    return LoadFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                reader.Close();
            }
        }

        public static WorkdayModel LoadFromReader(SQLiteDataReader reader)
        {
            WorkdayModel result = new WorkdayModel();
            UserModel user = UserModel.LoadFromReader(reader);
            result.User = user;
            result.Date = reader["day"] == DBNull.Value ? new DateTime() : ((DateTime)reader["day"]).Date;
            result.StartIn = reader["startIn"] == DBNull.Value ? new DateTime() : (DateTime)reader["startIn"];
            result.LunchOut = reader["lunchOut"] == DBNull.Value ? new DateTime() : (DateTime)reader["lunchOut"];
            result.LunchIn = reader["lunchIn"] == DBNull.Value ? new DateTime() : (DateTime)reader["lunchIn"];
            result.EndOut = reader["endOut"] == DBNull.Value ? new DateTime() : (DateTime)reader["endOut"];
            result.IsPaidTimeOff = ((byte)reader["isPaidTimeOff"]) > 0;
            result.IsHoliday = ((byte)reader["isHoliday"]) > 0;
            return result;
        }

    }
}