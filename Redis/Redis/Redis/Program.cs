using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Redis
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("poseidon01:6379, password = PRIME");
            IDatabase db = redis.GetDatabase(2);
            var hashKey = "D181211_1107";
            var field = "DownTime";


            HashEntry[] redisBookHash = {
                new HashEntry("DownTime", 5),
                new HashEntry("ErrorCode", 5)
              };

            db.HashSet(hashKey, redisBookHash);


            if (db.HashExists(hashKey, field))
            {
                var year = db.HashIncrement(hashKey, field, 100);
            }
        }
    }
}
