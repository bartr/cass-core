using Cassandra;
using Cassandra.Mapping;
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace CassandraQuickStartSample
{
    public class Program
    {
        // Change UserName / Password
        private const string UserName = "bartr3"; 
        private const string Password = "g9C9qZt0VcJZGP99zj6nYMH0wtgpZ80BGqqayqQEUhhbm7TTi1CjacWyZagOqODY4ekdmXqPJw3Xe1pLNn0brw==";

        public static void Main(string[] args)
        {
            const string CassandraContactPoint = UserName + ".cassandra.cosmosdb.azure.com";  
            const int CassandraPort = 10350;

            // Connect to cassandra cluster  (Cassandra API on Azure Cosmos DB supports only TLSv1.2)
            var options = new Cassandra.SSLOptions(SslProtocols.Tls12, true, ValidateServerCertificate);
            options.SetHostNameResolver((ipAddress) => CassandraContactPoint);
            Cluster cluster = Cluster.Builder().WithCredentials(UserName, Password).WithPort(CassandraPort).AddContactPoint(CassandraContactPoint).WithSSL(options).Build();
            ISession session = cluster.Connect();

            session.Execute("DROP TABLE IF EXISTS myapp.user;");
            // session.Execute("DROP KEYSPACE IF EXISTS myapp;");

            // Create KeySpace and table
            // session.Execute("CREATE KEYSPACE IF NOT EXISTS myapp WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 3 } ;");
             session.Execute("CREATE TABLE IF NOT EXISTS myapp.user (userid int, name text, city text, PRIMARY KEY (userid)) WITH cosmosdb_provisioned_throughput=400");

            int max = 0;
            // var rs = session.Execute("select userid from myapp.user order by userid desc limit 1");
            // foreach (var r in rs)
            // {
            //     max = r.GetValue<int>(0);
            // }
            // Console.WriteLine(max);

            int i = max;
            string sql = "insert into myapp.user (userid, name, city) values ({0}, '{1}', '{2}')";

            // insert via execute
            session.Execute(string.Format(sql, ++i, "LyubovK", "Dubai"));
            session.Execute(string.Format(sql, ++i, "JiriK", "Toronto"));
            session.Execute(string.Format(sql, ++i, "IvanH", "Mumbai"));
            session.Execute(string.Format(sql, ++i, "Bart", "Austin"));

            session = cluster.Connect("myapp");
            Console.WriteLine("connected to myapp");

            IMapper mapper = new Mapper(session);

            // insert via mapper
            // mapper.Insert<User>(new User(++i, "LiliyaB", "Seattle"));
            // mapper.Insert<User>(new User(++i, "JindrichH", "Buenos Aires"));

            Console.WriteLine("Select ALL");
            Console.WriteLine("-------------------------------");
            foreach (User user in mapper.Fetch<User>("Select * from user order by userid"))
            {
                Console.WriteLine(user);
            }

            Console.WriteLine("Getting by id 3");
            Console.WriteLine("-------------------------------");
            User userId3 = mapper.FirstOrDefault<User>("Select * from user where userid = ?", 3);
            Console.WriteLine(userId3);

        }

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None) {
                return true;
            }

             Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Fail
            return false;
        }
    }


    public class User
    {
        public int userid { get; set; }
        public string name { get; set; }
        public string city { get; set; }

        public User(int userid, string name, string city)
        {
            this.userid = userid;
            this.name = name;
            this.city = city;
        }

        public override string ToString()
        {
            return string.Format(" {0} | {1} | {2}", userid, name, city);
        }
    }
}
