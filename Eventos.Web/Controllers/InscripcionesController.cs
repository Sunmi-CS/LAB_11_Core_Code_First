using Eventos.Web.Models;
using Eventos.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Eventos.Web.Controllers;

public class InscripcionesController : Controller
{
    private readonly InscripcionRepositorio _repositorio;

    public InscripcionesController(
        InscripcionRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index()
    {
        var inscripciones = await _repositorio.ListarAsync();
        return View(inscripciones);
    }

    public async Task<IActionResult> Create()
    {
        await CargarListasAsync();
        return View(new Inscripcion());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inscripcion inscripcion)
    {
        var evento = await _repositorio.ObtenerEventoActivoAsync(
            inscripcion.EventoId);

        if (evento == null)
        {
            ModelState.AddModelError(
                nameof(inscripcion.EventoId),
                "Selecciona un evento activo.");
        }
        else if (evento.Fecha < DateTime.Now)
        {
            ModelState.AddModelError(
                nameof(inscripcion.EventoId),
                "No puedes inscribirte en un evento que ya pasó.");
        }

        if (!await _repositorio.AsistenteActivoAsync(
            inscripcion.AsistenteId))
        {
            ModelState.AddModelError(
                nameof(inscripcion.AsistenteId),
                "Selecciona un asistente activo.");
        }

        if (evento != null &&
            await _repositorio.YaEstaConfirmadaAsync(
                inscripcion.EventoId,
                inscripcion.AsistenteId))
        {
            ModelState.AddModelError(
                string.Empty,
                "El asistente ya tiene una inscripción confirmada a este evento.");
        }

        if (evento != null)
        {
            var confirmadas =
                await _repositorio.ContarConfirmadasAsync(evento.Id);

            if (confirmadas >= evento.Aforo)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El evento alcanzó su aforo máximo.");
            }
        }

        if (!ModelState.IsValid)
        {
            await CargarListasAsync(
                inscripcion.EventoId,
                inscripcion.AsistenteId);

            return View(inscripcion);
        }

        inscripcion.Estado = "Confirmada";
        inscripcion.FechaInscripcion = DateTime.Now;

        await _repositorio.CrearAsync(inscripcion);

        TempData["Mensaje"] = "Inscripción registrada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var cancelada = await _repositorio.CancelarAsync(id);

        if (!cancelada)
        {
            TempData["Error"] =
                "No se encontró una inscripción confirmada para cancelar.";

            return RedirectToAction(nameof(Index));
        }

        TempData["Mensaje"] = "Inscripción cancelada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync(
        int? eventoSeleccionado = null,
        int? asistenteSeleccionado = null)
    {
        ViewBag.EventoId =
            await _repositorio.ObtenerEventosActivosAsync();

        ViewBag.AsistenteId =
            await _repositorio.ObtenerAsistentesActivosAsync();

        ViewBag.EventoSeleccionado = eventoSeleccionado;
        ViewBag.AsistenteSeleccionado = asistenteSeleccionado;
    }
}