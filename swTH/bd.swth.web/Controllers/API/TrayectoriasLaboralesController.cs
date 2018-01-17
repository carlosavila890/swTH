using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bd.swth.datos;
using bd.swth.entidades.Negocio;
using bd.swth.entidades.Enumeradores;
using bd.log.guardar.Servicios;
using bd.log.guardar.ObjectTranfer;
using bd.log.guardar.Enumeradores;
using bd.swth.entidades.Utils;

namespace bd.swth.web.Controllers.API
{
    [Produces("application/json")]
    [Route("api/TrayectoriasLaborales")]
    public class TrayectoriasLaboralesController : Controller
    {
        private readonly SwTHDbContext db;

        public TrayectoriasLaboralesController(SwTHDbContext db)
        {
            this.db = db;
        }

        
        // GET: api/TrayectoriaLaboral
        [HttpGet]
        [Route("ListarTrayectoriasLaborales")]
        public async Task<List<TrayectoriaLaboral>> GetTrayectoriasLaborales()
        {
            try
            {
                return await db.TrayectoriaLaboral.Include(x => x.Persona).OrderBy(x => x.Empresa).ToListAsync();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new List<TrayectoriaLaboral>();
            }
        }

        // GET: api/TrayectoriaLaboral/5
        [HttpGet("{id}")]
        public async Task<Response> GetTrayectoriaLaboral([FromRoute] int id)
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

                var TrayectoriaLaboral = await db.TrayectoriaLaboral.SingleOrDefaultAsync(m => m.IdTrayectoriaLaboral == id);

                if (TrayectoriaLaboral == null)
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
                    Resultado = TrayectoriaLaboral,
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        // PUT: api/TrayectoriaLaboral/5
        [HttpPut("{id}")]
        public async Task<Response> PutTrayectoriaLaboral([FromRoute] int id, [FromBody] TrayectoriaLaboral trayectoriaLaboral)
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

                var existe = Existe(trayectoriaLaboral);
                var TrayectoriaLaboralActualizar = (TrayectoriaLaboral)existe.Resultado;
                if (existe.IsSuccess)
                {
                    if (TrayectoriaLaboralActualizar.IdTrayectoriaLaboral == trayectoriaLaboral.IdTrayectoriaLaboral)
                    {
                        return new Response
                        {
                            IsSuccess = true,
                        };
                    }
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ExisteRegistro,
                    };
                }
                var TrayectoriaLaboral = db.TrayectoriaLaboral.Find(trayectoriaLaboral.IdTrayectoriaLaboral);
                
                TrayectoriaLaboral.IdPersona = TrayectoriaLaboral.IdPersona;
                TrayectoriaLaboral.FechaInicio = TrayectoriaLaboral.FechaInicio;
                TrayectoriaLaboral.FechaFin = TrayectoriaLaboral.FechaFin;
                TrayectoriaLaboral.Empresa = TrayectoriaLaboral.Empresa;
                TrayectoriaLaboral.PuestoTrabajo = TrayectoriaLaboral.PuestoTrabajo;
                TrayectoriaLaboral.DescripcionFunciones = TrayectoriaLaboral.DescripcionFunciones;

                db.TrayectoriaLaboral.Update(TrayectoriaLaboral);

                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                };

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
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

        // POST: api/TrayectoriaLaboral
        [HttpPost]
        [Route("InsertarTrayectoriaLaboral")]
        public async Task<Response> PostTrayectoriaLaboral([FromBody] TrayectoriaLaboral TrayectoriaLaboral)
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

                var respuesta = Existe(TrayectoriaLaboral);
                if (!respuesta.IsSuccess)
                {
                    db.TrayectoriaLaboral.Add(TrayectoriaLaboral);
                    await db.SaveChangesAsync();
                    return new Response
                    {
                        IsSuccess = true,
                        Message = Mensaje.Satisfactorio
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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        // DELETE: api/TrayectoriaLaboral/5
        [HttpDelete("{id}")]
        public async Task<Response> DeleteTrayectoriaLaboral([FromRoute] int id)
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

                var respuesta = await db.TrayectoriaLaboral.SingleOrDefaultAsync(m => m.IdTrayectoriaLaboral == id);
                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.TrayectoriaLaboral.Remove(respuesta);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        private Response Existe(TrayectoriaLaboral TrayectoriaLaboral)
        {
            var fechaInicio = TrayectoriaLaboral.FechaInicio;
            var fechaFin = TrayectoriaLaboral.FechaFin;
            var TrayectoriaLaboralrespuesta = db.TrayectoriaLaboral.Where(p => p.FechaInicio == fechaInicio && p.FechaFin == fechaFin).FirstOrDefault();

            if (TrayectoriaLaboralrespuesta != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.ExisteRegistro,
                    Resultado = TrayectoriaLaboralrespuesta,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Resultado = TrayectoriaLaboralrespuesta,
            };
        }
    }
}