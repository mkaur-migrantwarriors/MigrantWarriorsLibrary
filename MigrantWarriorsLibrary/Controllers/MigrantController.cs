using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using MigrantWarriorsLibrary.Filters;
using MigrantWarriorsLibrary.Models;
using MigrantWarriorsLibrary.Services;

namespace MigrantWarriorsLibrary.Controllers
{
    [Route("api/migrants")]
    [ApiController]
    public class MigrantController : ControllerBase
    {
        private readonly MigrantService _migrantService;

        public MigrantController(MigrantService migrantService)
        {
            _migrantService = migrantService;
        }

        [HttpGet]
        [Route("GetStatus")]
        public ActionResult<string> GetStatus()
        {
            return "Hello, Web Service is working.";
        }

        [HttpGet]
        public ActionResult<List<Migrant>> Get()
        {
            return _migrantService.Get();
        }

        [Route("{id}")]
        [HttpGet("{id:length(24)}", Name = "GetMigrant")]
        public ActionResult<Migrant> Get(string id)
        {
            var migrant = _migrantService.Get(id);

            if (migrant == null)
            {
                return NotFound();
            }

            return migrant;
        }

        [HttpPost]
        public ActionResult<object> Create([FromBody] Migrant migrant)
        {
            Helper helper = new Helper();
            try
            {
                AdhaarIdFilter af = new AdhaarIdFilter();
                _migrantService.UpdateMigrantData(out migrant, migrant, af.IsValidAadhaarNumber(migrant.AadharNumber));
                return _migrantService.Create(migrant);
            }
            catch(Exception)
            {
                return helper.CreateResponse(Helper.FAILURE, Helper.DATANOTADDED);
            }
        }

        [Route("{id}")]
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Migrant migrantIn)
        {
            var book = _migrantService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _migrantService.Update(id, migrantIn);

            return NoContent();
        }

        [Route("{id}")]
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var migrant = _migrantService.Get(id);

            if (migrant == null)
            {
                return NotFound();
            }

            _migrantService.Remove(migrant.Id);

            return NoContent();
        }

        [Route("{pincode}")]
        [HttpGet()]
        public ActionResult<Coordinates> Get(long pincode)
        {
            var data = _migrantService.GetLatitudeLongitudeWithMigrantsCount(pincode);

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }

        [Route("statewise/{state}")]
        [HttpGet()]
        public ActionResult<List<Migrant>> GetMigrantsDataStateWise(string state)
        {
            var data = _migrantService.GetStateWiseData(state);

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }
    }
}