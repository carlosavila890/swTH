using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bd.swth.datos;
using bd.swth.entidades.Negocio;
using bd.log.guardar.Servicios;
using bd.log.guardar.ObjectTranfer;
using bd.swth.entidades.Enumeradores;
using bd.log.guardar.Enumeradores;
using bd.swth.entidades.Utils;
using bd.swth.entidades.ViewModels;

namespace bd.swth.web.Controllers.API
{
    [Produces("application/json")]
    [Route("api/Estudios")]
    public class EstudiosController : Controller
    {
        private readonly SwTHDbContext db;

        public EstudiosController(SwTHDbContext db)
        {
            this.db = db;
        }


        [HttpPost]
        [Route("ListarEstudiosPorIndiceOcupacional")]
        public async Task<List<EstudioViewModel>> ListarEstudiosPorIndiceOcupacional([FromBody]IndiceOcupacional indiceOcupacional)
        {
            var ListaEstudios = await db.IndiceOcupacionalEstudio
                                                 .Join(db.IndiceOcupacional
                                                 , rta => rta.IdIndiceOcupacional, ind => ind.IdIndiceOcupacional,
                                                 (rta, ind) => new { hm = rta, gh = ind })
                                                 .Join(db.Estudio
                                                 , ind_1 => ind_1.hm.Estudio.IdEstudio, valor => valor.IdEstudio,
                                                 (ind_1, valor) => new { ca = ind_1, rt = valor })
                                                 .Where(ds => ds.ca.hm.IdIndiceOcupacional == indiceOcupacional.IdIndiceOcupacional)
                                                 .Select(t => new EstudioViewModel
                                                 {
                                                     IdEstudio = t.rt.IdEstudio,
                                                     Nombre = t.rt.Nombre,
                                                     IdIndiceOcupacional=indiceOcupacional.IdIndiceOcupacional
                                                 })
                                                 .ToListAsync();


            if (ListaEstudios.Count == 0)
            {
                ListaEstudios.Add(new EstudioViewModel { IdIndiceOcupacional = indiceOcupacional.IdIndiceOcupacional, IdEstudio = -1 });
            }

            return ListaEstudios;
        }




        [HttpPost]
        [Route("ListarEstudiosNoAsignadasIndiceOcupacional")]
        public async Task<List<EstudioViewModel>> ListarEstudiosNoAsignadasIndiceOcupacional([FromBody]IndiceOcupacional indiceOcupacional)
        {

                var Lista = await db.Estudio
                                   .Where(e => !db.IndiceOcupacionalEstudio
                                                   .Where(a => a.IndiceOcupacional.IdIndiceOcupacional == indiceOcupacional.IdIndiceOcupacional)
                                                   .Select(ioe => ioe.IdEstudio)
                                                   .Contains(e.IdEstudio))
                                          .ToListAsync();

            var listaSalida = new List<EstudioViewModel>();
            if (Lista.Count == 0)
            {
                listaSalida.Add(new EstudioViewModel { IdIndiceOcupacional = indiceOcupacional.IdIndiceOcupacional, IdEstudio = -1 });
            }

            else
            {
                foreach (var item in Lista)
                {
                    listaSalida.Add(new EstudioViewModel
                    {
                        Nombre = item.Nombre,
                        IdEstudio = item.IdEstudio,
                        IdIndiceOcupacional = indiceOcupacional.IdIndiceOcupacional,
                    });
                }
            }

            return listaSalida;

        }

        // GET: api/BasesDatos
        [HttpGet]
        [Route("ListarEstudios")]
        public async Task<List<Estudio>> GetEstudios()
        {
            try
            {
                return await db.Estudio.OrderBy(x => x.Nombre).ToListAsync();
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
                return new List<Estudio>();
            }
        }


        [HttpPost]
        [Route("EliminarIncideOcupacionalEstudio")]
        public async Task<Response> EliminarIncideOcupacionalEstudio([FromBody] IndiceOcupacionalEstudio indiceOcupacionalEstudio)
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

                var respuesta = await db.IndiceOcupacionalEstudio.SingleOrDefaultAsync(m => m.IdEstudio == indiceOcupacionalEstudio.IdEstudio
                                      && m.IdIndiceOcupacional == indiceOcupacionalEstudio.IdIndiceOcupacional);
                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.IndiceOcupacionalEstudio.Remove(respuesta);
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


        // GET: api/BasesDatos/5
        [HttpGet("{id}")]
        public async Task<Response> GetEstudio([FromRoute] int id)
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

                var Estudio = await db.Estudio.SingleOrDefaultAsync(m => m.IdEstudio == id);

                if (Estudio == null)
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
                    Resultado = Estudio,
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
        public async Task<Response> PutEstudio([FromRoute] int id, [FromBody] Estudio Estudio)
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

                var existe = Existe(Estudio);
                if (existe.IsSuccess)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ExisteRegistro,
                    };
                }

                var EstudioActualizar = await db.Estudio.Where(x => x.IdEstudio == id).FirstOrDefaultAsync();
                if (EstudioActualizar != null)
                {
                    try
                    {

                        EstudioActualizar.Nombre = Estudio.Nombre;
                        db.Estudio.Update(EstudioActualizar);
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




                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.ExisteRegistro
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
        [Route("InsertarEstudio")]
        public async Task<Response> PostEstudio([FromBody] Estudio Estudio)
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

                var respuesta = Existe(Estudio);
                if (!respuesta.IsSuccess)
                {
                    db.Estudio.Add(Estudio);
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
        public async Task<Response> DeleteEstudio([FromRoute] int id)
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

                var respuesta = await db.Estudio.SingleOrDefaultAsync(m => m.IdEstudio == id);
                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.Estudio.Remove(respuesta);
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

        private Response Existe(Estudio Estudio)
        {
            var bdd = Estudio.Nombre.ToUpper().TrimEnd().TrimStart();
            var Estudiorespuesta = db.Estudio.Where(p => p.Nombre.ToUpper().TrimStart().TrimEnd() == bdd).FirstOrDefault();
            if (Estudiorespuesta != null)
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
                Resultado = Estudiorespuesta,
            };
        }
    }
}