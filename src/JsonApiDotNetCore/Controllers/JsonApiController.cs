using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiController<T> : Controller where T : class, IIdentifiable
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public JsonApiController(DbContext context)
        { 
            _context = context;
            _dbSet = context.GetDbSet<T>();
        }

        [HttpGet]
        public virtual IActionResult Get() 
        {
            var entities = _dbSet.ToList();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public virtual IActionResult Get(int id) 
        {
            var entity = _dbSet.FirstOrDefault(e => e.Id == id);
            
            if(entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpGet("{id}/{relationship}")]
        public virtual IActionResult GetRelationship(int id, string relation) 
        {
            return Ok("Get id/relationship");
        }

        [HttpPatch("{id}")]
        public virtual IActionResult Patch(int id) 
        {
            return Ok("Patch Id");
        }

        [HttpPatch("{id}/{relationship}")]
        public virtual IActionResult PatchRelationship(int id, string relation) 
        {
            return Ok("Patch Id/relationship");
        }

        [HttpDelete("{id}")]
        public virtual IActionResult Delete(int id) 
        {
            return Ok("Delete Id");
        }

        [HttpDelete("{id}/{relationship}")]
        public virtual IActionResult Delete(int id, string relation) 
        {
            return Ok("Delete Id/relationship");
        }
    }
}
