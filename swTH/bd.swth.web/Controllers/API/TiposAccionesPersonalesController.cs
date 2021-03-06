using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using bd.swth.datos;
using bd.swth.entidades.Negocio;
using bd.log.guardar.Servicios;
using bd.log.guardar.Enumeradores;
using Microsoft.EntityFrameworkCore;
using bd.log.guardar.ObjectTranfer;
using bd.swth.entidades.Enumeradores;
using bd.swth.entidades.Utils;

namespace bd.swth.web.Controllers.API
{
    [Produces("application/json")]
    [Route("api/TiposAccionesPersonales")]
    public class TiposAccionesPersonalesController : Controller
    {
        private readonly SwTHDbContext db;

        public TiposAccionesPersonalesController(SwTHDbContext db)
        {
            this.db = db;
        }

        /*
        [HttpPost]
        [Route("ListarTiposAccionesPersonalesPorEstado")]
        public async Task<List<TipoAccionPersonal>> ListarTiposAccionesPersonalesPorEstado([FromBody] EstadoTipoAccionPersonal estadoTipoAccionPersonal)
        {
            try
            {
                return await db.TipoAccionPersonal.Where(x => x.IdEstadoTipoAccionPersonal == estadoTipoAccionPersonal.IdEstadoTipoAccionPersonal).OrderBy(x => x.Nombre).ToListAsync();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex.Message,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new List<TipoAccionPersonal>();
            }
        }

    */

        // [HttpPost] api/TiposAccionesPersonales
        [HttpPost]
        [Route("ObtenerTipoAccionPersonal")]
        public async Task<TipoAccionPersonal> ObtenerTipoAccionPersonal([FromBody] TipoAccionPersonal tipoAccionPersonal)
        {
            try
            {
                return await db.TipoAccionPersonal.Where(x => x.IdTipoAccionPersonal == tipoAccionPersonal.IdTipoAccionPersonal).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                return new TipoAccionPersonal();
            }
        }


        [HttpPost]
        [Route("ListarTiposAccionesPersonalesPorEsTalentoHumano")]
        public async Task<List<TipoAccionPersonal>> ListarTiposAccionesPersonalesPorEsTalentoHumano([FromBody] TipoAccionPersonal tipoAccionPersonal)
        {
            try
            {
              return await db.TipoAccionPersonal.Where(x=>x.EsResponsableTH==tipoAccionPersonal.EsResponsableTH).OrderBy(x => x.Nombre).ToListAsync();
            }
            catch (Exception ex)
            {
              return new List<TipoAccionPersonal>();
            }
        }


        // GET: api/TiposAccionesPersonales
        [HttpGet]
        [Route("ListarTiposAccionesPersonales")]
        public async Task<List<TipoAccionPersonal>> GetTiposAccionesPersonales()
        {
            try
            {
                return await db.TipoAccionPersonal.OrderBy(x => x.Nombre).ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<TipoAccionPersonal>();
            }
        }
          

        // GET: api/TipoAccionPersonal/5
        [HttpGet("{id}")]
        public async Task<Response> GetTipoAccionPersonal([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }

                var TipoAccionPersonal = await db.TipoAccionPersonal.SingleOrDefaultAsync(m => m.IdTipoAccionPersonal == id);

                if (TipoAccionPersonal == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                    Resultado = TipoAccionPersonal,
                };
            }
            catch (Exception ex)
            {
               
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,

                };
            }
        }

        // PUT: api/TipoAccionPersonal/5
        [HttpPut("{id}")]
        public async Task<Response> PutTipoAccionPersonal([FromRoute] int id, [FromBody] TipoAccionPersonal tipoAccionPersonal)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido
                    };
                }

                if (tipoAccionPersonal.NHorasMinimo > tipoAccionPersonal.NHorasMaximo && tipoAccionPersonal.NDiasMinimo > tipoAccionPersonal.NDiasMaximo)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ErrorPorComparacionFechasTipoAccionPersonal
                    };
                }




                var TipoAccionPersonalActualizar = db.TipoAccionPersonal.Find(tipoAccionPersonal.IdTipoAccionPersonal);

                var TipoAccionPersonalPorNombre = await db.TipoAccionPersonal
                    .Where(w => 
                        w.Nombre == tipoAccionPersonal.Nombre.ToString().ToUpper() 
                        && w.IdTipoAccionPersonal != tipoAccionPersonal.IdTipoAccionPersonal)
                    .FirstOrDefaultAsync();
                ;

                if (TipoAccionPersonalPorNombre != null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ExisteRegistro
                    };
                }

                TipoAccionPersonalActualizar.Nombre = tipoAccionPersonal.Nombre.ToString().ToUpper();

                TipoAccionPersonalActualizar.NDiasMaximo = tipoAccionPersonal.NDiasMaximo;
                TipoAccionPersonalActualizar.NDiasMinimo = tipoAccionPersonal.NDiasMinimo;
                TipoAccionPersonalActualizar.NHorasMaximo = tipoAccionPersonal.NHorasMaximo;
                TipoAccionPersonalActualizar.NHorasMinimo = tipoAccionPersonal.NHorasMinimo;

                TipoAccionPersonalActualizar.DiasHabiles = tipoAccionPersonal.DiasHabiles;
                TipoAccionPersonalActualizar.ImputableVacaciones = tipoAccionPersonal.ImputableVacaciones;
                TipoAccionPersonalActualizar.ProcesoNomina = tipoAccionPersonal.ProcesoNomina;
                TipoAccionPersonalActualizar.EsResponsableTH = tipoAccionPersonal.EsResponsableTH;

                TipoAccionPersonalActualizar.Matriz = tipoAccionPersonal.Matriz.ToString().ToUpper();
                TipoAccionPersonalActualizar.Descripcion = (tipoAccionPersonal.Descripcion != null)?
                    tipoAccionPersonal.Descripcion.ToString().ToUpper():"";

                TipoAccionPersonalActualizar.GeneraAccionPersonal = tipoAccionPersonal.GeneraAccionPersonal;
                TipoAccionPersonalActualizar.ModificaDistributivo = tipoAccionPersonal.ModificaDistributivo;

                TipoAccionPersonalActualizar.MesesMaximo = tipoAccionPersonal.MesesMaximo;
                TipoAccionPersonalActualizar.YearsMaximo = tipoAccionPersonal.YearsMaximo;

                TipoAccionPersonalActualizar.DesactivarCargo = tipoAccionPersonal.DesactivarCargo;
                TipoAccionPersonalActualizar.Definitivo = tipoAccionPersonal.Definitivo;

                TipoAccionPersonalActualizar.DesactivarEmpleado = tipoAccionPersonal.DesactivarEmpleado;
                TipoAccionPersonalActualizar.ModalidadContratacion = tipoAccionPersonal.ModalidadContratacion;


                db.TipoAccionPersonal.Update(TipoAccionPersonalActualizar);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.GuardadoSatisfactorio,
                    Resultado = TipoAccionPersonalActualizar,
                };

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex.Message,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Excepcion,
                };
            }

        }

        // POST: api/TipoAccionPersonal
        [HttpPost]
        [Route("InsertarTipoAccionPersonal")]
        public async Task<Response> InsertarTipoAccionPersonal([FromBody] TipoAccionPersonal TipoAccionPersonal)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = ""
                    };
                }

                if (TipoAccionPersonal.NHorasMinimo > TipoAccionPersonal.NHorasMaximo && TipoAccionPersonal.NDiasMinimo > TipoAccionPersonal.NDiasMaximo)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = "La hora m�nimo debe ser menor a la hora m�ximo y el d�a m�nimo debe ser menor al d�a m�ximo"
                    };
                }

                var respuesta = Existe(TipoAccionPersonal);
                if (!respuesta.IsSuccess)
                {
                    // Convertir a may�sculas
                    TipoAccionPersonal.Nombre = TipoAccionPersonal.Nombre.ToString().ToUpper();
                    TipoAccionPersonal.Descripcion = (TipoAccionPersonal.Descripcion != null)?TipoAccionPersonal.Descripcion.ToString().ToUpper():"";
                    TipoAccionPersonal.Matriz = (TipoAccionPersonal.Matriz != null) ? TipoAccionPersonal.Matriz.ToString().ToUpper():"";

                    db.TipoAccionPersonal.Add(TipoAccionPersonal);
                    await db.SaveChangesAsync();
                    return new Response
                    {
                        IsSuccess = true,
                        Message = Mensaje.GuardadoSatisfactorio,
                        Resultado = TipoAccionPersonal
                    };
                }

                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.ExisteRegistro,
                };

            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        // DELETE: api/TipoAccionPersonal/5
        [HttpDelete("{id}")]
        public async Task<Response> DeleteTipoAccionPersonal([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }

                var respuesta = await db.TipoAccionPersonal
                    .Where(w => w.IdTipoAccionPersonal == id)
                    .FirstOrDefaultAsync();


                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.TipoAccionPersonal.Remove(respuesta);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.BorradoSatisfactorio,
                };
            }
            catch (Exception ex)
            {
                
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.BorradoNoSatisfactorio,
                };
            }
        }

        private Response Existe(TipoAccionPersonal TipoAccionPersonal)
        {
            var nombre = TipoAccionPersonal.Nombre.ToUpper().TrimEnd().TrimStart();
            var TipoAccionPersonalrespuesta = db.TipoAccionPersonal.Where(p => p.Nombre.ToUpper().TrimStart().TrimEnd() == nombre).FirstOrDefault();

            if (TipoAccionPersonalrespuesta != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.ExisteRegistro,
                    Resultado = TipoAccionPersonalrespuesta,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Resultado = TipoAccionPersonalrespuesta,
            };
        }


        



    }
}
