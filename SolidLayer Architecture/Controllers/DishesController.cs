using Microsoft.AspNetCore.Mvc;
using SolidLayer_Architecture.Interfaces.Repositories;
using Swipe2TryCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolidLayer_Architecture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishesController : ControllerBase
    {
        private readonly IDishRepository _dishRepository;

        public DishesController(IDishRepository dishRepository)
        {
            _dishRepository = dishRepository;
        }

        // Controller methods remain the same as before
        // Just update the using directive
    }
}