using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using bd.swth.datos;
using bd.swth.entidades.Negocio;
using Microsoft.EntityFrameworkCore;
using bd.log.guardar.Servicios;
using bd.log.guardar.ObjectTranfer;
using bd.log.guardar.Enumeradores;
using bd.swth.entidades.Enumeradores;
using bd.swth.entidades.Utils;
using bd.swth.entidades.ViewModels;

namespace bd.swth.web.Controllers.API
{
    [Produces("application/json")]
    [Route("api/Sucursal")]
    public class SucursalController : Controller
    {
        private readonly SwTHDbContext db;

        public SucursalController(SwTHDbContext db)
        {
            this.db = db;
        }

        // GET: api/BasesDatos
        [HttpGet]
        [Route("ListarSucursal")]
        public async Task<List<Sucursal>> GetSucursal()
        {
            try
            {
                return await db.Sucursal.OrderBy(x => x.Nombre).Include(X => X.Ciudad).ThenInclude(x=> x.Provincia).ThenInclude(x=> x.Pais).ToListAsync();
            }
            catch (Exception ex)
            {
               
                return new List<Sucursal>();
            }
        }

        [HttpPost]
        [Route("ListarSucursalporCiudad")]
        public async Task<List<Sucursal>> GetSucursalbyCity([FromBody] Ciudad ciudad)
        {
            try
            {
                return await db.Sucursal.Include(c => c.Ciudad).Where(x => x.IdCiudad == ciudad.IdCiudad).OrderBy(x => x.Nombre).ToListAsync();
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
                return new List<Sucursal>();
            }
        }

        // GET: api/BasesDatos/5
        [HttpGet("{id}")]
        public async Task<Response> GetSucursal([FromRoute] int id)
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

                var sucursal = await db.Sucursal.Include(c=> c.Ciudad).ThenInclude(c=> c.Provincia).ThenInclude(c=> c.Pais).SingleOrDefaultAsync(m => m.IdSucursal == id);

                if (sucursal == null)
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
                    Resultado = sucursal,
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
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        // PUT: api/BasesDatos/5
        [HttpPut("{id}")]
        public async Task<Response> PutSucursal([FromRoute] int id, [FromBody] Sucursal sucursal)
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

                var sucursalActualizar = await db.Sucursal.Where(x => x.IdSucursal == id).FirstOrDefaultAsync();
                if (sucursalActualizar != null)
                {
                    try
                    {
                        var respuesta = Existe(sucursal);
                        if (!respuesta.IsSuccess)
                        {   sucursalActualizar.Nombre = sucursal.Nombre;
                            sucursalActualizar.IdCiudad = sucursal.IdCiudad;

                            db.Sucursal.Update(sucursalActualizar);
                            await db.SaveChangesAsync();

                            return new Response
                            {
                                IsSuccess = true,
                                Message = Mensaje.Satisfactorio,
                            };
                        }

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
                        //return new Response
                        //{
                        //    IsSuccess = false,
                        //    Message = Mensaje.Error,
                        //};
                    }
                }

                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                     Message = Mensaje.Excepcion
                };
            }
        }

        // POST: api/BasesDatos
        [HttpPost]
        [Route("InsertarSucursal")]
        public async Task<Response> PostSucursal([FromBody] Sucursal sucursal)
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

                var respuesta = Existe(sucursal);
                if (!respuesta.IsSuccess)
                {
                    db.Sucursal.Add(sucursal);
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
                    Message = Mensaje.ExisteRegistro
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
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        // DELETE: api/BasesDatos/5
        [HttpDelete("{id}")]
        public async Task<Response> DeleteSucursal([FromRoute] int id)
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

                var respuesta = await db.Sucursal.SingleOrDefaultAsync(m => m.IdSucursal == id);
                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.Sucursal.Remove(respuesta);
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
                    ExceptionTrace = ex.Message,
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

        private Response Existe(Sucursal sucursal)
        {
            var bdd = sucursal.Nombre.ToUpper().TrimEnd().TrimStart();
            var bdd1 = sucursal.IdCiudad;
            var sucursalrespuesta = db.Sucursal.Where(p => p.Nombre.ToUpper().TrimStart().TrimEnd() == bdd && p.IdCiudad==bdd1).FirstOrDefault();
            if (sucursalrespuesta != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.ExisteRegistro,
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Resultado = sucursalrespuesta,
            };
        }


        // GET: api/Sucursal
        [HttpPost]
        [Route("ObtenerSucursalPorEmpleado")]
        public async Task<Sucursal> ObtenerSucursalPorEmpleado([FromBody] IdFiltrosViewModel filtro)
        {
            try
            {
                var modelo = await db.Empleado
                    .Where(w=>w.NombreUsuario == filtro.NombreUsuario)
                    .Select(s=>s.Dependencia.Sucursal)
                    .FirstOrDefaultAsync();

                return modelo;
            }
            catch (Exception ex)
            {

                return new Sucursal();
            }
        }

    }
}