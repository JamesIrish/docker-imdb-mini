using System;
using System.IO;
using System.Reflection;
using System.Text;
using CsvHelper.Configuration;
using ImdbMini.Contracts;
using MongoDB.Driver;

namespace ImdbMini.DataLoad
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load of type rubbish
            var movieProperties = typeof(Movie).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var inserted = 0;
            var tStr = typeof(string);
            var tInt = typeof(int?);
            var tLong = typeof(long?);
            var tDouble = typeof(double?);

            // Open a connection to MongoDb
            var mongoClient = new MongoClient("mongodb://localhost");
            var database = mongoClient.GetDatabase("Imdb");
            var collection = database.GetCollection<Movie>("Movies");

            // Empty the collection
            collection.DeleteMany(FilterDefinition<Movie>.Empty);

            // Open the CSV
            using (var stream = new FileStream(@"E:\Downloads\movie_metadata.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var csvFactory = new CsvHelper.CsvFactory();
                var csv = csvFactory.CreateReader(reader, new CsvConfiguration{Encoding = Encoding.UTF8, HasHeaderRecord = true, TrimFields = true, TrimHeaders = true});
                csv.ReadHeader();
                while (csv.Read())
                {
                    var fields = csv.CurrentRecord;
                    var movie = new Movie();
                    for (var f = 0; f < fields.Length; f++)
                    {
                        var prop = movieProperties[f];
                        if (prop.PropertyType == tStr)
                            prop.SetValue(movie, fields[f].Trim());
                        if (prop.PropertyType == tInt)
                            prop.SetValue(movie, NullableHelper.ParseNullable<int>(fields[f].Trim(), int.TryParse));
                        if (prop.PropertyType == tLong)
                            prop.SetValue(movie, NullableHelper.ParseNullable<long>(fields[f].Trim(), long.TryParse));
                        if (prop.PropertyType == tDouble)
                            prop.SetValue(movie, NullableHelper.ParseNullable<double>(fields[f].Trim(), double.TryParse));
                    }

                    // Insert into Mongo
                    collection.InsertOne(movie);
                    Console.WriteLine(movie.MovieTitle);
                    inserted++;
                }
            }

            Console.WriteLine("Done, {0:N0} movies inserted.", inserted);
        }

        public class NullableHelper
        {
            public delegate bool TryDelegate<T>(string s, out T result);

            public static bool TryParseNullable<T>(string s, out T? result, TryDelegate<T> tryDelegate) where T : struct
            {
                if (s == null)
                {
                    result = null;
                    return true;
                }

                T temp;
                bool success = tryDelegate(s, out temp);
                result = temp;
                return success;
            }

            public static T? ParseNullable<T>(string s, TryDelegate<T> tryDelegate) where T : struct
            {
                if (s == null)
                {
                    return null;
                }

                T temp;
                return tryDelegate(s, out temp)
                    ? (T?)temp
                    : null;
            }
        }
        
    }
}