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
    [Route("api/DatosBancarios")]
    public class DatosBancariosController : Controller
    {
        private readonly SwTHDbContext db;

        public DatosBancariosController(SwTHDbContext db)
        {
            this.db = db;
        }


        // GET: api/DatosBancarios
        [HttpGet]
        [Route("ListarDatosBancarios")]
        public async Task<List<DatosBancarios>> GetDatosBancarios()
        {
            try
            {
                return await db.DatosBancarios.Include(x => x.Empleado).Include(x => x.InstitucionFinanciera).OrderBy(x => x.NumeroCuenta).ToListAsync();
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
                return new List<DatosBancarios>();
            }
        }

        

        [HttpPost]
        [Route("DatosBancariosPorIdEmpleado")]
        public async Task<Response> DatosBancariosPorIdEmpleado([FromBody] DatosBancarios datosBancarios)
        {
            try
            {
                var DatosBancarios = await db.DatosBancarios.SingleOrDefaultAsync(m => m.IdEmpleado == datosBancarios.IdEmpleado);

                var response = new Response
                {
                    IsSuccess = true,
                    Resultado = DatosBancarios,
                };

                return response;

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
                return new Response { };
            }
        }

        // GET: api/DatosBancarios/5
        [HttpGet("{id}")]
        public async Task<Response> GetDatosBancarios([FromRoute] int id)
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

                var DatosBancarios = await db.DatosBancarios.SingleOrDefaultAsync(m => m.IdDatosBancarios == id);

                if (DatosBancarios == null)
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
                    Resultado = DatosBancarios,
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

        // PUT: api/DatosBancarios/5
        [HttpPut("{id}")]
        public async Task<Response> PutDatosBancarios([FromRoute] int id, [FromBody] DatosBancarios datosBancarios)
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

                var existe = Existe(datosBancarios);
                var DatosBancariosActualizar = (DatosBancarios)existe.Resultado;
                if (existe.IsSuccess)
                {
                    if (DatosBancariosActualizar.IdDatosBancarios == datosBancarios.IdDatosBancarios)
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
                var DatosBancarios = db.DatosBancarios.Find(datosBancarios.IdDatosBancarios);
                
                DatosBancarios.IdInstitucionFinanciera = datosBancarios.IdInstitucionFinanciera;
                DatosBancarios.NumeroCuenta = datosBancarios.NumeroCuenta;
                DatosBancarios.Ahorros = datosBancarios.Ahorros;

                db.DatosBancarios.Update(DatosBancarios);
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

        // POST: api/BasesDatos
        [HttpPost]
        [Route("InsertarDatosBancarios")]
        public async Task<Response> PostDatosBancarios([FromBody] DatosBancarios DatosBancarios)
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

                var respuesta = Existe(DatosBancarios);
                if (!respuesta.IsSuccess)
                {
                    db.DatosBancarios.Add(DatosBancarios);
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

        // DELETE: api/BasesDatos/5
        [HttpDelete("{id}")]
        public async Task<Response> DeleteDatosBancarios([FromRoute] int id)
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

                var respuesta = await db.DatosBancarios.SingleOrDefaultAsync(m => m.IdDatosBancarios == id);
                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.DatosBancarios.Remove(respuesta);
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

        private Response Existe(DatosBancarios DatosBancarios)
        {
            var numerocuenta = DatosBancarios.NumeroCuenta.ToUpper().TrimEnd().TrimStart();
            var DatosBancariosrespuesta = db.DatosBancarios.Where(p => p.NumeroCuenta.ToUpper().TrimStart().TrimEnd() == numerocuenta && p.IdEmpleado == DatosBancarios.IdEmpleado && p.IdInstitucionFinanciera == DatosBancarios.IdInstitucionFinanciera).FirstOrDefault();
            if (DatosBancariosrespuesta != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.ExisteRegistro,
                    Resultado = DatosBancariosrespuesta,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Resultado = DatosBancariosrespuesta,
            };
        }
    }
}