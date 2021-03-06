using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bd.swth.datos;
using bd.swth.entidades.Negocio;
using bd.swth.entidades.ObjectTransfer;
using bd.swth.entidades.Utils;
using bd.swth.entidades.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bd.swth.web.Controllers.API
{
    [Produces("application/json")]
    [Route("api/ConceptoNomina")]
    public class ConceptoNominaController : Controller
    {
        private readonly SwTHDbContext db;

        public ConceptoNominaController(SwTHDbContext db)
        {
            this.db = db;
        }

        [HttpPost]
        [Route("ListarConceptoNominaPorTipoRelacionDelEmpleado")]
        public async Task<List<ConceptoNomina>> ListarConceptoNominaPorTipoRelacionDelEmpleado([FromBody] Empleado empleado)
        {
            try
            {
                var relacionEmpleado =await db.Empleado.Where(e => e.IdEmpleado == empleado.IdEmpleado).FirstOrDefaultAsync();
                var lista= await db.ConceptoNomina.Where(x => x.Estatus =="Activo" && (x.RelacionLaboral==relacionEmpleado.TipoRelacion || x.RelacionLaboral=="AMBOS")).ToListAsync();
                return lista;
            }
            catch (Exception ex)
            {
                return new List<ConceptoNomina>();
            }
        }

        [HttpPost]
        [Route("ListarConceptoNomina")]
        public async Task<List<ConceptoNomina>> ListarConceptoNomina([FromBody] bool activo)
        {
            try
            {
                var estado = "";
                if (activo == true)
                {
                    estado = "Activo";
                }
                else { estado = "Inactivo"; }
                return await db.ConceptoNomina.Where(x=>x.Estatus==estado).ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<ConceptoNomina>();
            }
        }

        // GET: api/BasesDatos
        [HttpGet]
        [Route("ListarConceptoNomina")]
        public async Task<List<ConceptoNomina>> ListarConceptoNomina()
        {
            try
            {
                return await db.ConceptoNomina.Include(x => x.ProcesoNomina).ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<ConceptoNomina>();
            }
        }

        // GET: api/BasesDatos/5
        [HttpGet]
        [HttpPost("ObtenerConceptoNomina")]
        public async Task<Response> ObtenerConceptoNomina([FromBody] ConceptoNomina ConceptoNomina)
        {
            try
            {
                var conceptoNomina = await db.ConceptoNomina.SingleOrDefaultAsync(m => m.IdConcepto == ConceptoNomina.IdConcepto);

                if (conceptoNomina == null)
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
                    Resultado = conceptoNomina,
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }


        [HttpPost]
        [Route("InsertarReportadoNominaIndividual")]
        public async Task<Response> InsertarReportadoNominaIndividual([FromBody] AdicionarReportadoNominaViewModel adicionarReportado)
        {
            try
            {


                var concepto =await  db.ConceptoNomina.Where(x => x.IdConcepto == adicionarReportado.IdConcepto).FirstOrDefaultAsync();
                var empleado = await db.Empleado.Where(x => x.IdEmpleado == adicionarReportado.IdEmpleado).Include(x=>x.Persona).FirstOrDefaultAsync();

                var reportadoNomina = new ReportadoNomina
                {
                    IdCalculoNomina = adicionarReportado.IdCalculoNomina,
                    Importe = adicionarReportado.Valor,
                    NombreEmpleado = $"{empleado.Persona.Nombres + " " + empleado.Persona.Apellidos }",
                    IdentificacionEmpleado = $"{empleado.Persona.Identificacion}",
                    CodigoConcepto = concepto.Codigo,
                };

                await db.ReportadoNomina.AddAsync(reportadoNomina);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                };
            }
        }


        [HttpPost]
        [Route("InsertarDiasLaboradosNomina")]
        public async Task<Response> InsertarDiasLaboradosNomina([FromBody] List<DiasLaboradosNomina> listaSalvar)
        {
            try
            {
                var ListaDiasReportadosEliminar =await db.DiasLaboradosNomina.Where(x => x.IdCalculoNomina == listaSalvar.FirstOrDefault().IdCalculoNomina).ToListAsync();
                db.DiasLaboradosNomina.RemoveRange(ListaDiasReportadosEliminar);
                await db.SaveChangesAsync();
                await db.DiasLaboradosNomina.AddRangeAsync(listaSalvar);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                };
            }
        }

        [HttpPost]
        [Route("InsertarHorasExtrasNomina")]
        public async Task<Response> InsertarHorasExtrasNomina([FromBody] List<HorasExtrasNomina> listaSalvar)
        {
            try
            {
                await db.HorasExtrasNomina.AddRangeAsync(listaSalvar);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                };
            }
        }


        [HttpPost]
        [Route("InsertarHorasExtrasNominaPorEmpleado")]
        public async Task<Response> InsertarHorasExtrasNominaPorEmpleado([FromBody] HorasExtrasNomina horasExtrasNomina)
        {
            try
            {

                var empleado = await db.Empleado.Where(x => x.IdEmpleado == horasExtrasNomina.IdEmpleado).Include(x=>x.Persona).FirstOrDefaultAsync();
                horasExtrasNomina.IdentificacionEmpleado =empleado.Persona.Identificacion ;
                db.HorasExtrasNomina.Add(horasExtrasNomina);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                };
            }
        }


        [HttpPost]
        [Route("EliminarReportado")]
        public async Task<Response> EliminarReportado([FromBody] ReportadoNomina reportadoNomina)
        {
            try
            {
                var reportadoEliminar = await db.ReportadoNomina.Where(x => x.IdReportadoNomina == reportadoNomina.IdReportadoNomina).FirstOrDefaultAsync();
                db.ReportadoNomina.Remove(reportadoEliminar);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                };
            }
        }

        [HttpPost]
        [Route("EliminarDiasLaborados")]
        public async Task<Response> EliminarDiasLaborados([FromBody] DiasLaboradosNomina diasLaboradosNomina)
        {
            try
            {
                var diasLaboradosEliminar = await db.DiasLaboradosNomina.Where(x => x.IdDiasLaboradosNomina == diasLaboradosNomina.IdDiasLaboradosNomina).FirstOrDefaultAsync();
                db.DiasLaboradosNomina.Remove(diasLaboradosEliminar);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                };
            }
        }


        [HttpPost]
        [Route("EliminarHoraExtra")]
        public async Task<Response> EliminarHoraExtra([FromBody] HorasExtrasNomina horasExtrasNomina)
        {
            try
            {
                var horaExtraEliminar =await db.HorasExtrasNomina.Where(x => x.IdHorasExtrasNomina == horasExtrasNomina.IdHorasExtrasNomina).FirstOrDefaultAsync();
                db.HorasExtrasNomina.Remove(horaExtraEliminar);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                };
            }
        }



        [HttpPost]
        [Route("InsertarReportadoNomina")]
        public async Task<Response> InsertarReportadoNomina([FromBody] List<ReportadoNomina> listaSalvar)
        {
            try
            {
                var listadoBorrar = await db.ReportadoNomina.Where(x => x.IdCalculoNomina == listaSalvar.FirstOrDefault().IdCalculoNomina).ToListAsync();

                db.ReportadoNomina.RemoveRange(listadoBorrar);
                await db.SaveChangesAsync();
                    await db.ReportadoNomina.AddRangeAsync(listaSalvar);
                    await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                };
            }
        }


        [HttpPost]
        [Route("VerificarExcelDiasLaborados")]
        public async Task<List<DiasLaboradosNomina>> VerificarExcelDiasLaborados([FromBody] List<DiasLaboradosNomina> lista)
        {
            try
            {

                foreach (var item in lista)
                {
                    var empleado = await db.Empleado.Where(x => x.Activo == true && x.Persona.Identificacion == item.IdentificacionEmpleado).FirstOrDefaultAsync();

                    if (empleado == null)
                    {
                        item.Valido = false;
                        item.MensajeError = Mensaje.EmpleadoNoExiste;
                    }
                    else
                    {
                        item.Valido = true;
                        item.IdEmpleado = empleado.IdEmpleado;
                    }
                }
                return lista;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("VerificarExcelHorasExtras")]
        public async Task<List<HorasExtrasNomina>> VerificarExcelHorasExtras([FromBody] List<HorasExtrasNomina> lista)
        {
            try
            {
                
                foreach (var item in lista)
                {
                    var empleado = await db.Empleado.Where(x => x.Activo == true && x.Persona.Identificacion == item.IdentificacionEmpleado).FirstOrDefaultAsync();

                    if (empleado == null)
                    {
                        item.Valido = false;
                        item.MensajeError = Mensaje.EmpleadoNoExiste;
                    }
                    else
                    {
                        item.Valido = true;
                        item.IdEmpleado = empleado.IdEmpleado;
                    }
                }
                return lista;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        [Route("VerificarExcel")]
        public async Task<List<ReportadoNomina>> VerificarExcel([FromBody] List<ReportadoNomina> lista)
        {
            try
            {
                var proceso =  db.CalculoNomina.Where(x => x.IdCalculoNomina == lista.FirstOrDefault().IdCalculoNomina).FirstOrDefaultAsync().Result.IdProceso;

                foreach (var item in lista)
                {
                    var empleado = await db.Empleado.Where(x => x.Activo == true && x.Persona.Identificacion == item.IdentificacionEmpleado).FirstOrDefaultAsync();

                    if (empleado==null)
                    {
                        item.Valido = false;
                        item.MensajeError = Mensaje.EmpleadoNoExiste;
                    }

                    var concepto =await db.ConceptoNomina.Where(x => x.IdProceso == proceso && x.Codigo == item.CodigoConcepto).FirstOrDefaultAsync();
                    if (concepto==null)
                    {
                        item.Valido = false;
                        item.MensajeError = string.Format("{0} {1}", item.MensajeError, Mensaje.ConceptoNoExiste);
                    }
                }
                return lista;
                     
            }
            catch (Exception ex)
            {
              throw;
            }
        }

        [HttpPost]
        [Route("EditarFormula")]
        public async Task<Response> EditarFormula([FromBody] ConceptoNomina ConceptoNomina)
        {
            try
            {
                if (await Existe(ConceptoNomina))
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ExisteRegistro,
                    };
                }

                var ConceptoNominaActualizar = await db.ConceptoNomina.Where(x => x.IdConcepto == ConceptoNomina.IdConcepto).FirstOrDefaultAsync();
                if (ConceptoNominaActualizar == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                    };

                }

                ConceptoNominaActualizar.FormulaCalculo = ConceptoNomina.FormulaCalculo;
                db.ConceptoNomina.Update(ConceptoNominaActualizar);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Resultado = ConceptoNominaActualizar
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

        // PUT: api/BasesDatos/5
        [HttpPost]
        [Route("EditarConceptoNomina")]
        public async Task<Response> EditarConceptoNomina([FromBody] ConceptoNomina ConceptoNomina)
        {
            try
            {
                if (await Existe(ConceptoNomina))
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ExisteRegistro,
                    };
                }

                var ConceptoNominaActualizar = await db.ConceptoNomina.Where(x => x.IdConcepto == ConceptoNomina.IdConcepto).FirstOrDefaultAsync();
                if (ConceptoNominaActualizar == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                    };

                }

                ConceptoNominaActualizar.IdProceso = ConceptoNomina.IdProceso;
                ConceptoNominaActualizar.Codigo = ConceptoNomina.Codigo;
                ConceptoNominaActualizar.Descripcion = ConceptoNomina.Descripcion;
                ConceptoNominaActualizar.TipoConcepto = ConceptoNomina.TipoConcepto;
                ConceptoNominaActualizar.RelacionLaboral = ConceptoNomina.RelacionLaboral;
                ConceptoNominaActualizar.Estatus = ConceptoNomina.Estatus;
                ConceptoNominaActualizar.Abreviatura = ConceptoNomina.Abreviatura;
                ConceptoNominaActualizar.Prioridad = ConceptoNomina.Prioridad;

                db.ConceptoNomina.Update(ConceptoNominaActualizar);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Resultado = ConceptoNominaActualizar
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
        [Route("InsertarConceptoNomina")]
        public async Task<Response> PostConceptoNomina([FromBody] ConceptoNomina ConceptoNomina)
        {
            try
            {

                if (!await Existe(ConceptoNomina))
                {
                    db.ConceptoNomina.Add(ConceptoNomina);
                    await db.SaveChangesAsync();
                    return new Response
                    {
                        IsSuccess = true,
                        Message = Mensaje.Satisfactorio,
                        Resultado = ConceptoNomina,
                    };
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
                    Message = Mensaje.Error,
                };
            }
        }

        // DELETE: api/BasesDatos/5
        [HttpPost]
        [Route("EliminarConceptoNomina")]
        public async Task<Response> EliminarConceptoNomina([FromBody]ConceptoNomina ConceptoNomina)
        {
            try
            {
                var respuesta = await db.ConceptoNomina.Where(m => m.IdConcepto == ConceptoNomina.IdConcepto).FirstOrDefaultAsync();
                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.ConceptoNomina.Remove(respuesta);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                };
            }
            catch (Exception ex)
            {
                if (ex.InnerException.InnerException.Message.Contains("FK"))
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.BorradoNoSatisfactorio,
                    };
                }
                else
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = ex.InnerException.InnerException.Message,
                    };
                }

            }
        }

        private async Task<bool> Existe(ConceptoNomina ConceptoNomina)
        {
            var codigo = ConceptoNomina.Codigo;
            var ConceptoNominarespuesta = await db.ConceptoNomina.Where(p => p.Codigo == codigo).FirstOrDefaultAsync();

            if (ConceptoNominarespuesta == null || ConceptoNominarespuesta.IdConcepto == ConceptoNomina.IdConcepto)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}