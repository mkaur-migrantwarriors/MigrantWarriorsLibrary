using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using MigrantWarriorsLibrary.Filters;
using MigrantWarriorsLibrary.Models;
using MigrantWarriorsLibrary.Services;
using Microsoft.AspNetCore.Cors;

namespace MigrantWarriorsLibrary.Controllers
{
    [Route("api/migrants")]
    [ApiController]
    [EnableCors("AllowOrigin")]
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
        public ActionResult<bool> Get()
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
                return helper.CreateResponse(404);
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

        [Route("coordinates")]
        [HttpGet()]
        public ActionResult<List<Coordinates>> GetCountOfMigrantsAtEachLocation()
        {
            var data = _migrantService.GetLatitudeLongitudeWithMigrantsCount();

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

        [Route("paneldata")]
        [HttpGet()]
        public ActionResult<UIData> GetCountsForUIPanel()
        {
            var data = _migrantService.GetUIPanelCounts();

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }

        [Route("statewise")]
        [HttpGet()]
        public ActionResult<Dictionary<string, int>> GetMigrantsDataForAllStates()
        {
            var data = _migrantService.GetCountForAllStates();

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }

        [Route("districts/{state}")]
        [HttpGet()]
        public ActionResult<string[]> GetDistrictsOfState(string state)
        {
            var data = _migrantService.GetDistricts(state);

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }

        [Route("graph/{state?}/{district?}")]
        [HttpGet()]
        public ActionResult<object> GetDataOfVerifiedUnVerifiedMigrantsOfLast7Days(string state = null, string district = null)
        {
            var data = _migrantService.GetLast7DaysVerifiedUnverifiedCount(state, district);

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }

        [Route("skills/{state?}/{district?}")]
        [HttpGet()]
        public ActionResult<object> GetSkillsCount(string state = null, string district = null)
        {
            var data = _migrantService.GetSkillsCount(state, district);

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }

        [Route("skills/topfive/{state?}/{district?}")]
        [HttpGet()]
        public ActionResult<object> GetTopFiveSkillsCount(string state = null, string district = null)
        {
            var data = _migrantService.GetSkillsCount(state, district, true);

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }

        [Route("topfivestates/{gender}")]
        [HttpGet()]
        public ActionResult<object> GetTopFiveStatesGenderCount(string gender)
        {
            var data = _migrantService.GetGenderWiseInTopFiveStates(gender);

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }

        [Route("modesOfRegistration/{state?}/{district?}")]
        [HttpGet()]
        public ActionResult<object> GetTopFiveModeOfRegistration(string state, string district)
        {
            var data = _migrantService.GetTopFiveRegistration(state, district);

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }

        [Route("topfiveskills/{state?}/{district?}")]
        [HttpGet()]
        public ActionResult<object> GetTopFiveSkillsVerifiedUnverified(string state, string district)
        {
            var data = _migrantService.GetTopFiveSkillsVerifiedUnverifiedCount(state, district);

            if (data == null)
            {
                return NotFound();
            }

            return data;
        }
    }
}