using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DirectorySite.Data;
using DirectorySite.Models;
using DirectorySite.Services;

namespace DirectorySite.Controllers
{
    [Auth]
    [Route("[controller]")]
    public class RecoveryAccountController(ILogger<RecoveryAccountController> logger, RecoveryAccountService recoveryAccountService, PeopleSearchService ps) : Controller
    {
        private readonly ILogger<RecoveryAccountController> _logger = logger;
        private readonly RecoveryAccountService recoveryAccountService = recoveryAccountService;
        private readonly PeopleSearchService peopleSearchService = ps;

        public IActionResult Index([FromQuery] int p = 1, [FromQuery] int filter = 1)
        {
            ViewBag.CurrentPage = p;
            ViewBag.CurrentFilterStatus = filter;
            ViewData["Title"] = "Peticiones de recuperacion de cuentas";
            return View();
        }

        [HttpGet("{recordID}")]
        public async Task<IActionResult> Show(string recordID)
        {
            // * get catalog of templates
            IEnumerable<RecoveryAccountTemplate> templates = [];
            try
            {
                templates = await this.recoveryAccountService.GetTemplates();
            }
            catch(UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Authentication");
            }
            ViewBag.Templates = templates ?? Array.Empty<RecoveryAccountTemplate>();

            try
            {
                var record = await this.recoveryAccountService.GetRequestById(recordID);
                ViewData["Title"] = $"Peticion {recordID}";
                return View(record);
            }
            catch (KeyNotFoundException)
            {
                ViewData["Title"] = $"Peticion no encontrada";
                ViewBag.NotFoundMessage = "No se encontro ninguna solicitud activa con este id.";
                return View("~/Views/Shared/NotFound.cshtml");
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "Fal at attempt to retrive the request info: {message}", e.Message);
                var vm = new ErrorViewModel
                {
                    Message = "Error al obtener la peticion"
                };
                return View("~/Views/Shared/Error.cshtml", vm);
            }
        }


        [HttpPatch("{recordID}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> FinishRequest([FromRoute] string recordID, [FromForm] string comments, [FromForm] bool notifyEmail = false, [FromForm] int templateId = 0)
        {
            RecoveryAccountResponse recoveryRecord;
            try
            {
                recoveryRecord = await this.recoveryAccountService.GetRequestById(recordID);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return Conflict();
            }

            // * update the request
            try
            {
                await this.recoveryAccountService.UpdateTheRequest(recordID, comments, notifyEmail, templateId);
                return RedirectToAction("index", "RecoveryAccount");

            }catch(Exception)
            {
                return BadRequest();
            }
        }


        #region Partial views
        [Route("table-records")]
        public async Task<IActionResult> GetTableRecords([FromQuery] int p = 1, [FromQuery] int filter = 0){
            try
            {
                int take = 25;
                int skip = (p-1) * take;

                // * process the filters
                var excludeConcluded = filter switch {
                    1 => true,
                    2 => false,
                    3 => true,
                    _ => false,
                };
                var excludeDeleted = filter switch{
                    1 => true,
                    2 => true,
                    3 => false,
                    _ => false,
                };
                var excludePending = filter switch{
                    1 => false,
                    2 => true,
                    3 => true,
                    _ => false,
                };

                // * make the paginator component to display
                var recoveryAccountPaginator = await recoveryAccountService.GetRequest(
                    take:take,
                    offset:skip,
                    excludeConcluded:excludeConcluded,
                    excludeDeleted:excludeDeleted,
                    excludePending:excludePending
                );
                ViewBag.TotalRecords = recoveryAccountPaginator.Total;
                ViewBag.TotalPages = Math.Ceiling( (decimal) (recoveryAccountPaginator.Total / take) + 1);
                ViewBag.CurrentPage = p;
                ViewBag.Skip = skip;

                // * return the view with the data
                return PartialView("~/Views/RecoveryAccount/Partials/RecordsTable.cshtml", recoveryAccountPaginator.Data);
            }
            catch(Exception err)
            {
                this._logger.LogError(err,"Fail at retrive the records");
                var errorViewModel = new ErrorViewModel {
                    RequestId = "Fail at retrive the data"
                };
                return View("~/Views/Shared/Error.cshtml", errorViewModel);
            }
        }

        [Route("people-coincidence")]
        public async Task<IActionResult> GetPeopleCoincidence([FromQuery] string recordID)
        {
            // * get the request data information
            RecoveryAccountResponse recoveryRecord;
            try
            {
                recoveryRecord = await this.recoveryAccountService.GetRequestById(recordID);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "Error al obtener los datos de la peticion.";
                return View("~/Views/Shared/ErrorAlert.cshtml");
            }


            // * attempt to get the people with same concidence
            IEnumerable<SearchPersonResponse> people = [];
            try
            {
                var tasks = new List<Task<IEnumerable<SearchPersonResponse>>>();
                if(!string.IsNullOrEmpty(recoveryRecord.Curp))
                {
                    tasks.Add( Task.Run<IEnumerable<SearchPersonResponse>>( async () => await this.peopleSearchService.SearchPerson(recoveryRecord.Curp) ?? [] ));
                }

                if(!string.IsNullOrEmpty(recoveryRecord.ContactEmail))
                {
                    tasks.Add(Task.Run<IEnumerable<SearchPersonResponse>>( async () => await this.peopleSearchService.SearchPerson(recoveryRecord.ContactEmail) ?? [] ));
                }

                // wait fot the task to complete
                var results = await Task.WhenAll(tasks);

                // insert the results to 'people' collection
                people = results.SelectMany(result => result).ToList();

                // * prevent duplicates entries
                people = people.GroupBy(p => p.Id).Select(g => g.First()).ToList();

                return PartialView("~/Views/RecoveryAccount/Partials/PeopleCoincidences.cshtml", people);
            }
            catch(Exception err)
            {
                this._logger.LogError(err, "Fail at retrive the records: {message}", err.Message);
                ViewData["ErrorMessage"] = "Error al realizar la busqueda de coincidencias.";
                return View("~/Views/Shared/ErrorAlert.cshtml");
            }
        }

        #endregion
    }
}
