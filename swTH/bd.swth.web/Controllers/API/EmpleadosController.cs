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
using bd.swth.entidades.Utils;
using bd.log.guardar.Enumeradores;
using bd.swth.entidades.ObjectTransfer;
using bd.swth.entidades.ViewModels;
using MoreLinq;
using bd.swth.entidades.Constantes;
using System.Linq.Expressions;

namespace bd.swth.web.Controllers.API
{

    [Produces("application/json")]
    [Route("api/Empleados")]
    public class EmpleadosController : Controller
    {
        private readonly SwTHDbContext db;

        public EmpleadosController(SwTHDbContext db)
        {
            this.db = db;
        }


        private IQueryable<DatosBasicosEmpleadoViewModel> ListaDatosBasicosEmpleado()
        {

            try
            {
                var query = db.Empleado.Select(x => new DatosBasicosEmpleadoViewModel
                {
                    Identificacion = x.Persona.Identificacion,
                    Nombres = x.Persona.Nombres,
                    Apellidos = x.Persona.Apellidos,
                    IdEmpleado = x.IdEmpleado,
                    Activo = x.Activo,
                    AcumulaDecimos = x.AcumulaDecimos,
                    FondosReservas = x.FondosReservas,
                });

                return query;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private IQueryable<Empleado> Empleados()
        {

            try
            {
                var query = db.Empleado;
                return query;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        [Route("ListaEmpleadosPorEstado")]
        public async Task<List<DatosBasicosEmpleadoViewModel>> ListaEmpleadosPorEstado([FromBody] Empleado empleado)
        {

            if (empleado.Activo == true)
            {
                var listaResultado = await ListarEmpleadoDatosBasicos(filtro: x => x.Activo == true).ToListAsync();
                return listaResultado;
            }
            else
            {
                var listaResultado = await ListarEmpleadoDatosBasicos(filtro: x => x.Activo.Equals(false)).ToListAsync();
                return listaResultado;
            }

        }


        [HttpPost]
        [Route("CambiarEstadoFondosReservas")]
        public async Task<Response> CambiarEstadoFondosReservas([FromBody] Empleado empleado)
        {

            try
            {
                var empleadoActualizar = await ObtenerEmpleadoFiltro(filtro: x => x.IdEmpleado == empleado.IdEmpleado).FirstOrDefaultAsync();
                empleadoActualizar.FondosReservas = empleado.FondosReservas;
                await db.SaveChangesAsync();
                return new Response { IsSuccess = true };
            }
            catch (Exception)
            {
                return new Response { IsSuccess = false };
            }
        }


        [HttpPost]
        [Route("CambiarEstadoAcumulaDecimos")]
        public async Task<Response> CambiarEstadoAcumulaDecimos([FromBody] Empleado empleado)
        {

            try
            {
                var empleadoActualizar = await ObtenerEmpleadoFiltro(filtro: x => x.IdEmpleado == empleado.IdEmpleado).FirstOrDefaultAsync();

                empleadoActualizar.AcumulaDecimos = empleado.AcumulaDecimos;
                await db.SaveChangesAsync();

                return new Response { IsSuccess = true };
            }
            catch (Exception)
            {
                return new Response { IsSuccess = false };
            }
        }

        private IQueryable<Empleado> ObtenerEmpleadoFiltro(Expression<Func<Empleado, bool>> filtro = null)
        {
            try
            {
                var empleado = ((filtro != null ? Empleados().Where(filtro) : Empleados()));
                return empleado;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private IQueryable<DatosBasicosEmpleadoViewModel> ListarEmpleadoDatosBasicos(Expression<Func<DatosBasicosEmpleadoViewModel, bool>> filtro = null)
        {
            try
            {
                var lista = ((filtro != null ? ListaDatosBasicosEmpleado().Where(filtro) : ListaDatosBasicosEmpleado()));
                return lista;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        [HttpPost]
        [Route("ExisteEmpleadoPorIdentificacion")]
        public async Task<Response> ExisteEmpleadoPorIdentificacion([FromBody] Persona Persona)
        {
            try
            {
                var empleado = await db.Empleado.Where(x => x.Persona.Identificacion == Persona.Identificacion && x.Activo == true).FirstOrDefaultAsync();
                if (empleado != null)
                {
                    return new Response { IsSuccess = true };
                }
                return new Response { IsSuccess = false };

            }
            catch (Exception)
            {

                return new Response { IsSuccess = false, Message = Mensaje.Error }; ;
            }
        }
        [HttpPost]
        [Route("ObtenerEncabezadoEmpleadosFao")]
        public async Task<Response> ObtenerEncabezadoEmpleadosFao([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {
                var empleado = await db.Empleado.Include(x => x.FormularioAnalisisOcupacional).Where(x => x.NombreUsuario == documentoFAOViewModel.NombreUsuario && x.FormularioAnalisisOcupacional.FirstOrDefault().Estado == -1).Select(x => new DocumentoFAOViewModel
                {
                    IdEmpleado = x.IdEmpleado,
                    apellido = x.Persona.Apellidos,
                    nombre = x.Persona.Nombres,
                    Identificacion = x.Persona.Identificacion,
                    UnidadAdministrativa = x.Dependencia.Nombre,
                    LugarTrabajo = x.Persona.LugarTrabajo,
                    Institucion = x.Persona.LugarTrabajo,
                    //Puesto =, 
                }).FirstOrDefaultAsync();
                if (empleado != null)
                {
                    return new Response { IsSuccess = true, Resultado = empleado };
                }
                return new Response { IsSuccess = false };

            }
            catch (Exception ex)
            {

                return new Response { IsSuccess = false, Message = Mensaje.Error }; ;
            }
        }
        [HttpPost]
        [Route("ObtenerEncabezadoEmpleadosFaoValidarConActividades")]
        public async Task<Response> ObtenerEncabezadoEmpleadosFaoValidarConActividades([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {

                var b = db.ActividadesAnalisisOcupacional.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();

                var empleado = await db.Empleado.Where(x => x.IdEmpleado == documentoFAOViewModel.IdEmpleado && x.FormularioAnalisisOcupacional.FirstOrDefault().Estado == EstadosFAO.RealizadoEmpleado).Select(x => new DocumentoFAOViewModel
                {
                    apellido = x.Persona.Apellidos,
                    nombre = x.Persona.Nombres + " " + x.Persona.Apellidos,
                    Identificacion = x.Persona.Identificacion,
                    UnidadAdministrativa = x.Dependencia.Nombre,
                    LugarTrabajo = x.Persona.LugarTrabajo,
                    Institucion = x.Persona.LugarTrabajo,
                    Mision = x.FormularioAnalisisOcupacional.FirstOrDefault().MisionPuesto,
                    InternoMismoProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoMismoProceso,
                    InternoOtroProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoOtroProceso,
                    ExternosCiudadania = x.FormularioAnalisisOcupacional.FirstOrDefault().ExternosCiudadania,
                    ExtPersJuridicasPubNivelNacional = x.FormularioAnalisisOcupacional.FirstOrDefault().ExtPersJuridicasPubNivelNacional,
                    ListaActividad = b,
                    //ListaExepcion= exep,                 
                    //Puesto =, 
                }).FirstOrDefaultAsync();

                return new Response { IsSuccess = true, Resultado = empleado };

            }
            catch (Exception ex)
            {

                return new Response { IsSuccess = false, Message = Mensaje.Error }; ;
            }
        }
        [HttpPost]
        [Route("ObtenerEncabezadoEmpleadosFaoValidarConExepcionesVisualizarJefe")]
        public async Task<Response> ObtenerEncabezadoEmpleadosFaoValidarConExepcionesVisualizarJefe([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {
                //var puesto = db.RolPuesto.ToList();
                var a = db.ValidacionInmediatoSuperior.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();
                var exep = db.Exepciones.Where(x => x.IdValidacionJefe == a.FirstOrDefault().IdValidacionJefe).ToList();
                var b = db.ActividadesAnalisisOcupacional.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();

                var empleado = await db.Empleado.Where(x => x.IdEmpleado == documentoFAOViewModel.IdEmpleado && (x.FormularioAnalisisOcupacional.FirstOrDefault().Estado == EstadosFAO.RealizadoEspecialistaTH || x.FormularioAnalisisOcupacional.FirstOrDefault().Estado == EstadosFAO.RealizadoJefeTH)).Select(x => new DocumentoFAOViewModel
                {
                    IdEmpleado = x.IdEmpleado,
                    apellido = x.Persona.Apellidos,
                    nombre = x.Persona.Nombres + " " + x.Persona.Apellidos,
                    Identificacion = x.Persona.Identificacion,
                    UnidadAdministrativa = x.Dependencia.Nombre,
                    LugarTrabajo = x.Persona.LugarTrabajo,
                    Institucion = x.Persona.LugarTrabajo,
                    Mision = x.FormularioAnalisisOcupacional.FirstOrDefault().MisionPuesto,
                    InternoMismoProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoMismoProceso,
                    InternoOtroProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoOtroProceso,
                    ExternosCiudadania = x.FormularioAnalisisOcupacional.FirstOrDefault().ExternosCiudadania,
                    ExtPersJuridicasPubNivelNacional = x.FormularioAnalisisOcupacional.FirstOrDefault().ExtPersJuridicasPubNivelNacional,
                    ListaActividad = b,
                    ListaExepcion = exep,
                    //ListasRolPUestos=puesto
                    //Puesto =, 
                }).FirstOrDefaultAsync();

                return new Response { IsSuccess = true, Resultado = empleado };

            }
            catch (Exception ex)
            {

                return new Response { IsSuccess = false, Message = Mensaje.Error }; ;
            }
        }
        [HttpPost]
        [Route("ObtenerEncabezadoEmpleadosFaoValidarConExepciones")]
        public async Task<Response> ObtenerEncabezadoEmpleadosFaoValidarConExepciones([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {
                var puesto = db.RolPuesto.ToList();
                var a = db.ValidacionInmediatoSuperior.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();
                var exep = db.Exepciones.Where(x => x.IdValidacionJefe == a.FirstOrDefault().IdValidacionJefe).ToList();
                var b = db.ActividadesAnalisisOcupacional.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();

                var empleado = await db.Empleado.Where(x => x.IdEmpleado == documentoFAOViewModel.IdEmpleado && x.FormularioAnalisisOcupacional.FirstOrDefault().Estado == 1).Select(x => new DocumentoFAOViewModel
                {
                    IdEmpleado = x.IdEmpleado,
                    apellido = x.Persona.Apellidos,
                    nombre = x.Persona.Nombres + " " + x.Persona.Apellidos,
                    Identificacion = x.Persona.Identificacion,
                    UnidadAdministrativa = x.Dependencia.Nombre,
                    LugarTrabajo = x.Persona.LugarTrabajo,
                    Institucion = x.Persona.LugarTrabajo,
                    Mision = x.FormularioAnalisisOcupacional.FirstOrDefault().MisionPuesto,
                    InternoMismoProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoMismoProceso,
                    InternoOtroProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoOtroProceso,
                    ExternosCiudadania = x.FormularioAnalisisOcupacional.FirstOrDefault().ExternosCiudadania,
                    ExtPersJuridicasPubNivelNacional = x.FormularioAnalisisOcupacional.FirstOrDefault().ExtPersJuridicasPubNivelNacional,
                    ListaActividad = b,
                    ListaExepcion = exep,
                    ListasRolPUestos = puesto
                    //Puesto =, 
                }).FirstOrDefaultAsync();

                return new Response { IsSuccess = true, Resultado = empleado };

            }
            catch (Exception ex)
            {

                return new Response { IsSuccess = false, Message = Mensaje.Error }; ;
            }
        }
        [HttpPost]
        [Route("ObtenerEncabezadoEmpleadosFaoValidarConValidacionRH")]
        public async Task<Response> ObtenerEncabezadoEmpleadosFaoValidarConValidacionRH([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {

                var rh = db.AdministracionTalentoHumano.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).FirstOrDefault();
                var puesto = db.RolPuesto.Where(x => x.IdRolPuesto == rh.IdRolPuesto).ToList();
                var a = db.ValidacionInmediatoSuperior.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();
                var exep = db.Exepciones.Where(x => x.IdValidacionJefe == a.FirstOrDefault().IdValidacionJefe).ToList();
                var b = db.ActividadesAnalisisOcupacional.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();


                var empleado = await db.Empleado.Where(x => x.IdEmpleado == documentoFAOViewModel.IdEmpleado && x.FormularioAnalisisOcupacional.FirstOrDefault().Estado == EstadosFAO.RealizadoEspecialistaTH).Select(x => new DocumentoFAOViewModel
                {
                    IdEmpleado = x.IdEmpleado,
                    apellido = x.Persona.Apellidos,
                    nombre = x.Persona.Nombres + " " + x.Persona.Apellidos,
                    Identificacion = x.Persona.Identificacion,
                    UnidadAdministrativa = x.Dependencia.Nombre,
                    LugarTrabajo = x.Persona.LugarTrabajo,
                    Institucion = x.Persona.LugarTrabajo,
                    Mision = x.FormularioAnalisisOcupacional.FirstOrDefault().MisionPuesto,
                    InternoMismoProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoMismoProceso,
                    InternoOtroProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoOtroProceso,
                    ExternosCiudadania = x.FormularioAnalisisOcupacional.FirstOrDefault().ExternosCiudadania,
                    ExtPersJuridicasPubNivelNacional = x.FormularioAnalisisOcupacional.FirstOrDefault().ExtPersJuridicasPubNivelNacional,
                    ListaActividad = b,
                    ListaExepcion = exep,
                    ListasRolPUestos = puesto,
                    aplicapolitica = rh.SeAplicaraPolitica,
                    Cumple = rh.Cumple,
                    Descripcionpuesto = rh.Descripcion
                    //Puesto =, 
                }).FirstOrDefaultAsync();

                return new Response { IsSuccess = true, Resultado = empleado };

            }
            catch (Exception ex)
            {

                return new Response { IsSuccess = false, Message = Mensaje.Error }; ;
            }
        }

        [Route("ObtenerEncabezadoEmpleadosFaoValidarConValidacionJefeTH")]
        public async Task<Response> ObtenerEncabezadoEmpleadosFaoValidarConValidacionJefeTH([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {
                var rh = db.AdministracionTalentoHumano.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).FirstOrDefault();
                var puesto = db.RolPuesto.Where(x => x.IdRolPuesto == rh.IdRolPuesto).ToList();
                var manualPuesto = await db.ManualPuesto.ToListAsync();
                var a = db.ValidacionInmediatoSuperior.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();
                var exep = db.Exepciones.Where(x => x.IdValidacionJefe == a.FirstOrDefault().IdValidacionJefe).ToList();
                var b = db.ActividadesAnalisisOcupacional.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();
                var PuestoActual = db.IndiceOcupacionalModalidadPartida.OrderByDescending(x => x.Fecha).Where(x => x.IdEmpleado == documentoFAOViewModel.IdEmpleado).Select(v => new ManualPuesto
                {
                    IdManualPuesto = v.IndiceOcupacional.ManualPuesto.IdManualPuesto,
                    Nombre = v.IndiceOcupacional.ManualPuesto.Nombre
                }).FirstOrDefault();
                var empleado = await db.Empleado.Where(x => x.IdEmpleado == documentoFAOViewModel.IdEmpleado && x.FormularioAnalisisOcupacional.FirstOrDefault().Estado == EstadosFAO.RealizadoEspecialistaTH).Select(x => new DocumentoFAOViewModel
                {
                    IdEmpleado = x.IdEmpleado,
                    apellido = x.Persona.Apellidos,
                    nombre = x.Persona.Nombres + " " + x.Persona.Apellidos,
                    Identificacion = x.Persona.Identificacion,
                    UnidadAdministrativa = x.Dependencia.Nombre,
                    LugarTrabajo = x.Persona.LugarTrabajo,
                    Institucion = x.Persona.LugarTrabajo,
                    Mision = x.FormularioAnalisisOcupacional.FirstOrDefault().MisionPuesto,
                    InternoMismoProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoMismoProceso,
                    InternoOtroProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoOtroProceso,
                    ExternosCiudadania = x.FormularioAnalisisOcupacional.FirstOrDefault().ExternosCiudadania,
                    ExtPersJuridicasPubNivelNacional = x.FormularioAnalisisOcupacional.FirstOrDefault().ExtPersJuridicasPubNivelNacional,
                    ListaActividad = b,
                    ListaExepcion = exep,
                    ListasRolPUestos = puesto,
                    aplicapolitica = rh.SeAplicaraPolitica,
                    Cumple = rh.Cumple,
                    IdAdministracionTalentoHumano = rh.IdAdministracionTalentoHumano,
                    Descripcionpuesto = rh.Descripcion,
                    ListasManualPuesto = manualPuesto,
                    IdManualPuestoActual = PuestoActual.IdManualPuesto,
                    Puesto = PuestoActual.Nombre


                }).FirstOrDefaultAsync();

                return new Response { IsSuccess = true, Resultado = empleado };

            }
            catch (Exception ex)
            {

                return new Response { IsSuccess = false, Message = Mensaje.Error }; ;
            }
        }

        #region informe final FAO

        [Route("InformeFinalFAO")]
        public async Task<Response> InformeFinalFAO([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {
                var rh = db.AdministracionTalentoHumano.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).FirstOrDefault();
                var puesto = db.RolPuesto.Where(x => x.IdRolPuesto == rh.IdRolPuesto).ToList();
                var manualPuesto = await db.ManualPuesto.ToListAsync();
                var a = db.ValidacionInmediatoSuperior.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();
                var exep = db.Exepciones.Where(x => x.IdValidacionJefe == a.FirstOrDefault().IdValidacionJefe).ToList();
                var b = db.ActividadesAnalisisOcupacional.Where(x => x.IdFormularioAnalisisOcupacional == documentoFAOViewModel.IdFormularioAnalisisOcupacional).ToList();
                var informeUTH = db.InformeUATH.Where(x => x.IdAdministracionTalentoHumano == rh.IdAdministracionTalentoHumano).FirstOrDefault();
                var PuestoActual = db.IndiceOcupacionalModalidadPartida.OrderByDescending(x => x.Fecha).Where(x => x.IdEmpleado == documentoFAOViewModel.IdEmpleado).Select(v => new ManualPuesto
                {
                    IdManualPuesto = v.IndiceOcupacional.ManualPuesto.IdManualPuesto,
                    Nombre = v.IndiceOcupacional.ManualPuesto.Nombre
                }).FirstOrDefault();
                var puestodesignado = db.ManualPuesto.Where(x => x.IdManualPuesto == informeUTH.IdManualPuestoDestino).FirstOrDefault();
                var empleado = await db.Empleado.Where(x => x.IdEmpleado == documentoFAOViewModel.IdEmpleado && x.FormularioAnalisisOcupacional.FirstOrDefault().Estado == EstadosFAO.RealizadoJefeTH).Select(x => new DocumentoFAOViewModel
                {
                    IdEmpleado = x.IdEmpleado,
                    apellido = x.Persona.Apellidos,
                    nombre = x.Persona.Nombres + " " + x.Persona.Apellidos,
                    Identificacion = x.Persona.Identificacion,
                    UnidadAdministrativa = x.Dependencia.Nombre,
                    LugarTrabajo = x.Persona.LugarTrabajo,
                    Institucion = x.Persona.LugarTrabajo,
                    Mision = x.FormularioAnalisisOcupacional.FirstOrDefault().MisionPuesto,
                    InternoMismoProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoMismoProceso,
                    InternoOtroProceso = x.FormularioAnalisisOcupacional.FirstOrDefault().InternoOtroProceso,
                    ExternosCiudadania = x.FormularioAnalisisOcupacional.FirstOrDefault().ExternosCiudadania,
                    ExtPersJuridicasPubNivelNacional = x.FormularioAnalisisOcupacional.FirstOrDefault().ExtPersJuridicasPubNivelNacional,
                    ListaActividad = b,
                    ListaExepcion = exep,
                    ListasRolPUestos = puesto,
                    aplicapolitica = rh.SeAplicaraPolitica,
                    Cumple = rh.Cumple,
                    IdAdministracionTalentoHumano = rh.IdAdministracionTalentoHumano,
                    Descripcionpuesto = rh.Descripcion,
                    ListasManualPuesto = manualPuesto,
                    IdManualPuestoActual = PuestoActual.IdManualPuesto,
                    Puesto = PuestoActual.Nombre,
                    Revisar = informeUTH.Revisar,
                    NuevoPuesto = puestodesignado.Nombre

                }).FirstOrDefaultAsync();

                return new Response { IsSuccess = true, Resultado = empleado };

            }
            catch (Exception ex)
            {

                return new Response { IsSuccess = false, Message = Mensaje.Error }; ;
            }
        }


        #endregion
        [HttpPost]
        [Route("ListarEmpleadosSinFAO")]
        public async Task<List<DocumentoFAOViewModel>> ListarEmpleadosSinFAO([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {

                var anio = DateTime.Now.Year;
                var listaSalida2 = new List<DocumentoFAOViewModel>();

                var EmpleadoEncontrado = await db.Empleado
                    .OrderBy(x => x.FechaIngreso)
                    .Where(x => x.NombreUsuario == documentoFAOViewModel.NombreUsuario && x.Activo == true)
                    .FirstOrDefaultAsync();

                if (EmpleadoEncontrado != null)
                {
                    //var modalidar = await db.ModalidadPartida
                    //    .Where(x => x.Nombre == Constantes.PartidaOcupada)
                    //    .FirstOrDefaultAsync();

                    var EmpleadoDeJefe = await db.Empleado
                        //.Where(x => x.IdDependencia == EmpleadoEncontrado.IdDependencia)
                        .ToListAsync();

                    foreach (var item in EmpleadoDeJefe)
                    {
                        var iompEmpleado = await db.IndiceOcupacionalModalidadPartida
                            .Where(x =>
                                x.IdEmpleado == item.IdEmpleado
                                && x.Empleado.Activo == true
                            //&& x.IdModalidadPartida == modalidar.IdModalidadPartida
                            )
                            .OrderByDescending(o => o.Fecha)
                            .Select(x => new DocumentoFAOViewModel
                            {
                                //IdEmpleado =Convert.ToInt32( x.IdEmpleado),
                                Modalidad = "",//x.ModalidadPartida.Nombre, NO SE QUE HACE ESTE CAMPO DESCOMENTAR AL VERIFICAR SU USO
                                Puesto = x.IndiceOcupacional.ManualPuesto.Nombre
                            })
                            .FirstOrDefaultAsync();


                        if (iompEmpleado != null)
                        {
                            var formulario = await db.FormularioAnalisisOcupacional
                                .Where(x => x.Anio == anio && x.IdEmpleado == item.IdEmpleado)
                                .FirstOrDefaultAsync();

                            if (formulario == null)
                            {
                                var EmpleadoDeJefe1 = await db.Empleado.Where(x => x.IdEmpleado == item.IdEmpleado).Select(x => new DocumentoFAOViewModel
                                {
                                    IdEmpleado = x.IdEmpleado,
                                    idDependencia = x.Dependencia.IdDependencia,
                                    idsucursal = x.Dependencia.IdSucursal,
                                    nombre = x.Persona.Nombres,
                                    apellido = x.Persona.Apellidos,
                                    NombreUsuario = x.NombreUsuario,
                                    Identificacion = x.Persona.Identificacion,
                                    //Dependencia = x.Dependencia.DependenciaPadre.Nombre,
                                    UnidadAdministrativa = x.Dependencia.Nombre,
                                    TipoNombramiento = x.IndiceOcupacionalModalidadPartida.FirstOrDefault().TipoNombramiento.Nombre,
                                    Modalidad = iompEmpleado.Modalidad,
                                    Puesto = iompEmpleado.Puesto,
                                })
                                .FirstOrDefaultAsync();

                                listaSalida2.Add(new DocumentoFAOViewModel
                                {
                                    IdEmpleado = EmpleadoDeJefe1.IdEmpleado,
                                    idDependencia = EmpleadoDeJefe1.idDependencia,
                                    idsucursal = EmpleadoDeJefe1.idsucursal,
                                    nombre = EmpleadoDeJefe1.nombre,
                                    apellido = EmpleadoDeJefe1.apellido,
                                    NombreUsuario = EmpleadoDeJefe1.NombreUsuario,
                                    Identificacion = EmpleadoDeJefe1.Identificacion,
                                    //Dependencia = EmpleadoDeJefe1.Dependencia,
                                    UnidadAdministrativa = EmpleadoDeJefe1.UnidadAdministrativa,
                                    TipoNombramiento = EmpleadoDeJefe1.TipoNombramiento,
                                    Modalidad = EmpleadoDeJefe1.Modalidad,
                                    Puesto = EmpleadoDeJefe1.Puesto
                                });

                            }
                        }

                    }

                }

                return listaSalida2;

            }
            catch (Exception ex)
            {

                return new List<DocumentoFAOViewModel>();
            }
        }


        [HttpPost]
        [Route("ListarEmpleadosConFAO")]
        public async Task<List<DocumentoFAOViewModel>> ListarEmpleadosConFAO([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {

                var lista = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).OrderBy(x => x.FechaIngreso).Where(x => x.NombreUsuario == documentoFAOViewModel.NombreUsuario && x.Activo == true).ToListAsync();

                var listaSalida = new List<DocumentoFAOViewModel>();
                var listaSalida2 = new List<DocumentoFAOViewModel>();

                var NombreDependencia = "";
                int idDependencia;
                int idsucursal;
                int idempleado;
                foreach (var item in lista)
                {
                    if (item.Dependencia == null)
                    {
                        NombreDependencia = "No Asignado";
                        //idDependencia = "";
                    }
                    else
                    {
                        NombreDependencia = item.Dependencia.Nombre;
                        idDependencia = item.Dependencia.IdDependencia;
                        idsucursal = item.Dependencia.IdSucursal;
                        idempleado = item.IdEmpleado;


                        var anio = DateTime.Now.Year;

                        var lista1 = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).Where(x => x.Dependencia.IdDependencia == idDependencia && x.Dependencia.IdSucursal == idsucursal).ToListAsync();
                        foreach (var item1 in lista1)
                        {
                            var empleadoid = item1.IdEmpleado;

                            var a = await db.FormularioAnalisisOcupacional.Where(x => x.Anio == anio && x.IdEmpleado == empleadoid).FirstOrDefaultAsync();
                            if (a != null)
                            {
                                listaSalida2.Add(new DocumentoFAOViewModel
                                {
                                    IdEmpleado = item1.IdEmpleado,
                                    idDependencia = item1.Dependencia.IdDependencia,
                                    idsucursal = item1.Dependencia.IdSucursal,
                                    nombre = item1.Persona.Nombres,
                                    apellido = item1.Persona.Apellidos,
                                    NombreUsuario = item1.NombreUsuario,
                                    Identificacion = item1.Persona.Identificacion,
                                    estado = item1.FormularioAnalisisOcupacional.FirstOrDefault().Estado,
                                    IdFormularioAnalisisOcupacional = item1.FormularioAnalisisOcupacional.FirstOrDefault().IdFormularioAnalisisOcupacional


                                });

                            }

                        }

                    }

                }
                return listaSalida2;



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
                return new List<DocumentoFAOViewModel>();
            }
        }
        [HttpPost]
        [Route("ListarEmpleadosConFAOJefes")]
        public async Task<List<DocumentoFAOViewModel>> ListarEmpleadosConFAOJefes([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {

                var lista = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).OrderBy(x => x.FechaIngreso).Where(x => x.NombreUsuario == documentoFAOViewModel.NombreUsuario).ToListAsync();

                var listaSalida2 = new List<DocumentoFAOViewModel>();

                var NombreDependencia = "";
                int idDependencia;
                int idsucursal;
                int idempleado;
                bool jefe;
                foreach (var item in lista)
                {
                    if (item.Dependencia == null)
                    {
                        NombreDependencia = "No Asignado";
                        //idDependencia = "";
                    }
                    else
                    {
                        NombreDependencia = item.Dependencia.Nombre;
                        idDependencia = item.Dependencia.IdDependencia;
                        idsucursal = item.Dependencia.IdSucursal;
                        idempleado = item.IdEmpleado;
                        jefe = item.EsJefe;
                        if (jefe == true)
                        {
                            var anio = DateTime.Now.Year;

                            var lista1 = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).Where(x => x.Dependencia.IdDependencia == idDependencia && x.Dependencia.IdSucursal == idsucursal).ToListAsync();
                            foreach (var item1 in lista1)
                            {
                                var empleadoid = item1.IdEmpleado;

                                var a = await db.FormularioAnalisisOcupacional.Where(x => x.Anio == anio && x.IdEmpleado == empleadoid && (x.Estado != EstadosFAO.RealizadoJefeTH || x.Estado == EstadosFAO.RealizadoJefeTH || x.Estado == EstadosFAO.RealizadoEspecialistaTH)).FirstOrDefaultAsync();
                                if (a != null)
                                {
                                    listaSalida2.Add(new DocumentoFAOViewModel
                                    {
                                        IdEmpleado = item1.IdEmpleado,
                                        idDependencia = item1.Dependencia.IdDependencia,
                                        idsucursal = item1.Dependencia.IdSucursal,
                                        nombre = item1.Persona.Nombres,
                                        apellido = item1.Persona.Apellidos,
                                        NombreUsuario = item1.NombreUsuario,
                                        Identificacion = item1.Persona.Identificacion,
                                        estado = item1.FormularioAnalisisOcupacional.FirstOrDefault().Estado,
                                        IdFormularioAnalisisOcupacional = item1.FormularioAnalisisOcupacional.FirstOrDefault().IdFormularioAnalisisOcupacional

                                    });

                                }
                            }



                        }

                    }

                }
                return listaSalida2;



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
                return new List<DocumentoFAOViewModel>();
            }
        }

        // GET: api/Empleados
        [HttpGet]
        [Route("ListarEmpleadosActivos")]
        public async Task<List<ListaEmpleadoViewModel>> ListarEmpleadosActivos()
        {
            try
            {

                var listaIOMP = await db.IndiceOcupacionalModalidadPartida
                    .Include(i => i.TipoNombramiento)
                    .Include(i => i.TipoNombramiento.RelacionLaboral)

                    .Include(i => i.IndiceOcupacional)
                    .Include(i => i.IndiceOcupacional.ManualPuesto)
                    .Where(w => w.IdEmpleado != null)
                    .ToListAsync();


                var lista = await db.Empleado
                                    .Where(w => w.Activo == true)
                                    .Select(x => new ListaEmpleadoViewModel
                                    {
                                        IdEmpleado = x.IdEmpleado,
                                        IdPersona = x.Persona.IdPersona,
                                        NombreApellido = string.Format("{0} {1}", x.Persona.Nombres, x.Persona.Apellidos),
                                        TelefonoPrivado = x.Persona.TelefonoPrivado,
                                        CorreoPrivado = x.Persona.CorreoPrivado,
                                        Dependencia = x.IdDependencia == null ? "No asignado" : x.Dependencia.Nombre,
                                        Identificacion = x.Persona.Identificacion
                                    })
                                    .DistinctBy(d => d.IdEmpleado)
                                    .ToAsyncEnumerable().ToList();

                foreach (var item in lista)
                {

                    if (listaIOMP.Where(w => w.IdEmpleado == item.IdEmpleado).FirstOrDefault() != null)
                    {

                        var itemIomp = listaIOMP.Where(w => w.IdEmpleado == item.IdEmpleado).FirstOrDefault();

                        item.IdRelacionLaboral = itemIomp.TipoNombramiento.RelacionLaboral.IdRelacionLaboral;
                        item.NombreRelacionLaboral = itemIomp.TipoNombramiento.RelacionLaboral.Nombre;
                        item.ManualPuesto = itemIomp.IndiceOcupacional.ManualPuesto.Nombre;
                        item.IdManualPuesto = itemIomp.IndiceOcupacional.ManualPuesto.IdManualPuesto;
                        item.PartidaIndividual = itemIomp.NumeroPartidaIndividual + itemIomp.CodigoContrato;
                    }
                }


                return lista;

            }
            catch (Exception ex)
            {
                return new List<ListaEmpleadoViewModel>();
            }
        }

        [HttpGet]
        [Route("ListarEmpleados")]
        public async Task<List<ListaEmpleadoViewModel>> GetEmpleados()
        {
            try
            {
                 var listaIOMP = await db.IndiceOcupacionalModalidadPartida
                    .Include(i => i.TipoNombramiento)
                    .Include(i => i.TipoNombramiento.RelacionLaboral)
                    .Include(i => i.IndiceOcupacional.Dependencia)
                    .Include(i => i.IndiceOcupacional.Dependencia.Sucursal)

                    .Include(i => i.IndiceOcupacional)
                    .Include(i => i.IndiceOcupacional.ManualPuesto)

                    .Include(i=>i.Empleado)
                    .Where(w => 
                        w.IdEmpleado != null
                    )
                    .OrderByDescending(o=>o.Fecha)
                    .ToListAsync();


                var lista = await db.Empleado
                                    //.Where(w => w.Dependencia.IdSucursal == usuarioActual.Dependencia.IdSucursal)
                                    .Select(x => new ListaEmpleadoViewModel
                                    {
                                        IdEmpleado = x.IdEmpleado,
                                        IdPersona = x.Persona.IdPersona,
                                        NombreApellido = string.Format("{0} {1}", x.Persona.Nombres,
                                        x.Persona.Apellidos),
                                        TelefonoPrivado = x.Persona.TelefonoPrivado,
                                        CorreoPrivado = x.Persona.CorreoPrivado,
                                        Dependencia = x.IdDependencia == null ? "" : x.Dependencia.Nombre,
                                        Identificacion = x.Persona.Identificacion
                                    })
                                    .DistinctBy(d => d.IdEmpleado)
                                    .ToAsyncEnumerable().ToList();

                foreach (var item in lista)
                {

                    if (
                        listaIOMP.Where(w =>
                            w.IdEmpleado == item.IdEmpleado
                            && w.Empleado.Activo == true
                        )
                        .FirstOrDefault() != null
                    )
                    {
                        // crear una lista para filtrar por si hay un despido y un reingreso el mismo d�a
                        var fechaMax = listaIOMP
                            .Where(w => w.IdEmpleado == item.IdEmpleado)
                            .OrderByDescending(o => o.Fecha)
                            .FirstOrDefault().Fecha;

                        var itemIomp = listaIOMP
                            .Where(w =>
                                w.IdEmpleado == item.IdEmpleado
                                && w.Fecha == fechaMax
                            )
                            .OrderByDescending(o => o.IdIndiceOcupacionalModalidadPartida)
                            .FirstOrDefault();

                        item.IdRelacionLaboral = itemIomp.TipoNombramiento.RelacionLaboral.IdRelacionLaboral;
                        item.NombreRelacionLaboral = itemIomp.TipoNombramiento.RelacionLaboral.Nombre;
                        item.ManualPuesto = itemIomp.IndiceOcupacional.ManualPuesto.Nombre;
                        item.IdManualPuesto = itemIomp.IndiceOcupacional.ManualPuesto.IdManualPuesto;
                        item.PartidaIndividual = itemIomp.NumeroPartidaIndividual + itemIomp.CodigoContrato;
                        item.NombreSucursal = itemIomp.IndiceOcupacional.Dependencia.Sucursal.Nombre;
                        item.CodigoEmpleado = itemIomp.NumeroPartidaIndividual + itemIomp.CodigoContrato;
                        
                    }
                }


                return lista;

            }
            catch (Exception ex)
            {
                return new List<ListaEmpleadoViewModel>();
            }

            
        }



        /// <summary>
        ///  Obtiene la lista de empleados asignados al distributivo a partir del usuario ingresado
        ///  Receta: obtiene la sucursal del usuario logueado y filtra la lista de empleados por esa sucursal
        /// </summary>
        /// <param name="NombreUsuario"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ListarEmpleadosIndiceOcupacionalModalidadPartida")]
        public async Task<List<ListaEmpleadoViewModel>> ListarEmpleadosIndiceOcupacionalModalidadPartida([FromBody] string NombreUsuario)
        {
            try
            {
                var usuarioActual = await db.Empleado.Include(i => i.Dependencia)
                    .Where(w => w.NombreUsuario == NombreUsuario).FirstOrDefaultAsync();

                var lista = await db.IndiceOcupacionalModalidadPartida
                                    .Where(w => w.Empleado.Dependencia.IdSucursal == usuarioActual.Dependencia.IdSucursal)
                                    .OrderByDescending(o => new { o.Fecha, o.IdIndiceOcupacionalModalidadPartida })
                                    .Select(x => new ListaEmpleadoViewModel
                                    {
                                        IdEmpleado = (int)x.IdEmpleado,
                                        IdPersona = x.Empleado.Persona.IdPersona,
                                        NombreApellido = string.Format("{0} {1}", x.Empleado.Persona.Nombres, x.Empleado.Persona.Apellidos),
                                        TelefonoPrivado = x.Empleado.Persona.TelefonoPrivado,
                                        CorreoPrivado = x.Empleado.Persona.CorreoPrivado,
                                        Dependencia = x.Empleado.IdDependencia == null ? "No asignado" : x.Empleado.Dependencia.Nombre,
                                        Identificacion = x.Empleado.Persona.Identificacion,
                                        IdRelacionLaboral = x.TipoNombramiento.IdRelacionLaboral,
                                        NombreRelacionLaboral = x.TipoNombramiento.RelacionLaboral.Nombre,
                                        ManualPuesto = x.IndiceOcupacional.ManualPuesto.Nombre,
                                        IdManualPuesto = Convert.ToInt32(x.IndiceOcupacional.IdManualPuesto),
                                        PartidaIndividual = x.NumeroPartidaIndividual

                                    })
                                    .DistinctBy(d => d.IdEmpleado)
                                    .ToAsyncEnumerable().ToList();


                return lista;

            }
            catch (Exception ex)
            {
                return new List<ListaEmpleadoViewModel>();
            }
        }

        /// <summary>
        /// Devuelve una lista de empleadosViewModel activos y no activos, con los datos de la modalidad pardida 
        /// si llegasen a tener una
        /// 
        /// </summary>
        /// <param name="NombreUsuario"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ListarTodosEmpleadosRegistrados")]
        public async Task<List<ListaEmpleadoViewModel>> ListarTodosEmpleadosRegistrados([FromBody] string NombreUsuario)
        {
            try
            {
                var usuarioActual = await db.Empleado.Include(i => i.Dependencia)
                    .Where(w => w.NombreUsuario == NombreUsuario).FirstOrDefaultAsync();

                var listaIOMP = await db.IndiceOcupacionalModalidadPartida
                    .Include(i => i.TipoNombramiento)
                    .Include(i => i.TipoNombramiento.RelacionLaboral)

                    .Include(i => i.IndiceOcupacional)
                    .Include(i => i.IndiceOcupacional.ManualPuesto)

                    .Include(i=>i.Empleado)
                    .Where(w => 
                        w.IdEmpleado != null
                    )
                    .OrderByDescending(o=>o.Fecha)
                    .ToListAsync();


                var lista = await db.Empleado
                                    //.Where(w => w.Dependencia.IdSucursal == usuarioActual.Dependencia.IdSucursal)
                                    .Select(x => new ListaEmpleadoViewModel
                                    {
                                        IdEmpleado = x.IdEmpleado,
                                        IdPersona = x.Persona.IdPersona,
                                        NombreApellido = string.Format("{0} {1}", x.Persona.Nombres, x.Persona.Apellidos),
                                        TelefonoPrivado = x.Persona.TelefonoPrivado,
                                        CorreoPrivado = x.Persona.CorreoPrivado,
                                        Dependencia = x.IdDependencia == null ? "" : x.Dependencia.Nombre,
                                        Identificacion = x.Persona.Identificacion
                                    })
                                    .DistinctBy(d => d.IdEmpleado)
                                    .ToAsyncEnumerable().ToList();

                foreach (var item in lista)
                {

                    if (
                        listaIOMP.Where(w => 
                            w.IdEmpleado == item.IdEmpleado
                            && w.Empleado.Activo == true
                        )
                        .FirstOrDefault() != null
                    )
                    {
                        // crear una lista para filtrar por si hay un despido y un reingreso el mismo d�a
                        var fechaMax = listaIOMP
                            .Where(w => w.IdEmpleado == item.IdEmpleado)
                            .OrderByDescending(o => o.Fecha)
                            .FirstOrDefault().Fecha;

                        var itemIomp = listaIOMP
                            .Where(w => 
                                w.IdEmpleado == item.IdEmpleado
                                && w.Fecha == fechaMax
                            )
                            .OrderByDescending(o=> o.IdIndiceOcupacionalModalidadPartida)                            
                            .FirstOrDefault();

                        item.IdRelacionLaboral = itemIomp.TipoNombramiento.RelacionLaboral.IdRelacionLaboral;
                        item.NombreRelacionLaboral = itemIomp.TipoNombramiento.RelacionLaboral.Nombre;
                        item.ManualPuesto = itemIomp.IndiceOcupacional.ManualPuesto.Nombre;
                        item.IdManualPuesto = itemIomp.IndiceOcupacional.ManualPuesto.IdManualPuesto;
                        item.PartidaIndividual = itemIomp.NumeroPartidaIndividual + itemIomp.CodigoContrato;
                    }
                }


                return lista;

            }
            catch (Exception ex)
            {
                return new List<ListaEmpleadoViewModel>();
            }
        }



        [HttpPost]
        [Route("ListarDistributivo")]
        public async Task<List<ListaEmpleadoViewModel>> ListarDistributivo([FromBody] string NombreUsuario)
        {
            try
            {
                var lista = new List<ListaEmpleadoViewModel>();

                var listaDistributivo = await db.IndiceOcupacionalModalidadPartida
                    .OrderByDescending(o => o.IdIndiceOcupacionalModalidadPartida)
                    .Select(s => new ListaEmpleadoViewModel
                    {
                        IdEmpleado = (s.IdEmpleado!= null)
                            ?(s.Empleado.IdEmpleado)
                            :0,

                        IdPersona = (s.IdEmpleado != null && s.Empleado.IdPersona > 0)
                            ? (s.Empleado.IdPersona)
                            :0,

                        NombreApellido = (s.IdEmpleado != null && s.Empleado.IdPersona > 0)
                            ?string.Format("{0} {1}", s.Empleado.Persona.Nombres, s.Empleado.Persona.Apellidos)
                            :"",

                        TelefonoPrivado = (s.IdEmpleado != null && s.Empleado.IdPersona > 0)
                            ?s.Empleado.Persona.TelefonoPrivado
                            :"",

                        CorreoPrivado = (s.IdEmpleado != null && s.Empleado.IdPersona > 0)
                            ? s.Empleado.Persona.CorreoPrivado
                            :"",

                        Dependencia = s.IndiceOcupacional.Dependencia.Nombre,

                        Identificacion = (s.IdEmpleado != null && s.Empleado.IdPersona > 0)
                            ? s.Empleado.Persona.Identificacion
                            : "",

                        PartidaIndividual = (s.NumeroPartidaIndividual + s.CodigoContrato),

                        NombreRelacionLaboral = (s.IdTipoNombramiento != null)
                            ? s.TipoNombramiento.Nombre
                            :"",

                        ManualPuesto = s.IndiceOcupacional.ManualPuesto.Nombre

                    })
                    .DistinctBy(d=>d.PartidaIndividual)
                    .ToAsyncEnumerable()
                    .Where(w=> !String.IsNullOrEmpty(w.PartidaIndividual))
                    .ToList();

                

                


                return listaDistributivo;

            }
            catch (Exception ex)
            {
                return new List<ListaEmpleadoViewModel>();
            }
        }


        [HttpPost]
        [Route("ListarManualPuestoporDependencia")]
        public async Task<List<IndiceOcupacional>> GetManualPuestobyDependency([FromBody] IndiceOcupacional indiceocupacional)
        {
            try
            {
                var name = Constantes.PartidaVacante;
                var listaIndiceOcupacional = db.IndiceOcupacional
                    .Include(x => x.ManualPuesto)
                    .Include(x => x.IndiceOcupacionalModalidadPartida)
                    .Where(x => x.IdDependencia == indiceocupacional.IdDependencia && x.IndiceOcupacionalModalidadPartida.FirstOrDefault().ModalidadPartida.Nombre.ToUpper() == Constantes.PartidaVacante.ToUpper())
                    .OrderBy(x => x.IdDependencia).DistinctBy(x => x.IdManualPuesto).ToList();

                return listaIndiceOcupacional;
            }
            catch (Exception ex)
            {

                return new List<IndiceOcupacional>();
            }
        }



        [HttpPost]
        [Route("ListarManualPuestoporDependenciaTodosEstados")]
        public async Task<List<IndiceOcupacional>> ListarManualPuestoporDependenciaTodosEstados([FromBody] IndiceOcupacional indiceocupacional)
        {
            try
            {
                var listaIndiceOcupacional = await db.IndiceOcupacional
                    .Include(x => x.ManualPuesto)
                    .Where(x => x.IdDependencia == indiceocupacional.IdDependencia)
                    .OrderBy(x => x.IdDependencia)
                    .DistinctBy(x => x.IdManualPuesto).ToAsyncEnumerable().ToList();

                return listaIndiceOcupacional;
            }
            catch (Exception ex)
            {
                return new List<IndiceOcupacional>();
            }
        }


        [HttpPost]
        [Route("ListarManualPuestoporDependenciaYRelacionLaboral")]
        public async Task<List<IndicesOcupacionalesModalidadPartidaViewModel>> ListarManualPuestoporDependenciaYRelacionLaboral([FromBody] IndiceOcupacional indiceocupacional)
        {
            try
            {
                // Se obtienen todos los perfiles de puesto
                var lista = await db.IndiceOcupacional
                    .Where(w => w.IdDependencia == indiceocupacional.IdDependencia)
                    .Select(
                        s => new IndicesOcupacionalesModalidadPartidaViewModel
                        {

                            NumeroPartidaIndividual = "",

                            IndiceOcupacionalViewModel = new IndiceOcupacionalViewModel
                            {

                                IdIndiceOcupacional = s.IdIndiceOcupacional,
                                IdDependencia = s.IdDependencia,
                                IdManualPuesto = s.IdManualPuesto,
                                IdRolPuesto = s.IdRolPuesto,
                                IdEscalaGrados = s.IdEscalaGrados,
                                IdPartidaGeneral = s.IdPartidaGeneral,
                                IdAmbito = s.IdAmbito,
                                Nivel = s.Nivel,


                                NombreDependencia = s.Dependencia.Nombre,
                                CodigoDependencia = s.Dependencia.Codigo,

                                IdSucursal = s.Dependencia.Sucursal.IdSucursal,
                                NombreSucursal = s.Dependencia.Sucursal.Nombre,

                                NombreManualPuesto = s.ManualPuesto.Nombre,
                                DescripcionManualPuesto = s.ManualPuesto.Descripcion,
                                MisionManualPuesto = s.ManualPuesto.Mision,

                                IdRelacionesInternasExternas =
                                        s.ManualPuesto.RelacionesInternasExternas.IdRelacionesInternasExternas,
                                NombreRelacionesInternasExternas =
                                        s.ManualPuesto.RelacionesInternasExternas.Nombre,
                                DescripcionRelacionesInternasExternas =
                                        s.ManualPuesto.RelacionesInternasExternas.Descripcion,


                                NombreRolPuesto = s.RolPuesto.Nombre,


                                NombreEscalaGrados = s.EscalaGrados.Nombre,
                                Remuneracion = s.EscalaGrados.Remuneracion,
                                Grado = s.EscalaGrados.Grado,

                                NumeroPartidaGeneral =
                                    (s.PartidaGeneral == null)
                                    ? ""
                                    : s.PartidaGeneral.NumeroPartida,

                                NombreAmbito = s.Ambito.Nombre

                            }
                            ,

                        }

                        ).ToListAsync();


                // Se obtienen los diferentes tipos de relaci�n laboral
                var relacionLaboral = await db.RelacionLaboral
                    .Where(w => w.IdRelacionLaboral == indiceocupacional.IdRelacionLaboral)
                    .FirstOrDefaultAsync();


                // Se obtienen los diferentes tipos de modalidad partida
                var modalidadPartida = await db.ModalidadPartida.ToListAsync();



                if (
                    relacionLaboral != null
                    && relacionLaboral.Nombre.ToUpper() == ConstantesTipoRelacion.Contrato.ToUpper()
                    )
                {

                    return lista;
                }

                else if (
                    relacionLaboral != null
                    && relacionLaboral.Nombre.ToUpper() == ConstantesTipoRelacion.Nombramiento.ToUpper()
                    )
                {



                    // Se obtienen los iomp que est�n vacantes y tengan un numero de partida


                    var listaIOMP = await db.IndiceOcupacionalModalidadPartida
                        .Include(i => i.IndiceOcupacional)
                        .Include(i => i.IndiceOcupacional.ManualPuesto)
                        .Where(w =>
                                !String.IsNullOrEmpty(w.NumeroPartidaIndividual)
                                && w.IdModalidadPartida == modalidadPartida
                                            .Where(wm => wm.Nombre.ToUpper() == Constantes.PartidaVacante.ToUpper())
                                            .FirstOrDefault()
                                            .IdModalidadPartida

                            )
                        .OrderByDescending(o => o.Fecha)
                        .DistinctBy(d => d.IndiceOcupacional.ManualPuesto.Nombre)
                        .ToAsyncEnumerable()
                        .ToList();




                    foreach (var item in lista)
                    {

                        if (
                            listaIOMP
                            .Where(w => w.IdIndiceOcupacional == item.IdIndiceOcupacional)
                            .FirstOrDefault() != null
                            )
                        {
                            var item2 = listaIOMP
                            .Where(w => w.IdIndiceOcupacional == item.IdIndiceOcupacional)
                            .FirstOrDefault();

                            item.NumeroPartidaIndividual = item2.NumeroPartidaIndividual;
                            item.IdModalidadPartida = item2.IdModalidadPartida;

                        }

                    }

                    return lista;

                }


                return new List<IndicesOcupacionalesModalidadPartidaViewModel>();
            }
            catch (Exception ex)
            {
                return new List<IndicesOcupacionalesModalidadPartidaViewModel>();
            }
        }




        [HttpPost]
        [Route("ListarRolPuestoporManualPuesto")]
        public async Task<List<IndiceOcupacional>> GetRolPuestoPorManualPuesto([FromBody] IndiceOcupacional indiceocupacional)
        {
            try
            {

                var listaIndiceOcupacional = db.IndiceOcupacional.Include(x => x.RolPuesto)
                    .Where(x => x.IdManualPuesto == indiceocupacional.IdManualPuesto && x.IdDependencia == indiceocupacional.IdDependencia)
                    .OrderBy(x => x.IdDependencia).DistinctBy(x => x.IdRolPuesto).ToList();

                return listaIndiceOcupacional;
            }
            catch (Exception ex)
            {

                return new List<IndiceOcupacional>();
            }
        }



        [HttpPost]
        [Route("ListarRolPuestoporManualPuestoYRelacionLaboral")]
        public async Task<IndicesOcupacionalesModalidadPartidaViewModel> ListarRolPuestoporManualPuestoYRelacionLaboral([FromBody] IndiceOcupacional indiceocupacional)
        {
            try
            {

                // Se obtienen todos los perfiles de puesto
                var modelo = await db.IndiceOcupacional
                    .Where(w =>
                        w.IdDependencia == indiceocupacional.IdDependencia
                        && w.ManualPuesto.IdManualPuesto == indiceocupacional.IdManualPuesto
                    )
                    .Select(
                        s => new IndicesOcupacionalesModalidadPartidaViewModel
                        {

                            NumeroPartidaIndividual = "",

                            IndiceOcupacionalViewModel = new IndiceOcupacionalViewModel
                            {

                                IdIndiceOcupacional = s.IdIndiceOcupacional,
                                IdDependencia = s.IdDependencia,
                                IdManualPuesto = s.IdManualPuesto,
                                IdRolPuesto = s.IdRolPuesto,
                                IdEscalaGrados = s.IdEscalaGrados,
                                IdPartidaGeneral = s.IdPartidaGeneral,
                                IdAmbito = s.IdAmbito,
                                Nivel = s.Nivel,


                                NombreDependencia = s.Dependencia.Nombre,
                                CodigoDependencia = s.Dependencia.Codigo,

                                IdSucursal = s.Dependencia.Sucursal.IdSucursal,
                                NombreSucursal = s.Dependencia.Sucursal.Nombre,

                                NombreManualPuesto = s.ManualPuesto.Nombre,
                                DescripcionManualPuesto = s.ManualPuesto.Descripcion,
                                MisionManualPuesto = s.ManualPuesto.Mision,

                                IdRelacionesInternasExternas =
                                        s.ManualPuesto.RelacionesInternasExternas.IdRelacionesInternasExternas,
                                NombreRelacionesInternasExternas =
                                        s.ManualPuesto.RelacionesInternasExternas.Nombre,
                                DescripcionRelacionesInternasExternas =
                                        s.ManualPuesto.RelacionesInternasExternas.Descripcion,


                                NombreRolPuesto = s.RolPuesto.Nombre,


                                NombreEscalaGrados = s.EscalaGrados.Nombre,
                                Remuneracion = s.EscalaGrados.Remuneracion,
                                Grado = s.EscalaGrados.Grado,

                                NumeroPartidaGeneral =
                                    (s.PartidaGeneral == null)
                                    ? ""
                                    : s.PartidaGeneral.NumeroPartida,

                                NombreAmbito = s.Ambito.Nombre

                            }
                            ,

                        }

                        ).FirstOrDefaultAsync();


                // Se obtienen los diferentes tipos de relaci�n laboral
                var relacionLaboral = await db.RelacionLaboral
                    .Where(w => w.IdRelacionLaboral == indiceocupacional.IdRelacionLaboral)
                    .FirstOrDefaultAsync();


                // Se obtienen los diferentes tipos de modalidad partida
                var modalidadPartida = await db.ModalidadPartida.ToListAsync();



                if (
                    relacionLaboral != null
                    && relacionLaboral.Nombre.ToUpper() == ConstantesTipoRelacion.Contrato.ToUpper()
                    )
                {

                    return modelo;
                }

                else if (
                    relacionLaboral != null
                    && relacionLaboral.Nombre.ToUpper() == ConstantesTipoRelacion.Nombramiento.ToUpper()
                    )
                {

                    // Se obtienen los iomp que est�n vacantes y tengan un numero de partida
                    var listaIOMP = await db.IndiceOcupacionalModalidadPartida
                        .Include(i => i.IndiceOcupacional)
                        .Include(i => i.IndiceOcupacional.ManualPuesto)
                        .Where(w =>
                                !String.IsNullOrEmpty(w.NumeroPartidaIndividual)
                                && w.IdModalidadPartida == modalidadPartida
                                            .Where(wm => wm.Nombre.ToUpper() == Constantes.PartidaVacante.ToUpper())
                                            .FirstOrDefault()
                                            .IdModalidadPartida

                                && w.IdIndiceOcupacional == modelo.IndiceOcupacionalViewModel.IdIndiceOcupacional
                            )
                        .OrderByDescending(o => o.Fecha)
                        .DistinctBy(d => d.IndiceOcupacional.ManualPuesto.Nombre)
                        .ToAsyncEnumerable().FirstOrDefault();




                    if (
                        listaIOMP != null
                        )
                    {
                        var item2 = listaIOMP;

                        modelo.NumeroPartidaIndividual = item2.NumeroPartidaIndividual;
                        modelo.IdModalidadPartida = item2.IdModalidadPartida;

                    }

                    return modelo;

                }


                return new IndicesOcupacionalesModalidadPartidaViewModel();
            }
            catch (Exception ex)
            {

                return new IndicesOcupacionalesModalidadPartidaViewModel();
            }
        }




        [HttpPost]
        [Route("ListarEscalaGradosPorRolPuesto")]
        public async Task<List<IndiceOcupacional>> GetEscalaGradosPorRolPuesto([FromBody] IndiceOcupacional indiceocupacional)
        {
            try
            {

                var listaIndiceOcupacional = db.IndiceOcupacional.Include(x => x.EscalaGrados)
                    .Where(x => x.IdManualPuesto == indiceocupacional.IdManualPuesto && x.IdDependencia == indiceocupacional.IdDependencia && x.IdRolPuesto == indiceocupacional.IdRolPuesto)
                    .OrderBy(x => x.IdDependencia).DistinctBy(x => x.EscalaGrados).ToList();

                return listaIndiceOcupacional;
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
                return new List<IndiceOcupacional>();
            }
        }


        [HttpPost]
        [Route("ListarModalidadesPartidaPorEscalaGrados")]
        public async Task<List<IndiceOcupacional>> GetModalidadesPartidaPorEscalaGrados([FromBody] IndiceOcupacional indiceocupacional)
        {
            try
            {

                var listaIndiceOcupacional = db.IndiceOcupacional.Include(x => x.IndiceOcupacionalModalidadPartida)
                    .Where(x => x.IdManualPuesto == indiceocupacional.IdManualPuesto && x.IdDependencia == indiceocupacional.IdDependencia && x.IdRolPuesto == indiceocupacional.IdRolPuesto && x.IdEscalaGrados == indiceocupacional.IdEscalaGrados)
                    .OrderBy(x => x.IdDependencia).DistinctBy(x => x.IndiceOcupacionalModalidadPartida.FirstOrDefault().ModalidadPartida).ToList();

                return listaIndiceOcupacional;
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
                return new List<IndiceOcupacional>();
            }
        }





        [HttpGet]
        [Route("ListarEmpleadoconAccionPersonalPendiente")]
        public async Task<List<ListaEmpleadoViewModel>> ListarEmpleadoconAccionPersonalPendiente()
        {
            try
            {
                var lista = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).Include(x => x.AccionPersonal).OrderBy(x => x.FechaIngreso).ToListAsync();
                var listaSalida = new List<ListaEmpleadoViewModel>();
                foreach (var item in lista)
                {
                    foreach (var item2 in item.AccionPersonal)
                    {
                        if (item2.Estado == 0)
                        {
                            listaSalida.Add(new ListaEmpleadoViewModel
                            {
                                Dependencia = item.Dependencia.Nombre,
                                IdEmpleado = item.IdEmpleado,
                                NombreApellido = string.Format("{0} {1}", item.Persona.Nombres, item.Persona.Apellidos),
                                Identificacion = item.Persona.Identificacion,
                                TelefonoPrivado = item.Persona.TelefonoPrivado,
                                CorreoPrivado = item.Persona.CorreoPrivado

                            });
                        }
                    }


                }
                return listaSalida;

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
                return new List<ListaEmpleadoViewModel>();
            }
        }



        // GET: api/Empleados/5
        [HttpGet("{id}")]
        public async Task<Response> GetEmpleado([FromRoute] int id)
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

                var Empleado = await db.Empleado
                                                .Include(x => x.Persona)
                                                .Include(x => x.Dependencia)
                                                .SingleOrDefaultAsync(m => m.IdEmpleado == id);

                if (Empleado == null)
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
                    Resultado = Empleado,
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

        [HttpPost]
        [Route("EmpleadosAsuCargoSolicitudPermiso")]
        public async Task<List<ListaEmpleadoViewModel>> ListaEmpleadosAsuCargo([FromBody] Empleado empleado)
        {
            try
            {

                var listaEmpleado = await db.Empleado
                                  .Include(x => x.Persona)
                                  .Include(x => x.SolicitudPermiso)
                                  .Where(x => x.IdDependencia == empleado.IdDependencia && x.EsJefe != empleado.EsJefe)
                                  .OrderBy(x => x.Persona.Apellidos)
                                  .ToListAsync();

                var listaSolicitudPermiso = await db.SolicitudPermiso
                                  .OrderBy(x => x.IdEmpleado)
                                  .ToListAsync();

                var listaSalida = new List<ListaEmpleadoViewModel>();


                foreach (var item in listaEmpleado)
                {

                    bool existeConsulta = listaSolicitudPermiso.Exists(x => (x.IdEmpleado == item.IdEmpleado) && (x.Estado == EstadosFAO.Asignado || x.Estado == EstadosFAO.RealizadoEmpleado));
                    bool existeListaSalida = listaSalida.Exists(x => x.IdEmpleado == item.IdEmpleado);

                    if (existeConsulta)
                    {
                        if (!existeListaSalida)
                        {
                            listaSalida.Add(new ListaEmpleadoViewModel
                            {
                                IdEmpleado = item.IdEmpleado,
                                IdPersona = item.IdPersona,
                                NombreApellido = string.Format("{0} {1}", item.Persona.Nombres, item.Persona.Apellidos),
                                Identificacion = item.Persona.Identificacion,
                                TelefonoPrivado = item.Persona.TelefonoPrivado,
                                CorreoPrivado = item.Persona.CorreoPrivado

                            });
                        }
                    }

                }


                return listaSalida;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        [Route("SolicitudPermisoEmpleados")]
        public async Task<List<ListaEmpleadoViewModel>> SolicitudPermisoEmpleados([FromBody] Empleado empleado)
        {
            try
            {
                var dependenciaJefe = await db.Empleado
                    .Where(w => w.IdEmpleado == empleado.IdEmpleado)
                    .Select(s => new Dependencia
                    {
                        IdDependencia = (int)s.IdDependencia
                    }
                    )
                    .FirstOrDefaultAsync();

                var listaSalida = await db.SolicitudPermiso
                    .Where(w => w.Empleado.IdDependencia == dependenciaJefe.IdDependencia)
                    .Select(s => new ListaEmpleadoViewModel
                    {
                        IdEmpleado = s.IdEmpleado,
                        Identificacion = s.Empleado.Persona.Identificacion,
                        NombreApellido = String.Format("{0} {1}", s.Empleado.Persona.Nombres, s.Empleado.Persona.Apellidos)

                    }
                    )

                    .ToListAsync();


                return listaSalida;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        [Route("ObtenerDatosEmpleadoActualizar")]
        public async Task<EmpleadoViewModel> ObtenerEmpleadoActualizar([FromBody] int idEmpleado)
        {
            try
            {

                IndiceOcupacionalModalidadPartida indice = await db.IndiceOcupacionalModalidadPartida
                                .Where(x => x.IdEmpleado == idEmpleado)
                                .Include(x => x.TipoNombramiento).ThenInclude(x => x.RelacionLaboral)
                                //.Include(x=>x.ModalidadPartida).ThenInclude(x=>x.RelacionLaboral)
                                .SingleOrDefaultAsync();

                Empleado oEmpleado = await db.Empleado
                                  .Where(x => x.IdEmpleado == idEmpleado)
                                  .Include(x => x.ProvinciaSufragio)
                                  .Include(x => x.CiudadNacimiento)
                                  .SingleOrDefaultAsync();

                Persona persona = await db.Persona
                                         .Where(m => m.IdPersona == oEmpleado.IdPersona)
                                         .Include(x => x.Parroquia)
                                         .ThenInclude(x => x.Ciudad)
                                         .ThenInclude(x => x.Provincia)
                                         .ThenInclude(x => x.Pais)
                                         .FirstOrDefaultAsync();

                DatosBancarios datosBancarios = await db.DatosBancarios
                                  .Where(x => x.IdEmpleado == idEmpleado)
                                  .SingleOrDefaultAsync();

                EmpleadoContactoEmergencia contactoEmergencia = await db.EmpleadoContactoEmergencia
                                  .Include(x => x.Persona)
                                 .Where(x => x.IdEmpleado == idEmpleado)
                                 .SingleOrDefaultAsync();


                EmpleadoViewModel item = new EmpleadoViewModel
                {
                    IndiceOcupacionalModalidadPartida = indice,
                    Persona = persona,
                    Empleado = oEmpleado,
                    DatosBancarios = datosBancarios,
                    EmpleadoContactoEmergencia = contactoEmergencia,

                };

                return item;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("ObtenerTrayectoriaLaboralEmpleado")]
        public async Task<EmpleadoViewModel> ObtenerTrayectoriaLaboralEmpleado([FromBody] int idEmpleado)
        {
            try
            {


                Empleado oEmpleado = await db.Empleado
                                  .Where(x => x.IdEmpleado == idEmpleado)
                                  .SingleOrDefaultAsync();

                Persona persona = await db.Persona
                                         .Where(m => m.IdPersona == oEmpleado.IdPersona)
                                         .Include(x => x.Parroquia)
                                         .ThenInclude(x => x.Ciudad)
                                         .ThenInclude(x => x.Provincia)
                                         .ThenInclude(x => x.Pais)
                                         .FirstOrDefaultAsync();

                List<TrayectoriaLaboral> trayectoriaLaboral = await db.TrayectoriaLaboral
                .Where(x => x.IdPersona == oEmpleado.IdPersona)
                .ToListAsync();


                EmpleadoViewModel item = new EmpleadoViewModel
                {
                    Persona = persona,
                    Empleado = oEmpleado,
                    TrayectoriaLaboral = trayectoriaLaboral
                };

                return item;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        [Route("ObtenerPersonaEstudioEmpleado")]
        public async Task<List<PersonaEstudioViewModel>> ObtenerPersonaEstudioEmpleado([FromBody] int idEmpleado)
        {
            try
            {


                Empleado oEmpleado = await db.Empleado
                                  .Where(x => x.IdEmpleado == idEmpleado)
                                  .SingleOrDefaultAsync();

                Persona persona = await db.Persona
                                         .Where(m => m.IdPersona == oEmpleado.IdPersona)
                                         .Include(x => x.Parroquia)
                                         .ThenInclude(x => x.Ciudad)
                                         .ThenInclude(x => x.Provincia)
                                         .ThenInclude(x => x.Pais)
                                         .FirstOrDefaultAsync();

                List<PersonaEstudio> personaEstudio = await db.PersonaEstudio
                                .Where(x => x.IdPersona == oEmpleado.IdPersona)
                                .Include(x => x.Titulo).ThenInclude(x => x.Estudio)
                                .Include(x => x.Titulo).ThenInclude(x => x.AreaConocimiento)
                                .ToListAsync();

                List<PersonaEstudioViewModel> listaPersonaEstudio = new List<PersonaEstudioViewModel>();


                foreach (PersonaEstudio item in personaEstudio)
                {

                    PersonaEstudioViewModel objetoPersonaEstudio = new PersonaEstudioViewModel
                    {
                        estudio = item.Titulo.Estudio.Nombre,
                        titulo = item.Titulo.Nombre,
                        areaConocimiento = item.Titulo.AreaConocimiento.Descripcion,
                        fechaGraduado = String.Format(item.FechaGraduado.ToString(), "dd/mm/aaaa")
                    };

                    listaPersonaEstudio.Add(objetoPersonaEstudio);
                }

                return listaPersonaEstudio;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        [Route("ObtenerEmpleadoFamiliarEmpleado")]
        public async Task<EmpleadoViewModel> ObtenerEmpleadoFamiliarEmpleado([FromBody] int idEmpleado)
        {
            try
            {


                Empleado oEmpleado = await db.Empleado
                                  .Where(x => x.IdEmpleado == idEmpleado)
                                  .SingleOrDefaultAsync();

                Persona persona = await db.Persona
                                         .Where(m => m.IdPersona == oEmpleado.IdPersona)
                                         .Include(x => x.Parroquia)
                                         .ThenInclude(x => x.Ciudad)
                                         .ThenInclude(x => x.Provincia)
                                         .ThenInclude(x => x.Pais)
                                         .FirstOrDefaultAsync();

                List<EmpleadoFamiliar> empleadoFamiliar = await db.EmpleadoFamiliar
                                 .Where(x => x.IdEmpleado == oEmpleado.IdEmpleado)
                                 .ToListAsync();


                EmpleadoViewModel item = new EmpleadoViewModel
                {
                    Persona = persona,
                    Empleado = oEmpleado,
                    EmpleadoFamiliar = empleadoFamiliar
                };

                return item;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("ObtenerPersonaDiscapacidadEmpleado")]
        public async Task<EmpleadoViewModel> ObtenerPersonaDiscapacidadEmpleado([FromBody] int idEmpleado)
        {
            try
            {

                Empleado oEmpleado = await db.Empleado
                                  .Where(x => x.IdEmpleado == idEmpleado)
                                  .SingleOrDefaultAsync();

                Persona persona = await db.Persona
                                         .Where(m => m.IdPersona == oEmpleado.IdPersona)
                                         .Include(x => x.Parroquia)
                                         .ThenInclude(x => x.Ciudad)
                                         .ThenInclude(x => x.Provincia)
                                         .ThenInclude(x => x.Pais)
                                         .FirstOrDefaultAsync();

                List<PersonaDiscapacidad> personaDiscapacidad = await db.PersonaDiscapacidad
                                  .Where(x => x.IdPersona == oEmpleado.IdPersona)
                                  .ToListAsync();


                EmpleadoViewModel item = new EmpleadoViewModel
                {
                    Persona = persona,
                    Empleado = oEmpleado,
                    PersonaDiscapacidad = personaDiscapacidad
                };

                return item;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        [Route("ObtenerPersonaEnfermedadEmpleado")]
        public async Task<EmpleadoViewModel> ObtenerPersonaEnfermedadEmpleado([FromBody] int idEmpleado)
        {
            try
            {

                Empleado oEmpleado = await db.Empleado
                                  .Where(x => x.IdEmpleado == idEmpleado)
                                  .SingleOrDefaultAsync();

                Persona persona = await db.Persona
                                         .Where(m => m.IdPersona == oEmpleado.IdPersona)
                                         .Include(x => x.Parroquia)
                                         .ThenInclude(x => x.Ciudad)
                                         .ThenInclude(x => x.Provincia)
                                         .ThenInclude(x => x.Pais)
                                         .FirstOrDefaultAsync();

                List<PersonaEnfermedad> personaEnfermedad = await db.PersonaEnfermedad
                  .Where(x => x.IdPersona == oEmpleado.IdPersona)
                  .ToListAsync();

                EmpleadoViewModel item = new EmpleadoViewModel
                {
                    Persona = persona,
                    Empleado = oEmpleado,
                    PersonaEnfermedad = personaEnfermedad
                };

                return item;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        [Route("ObtenerDiscapacidadSustitutoEmpleado")]
        public async Task<EmpleadoViewModel> ObtenerDiscapacidadSustitutoEmpleado([FromBody] int idEmpleado)
        {
            try
            {

                Empleado oEmpleado = await db.Empleado
                                  .Where(x => x.IdEmpleado == idEmpleado)
                                  .SingleOrDefaultAsync();

                Persona persona = await db.Persona
                                         .Where(m => m.IdPersona == oEmpleado.IdPersona)
                                         .Include(x => x.Parroquia)
                                         .ThenInclude(x => x.Ciudad)
                                         .ThenInclude(x => x.Provincia)
                                         .ThenInclude(x => x.Pais)
                                         .FirstOrDefaultAsync();

                PersonaSustituto personaSustituto = await db.PersonaSustituto
                                 .Where(x => x.IdPersona == oEmpleado.IdPersona)
                                 .SingleOrDefaultAsync();

                List<DiscapacidadSustituto> discapacidadSustituto = await db.DiscapacidadSustituto
                                  .Where(x => x.IdPersonaSustituto == personaSustituto.IdPersonaSustituto)
                                  .ToListAsync();

                EmpleadoViewModel item = new EmpleadoViewModel
                {
                    Persona = persona,
                    Empleado = oEmpleado,
                    DiscapacidadSustituto = discapacidadSustituto
                };

                return item;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("ObtenerEnfermedadSustitutoEmpleado")]
        public async Task<EmpleadoViewModel> ObtenerEnfermedadSustitutoEmpleado([FromBody] int idEmpleado)
        {
            try
            {

                Empleado oEmpleado = await db.Empleado
                                  .Where(x => x.IdEmpleado == idEmpleado)
                                  .SingleOrDefaultAsync();

                Persona persona = await db.Persona
                                         .Where(m => m.IdPersona == oEmpleado.IdPersona)
                                         .Include(x => x.Parroquia)
                                         .ThenInclude(x => x.Ciudad)
                                         .ThenInclude(x => x.Provincia)
                                         .ThenInclude(x => x.Pais)
                                         .FirstOrDefaultAsync();

                PersonaSustituto personaSustituto = await db.PersonaSustituto
                                 .Where(x => x.IdPersona == oEmpleado.IdPersona)
                                 .SingleOrDefaultAsync();

                List<EnfermedadSustituto> enfermedadSustituto = await db.EnfermedadSustituto
                  .Where(x => x.IdPersonaSustituto == personaSustituto.IdPersonaSustituto)
                  .ToListAsync();


                EmpleadoViewModel item = new EmpleadoViewModel
                {
                    Persona = persona,
                    Empleado = oEmpleado,
                    EnfermedadSustituto = enfermedadSustituto
                };

                return item;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        [Route("ObtenerEmpleadoFichaEmpleado")]
        public async Task<Response> ObtenerEmpleadoFichaEmpleado([FromBody]DatosBasicosEmpleadoViewModel empleado)
        {
            //Persona persona = new Persona();
            try
            {
                var Empleado = await db.Empleado
                                   .Where(e => e.IdEmpleado == empleado.IdEmpleado && e.IdDependencia != null)
                                   .Select(x => new FichaEmpleadoViewModel
                                   {

                                       FechaNacimiento = x.Persona.FechaNacimiento.Value.Date,
                                       IdSexo = Convert.ToInt32(x.Persona.IdSexo),
                                       IdTipoIdentificacion = Convert.ToInt32(x.Persona.IdTipoIdentificacion),
                                       IdEstadoCivil = Convert.ToInt32(x.Persona.IdEstadoCivil),
                                       IdGenero = Convert.ToInt32(x.Persona.IdGenero),
                                       IdNacionalidad = Convert.ToInt32(x.Persona.IdNacionalidad),
                                       IdTipoSangre = Convert.ToInt32(x.Persona.IdTipoSangre),
                                       IdEtnia = Convert.ToInt32(x.Persona.IdEtnia),
                                       Identificacion = x.Persona.Identificacion,
                                       Nombres = x.Persona.Nombres,
                                       Apellidos = x.Persona.Apellidos,
                                       TelefonoPrivado = x.Persona.TelefonoPrivado,
                                       TelefonoCasa = x.Persona.TelefonoCasa,
                                       CorreoPrivado = x.Persona.CorreoPrivado,
                                       LugarTrabajo = x.Persona.LugarTrabajo,
                                       IdNacionalidadIndigena = x.Persona.IdNacionalidadIndigena,
                                       CallePrincipal = x.Persona.CallePrincipal,
                                       CalleSecundaria = x.Persona.CalleSecundaria,
                                       Referencia = x.Persona.Referencia,
                                       Numero = x.Persona.Numero,
                                       IdParroquia = Convert.ToInt32(x.Persona.IdParroquia),
                                       Ocupacion = x.Persona.Ocupacion,
                                       IdEmpleado = x.IdEmpleado,
                                       IdProvinciaLugarSufragio = Convert.ToInt32(x.IdProvinciaLugarSufragio),
                                       IdPaisLugarNacimiento = x.CiudadNacimiento.Provincia.Pais.IdPais,
                                       IdCiudadLugarNacimiento = x.IdCiudadLugarNacimiento,
                                       IdPaisLugarSufragio = x.ProvinciaSufragio.Pais.IdPais,
                                       IdPaisLugarPersona = x.Persona.Parroquia.Ciudad.Provincia.Pais.IdPais,
                                       IdCiudadLugarPersona = x.Persona.Parroquia.Ciudad.IdCiudad,
                                       IdProvinciaLugarPersona = x.Persona.Parroquia.Ciudad.Provincia.IdProvincia,
                                       AnoDesvinculacion = x.AnoDesvinculacion,
                                       DeclaracionJurada = x.DeclaracionJurada,
                                       Detalle = x.Detalle,
                                       DiasImposiciones = x.DiasImposiciones,
                                       EsJefe = x.EsJefe,
                                       ExtencionTelefonica = x.Extension,
                                       FechaIngresoSectorPublico = x.FechaIngresoSectorPublico,
                                       FondosReservas = x.FondosReservas != null ? x.FondosReservas :false,
                                       IdBrigadaSSORol = x.IdBrigadaSSORol == null ? 0 : x.IdBrigadaSSORol,
                                       IdBrigadaSSO = x.BrigadaSSORol.BrigadaSSO.IdBrigadaSSO == null ? 0 : x.BrigadaSSORol.BrigadaSSO.IdBrigadaSSO,
                                       IdPersona = x.IdPersona,
                                       IngresosOtraActividad = x.IngresosOtraActividad,
                                       MesesImposiciones = x.MesesImposiciones,
                                       Nepotismo = x.Nepotismo == null ? false : x.Nepotismo,
                                       NombreUsuario = x.NombreUsuario,
                                       OtrosIngresos = x.OtrosIngresos == null ? false : x.OtrosIngresos,
                                       Telefono = x.Telefono,
                                       TipoRelacion = x.TipoRelacion,
                                       TrabajoSuperintendenciaBanco = x.TrabajoSuperintendenciaBanco,
                                       RelacionSuperintendencia = x.RelacionSuperintendencia
                                   }
                                   ).FirstOrDefaultAsync();

                if (Empleado != null)
                {
                    return new Response { IsSuccess = true, Resultado = Empleado };
                }
                return new Response { IsSuccess = false };

            }
            catch (Exception ex)
            {
                return new Response { IsSuccess = false };
            }
        }

        [HttpPost]
        [Route("ObtenerDatosBasicosEmpleado")]
        public async Task<Response> ObtenerDatosBasicosEmpleado([FromBody]DatosBasicosEmpleadoViewModel empleado)
        {
            //Persona persona = new Persona();
            try
            {
                var Empleado = await db.Empleado
                                   .Where(e => e.IdEmpleado == empleado.IdEmpleado)
                                   .Select(x => new DatosBasicosEmpleadoViewModel
                                   {

                                       FechaNacimiento = x.Persona.FechaNacimiento.Value.Date,
                                       IdSexo = Convert.ToInt32(x.Persona.IdSexo),
                                       IdTipoIdentificacion = Convert.ToInt32(x.Persona.IdTipoIdentificacion),
                                       IdEstadoCivil = Convert.ToInt32(x.Persona.IdEstadoCivil),
                                       IdGenero = Convert.ToInt32(x.Persona.IdGenero),
                                       IdNacionalidad = Convert.ToInt32(x.Persona.IdNacionalidad),
                                       IdTipoSangre = Convert.ToInt32(x.Persona.IdTipoSangre),
                                       IdEtnia = Convert.ToInt32(x.Persona.IdEtnia),
                                       Identificacion = x.Persona.Identificacion,
                                       Nombres = x.Persona.Nombres,
                                       Apellidos = x.Persona.Apellidos,
                                       TelefonoPrivado = x.Persona.TelefonoPrivado,
                                       TelefonoCasa = x.Persona.TelefonoCasa,
                                       CorreoPrivado = x.Persona.CorreoPrivado,
                                       LugarTrabajo = x.Persona.LugarTrabajo,
                                       IdNacionalidadIndigena = x.Persona.IdNacionalidadIndigena,
                                       CallePrincipal = x.Persona.CallePrincipal,
                                       CalleSecundaria = x.Persona.CalleSecundaria,
                                       Referencia = x.Persona.Referencia,
                                       Numero = x.Persona.Numero,
                                       IdParroquia = Convert.ToInt32(x.Persona.IdParroquia),
                                       Ocupacion = x.Persona.Ocupacion,
                                       IdEmpleado = x.IdEmpleado,
                                       IdProvinciaLugarSufragio = Convert.ToInt32(x.IdProvinciaLugarSufragio),
                                       IdPaisLugarNacimiento = x.CiudadNacimiento.Provincia.Pais.IdPais,
                                       IdCiudadLugarNacimiento = x.IdCiudadLugarNacimiento,
                                       IdPaisLugarSufragio = x.ProvinciaSufragio.Pais.IdPais,
                                       IdPaisLugarPersona = x.Persona.Parroquia.Ciudad.Provincia.Pais.IdPais,
                                       IdCiudadLugarPersona = x.Persona.Parroquia.Ciudad.IdCiudad,
                                       IdProvinciaLugarPersona = x.Persona.Parroquia.Ciudad.Provincia.IdProvincia,
                                       RelacionSuperintendencia = x.RelacionSuperintendencia

                                   }
                                   ).FirstOrDefaultAsync();


                return new Response { IsSuccess = true, Resultado = Empleado };
            }
            catch (Exception ex)
            {
                return new Response { IsSuccess = false };
            }
        }

        [HttpPost]
        [Route("ObtenerEmpleadoLogueado")]
        public async Task<Empleado> ObtenerEmpleadoLogueado([FromBody]Empleado empleado)
        {
            //Persona persona = new Persona();
            try
            {

                var Empleado = await db.Empleado
                                   .Where(e => e.NombreUsuario == empleado.NombreUsuario).FirstOrDefaultAsync();
                var empl = new Empleado { IdEmpleado = Empleado.IdEmpleado, IdPersona = Empleado.IdPersona };


                return empl;
            }
            catch (Exception ex)
            {
                return new Empleado();
            }
        }



        [HttpPost]
        [Route("ObtenerDatosCompletosEmpleado")]
        public async Task<Response> ObtenerDatosCompletosEmpleado([FromBody]Empleado empleado)
        {
            Empleado empleadoObtenido = new Empleado();
            ListaEmpleadoViewModel empleadoEnviar = new ListaEmpleadoViewModel();
            var iomp = await db.IndiceOcupacionalModalidadPartida
                .Include(i => i.FondoFinanciamiento)

                .Include(i => i.IndiceOcupacional)
                .Include(i => i.IndiceOcupacional.ManualPuesto)
                .Include(i => i.IndiceOcupacional.RolPuesto)
                .ToListAsync();

            var configuracionViatico = await db.ConfiguracionViatico.ToListAsync();

            try
            {
                if (empleado.NombreUsuario != null)
                {
                    empleadoObtenido = await db.Empleado.Where(x => x.NombreUsuario == empleado.NombreUsuario)
                   .Include(x => x.Persona).Include(x => x.Dependencia).Include(x => x.IndiceOcupacionalModalidadPartida).ThenInclude(x => x.FondoFinanciamiento)
                    .Include(x => x.IndiceOcupacionalModalidadPartida).ThenInclude(x => x.IndiceOcupacional).ThenInclude(x => x.RolPuesto).ThenInclude(x => x.ConfiguracionViatico)
                    .Include(x => x.DatosBancarios).ThenInclude(x => x.InstitucionFinanciera)
                    .FirstOrDefaultAsync();
                }
                else if (empleado.Persona.Identificacion != null)
                {
                    empleadoObtenido = await db.Empleado
                        .Where(x => x.Persona.Identificacion == empleado.Persona.Identificacion)
                        .Include(x => x.Persona)
                        .Include(x => x.Dependencia)
                        //.Include(x => x.IndiceOcupacionalModalidadPartida).ThenInclude(x => x.FondoFinanciamiento)
                        //.Include(x => x.IndiceOcupacionalModalidadPartida)
                        //    .ThenInclude(x => x.IndiceOcupacional)
                        //        .ThenInclude(x => x.RolPuesto)
                        //            .ThenInclude(x => x.ConfiguracionViatico)
                        .Include(x => x.DatosBancarios)
                            .ThenInclude(x => x.InstitucionFinanciera)
                    .FirstOrDefaultAsync();
                }

                var empleadoVM = new ListaEmpleadoViewModel
                {
                    IdEmpleado = empleadoObtenido.IdEmpleado,

                    NombreApellido = string.Format(
                        "{0} {1}", 
                        empleadoObtenido.Persona.Nombres, 
                        empleadoObtenido.Persona.Apellidos
                    ),

                    Identificacion = empleadoObtenido.Persona.Identificacion,
                    TelefonoPrivado = empleadoObtenido.Persona.TelefonoPrivado,
                    CorreoPrivado = empleadoObtenido.Persona.CorreoPrivado,
                    Dependencia = (empleadoObtenido.Dependencia != null) ? empleadoObtenido.Dependencia.Nombre : "",


                    InstitucionBancaria =
                        (empleadoObtenido.DatosBancarios != null && empleadoObtenido.DatosBancarios.Count > 0)
                        ? empleadoObtenido.DatosBancarios.FirstOrDefault().InstitucionFinanciera.Nombre
                        : "",

                    NoCuenta =
                        (empleadoObtenido.DatosBancarios != null && empleadoObtenido.DatosBancarios.Count > 0)
                        ? empleadoObtenido.DatosBancarios.FirstOrDefault().NumeroCuenta
                        : "",

                    TipoCuenta =
                        (empleadoObtenido.DatosBancarios != null && empleadoObtenido.DatosBancarios.Count > 0)
                        ? empleadoObtenido.DatosBancarios.FirstOrDefault().Ahorros
                        : false,
                };

                if (empleadoObtenido.Activo == true)
                {

                    var iompItem = iomp
                        .Where(w=>w.IdEmpleado == empleadoObtenido.IdEmpleado)
                        .FirstOrDefault();


                    empleadoVM.RolPuesto = iompItem.IndiceOcupacional.ManualPuesto.Nombre;

                    empleadoVM.FondoFinanciamiento = iompItem.FondoFinanciamiento.Nombre;

                    empleadoVM.IdConfiguracionViatico = configuracionViatico
                        .Where(w=>w.IdRolPuesto == iompItem.IndiceOcupacional.IdRolPuesto)
                        .FirstOrDefault() != null
                        ? configuracionViatico
                        .Where(w => w.IdRolPuesto == iompItem.IndiceOcupacional.IdRolPuesto)
                        .FirstOrDefault()
                        .IdConfiguracionViatico
                        :0;

                    empleadoVM.FechaIngreso = iompItem.Fecha;
                    
                }

                empleadoEnviar = empleadoVM;

                return new Response
                {
                    Resultado = empleadoEnviar,
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Excepcion,
                    Resultado = null
                };
            }
        }





        [HttpPost]
        [Route("ListarEmpleadosdeDependenciaSinCesacion")]
        public async Task<List<EmpleadoSolicitudViewModel>> ListarEmpleadosdeDependencia([FromBody]Empleado empleado)
        {
            try
            {

                var listaEmpleados = await db.Empleado.Where(x => x.IdDependencia == empleado.IdDependencia && x.Activo == true).Include(x => x.Persona).ToListAsync();

                var listaEmpleado = new List<EmpleadoSolicitudViewModel>();
                foreach (var item in listaEmpleados)
                {

                    var empleados = new EmpleadoSolicitudViewModel
                    {
                        NombreApellido = item.Persona.Nombres + " " + item.Persona.Apellidos,
                        Identificacion = item.Persona.Identificacion,
                        IdEmpleado = item.IdEmpleado,
                    };

                    listaEmpleado.Add(empleados);
                }

                return listaEmpleado;



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
                return new List<EmpleadoSolicitudViewModel>();
            }
        }


        [HttpPost]
        [Route("ObtenerDatosEmpleadoSeleccionado")]
        public async Task<EmpleadoSolicitudViewModel> ObtenerDatosEmpleadoSeleccionado([FromBody]Empleado empleado)
        {
            try
            {

                var Empleado = await db.Empleado.Include(x => x.Persona).SingleOrDefaultAsync(m => m.IdEmpleado == empleado.IdEmpleado);


                var empleadoObtenido = new EmpleadoSolicitudViewModel
                {
                    NombreApellido = Empleado.Persona.Nombres + " " + Empleado.Persona.Apellidos,
                    Identificacion = Empleado.Persona.Identificacion,
                    IdEmpleado = Empleado.IdEmpleado,
                };



                return empleadoObtenido;



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
                return new EmpleadoSolicitudViewModel();
            }
        }




        [HttpPost]
        [Route("ListarEmpleadosdeJefe")]
        public async Task<List<EmpleadoSolicitudViewModel>> ListarEmpleadosdeJefe([FromBody]Empleado empleado)
        {
            try
            {
                var EmpleadoJefe = await db.Empleado
                                   .Where(e => e.NombreUsuario == empleado.NombreUsuario && e.EsJefe == true).FirstOrDefaultAsync();

                if (EmpleadoJefe != null)
                {

                    var listaSubordinados = await db.Empleado.Where(x => x.IdDependencia == EmpleadoJefe.IdDependencia && x.EsJefe == false).Include(x => x.Persona).Include(x => x.SolicitudPlanificacionVacaciones).ToListAsync();

                    var listaEmpleado = new List<EmpleadoSolicitudViewModel>();
                    foreach (var item in listaSubordinados)
                    {
                        var haSolicitado = false;
                        var aprobado = true;

                        if (item.SolicitudPlanificacionVacaciones.Count == 0)
                        {
                            haSolicitado = false;
                            aprobado = false;
                        }
                        else
                        {
                            foreach (var item1 in item.SolicitudPlanificacionVacaciones)
                            {
                                if (item1.Estado == 0)
                                {
                                    haSolicitado = true;
                                    aprobado = false;
                                    break;
                                }
                            }
                        }




                        var empleadoSolicitud = new EmpleadoSolicitudViewModel
                        {
                            NombreApellido = item.Persona.Nombres + " " + item.Persona.Apellidos,
                            Identificacion = item.Persona.Identificacion,
                            Aprobado = aprobado,
                            IdEmpleado = item.IdEmpleado,
                            HaSolicitado = haSolicitado,
                        };

                        listaEmpleado.Add(empleadoSolicitud);
                    }

                    return listaEmpleado;
                }

                return new List<EmpleadoSolicitudViewModel>();

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
                return new List<EmpleadoSolicitudViewModel>();
            }
        }

        [HttpPost]
        [Route("ListarEmpleadosdeJefeconSolucitudesVacaciones")]
        public async Task<List<EmpleadoSolicitudViewModel>> ListarEmpleadosdeJefeconSolucitudesVacaciones([FromBody]Empleado empleado)
        {
            try
            {
                var EmpleadoJefe = await db.Empleado
                                   .Where(e => e.NombreUsuario == empleado.NombreUsuario && e.EsJefe == true).FirstOrDefaultAsync();

                if (EmpleadoJefe != null)
                {

                    var listaSubordinados = await db.Empleado
                        .Where(x =>
                            x.IdDependencia == EmpleadoJefe.IdDependencia
                            && x.EsJefe == false
                            && x.Activo == true
                            ).Include(x => x.Persona).Include(x => x.SolicitudVacaciones).ToListAsync();

                    var listaEmpleado = new List<EmpleadoSolicitudViewModel>();
                    foreach (var item in listaSubordinados)
                    {
                        var haSolicitado = false;
                        var aprobado = true;

                        if (item.SolicitudVacaciones.Count == 0)
                        {
                            haSolicitado = false;
                            aprobado = false;
                        }
                        else
                        {
                            foreach (var item1 in item.SolicitudVacaciones)
                            {

                                if (item1.Estado == 3)
                                {
                                    haSolicitado = true;
                                    aprobado = false;
                                    break;
                                }
                            }
                        }




                        var empleadoSolicitud = new EmpleadoSolicitudViewModel
                        {
                            NombreApellido = item.Persona.Nombres + " " + item.Persona.Apellidos,
                            Identificacion = item.Persona.Identificacion,
                            Aprobado = aprobado,
                            IdEmpleado = item.IdEmpleado,
                            HaSolicitado = haSolicitado,
                        };

                        listaEmpleado.Add(empleadoSolicitud);
                    }

                    return listaEmpleado;
                }

                return new List<EmpleadoSolicitudViewModel>();
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
                return new List<EmpleadoSolicitudViewModel>();
            }
        }

        [HttpPost]
        [Route("ListarEmpleadosdeJefeconSolucitudesViaticos")]
        public async Task<List<EmpleadoSolicitudViewModel>> ListarEmpleadosdeJefeconSolucitudesViaticos([FromBody]Empleado empleado)
        {
            try
            {
                var EmpleadoJefe = await db.Empleado
                                   .Where(e => e.NombreUsuario == empleado.NombreUsuario && e.EsJefe == true && e.Activo == true).FirstOrDefaultAsync();

                if (EmpleadoJefe != null)
                {

                    var listaSubordinados = await db.Empleado.Where(x => x.IdDependencia == EmpleadoJefe.IdDependencia && x.EsJefe == false && x.Activo == true).Include(x => x.Persona).Include(x => x.SolicitudViatico).ToListAsync();

                    var listaEmpleado = new List<EmpleadoSolicitudViewModel>();
                    foreach (var item in listaSubordinados)
                    {
                        var haSolicitado = false;
                        var aprobado = true;

                        if (item.SolicitudViatico.Count != 0)
                        {
                            haSolicitado = false;
                            aprobado = false;

                            foreach (var item1 in item.SolicitudViatico)
                            {

                                if (item1.Estado == 0)
                                {
                                    haSolicitado = true;
                                    aprobado = false;
                                    break;
                                }
                            }

                            var empleadoSolicitud = new EmpleadoSolicitudViewModel
                            {
                                NombreApellido = item.Persona.Nombres + " " + item.Persona.Apellidos,
                                Identificacion = item.Persona.Identificacion,
                                Aprobado = aprobado,
                                IdEmpleado = item.IdEmpleado,
                                HaSolicitado = haSolicitado,
                            };

                            listaEmpleado.Add(empleadoSolicitud);
                        }
                    }
                    return listaEmpleado;
                }

                return new List<EmpleadoSolicitudViewModel>();
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
                return new List<EmpleadoSolicitudViewModel>();
            }
        }

        [HttpGet]
        [Route("ListarEmpleadosSolucitudesViaticosMDT")]
        public async Task<List<EmpleadoSolicitudViewModel>> ListarEmpleadosSolucitudesViaticosMDT()
        {
            try
            {
                //var solicitudVaticos = await db.Empleado.Select(x => new EmpleadoSolicitudViewModel
                //{
                //    IdSolicitud = x.SolicitudV,
                //    IdEmpleado = x.IdEmpleado,
                //    Identificacion = x.e
                //}).ToListAsync();
                var listaSubordinados = await db.Empleado.Where(x => x.Activo == true).Include(x => x.Persona).Include(x => x.SolicitudViatico).ToListAsync();

                var listaEmpleado = new List<EmpleadoSolicitudViewModel>();
                foreach (var item in listaSubordinados)
                {
                    var haSolicitado = false;
                    var aprobado = true;
                    var idSolicitud = 0;

                    if (item.SolicitudViatico.Count != 0)
                    {


                        foreach (var item1 in item.SolicitudViatico)
                        {
                            idSolicitud = item1.IdSolicitudViatico;
                        }

                        var empleadoSolicitud = new EmpleadoSolicitudViewModel
                        {
                            NombreApellido = item.Persona.Nombres + " " + item.Persona.Apellidos,
                            Identificacion = item.Persona.Identificacion,
                            Aprobado = aprobado,
                            IdEmpleado = item.IdEmpleado,
                            HaSolicitado = haSolicitado,
                            IdSolicitud = idSolicitud
                        };

                        listaEmpleado.Add(empleadoSolicitud);
                    }
                }
                return listaEmpleado;
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
                return new List<EmpleadoSolicitudViewModel>();
            }
        }

        [HttpGet]
        [Route("ListarEmpleadosTalentoHumanoconSolucitudesViaticos")]
        public async Task<List<EmpleadoSolicitudViewModel>> ListarEmpleadosTalentoHumanoconSolucitudesViaticos()
        {
            try
            {

                var listaSubordinados = await db.Empleado.Where(x => x.Activo == true).Include(x => x.Persona).Include(x => x.SolicitudViatico).ToListAsync();

                var listaEmpleado = new List<EmpleadoSolicitudViewModel>();
                foreach (var item in listaSubordinados)
                {
                    var haSolicitado = false;
                    var aprobado = true;

                    if (item.SolicitudViatico.Count != 0)
                    {
                        haSolicitado = false;
                        aprobado = false;

                        foreach (var item1 in item.SolicitudViatico)
                        {

                            if (item1.Estado == 0)
                            {
                                haSolicitado = true;
                                aprobado = false;
                                break;
                            }
                        }

                        var empleadoSolicitud = new EmpleadoSolicitudViewModel
                        {
                            NombreApellido = item.Persona.Nombres + " " + item.Persona.Apellidos,
                            Identificacion = item.Persona.Identificacion,
                            Aprobado = aprobado,
                            IdEmpleado = item.IdEmpleado,
                            HaSolicitado = haSolicitado,
                        };

                        listaEmpleado.Add(empleadoSolicitud);
                    }

                }
                return listaEmpleado;
                // return new List<EmpleadoSolicitudViewModel>();
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
                return new List<EmpleadoSolicitudViewModel>();
            }
        }


        [HttpPost]
        [Route("ListarEmpleadosdeJefeconHorasExtra")]
        public async Task<List<EmpleadoSolicitudViewModel>> ListarEmpleadosdeJefeconHorasExtra([FromBody]Empleado empleado)
        {
            try
            {
                var EmpleadoJefe = await db.Empleado
                                   .Where(e => e.NombreUsuario == empleado.NombreUsuario && e.EsJefe == true).FirstOrDefaultAsync();

                if (EmpleadoJefe != null)
                {

                    var listaSubordinados = await db.Empleado.Where(x => x.IdDependencia == EmpleadoJefe.IdDependencia && x.EsJefe == false).Include(x => x.Persona).Include(x => x.SolicitudHorasExtras).ToListAsync();

                    var listaEmpleado = new List<EmpleadoSolicitudViewModel>();
                    foreach (var item in listaSubordinados)
                    {
                        var haSolicitado = false;
                        var aprobado = true;

                        if (item.SolicitudHorasExtras.Count == 0)
                        {
                            haSolicitado = false;
                            aprobado = false;
                        }
                        else
                        {
                            foreach (var item1 in item.SolicitudHorasExtras)
                            {

                                if (item1.Estado == 0)
                                {
                                    haSolicitado = true;
                                    aprobado = false;
                                    break;
                                }
                            }
                        }




                        var empleadoSolicitud = new EmpleadoSolicitudViewModel
                        {
                            NombreApellido = item.Persona.Nombres + " " + item.Persona.Apellidos,
                            Identificacion = item.Persona.Identificacion,
                            Aprobado = aprobado,
                            IdEmpleado = item.IdEmpleado,
                            HaSolicitado = haSolicitado,
                        };

                        listaEmpleado.Add(empleadoSolicitud);
                    }

                    return listaEmpleado;
                }

                return new List<EmpleadoSolicitudViewModel>();
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
                return new List<EmpleadoSolicitudViewModel>();
            }
        }


        [HttpPost]
        [Route("EmpleadoSegunNombreUsuario")]
        public async Task<Empleado> EmpleadoSegunNombreUsuario([FromBody] string nombreUsuario)
        {
            try
            {

                var empleadoSegunNombre = await db.Empleado
                                  .Include(x => x.Persona)
                                  .Where(x => x.NombreUsuario == nombreUsuario)
                                  .FirstOrDefaultAsync();


                return empleadoSegunNombre;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // POST: api/Empleados

        [HttpPost]
        [Route("EditarEmpleado")]
        public async Task<Response> EditarEmpleado([FromBody] FichaEmpleadoViewModel datosBasicosEmpleado)
        {
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {

                    var empleadoActual = await db.Empleado.Where(x => x.IdEmpleado == datosBasicosEmpleado.IdEmpleado).FirstOrDefaultAsync();
                    var personaActual = await db.Persona.Where(x => x.IdPersona == empleadoActual.IdPersona).FirstOrDefaultAsync();

                    var personaCoincidencia = await db.Persona.Where(w => w.Identificacion == datosBasicosEmpleado.Identificacion).FirstOrDefaultAsync();



                    if (datosBasicosEmpleado.IdNacionalidadIndigena == 0)
                    {
                        datosBasicosEmpleado.IdNacionalidadIndigena = null;
                    }

                    if (personaCoincidencia == null || personaCoincidencia.IdPersona == personaActual.IdPersona)
                    {

                        personaActual.FechaNacimiento = datosBasicosEmpleado.FechaNacimiento;
                        personaActual.IdSexo = datosBasicosEmpleado.IdSexo;
                        personaActual.IdTipoIdentificacion = datosBasicosEmpleado.IdTipoIdentificacion;
                        personaActual.IdEstadoCivil = datosBasicosEmpleado.IdEstadoCivil;
                        personaActual.IdGenero = datosBasicosEmpleado.IdGenero;
                        personaActual.IdNacionalidad = datosBasicosEmpleado.IdNacionalidad;
                        personaActual.IdTipoSangre = datosBasicosEmpleado.IdTipoSangre;
                        personaActual.IdEtnia = datosBasicosEmpleado.IdEtnia;
                        personaActual.Identificacion = (datosBasicosEmpleado.Identificacion != null) ? datosBasicosEmpleado.Identificacion.ToString().ToUpper() : "";
                        personaActual.Nombres = (datosBasicosEmpleado.Nombres != null) ? datosBasicosEmpleado.Nombres.ToString().ToUpper() : "";
                        personaActual.Apellidos = (datosBasicosEmpleado.Apellidos != null) ? datosBasicosEmpleado.Apellidos.ToString().ToUpper() : "";
                        personaActual.TelefonoPrivado = datosBasicosEmpleado.TelefonoPrivado;
                        personaActual.TelefonoCasa = datosBasicosEmpleado.TelefonoCasa;
                        personaActual.CorreoPrivado = datosBasicosEmpleado.CorreoPrivado;
                        personaActual.LugarTrabajo = (datosBasicosEmpleado.LugarTrabajo != null) ? datosBasicosEmpleado.LugarTrabajo.ToString().ToUpper() : "";
                        personaActual.IdNacionalidadIndigena = datosBasicosEmpleado.IdNacionalidadIndigena;
                        personaActual.CallePrincipal = (datosBasicosEmpleado.CallePrincipal != null) ? datosBasicosEmpleado.CallePrincipal.ToString().ToUpper() : "";
                        personaActual.CalleSecundaria = (datosBasicosEmpleado.CalleSecundaria != null) ? datosBasicosEmpleado.CalleSecundaria.ToString().ToUpper() : "";
                        personaActual.Referencia = (datosBasicosEmpleado.Referencia != null) ? datosBasicosEmpleado.Referencia.ToString().ToUpper() : "";
                        personaActual.Numero = (datosBasicosEmpleado.Numero != null) ? datosBasicosEmpleado.Numero.ToString().ToUpper() : "";
                        personaActual.IdParroquia = datosBasicosEmpleado.IdParroquia;
                        personaActual.Ocupacion = (datosBasicosEmpleado.Ocupacion != null) ? datosBasicosEmpleado.Ocupacion.ToString().ToUpper() : "";
                        //1. Actualizar Persona Persona 
                        var personaInsertarda = db.Persona.Update(personaActual);

                        //2. Insertar Empleado 
                        empleadoActual.IdPersona = personaInsertarda.Entity.IdPersona;
                        empleadoActual.IdCiudadLugarNacimiento = datosBasicosEmpleado.IdCiudadLugarNacimiento;
                        empleadoActual.IdProvinciaLugarSufragio = datosBasicosEmpleado.IdProvinciaLugarSufragio;


                        empleadoActual.FechaIngresoSectorPublico = datosBasicosEmpleado.FechaIngresoSectorPublico;
                        empleadoActual.EsJefe = datosBasicosEmpleado.EsJefe;
                        empleadoActual.TrabajoSuperintendenciaBanco = datosBasicosEmpleado.TrabajoSuperintendenciaBanco;
                        empleadoActual.DeclaracionJurada = datosBasicosEmpleado.DeclaracionJurada;
                        empleadoActual.IngresosOtraActividad = (datosBasicosEmpleado.IngresosOtraActividad != null) ? datosBasicosEmpleado.IngresosOtraActividad.ToString().ToUpper() : "";
                        empleadoActual.MesesImposiciones = datosBasicosEmpleado.MesesImposiciones;
                        empleadoActual.DiasImposiciones = datosBasicosEmpleado.DiasImposiciones;
                        empleadoActual.FondosReservas = datosBasicosEmpleado.FondosReservas;
                        empleadoActual.NombreUsuario = datosBasicosEmpleado.NombreUsuario;
                        empleadoActual.Telefono = datosBasicosEmpleado.Telefono;
                        empleadoActual.Extension = datosBasicosEmpleado.ExtencionTelefonica;
                        empleadoActual.Nepotismo = datosBasicosEmpleado.Nepotismo;
                        empleadoActual.OtrosIngresos = datosBasicosEmpleado.OtrosIngresos;
                        empleadoActual.Detalle = (datosBasicosEmpleado.Detalle != null) ? datosBasicosEmpleado.Detalle.ToString().ToUpper() : "";
                        empleadoActual.RelacionSuperintendencia = (datosBasicosEmpleado.RelacionSuperintendencia != null) ? datosBasicosEmpleado.RelacionSuperintendencia.ToString().ToUpper() : "";
                        empleadoActual.AnoDesvinculacion = datosBasicosEmpleado.AnoDesvinculacion;
                        empleadoActual.TipoRelacion = (datosBasicosEmpleado.TipoRelacion != null) ? datosBasicosEmpleado.TipoRelacion.ToString().ToUpper() : "";
                        empleadoActual.IdBrigadaSSORol = datosBasicosEmpleado.IdBrigadaSSORol;

                        var empleado = db.Empleado.Update(empleadoActual);
                        await db.SaveChangesAsync();


                        transaction.Commit();

                        return new Response
                        {
                            IsSuccess = true,
                            Message = Mensaje.Satisfactorio,
                            Resultado = empleado.Entity
                        };
                    }

                    transaction.Rollback();

                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ExisteIdentificacion,
                    };


                }
                catch (Exception ex)
                {

                    transaction.Rollback();

                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.Error,
                    };
                }
            }

        }

        [HttpPost]
        [Route("InsertarEmpleado")]
        public async Task<Response> InsertarEmpleado([FromBody] DatosBasicosEmpleadoViewModel datosBasicosEmpleado)
        {

            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {

                    var respuesta = Existe(datosBasicosEmpleado);
                    if (datosBasicosEmpleado.IdNacionalidadIndigena == 0)
                    {
                        datosBasicosEmpleado.IdNacionalidadIndigena = null;
                    }
                    if (!respuesta.IsSuccess)
                    {
                        var persona = new Persona
                        {
                            FechaNacimiento = datosBasicosEmpleado.FechaNacimiento,
                            IdSexo = datosBasicosEmpleado.IdSexo,
                            IdTipoIdentificacion = datosBasicosEmpleado.IdTipoIdentificacion,
                            IdEstadoCivil = datosBasicosEmpleado.IdEstadoCivil,
                            IdGenero = datosBasicosEmpleado.IdGenero,
                            IdNacionalidad = datosBasicosEmpleado.IdNacionalidad,
                            IdTipoSangre = datosBasicosEmpleado.IdTipoSangre,
                            IdEtnia = datosBasicosEmpleado.IdEtnia,
                            Identificacion = datosBasicosEmpleado.Identificacion.ToString().ToUpper(),
                            Nombres = (datosBasicosEmpleado.Nombres != null) ? datosBasicosEmpleado.Nombres.ToString().ToUpper() : "",
                            Apellidos = (datosBasicosEmpleado.Apellidos != null) ? datosBasicosEmpleado.Apellidos.ToString().ToUpper() : "",
                            TelefonoPrivado = datosBasicosEmpleado.TelefonoPrivado,
                            TelefonoCasa = datosBasicosEmpleado.TelefonoCasa,
                            CorreoPrivado = datosBasicosEmpleado.CorreoPrivado,
                            LugarTrabajo = datosBasicosEmpleado.LugarTrabajo,
                            IdNacionalidadIndigena = datosBasicosEmpleado.IdNacionalidadIndigena,
                            CallePrincipal = (datosBasicosEmpleado.CallePrincipal != null) ? datosBasicosEmpleado.CallePrincipal.ToString().ToUpper() : "",
                            CalleSecundaria = (datosBasicosEmpleado.CalleSecundaria != null) ? datosBasicosEmpleado.CalleSecundaria.ToString().ToUpper() : "",
                            Referencia = (datosBasicosEmpleado.Referencia != null) ? datosBasicosEmpleado.Referencia.ToString().ToUpper() : "",
                            Numero = (datosBasicosEmpleado.Numero != null) ? datosBasicosEmpleado.Numero.ToString().ToUpper() : "",
                            IdParroquia = datosBasicosEmpleado.IdParroquia,
                            Ocupacion = (datosBasicosEmpleado.Ocupacion != null) ? datosBasicosEmpleado.Ocupacion.ToString().ToUpper() : "",



                        };
                        //1. Insertar Persona 
                        var personaInsertarda = await db.Persona.AddAsync(persona);
                        await db.SaveChangesAsync();

                        //2. Insertar Empleado (Inicializado : IdPersona, IdDependencia)
                        var empleadoinsertado = new Empleado
                        {
                            IdPersona = personaInsertarda.Entity.IdPersona,
                            IdCiudadLugarNacimiento = datosBasicosEmpleado.IdCiudadLugarNacimiento,
                            IdProvinciaLugarSufragio = datosBasicosEmpleado.IdProvinciaLugarSufragio,
                            FondosReservas = false,
                            MesesImposiciones = 0,
                            DiasImposiciones = 0,
                            FechaIngreso = DateTime.Now,
                        };
                        var empleado = await db.Empleado.AddAsync(empleadoinsertado);
                        await db.SaveChangesAsync();


                        transaction.Commit();

                        return new Response
                        {
                            IsSuccess = true,
                            Message = Mensaje.Satisfactorio,
                            Resultado = empleado.Entity
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

                    transaction.Rollback();

                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.Error,
                    };
                }
            }

        }

        // PUT: api/Empleados/5
        [HttpPut("{id}")]
        public async Task<Response> PutEmpleado([FromRoute] int id, [FromBody] EmpleadoViewModel empleadoViewModel)
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

                //1. Tabla Empleado
                var empleado = db.Empleado.Find(empleadoViewModel.Empleado.IdEmpleado);
                empleado.IdPersona = empleadoViewModel.Empleado.IdPersona;
                empleado.IdCiudadLugarNacimiento = empleadoViewModel.Empleado.IdCiudadLugarNacimiento;
                empleado.IdProvinciaLugarSufragio = empleadoViewModel.Empleado.IdProvinciaLugarSufragio;
                empleado.IdDependencia = empleadoViewModel.Empleado.IdDependencia;
                empleado.FechaIngreso = empleadoViewModel.Empleado.FechaIngreso;
                empleado.FechaIngresoSectorPublico = empleadoViewModel.Empleado.FechaIngresoSectorPublico;
                empleado.EsJefe = empleadoViewModel.Empleado.EsJefe;
                empleado.TrabajoSuperintendenciaBanco = empleadoViewModel.Empleado.TrabajoSuperintendenciaBanco;
                empleado.DeclaracionJurada = empleadoViewModel.Empleado.DeclaracionJurada;
                empleado.IngresosOtraActividad = empleadoViewModel.Empleado.IngresosOtraActividad;
                empleado.MesesImposiciones = empleadoViewModel.Empleado.MesesImposiciones;
                empleado.DiasImposiciones = empleadoViewModel.Empleado.DiasImposiciones;
                empleado.FondosReservas = empleadoViewModel.Empleado.FondosReservas;
                empleado.NombreUsuario = empleadoViewModel.Empleado.NombreUsuario;
                empleado.Activo = empleadoViewModel.Empleado.Activo;
                db.Empleado.Update(empleado);
                await db.SaveChangesAsync();

                //2. Tabla Persona
                var persona = db.Persona.Find(empleadoViewModel.Persona.IdPersona);
                persona.FechaNacimiento = empleadoViewModel.Persona.FechaNacimiento;
                persona.IdSexo = empleadoViewModel.Persona.IdSexo;
                persona.IdTipoIdentificacion = empleadoViewModel.Persona.IdTipoIdentificacion;
                persona.IdEstadoCivil = empleadoViewModel.Persona.IdEstadoCivil;
                persona.IdGenero = empleadoViewModel.Persona.IdGenero;
                persona.IdNacionalidad = empleadoViewModel.Persona.IdNacionalidad;
                persona.IdTipoSangre = empleadoViewModel.Persona.IdTipoSangre;
                persona.IdEtnia = empleadoViewModel.Persona.IdEtnia;
                persona.Identificacion = empleadoViewModel.Persona.Identificacion;
                persona.Nombres = empleadoViewModel.Persona.Nombres;
                persona.Apellidos = empleadoViewModel.Persona.Apellidos;
                persona.TelefonoPrivado = empleadoViewModel.Persona.TelefonoPrivado;
                persona.TelefonoCasa = empleadoViewModel.Persona.TelefonoCasa;
                persona.CorreoPrivado = empleadoViewModel.Persona.CorreoPrivado;
                persona.LugarTrabajo = empleadoViewModel.Persona.LugarTrabajo;
                persona.IdNacionalidadIndigena = empleadoViewModel.Persona.IdNacionalidadIndigena;
                persona.CallePrincipal = empleadoViewModel.Persona.CallePrincipal;
                persona.CalleSecundaria = empleadoViewModel.Persona.CalleSecundaria;
                persona.Referencia = empleadoViewModel.Persona.Referencia;
                persona.Numero = empleadoViewModel.Persona.Numero;
                persona.IdParroquia = empleadoViewModel.Persona.IdParroquia;
                persona.Ocupacion = empleadoViewModel.Persona.Ocupacion;
                db.Persona.Update(persona);
                await db.SaveChangesAsync();

                //3. Tabla Datos Bancarios
                var datosbancarios = db.DatosBancarios.Find(empleadoViewModel.DatosBancarios.IdDatosBancarios);
                datosbancarios.IdEmpleado = empleadoViewModel.DatosBancarios.IdEmpleado;
                datosbancarios.IdInstitucionFinanciera = empleadoViewModel.DatosBancarios.IdInstitucionFinanciera;
                datosbancarios.NumeroCuenta = empleadoViewModel.DatosBancarios.NumeroCuenta;
                datosbancarios.Ahorros = empleadoViewModel.DatosBancarios.Ahorros;
                db.DatosBancarios.Update(datosbancarios);
                await db.SaveChangesAsync();

                //4. Tabla Empleado Contacto Emergencia
                var empleadoContactoEmergencia = db.EmpleadoContactoEmergencia.Find(empleadoViewModel.EmpleadoContactoEmergencia.IdEmpleadoContactoEmergencia);
                empleadoContactoEmergencia.IdPersona = empleadoViewModel.EmpleadoContactoEmergencia.IdPersona;
                empleadoContactoEmergencia.IdEmpleado = empleadoViewModel.EmpleadoContactoEmergencia.IdEmpleado;
                empleadoContactoEmergencia.IdParentesco = empleadoViewModel.EmpleadoContactoEmergencia.IdParentesco;
                db.EmpleadoContactoEmergencia.Update(empleadoContactoEmergencia);
                await db.SaveChangesAsync();

                //5. Tabla Indice Ocupacional Modalidad Partida
                var indiceOcupacionalModalidadPartida = db.IndiceOcupacionalModalidadPartida.Find(empleadoViewModel.IndiceOcupacionalModalidadPartida.IdIndiceOcupacionalModalidadPartida);
                indiceOcupacionalModalidadPartida.IdIndiceOcupacional = empleadoViewModel.IndiceOcupacionalModalidadPartida.IdIndiceOcupacional;
                indiceOcupacionalModalidadPartida.IdEmpleado = empleadoViewModel.IndiceOcupacionalModalidadPartida.IdEmpleado;
                indiceOcupacionalModalidadPartida.IdFondoFinanciamiento = empleadoViewModel.IndiceOcupacionalModalidadPartida.IdFondoFinanciamiento;
                //indiceOcupacionalModalidadPartida.IdModalidadPartida = empleadoViewModel.IndiceOcupacionalModalidadPartida.IdModalidadPartida;
                indiceOcupacionalModalidadPartida.IdTipoNombramiento = empleadoViewModel.IndiceOcupacionalModalidadPartida.IdTipoNombramiento;
                indiceOcupacionalModalidadPartida.Fecha = empleadoViewModel.IndiceOcupacionalModalidadPartida.Fecha;
                indiceOcupacionalModalidadPartida.SalarioReal = empleadoViewModel.IndiceOcupacionalModalidadPartida.SalarioReal;
                db.IndiceOcupacionalModalidadPartida.Update(indiceOcupacionalModalidadPartida);
                await db.SaveChangesAsync();

                //6. Persona Estudio
                foreach (PersonaEstudio itemPersonaEstudio in empleadoViewModel.PersonaEstudio.Where(x => x.IdPersona == empleadoViewModel.Persona.IdPersona).ToList())
                {
                    var personaEstudio = db.PersonaEstudio.Find(itemPersonaEstudio.IdPersonaEstudio);
                    personaEstudio.FechaGraduado = itemPersonaEstudio.FechaGraduado;
                    personaEstudio.Observaciones = itemPersonaEstudio.Observaciones;
                    personaEstudio.IdTitulo = itemPersonaEstudio.IdTitulo;
                    personaEstudio.IdPersona = itemPersonaEstudio.IdPersona;
                    personaEstudio.NoSenescyt = itemPersonaEstudio.NoSenescyt;
                    db.PersonaEstudio.Update(personaEstudio);
                    await db.SaveChangesAsync();
                }

                //7. Trayectoria Laboral
                foreach (TrayectoriaLaboral itemTrayectoriaLaboral in empleadoViewModel.TrayectoriaLaboral.Where(x => x.IdPersona == empleadoViewModel.Persona.IdPersona).ToList())
                {
                    var trayectoriaLaboral = db.TrayectoriaLaboral.Find(itemTrayectoriaLaboral.IdTrayectoriaLaboral);
                    trayectoriaLaboral.IdPersona = itemTrayectoriaLaboral.IdPersona;
                    trayectoriaLaboral.FechaInicio = itemTrayectoriaLaboral.FechaInicio;
                    trayectoriaLaboral.FechaFin = itemTrayectoriaLaboral.FechaFin;
                    trayectoriaLaboral.Empresa = itemTrayectoriaLaboral.Empresa;
                    trayectoriaLaboral.PuestoTrabajo = itemTrayectoriaLaboral.PuestoTrabajo;
                    trayectoriaLaboral.DescripcionFunciones = itemTrayectoriaLaboral.DescripcionFunciones;
                    db.TrayectoriaLaboral.Update(trayectoriaLaboral);
                    await db.SaveChangesAsync();
                }


                //8. Persona Discapacidad
                foreach (PersonaDiscapacidad itemPersonaDiscapacidad in empleadoViewModel.PersonaDiscapacidad.Where(x => x.IdPersona == empleadoViewModel.Persona.IdPersona).ToList())
                {
                    var personaDiscapacidad = db.PersonaDiscapacidad.Find(itemPersonaDiscapacidad.IdPersonaDiscapacidad);
                    personaDiscapacidad.IdTipoDiscapacidad = itemPersonaDiscapacidad.IdTipoDiscapacidad;
                    personaDiscapacidad.IdPersona = itemPersonaDiscapacidad.IdPersona;
                    personaDiscapacidad.NumeroCarnet = itemPersonaDiscapacidad.NumeroCarnet;
                    personaDiscapacidad.Porciento = itemPersonaDiscapacidad.Porciento;
                    db.PersonaDiscapacidad.Update(personaDiscapacidad);
                    await db.SaveChangesAsync();
                }

                //9. Persona Enfermedad
                foreach (PersonaEnfermedad itemPersonaEnfermedad in empleadoViewModel.PersonaEnfermedad.Where(x => x.IdPersona == empleadoViewModel.Persona.IdPersona).ToList())
                {
                    var personaEnfermedad = db.PersonaEnfermedad.Find(itemPersonaEnfermedad.IdPersonaEnfermedad);
                    personaEnfermedad.IdTipoEnfermedad = itemPersonaEnfermedad.IdTipoEnfermedad;
                    personaEnfermedad.IdPersona = itemPersonaEnfermedad.IdPersona;
                    personaEnfermedad.InstitucionEmite = itemPersonaEnfermedad.InstitucionEmite;
                    db.PersonaEnfermedad.Update(personaEnfermedad);
                    await db.SaveChangesAsync();
                }

                //10. Persona Sustituto
                var personaSustituto = db.PersonaSustituto.Find(empleadoViewModel.PersonaSustituto.IdPersonaSustituto);
                personaSustituto.IdParentesco = empleadoViewModel.PersonaSustituto.IdParentesco;
                personaSustituto.IdPersona = empleadoViewModel.PersonaSustituto.IdPersona;
                db.PersonaSustituto.Update(personaSustituto);
                await db.SaveChangesAsync();

                //11. Discapacidad Sustituto
                foreach (DiscapacidadSustituto itemDiscapacidadSustituto in empleadoViewModel.DiscapacidadSustituto.Where(x => x.IdPersonaSustituto == empleadoViewModel.PersonaSustituto.IdPersonaSustituto).ToList())
                {
                    var discapacidadSustituto = db.DiscapacidadSustituto.Find(itemDiscapacidadSustituto.IdDiscapacidadSustituto);
                    discapacidadSustituto.IdTipoDiscapacidad = itemDiscapacidadSustituto.IdTipoDiscapacidad;
                    discapacidadSustituto.PorcentajeDiscapacidad = itemDiscapacidadSustituto.PorcentajeDiscapacidad;
                    discapacidadSustituto.NumeroCarnet = itemDiscapacidadSustituto.NumeroCarnet;
                    discapacidadSustituto.IdPersonaSustituto = itemDiscapacidadSustituto.IdPersonaSustituto;
                    db.DiscapacidadSustituto.Update(discapacidadSustituto);
                    await db.SaveChangesAsync();
                }


                //12. Enfermedad Sustituto
                foreach (EnfermedadSustituto itemEnfermedadSustituto in empleadoViewModel.EnfermedadSustituto.Where(x => x.IdPersonaSustituto == empleadoViewModel.PersonaSustituto.IdPersonaSustituto).ToList())
                {
                    var enfermedadSustituto = db.EnfermedadSustituto.Find(itemEnfermedadSustituto.IdEnfermedadSustituto);
                    enfermedadSustituto.IdTipoEnfermedad = itemEnfermedadSustituto.IdTipoEnfermedad;
                    enfermedadSustituto.InstitucionEmite = itemEnfermedadSustituto.InstitucionEmite;
                    enfermedadSustituto.IdPersonaSustituto = itemEnfermedadSustituto.IdPersonaSustituto;
                    db.EnfermedadSustituto.Update(enfermedadSustituto);
                    await db.SaveChangesAsync();
                }

                //13. Empleado Familiar 
                foreach (EmpleadoFamiliar itemEmpleadoFamiliar in empleadoViewModel.EmpleadoFamiliar.Where(x => x.IdEmpleado == empleadoViewModel.Empleado.IdEmpleado).ToList())
                {
                    var empleadoFamiliar = db.EmpleadoFamiliar.Find(itemEmpleadoFamiliar.IdEmpleadoFamiliar);
                    empleadoFamiliar.IdPersona = itemEmpleadoFamiliar.IdPersona;
                    empleadoFamiliar.IdEmpleado = itemEmpleadoFamiliar.IdEmpleado;
                    empleadoFamiliar.IdParentesco = itemEmpleadoFamiliar.IdParentesco;
                    db.EmpleadoFamiliar.Update(empleadoFamiliar);
                    await db.SaveChangesAsync();
                }


                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                    Resultado = empleadoViewModel
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


        // PUT: api/BasesDatos/5
        //[HttpPut("{id}")]
        //public async Task<Response> PutEmpleado([FromRoute] int id, [FromBody] Empleado empleado)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return new Response
        //            {
        //                IsSuccess = false,
        //                Message = Mensaje.ModeloInvalido
        //            };
        //        }


        //        var Empleado = db.Empleado.Find(empleado.IdEmpleado);

        //        Empleado.IdPersona = 5;
        //        Empleado.IdDependencia = 1;
        //        Empleado.IdCiudadLugarNacimiento = 1;
        //        Empleado.IdProvinciaLugarSufragio = 1;


        //        Empleado.FechaIngreso = empleado.FechaIngreso;
        //        Empleado.FechaIngresoSectorPublico = empleado.FechaIngresoSectorPublico;
        //        Empleado.TrabajoSuperintendenciaBanco = empleado.TrabajoSuperintendenciaBanco;
        //        Empleado.DeclaracionJurada = empleado.DeclaracionJurada;
        //        Empleado.IngresosOtraActividad = empleado.IngresosOtraActividad;
        //        Empleado.MesesImposiciones = empleado.MesesImposiciones;
        //        Empleado.DiasImposiciones = empleado.DiasImposiciones;
        //        Empleado.FondosReservas = empleado.FondosReservas;

        //        db.Empleado.Update(Empleado);
        //        await db.SaveChangesAsync();

        //        return new Response
        //        {
        //            IsSuccess = true,
        //            Message = Mensaje.Satisfactorio,
        //        };

        //    }
        //    catch (Exception ex)
        //    {
        //        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
        //        {
        //            ApplicationName = Convert.ToString(Aplicacion.SwTH),
        //            ExceptionTrace = ex.Message,
        //            Message = Mensaje.Excepcion,
        //            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
        //            LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
        //            UserName = "",

        //        });

        //        return new Response
        //        {
        //            IsSuccess = true,
        //            Message = Mensaje.Excepcion,
        //        };
        //    }

        //}




        // DELETE: api/BasesDatos/5
        [HttpDelete("{id}")]
        public async Task<Response> DeleteEmpleado([FromRoute] int id)
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

                var respuesta = await db.Empleado.SingleOrDefaultAsync(m => m.IdEmpleado == id);
                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.Empleado.Remove(respuesta);
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

        [HttpPost]
        [Route("InsertarEmpleadoContactoEmergencia")]
        public async Task<Response> InsertarEmpleadoContactoEmergencia([FromBody] EmpleadoContactoEmergencia empleadoContactoEmergencia)
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
                db.EmpleadoContactoEmergencia.Add(empleadoContactoEmergencia);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio
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


        [HttpPost]
        [Route("InsertarEmpleadoFamiliar")]
        public async Task<Response> InsertarEmpleadoFamiliar([FromBody] EmpleadoFamiliar empleadoFamiliar)
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
                db.EmpleadoFamiliar.Add(empleadoFamiliar);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio
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



        [HttpPost]
        [Route("InsertarEmpleadoNepotismo")]
        public async Task<Response> InsertarEmpleadoNepotismo([FromBody] EmpleadoNepotismo empleadoNepotismo)
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
                db.EmpleadoNepotismo.Add(empleadoNepotismo);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio
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



        [HttpPost]
        [Route("InsertarEmpleadoDatosBancarios")]
        public async Task<Response> InsertarEmpleadoDatosBancarios([FromBody] DatosBancarios datosBancarios)
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
                db.DatosBancarios.Add(datosBancarios);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio
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


        private Response Existe(DatosBasicosEmpleadoViewModel datosBasicosEmpleado)
        {
            var identificacion = datosBasicosEmpleado.Identificacion.ToUpper().TrimEnd().TrimStart();
            var Empleadorespuesta = db.Persona.Where(p => p.Identificacion == identificacion).Include(x => x.Empleado).FirstOrDefault();

            if (Empleadorespuesta != null)
            {
                if (datosBasicosEmpleado.IdEmpleado == Empleadorespuesta.Empleado.FirstOrDefault().IdEmpleado)
                {
                    return new Response
                    {
                        IsSuccess = false,
                    };
                }
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.ExisteRegistro,
                };

            }

            return new Response
            {
                IsSuccess = false,
            };
        }

        // PUT: api/BasesDatos/5
        [HttpPut("{id}")]
        public async Task<Response> PutEstadoEmpleado([FromRoute] int id, [FromBody] Empleado empleado)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return new Response
                //    {
                //        IsSuccess = false,
                //        Message = Mensaje.ModeloInvalido
                //    };
                //}

                var empleadoActualizar = await db.Empleado.Where(x => x.IdEmpleado == id).FirstOrDefaultAsync();

                if (empleadoActualizar != null)
                {
                    try
                    {
                        empleadoActualizar.Activo = false;
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


        [HttpPost]
        [Route("ListarEmpleadosConFAOTH")]
        public async Task<List<DocumentoFAOViewModel>> ListarEmpleadosConFAOTH([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {

                var lista = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).OrderBy(x => x.FechaIngreso).Where(x => x.NombreUsuario == documentoFAOViewModel.NombreUsuario).ToListAsync();

                var listaSalida2 = new List<DocumentoFAOViewModel>();

                var NombreDependencia = "";
                int idDependencia;
                int idsucursal;
                int idempleado;
                bool jefe;
                foreach (var item in lista)
                {
                    if (item.Dependencia == null)
                    {
                        NombreDependencia = "No Asignado";
                        //idDependencia = "";
                    }
                    else
                    {
                        NombreDependencia = item.Dependencia.Nombre;
                        idDependencia = item.Dependencia.IdDependencia;
                        idsucursal = item.Dependencia.IdSucursal;
                        idempleado = item.IdEmpleado;
                        jefe = item.EsJefe;
                        if (jefe == true)
                        {
                            var anio = DateTime.Now.Year;

                            var lista1 = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).Where(x => x.Dependencia.IdDependencia == idDependencia && x.Dependencia.IdSucursal == idsucursal).ToListAsync();
                            foreach (var item1 in lista1)
                            {
                                var empleadoid = item1.IdEmpleado;

                                var a = await db.FormularioAnalisisOcupacional.Where(x => x.Anio == anio && x.IdEmpleado == empleadoid && (x.Estado == EstadosFAO.RealizadoEspecialistaTH || x.Estado == EstadosFAO.RealizadoJefeTH)).FirstOrDefaultAsync();
                                if (a != null)
                                {
                                    listaSalida2.Add(new DocumentoFAOViewModel
                                    {
                                        IdEmpleado = item1.IdEmpleado,
                                        idDependencia = item1.Dependencia.IdDependencia,
                                        idsucursal = item1.Dependencia.IdSucursal,
                                        nombre = item1.Persona.Nombres,
                                        apellido = item1.Persona.Apellidos,
                                        NombreUsuario = item1.NombreUsuario,
                                        Identificacion = item1.Persona.Identificacion,
                                        estado = item1.FormularioAnalisisOcupacional.FirstOrDefault().Estado,
                                        IdFormularioAnalisisOcupacional = item1.FormularioAnalisisOcupacional.FirstOrDefault().IdFormularioAnalisisOcupacional



                                    });

                                }
                            }



                        }

                    }

                }
                return listaSalida2;



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
                return new List<DocumentoFAOViewModel>();
            }
        }

        #region cambio de puesto fao
        [HttpPost]
        [Route("ListarEmpleadosCambioPuestoFao")]
        public async Task<List<DocumentoFAOViewModel>> ListarEmpleadosCambioPuestoFao([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {

                var lista = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).OrderBy(x => x.FechaIngreso).Where(x => x.NombreUsuario == documentoFAOViewModel.NombreUsuario).ToListAsync();

                var listaSalida2 = new List<DocumentoFAOViewModel>();

                var NombreDependencia = "";
                int idDependencia;
                int idsucursal;
                int idempleado;
                bool jefe;
                foreach (var item in lista)
                {
                    if (item.Dependencia == null)
                    {
                        NombreDependencia = "No Asignado";
                        //idDependencia = "";
                    }
                    else
                    {
                        NombreDependencia = item.Dependencia.Nombre;
                        idDependencia = item.Dependencia.IdDependencia;
                        idsucursal = item.Dependencia.IdSucursal;
                        idempleado = item.IdEmpleado;
                        jefe = item.EsJefe;

                        var anio = DateTime.Now.Year;

                        var lista1 = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).Where(x => x.Dependencia.IdDependencia == idDependencia && x.Dependencia.IdSucursal == idsucursal).ToListAsync();
                        foreach (var item1 in lista1)
                        {
                            var empleadoid = item1.IdEmpleado;

                            var a = await db.FormularioAnalisisOcupacional.Where(x => x.Anio == anio && x.IdEmpleado == empleadoid && x.Estado == EstadosFAO.RealizadoJefeTH).FirstOrDefaultAsync();
                            if (a != null)
                            {
                                var cambiopuesto = db.AdministracionTalentoHumano.Where(x => x.IdFormularioAnalisisOcupacional == a.IdFormularioAnalisisOcupacional).Select(v => new InformeUATH
                                {
                                    IdManualPuestoOrigen = v.InformeUATH.FirstOrDefault().IdManualPuestoOrigen,
                                    IdManualPuestoDestino = v.InformeUATH.FirstOrDefault().IdManualPuestoDestino

                                }).FirstOrDefault();

                                if (cambiopuesto.IdManualPuestoDestino != cambiopuesto.IdManualPuestoOrigen)
                                {
                                    listaSalida2.Add(new DocumentoFAOViewModel
                                    {
                                        IdEmpleado = item1.IdEmpleado,
                                        idDependencia = item1.Dependencia.IdDependencia,
                                        idsucursal = item1.Dependencia.IdSucursal,
                                        nombre = item1.Persona.Nombres,
                                        apellido = item1.Persona.Apellidos,
                                        NombreUsuario = item1.NombreUsuario,
                                        Identificacion = item1.Persona.Identificacion,
                                        estado = item1.FormularioAnalisisOcupacional.FirstOrDefault().Estado,
                                        IdFormularioAnalisisOcupacional = item1.FormularioAnalisisOcupacional.FirstOrDefault().IdFormularioAnalisisOcupacional

                                    });
                                }

                            }
                        }


                    }

                }
                return listaSalida2;



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
                return new List<DocumentoFAOViewModel>();
            }
        }

        #endregion

        #region Sin cambio de puesto fao
        [HttpPost]
        [Route("ListarEmpleadosSinCambioPuestoFao")]
        public async Task<List<DocumentoFAOViewModel>> ListarEmpleadosSinCambioPuestoFao([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {

                var lista = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).OrderBy(x => x.FechaIngreso).Where(x => x.NombreUsuario == documentoFAOViewModel.NombreUsuario).ToListAsync();

                var listaSalida2 = new List<DocumentoFAOViewModel>();

                var NombreDependencia = "";
                int idDependencia;
                int idsucursal;
                int idempleado;
                bool jefe;
                foreach (var item in lista)
                {
                    if (item.Dependencia == null)
                    {
                        NombreDependencia = "No Asignado";
                        //idDependencia = "";
                    }
                    else
                    {
                        NombreDependencia = item.Dependencia.Nombre;
                        idDependencia = item.Dependencia.IdDependencia;
                        idsucursal = item.Dependencia.IdSucursal;
                        idempleado = item.IdEmpleado;
                        jefe = item.EsJefe;

                        var anio = DateTime.Now.Year;

                        var lista1 = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).Where(x => x.Dependencia.IdDependencia == idDependencia && x.Dependencia.IdSucursal == idsucursal).ToListAsync();
                        foreach (var item1 in lista1)
                        {
                            var empleadoid = item1.IdEmpleado;

                            var a = await db.FormularioAnalisisOcupacional.Where(x => x.Anio == anio && x.IdEmpleado == empleadoid && x.Estado == EstadosFAO.RealizadoJefeTH).FirstOrDefaultAsync();
                            if (a != null)
                            {
                                var cambiopuesto = db.AdministracionTalentoHumano.Where(x => x.IdFormularioAnalisisOcupacional == a.IdFormularioAnalisisOcupacional).Select(v => new InformeUATH
                                {
                                    IdManualPuestoOrigen = v.InformeUATH.FirstOrDefault().IdManualPuestoOrigen,
                                    IdManualPuestoDestino = v.InformeUATH.FirstOrDefault().IdManualPuestoDestino

                                }).FirstOrDefault();

                                if (cambiopuesto.IdManualPuestoDestino == cambiopuesto.IdManualPuestoOrigen)
                                {
                                    listaSalida2.Add(new DocumentoFAOViewModel
                                    {
                                        IdEmpleado = item1.IdEmpleado,
                                        idDependencia = item1.Dependencia.IdDependencia,
                                        idsucursal = item1.Dependencia.IdSucursal,
                                        nombre = item1.Persona.Nombres,
                                        apellido = item1.Persona.Apellidos,
                                        NombreUsuario = item1.NombreUsuario,
                                        Identificacion = item1.Persona.Identificacion,
                                        estado = item1.FormularioAnalisisOcupacional.FirstOrDefault().Estado,
                                        IdFormularioAnalisisOcupacional = item1.FormularioAnalisisOcupacional.FirstOrDefault().IdFormularioAnalisisOcupacional

                                    });
                                }

                            }
                        }

                    }

                }
                return listaSalida2;



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
                return new List<DocumentoFAOViewModel>();
            }
        }

        #endregion


        #region Historico fao
        [HttpPost]
        [Route("ListarEmpleadosHistoricoFao")]
        public async Task<List<DocumentoFAOViewModel>> ListarEmpleadosHistoricoFao([FromBody] DocumentoFAOViewModel documentoFAOViewModel)
        {
            try
            {

                var emple = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).OrderBy(x => x.FechaIngreso).Where(x => x.NombreUsuario == documentoFAOViewModel.NombreUsuario).FirstOrDefaultAsync();

                var listaSalida2 = new List<DocumentoFAOViewModel>();

                var lista1 = await db.Empleado.Include(x => x.Persona).Include(x => x.Dependencia).Where(x => x.Dependencia.IdDependencia == emple.IdDependencia).ToListAsync();
                foreach (var item1 in lista1)
                {
                    var empleadoid = item1.IdEmpleado;

                    var a = await db.FormularioAnalisisOcupacional.Where(x => x.IdEmpleado == empleadoid && x.Estado == EstadosFAO.RealizadoJefeTH).FirstOrDefaultAsync();
                    if (a != null)
                    {
                        var PuestoActual = db.IndiceOcupacionalModalidadPartida.OrderByDescending(x => x.Fecha).Where(x => x.IdEmpleado == empleadoid).Select(v => new ManualPuesto
                        {
                            IdManualPuesto = v.IndiceOcupacional.ManualPuesto.IdManualPuesto,
                            Nombre = v.IndiceOcupacional.ManualPuesto.Nombre,
                            Partida = v.NumeroPartidaIndividual,
                            GrupoOcupacional = v.IndiceOcupacional.EscalaGrados.GrupoOcupacional.TipoEscala,
                            Remuneracion = Convert.ToDecimal(v.IndiceOcupacional.EscalaGrados.Remuneracion)

                        }).FirstOrDefault();
                        if (PuestoActual != null)
                        {
                            var informeuth = await db.InformeUATH.Where(x => x.IdManualPuestoOrigen == PuestoActual.IdManualPuesto).FirstOrDefaultAsync();
                            var puestopropuesto = await db.IndiceOcupacional.Where(x => x.IdManualPuesto == informeuth.IdManualPuestoDestino).Select(e => new ManualPuesto
                            {
                                Nombre = e.ManualPuesto.Nombre,
                                GrupoOcupacional = e.EscalaGrados.GrupoOcupacional.TipoEscala,
                                Remuneracion = Convert.ToDecimal(e.EscalaGrados.Remuneracion)
                            }).FirstOrDefaultAsync();
                            if (puestopropuesto != null)
                            {

                                listaSalida2.Add(new DocumentoFAOViewModel
                                {
                                    IdEmpleado = item1.IdEmpleado,
                                    idDependencia = item1.Dependencia.IdDependencia,
                                    idsucursal = item1.Dependencia.IdSucursal,
                                    nombre = item1.Persona.Nombres,
                                    apellido = item1.Persona.Apellidos,
                                    NombreUsuario = item1.NombreUsuario,
                                    Identificacion = item1.Persona.Identificacion,
                                    Anio = item1.FormularioAnalisisOcupacional.FirstOrDefault().Anio,
                                    estado = item1.FormularioAnalisisOcupacional.FirstOrDefault().Estado,
                                    IdFormularioAnalisisOcupacional = item1.FormularioAnalisisOcupacional.FirstOrDefault().IdFormularioAnalisisOcupacional,
                                    PuestoActual = PuestoActual.Nombre,
                                    Partida = PuestoActual.Partida,
                                    GrupoOcupacional = PuestoActual.GrupoOcupacional,
                                    Remuneracion = PuestoActual.Remuneracion,

                                    NuevoPuesto = puestopropuesto.Nombre,
                                    GrupoOcupacionalPropuesta = puestopropuesto.GrupoOcupacional,
                                    RemuneracionPropuesta = puestopropuesto.Remuneracion

                                });
                            }
                        }
                    }
                }
                return listaSalida2;


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
                return new List<DocumentoFAOViewModel>();
            }
        }

        #endregion


        /// <summary>
        /// Requiere IdEmpleado
        /// </summary>
        /// <param name="situacionActualEmpleadoViewModel"></param>
        /// <returns></returns>
        [Route("ObtenerSituacionActualEmpleadoViewModel")]
        public async Task<Response> ObtenerSituacionActualEmpleadoViewModel([FromBody] SituacionActualEmpleadoViewModel situacionActualEmpleadoViewModel)
        {
            try
            {
                var modPar = db.IndiceOcupacionalModalidadPartida
                    .Include(i => i.IndiceOcupacional).ThenInclude(i => i.RolPuesto)
                    .Where(w => w.IdEmpleado == situacionActualEmpleadoViewModel.IdEmpleado)
                    .OrderByDescending(o => o.Fecha)
                    .FirstOrDefault();


                var modelo = await db.Empleado.Include(i => i.Dependencia).ThenInclude(i => i.Sucursal)
                    .Where(w => w.IdEmpleado == situacionActualEmpleadoViewModel.IdEmpleado)
                    .Select(s => new SituacionActualEmpleadoViewModel
                    {
                        IdEmpleado = s.IdEmpleado,
                        IdDependencia = Convert.ToInt32(s.IdDependencia),
                        NombreDependencia = s.Dependencia.Nombre,
                        IdSucursal = s.Dependencia.Sucursal.IdSucursal,
                        NombreSucursal = s.Dependencia.Sucursal.Nombre,
                        IdIndiceOcupacionalModalidadPartida = modPar.IdIndiceOcupacionalModalidadPartida

                    }
                    )
                    .FirstOrDefaultAsync();

                if (modelo != null)
                {

                    if (modPar != null)
                    {
                        modelo.IdCargo = modPar.IndiceOcupacional.RolPuesto.IdRolPuesto;
                        modelo.NombreCargo = modPar.IndiceOcupacional.RolPuesto.Nombre;
                        modelo.Remuneracion = (Decimal)modPar.SalarioReal;
                    }


                    return new Response
                    {
                        IsSuccess = true,
                        Resultado = modelo
                    };
                }

                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.RegistroNoEncontrado
                };

            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Excepcion
                };
            }
        }


        // POST: api/Empleados
        [HttpPost]
        [Route("ListarEmpleadosPorSucursal")]
        public async Task<List<DatosBasicosEmpleadoViewModel>> ListarEmpleadosPorSucursal([FromBody] EmpleadosPorSucursalViewModel empleadosPorSucursalViewModel)
        {


            try
            {

                var lista = new List<DatosBasicosEmpleadoViewModel>();

                if (empleadosPorSucursalViewModel.EmpleadosActivos == true)
                {
                    lista = await db.Empleado
                        .Where(w =>
                            w.Activo == true
                            && w.Dependencia.IdSucursal == empleadosPorSucursalViewModel.IdSucursal
                        )
                        .Select(x => new DatosBasicosEmpleadoViewModel
                        {
                            FechaNacimiento = x.Persona.FechaNacimiento.Value.Date,
                            IdSexo = Convert.ToInt32(x.Persona.IdSexo),
                            IdTipoIdentificacion = Convert.ToInt32(x.Persona.IdTipoIdentificacion),
                            IdEstadoCivil = Convert.ToInt32(x.Persona.IdEstadoCivil),
                            IdGenero = Convert.ToInt32(x.Persona.IdGenero),
                            IdNacionalidad = Convert.ToInt32(x.Persona.IdNacionalidad),
                            IdTipoSangre = Convert.ToInt32(x.Persona.IdTipoSangre),
                            IdEtnia = Convert.ToInt32(x.Persona.IdEtnia),
                            Identificacion = x.Persona.Identificacion,
                            Nombres = x.Persona.Nombres,
                            Apellidos = x.Persona.Apellidos,
                            TelefonoPrivado = x.Persona.TelefonoPrivado,
                            TelefonoCasa = x.Persona.TelefonoCasa,
                            CorreoPrivado = x.Persona.CorreoPrivado,
                            LugarTrabajo = x.Persona.LugarTrabajo,
                            IdNacionalidadIndigena = x.Persona.IdNacionalidadIndigena,
                            CallePrincipal = x.Persona.CallePrincipal,
                            CalleSecundaria = x.Persona.CalleSecundaria,
                            Referencia = x.Persona.Referencia,
                            Numero = x.Persona.Numero,
                            IdParroquia = Convert.ToInt32(x.Persona.IdParroquia),
                            Ocupacion = x.Persona.Ocupacion,
                            IdEmpleado = x.IdEmpleado,
                            IdProvinciaLugarSufragio = Convert.ToInt32(x.IdProvinciaLugarSufragio),
                            IdPaisLugarNacimiento = x.CiudadNacimiento.Provincia.Pais.IdPais,
                            IdCiudadLugarNacimiento = x.IdCiudadLugarNacimiento,
                            IdPaisLugarSufragio = x.ProvinciaSufragio.Pais.IdPais,
                            IdPaisLugarPersona = x.Persona.Parroquia.Ciudad.Provincia.Pais.IdPais,
                            IdCiudadLugarPersona = x.Persona.Parroquia.Ciudad.IdCiudad,
                            IdProvinciaLugarPersona = x.Persona.Parroquia.Ciudad.Provincia.IdProvincia,
                        }).ToListAsync();

                }
                else
                {

                    lista = await db.Empleado
                        .Where(x =>
                            x.Dependencia.IdSucursal == empleadosPorSucursalViewModel.IdSucursal
                        )
                        .Select(x => new DatosBasicosEmpleadoViewModel
                        {
                            FechaNacimiento = x.Persona.FechaNacimiento.Value.Date,
                            IdSexo = Convert.ToInt32(x.Persona.IdSexo),
                            IdTipoIdentificacion = Convert.ToInt32(x.Persona.IdTipoIdentificacion),
                            IdEstadoCivil = Convert.ToInt32(x.Persona.IdEstadoCivil),
                            IdGenero = Convert.ToInt32(x.Persona.IdGenero),
                            IdNacionalidad = Convert.ToInt32(x.Persona.IdNacionalidad),
                            IdTipoSangre = Convert.ToInt32(x.Persona.IdTipoSangre),
                            IdEtnia = Convert.ToInt32(x.Persona.IdEtnia),
                            Identificacion = x.Persona.Identificacion,
                            Nombres = x.Persona.Nombres,
                            Apellidos = x.Persona.Apellidos,
                            TelefonoPrivado = x.Persona.TelefonoPrivado,
                            TelefonoCasa = x.Persona.TelefonoCasa,
                            CorreoPrivado = x.Persona.CorreoPrivado,
                            LugarTrabajo = x.Persona.LugarTrabajo,
                            IdNacionalidadIndigena = x.Persona.IdNacionalidadIndigena,
                            CallePrincipal = x.Persona.CallePrincipal,
                            CalleSecundaria = x.Persona.CalleSecundaria,
                            Referencia = x.Persona.Referencia,
                            Numero = x.Persona.Numero,
                            IdParroquia = Convert.ToInt32(x.Persona.IdParroquia),
                            Ocupacion = x.Persona.Ocupacion,
                            IdEmpleado = x.IdEmpleado,
                            IdProvinciaLugarSufragio = Convert.ToInt32(x.IdProvinciaLugarSufragio),
                            IdPaisLugarNacimiento = x.CiudadNacimiento.Provincia.Pais.IdPais,
                            IdCiudadLugarNacimiento = x.IdCiudadLugarNacimiento,
                            IdPaisLugarSufragio = x.ProvinciaSufragio.Pais.IdPais,
                            IdPaisLugarPersona = x.Persona.Parroquia.Ciudad.Provincia.Pais.IdPais,
                            IdCiudadLugarPersona = x.Persona.Parroquia.Ciudad.IdCiudad,
                            IdProvinciaLugarPersona = x.Persona.Parroquia.Ciudad.Provincia.IdProvincia,
                        }).ToListAsync();
                }



                return lista;

            }
            catch (Exception ex)
            {

                return new List<DatosBasicosEmpleadoViewModel>();
            }
        }


        /// <summary>
        /// Necesario: NombreUsuario
        /// Devuelve la lista de los empleados que pertenecen a la dependencia del usuario logueado
        /// que est�n activos exceptuando al usuario actual
        /// </summary>
        /// <param name="idFiltrosViewModel"></param>
        /// <returns></returns>
        // POST: api/Empleados
        [HttpPost]
        [Route("ListarMisEmpleados")]
        public async Task<List<DatosBasicosEmpleadoViewModel>> ListarMisEmpleados([FromBody] IdFiltrosViewModel idFiltrosViewModel)
        {

            var lista = new List<DatosBasicosEmpleadoViewModel>();

            try
            {

                var empleado = await db.Empleado
                    .Where(w => w.NombreUsuario == idFiltrosViewModel.NombreUsuario)
                    .FirstOrDefaultAsync();


                lista = await db.Empleado
                        .Where(w =>
                            w.Activo == true
                            && w.IdDependencia == empleado.IdDependencia
                            && w.IdEmpleado != empleado.IdEmpleado
                        )
                        .Select(x => new DatosBasicosEmpleadoViewModel
                        {
                            FechaNacimiento = x.Persona.FechaNacimiento.Value.Date,
                            IdSexo = Convert.ToInt32(x.Persona.IdSexo),
                            IdTipoIdentificacion = Convert.ToInt32(x.Persona.IdTipoIdentificacion),
                            IdEstadoCivil = Convert.ToInt32(x.Persona.IdEstadoCivil),
                            IdGenero = Convert.ToInt32(x.Persona.IdGenero),
                            IdNacionalidad = Convert.ToInt32(x.Persona.IdNacionalidad),
                            IdTipoSangre = Convert.ToInt32(x.Persona.IdTipoSangre),
                            IdEtnia = Convert.ToInt32(x.Persona.IdEtnia),
                            Identificacion = x.Persona.Identificacion,
                            Nombres = x.Persona.Nombres,
                            Apellidos = x.Persona.Apellidos,
                            TelefonoPrivado = x.Persona.TelefonoPrivado,
                            TelefonoCasa = x.Persona.TelefonoCasa,
                            CorreoPrivado = x.Persona.CorreoPrivado,
                            LugarTrabajo = x.Persona.LugarTrabajo,
                            IdNacionalidadIndigena = x.Persona.IdNacionalidadIndigena,
                            CallePrincipal = x.Persona.CallePrincipal,
                            CalleSecundaria = x.Persona.CalleSecundaria,
                            Referencia = x.Persona.Referencia,
                            Numero = x.Persona.Numero,
                            IdParroquia = Convert.ToInt32(x.Persona.IdParroquia),
                            Ocupacion = x.Persona.Ocupacion,
                            IdEmpleado = x.IdEmpleado,
                            IdProvinciaLugarSufragio = Convert.ToInt32(x.IdProvinciaLugarSufragio),
                            IdPaisLugarNacimiento = x.CiudadNacimiento.Provincia.Pais.IdPais,
                            IdCiudadLugarNacimiento = x.IdCiudadLugarNacimiento,
                            IdPaisLugarSufragio = x.ProvinciaSufragio.Pais.IdPais,
                            IdPaisLugarPersona = x.Persona.Parroquia.Ciudad.Provincia.Pais.IdPais,
                            IdCiudadLugarPersona = x.Persona.Parroquia.Ciudad.IdCiudad,
                            IdProvinciaLugarPersona = x.Persona.Parroquia.Ciudad.Provincia.IdProvincia,
                        }).ToListAsync();

                return lista;
            }
            catch (Exception ex)
            {
                return lista;
            }
        }


        [HttpPost]
        [Route("ObtenerEmpleadoDistributivo")]
        public async Task<Response> ObtenerEmpleadoDistributivo([FromBody] int IdEmpleado)
        {
            try
            {

                String mensaje = "";

                var Empleado = await db.Empleado.Where(w => w.IdEmpleado == IdEmpleado).FirstOrDefaultAsync();

                var iomp = await db.IndiceOcupacionalModalidadPartida
                    .Include(i => i.TipoNombramiento.RelacionLaboral)
                    
                    .Include(i => i.IndiceOcupacional)
                    .Include(i => i.IndiceOcupacional.Dependencia.Sucursal)
                    .Include(i => i.IndiceOcupacional.ManualPuesto)
                    .Include(i => i.IndiceOcupacional.RolPuesto)
                    .Include(i => i.IndiceOcupacional.EscalaGrados)

                    .Include(i => i.ModalidadPartida)
                    .Where(w => w.IdEmpleado == IdEmpleado)
                    .OrderByDescending(o => o.IdIndiceOcupacionalModalidadPartida)
                    .FirstOrDefaultAsync();

                var primerIOMP = await db.IndiceOcupacionalModalidadPartida
                    .Include(i => i.TipoNombramiento.RelacionLaboral)
                    .Include(i => i.IndiceOcupacional)
                    .Include(i => i.IndiceOcupacional.Dependencia.Sucursal)
                    .Include(i => i.IndiceOcupacional.ManualPuesto)
                    .Include(i => i.IndiceOcupacional.RolPuesto)
                    .Include(i => i.IndiceOcupacional.EscalaGrados)
                    .Include(i => i.ModalidadPartida)
                    .Where(w => w.IdEmpleado == IdEmpleado)
                    .OrderBy(o => o.IdIndiceOcupacionalModalidadPartida)
                    .FirstOrDefaultAsync();

                var fechaPrimerIngreso = primerIOMP.Fecha;

                // ** Lista de acciones generadas para el empleado
                var listaAccionesEmpleado = await db.AccionPersonal
                    .Include(i => i.TipoAccionPersonal)
                    .Where(w =>
                        w.IdEmpleado == IdEmpleado
                    )
                    .ToListAsync();

                // ** Obtiene una lista de desvinculaciones del empleado 
                var listaDesvinculacionesEmpleado = listaAccionesEmpleado
                .Where(w =>
                    w.TipoAccionPersonal.Definitivo == true
                    && w.TipoAccionPersonal.DesactivarEmpleado == true
                )
                .OrderByDescending(o => o.FechaRige)
                .ToList();

                //** Obtener si hay una desvinculaci�n despu�s de la �ltima fecha de ingreso 
                var desvinculacion = listaDesvinculacionesEmpleado
                    .Where(w => w.FechaRige >= iomp.Fecha)
                    .FirstOrDefault();

                // ** modelo para enviarse como respuesta
                var modelo = new EmpleadoViewModel
                {
                    Empleado = Empleado,
                    FechaPrimerIngreso = fechaPrimerIngreso
                };


                if (Empleado.Activo == true)
                {

                    modelo.IndiceOcupacionalModalidadPartida = iomp;
                    modelo.Dependencia = iomp.IndiceOcupacional.Dependencia;
                    modelo.Dependencia.Sucursal = iomp.IndiceOcupacional.Dependencia.Sucursal;
                    modelo.IndiceOcupacional = iomp.IndiceOcupacional;
                    modelo.IndiceOcupacionalModalidadPartida.NumeroPartidaIndividual = iomp.NumeroPartidaIndividual
                        + iomp.CodigoContrato;
                    modelo.SalarioReal = iomp.SalarioReal != null && iomp.SalarioReal > 0?true:false;

                    if (iomp.ModalidadPartida != null)
                    {
                        modelo.IndiceOcupacional.IdModalidadPartida = iomp.ModalidadPartida.IdModalidadPartida;
                    }
                    modelo.FechaPrimerIngreso = primerIOMP.Fecha;
                }
                else
                {
                    mensaje = Mensaje.RegistroNoEncontrado;

                    modelo.FechaPrimerIngreso = primerIOMP.Fecha;
                }

                return new Response
                {
                    Message = mensaje,
                    IsSuccess = true,
                    Resultado = modelo
                };


            }
            catch (Exception ex)
            {

                return new Response
                {
                    Message = Mensaje.Excepcion,
                    IsSuccess = false
                };

            }
        }



    }
}