using Back.DataAccess;
using Back.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SizesController : ControllerBase
    {
        private IUnitOfWork context;
        public SizesController(IUnitOfWork context)
        {
            this.context = context;
        }

        //hien thi toan bo 
        [HttpGet("[action]")]
        public async Task<IEnumerable<Size>> Get()
        {
            return await context.SizeRepository.GetAsync();
        }
    }
}
