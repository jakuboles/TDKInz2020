using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TDK.Model
{
    public class UsersPlaces
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string PlaceId { get; set; }
    }
}
