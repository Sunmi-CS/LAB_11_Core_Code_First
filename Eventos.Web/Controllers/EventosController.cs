using Eventos.Web.Models;
using Eventos.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Eventos.Web.Controllers;

public class EventosController : Controller
{
    private readonly EventoRepositorio _repositorio;

    public EventosController(EventoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index(int? categoriaId)
    {
        var eventos = await _repositorio.ListarActivosAsync(categoriaId);

        var inscritos = new Dictionary<int, int>();

        foreach (var evento in eventos)
        {
            inscritos[evento.Id] =
                await _repositorio.ContarConfirmadasAsync(evento.Id);
        }

        ViewBag.Categorias =
            await _repositorio.ObtenerCategoriasActivasAsync();

        ViewBag.CategoriaId = categoriaId;
        ViewBag.Inscritos = inscritos;

        return View(eventos);
    }

    public async Task<IActionResult> Details(int id)
    {
        var evento = await _repositorio.ObtenerAsync(id);

        if (evento == null || !evento.Activo)
            return NotFound();

        ViewBag.Inscritos =
            await _repositorio.ContarConfirmadasAsync(id);

        return View(evento);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCategoriasAsync();
        return View(new Evento());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Evento evento)
    {
        if (!await _repositorio.CategoriaActivaAsync(
            evento.CategoriaId))
        {
            ModelState.AddModelError(
                nameof(evento.CategoriaId),
                "Selecciona una categoría activa.");
        }

        if (evento.Fecha.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(evento.Fecha),
                "La fecha del evento no puede estar en el pasado.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(evento.CategoriaId);
            return View(evento);
        }

        evento.Activo = true;

        await _repositorio.CrearAsync(evento);

        TempData["Mensaje"] = "Evento registrado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var evento = await _repositorio.ObtenerAsync(id);

        if (evento == null || !evento.Activo)
            return NotFound();

        await CargarCategoriasAsync(evento.CategoriaId);

        return View(evento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Evento evento)
    {
        if (id != evento.Id)
            return BadRequest();

        if (!await _repositorio.CategoriaActivaAsync(
            evento.CategoriaId))
        {
            ModelState.AddModelError(
                nameof(evento.CategoriaId),
                "Selecciona una categoría activa.");
        }

        if (evento.Fecha.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(evento.Fecha),
                "La fecha del evento no puede estar en el pasado.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(evento.CategoriaId);
            return View(evento);
        }

        var actualizado = await _repositorio.ActualizarAsync(evento);

        if (!actualizado)
            return NotFound();

        TempData["Mensaje"] = "Evento actualizado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var evento = await _repositorio.ObtenerAsync(id);

        if (evento == null || !evento.Activo)
            return NotFound();

        return View(evento);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var eliminado =
            await _repositorio.EliminarLogicamenteAsync(id);

        if (!eliminado)
            return NotFound();

        TempData["Mensaje"] = "Evento desactivado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCategoriasAsync(int? seleccionada = null)
    {
        ViewBag.CategoriaId = new SelectList(
            await _repositorio.ObtenerCategoriasActivasAsync(),
            "Value",
            "Text",
            seleccionada);
    }
}