using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ImdbMini.Contracts;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ImdbMini.WebApp.Core.Controllers
{
    [Route("api/[controller]")]
    public class MoviesController : Controller
    {
        private readonly IMongoCollection<Movie> _collection;

        public MoviesController()
        {
            var client = new MongoClient("mongodb://172.17.0.2");
            var database = client.GetDatabase("Imdb");
            _collection = database.GetCollection<Movie>("Movies");
        }

        // GET api/movies
        [HttpGet]
        public async Task<IEnumerable<Movie>> Get(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _collection.Find(FilterDefinition<Movie>.Empty).ToListAsync(cancellationToken);
        }

        // GET api/movies/avatar
        [HttpGet("{title}")]
        public async Task<Movie> Get(string title, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _collection.Find(Builders<Movie>.Filter.Eq(m => m.MovieTitle, title)).FirstOrDefaultAsync(cancellationToken);
        }
    }
}