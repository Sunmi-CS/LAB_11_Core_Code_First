using Eventos.Web.Models;
using Eventos.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Eventos.Web.Controllers;

public class AsistentesController : Controller
{
    private readonly AsistenteRepositorio _repositorio;

    public AsistentesController(AsistenteRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index()
    {
        var asistentes = await _repositorio.ListarActivosAsync();
        return View(asistentes);
    }

    public IActionResult Create()
    {
        return View(new Asistente());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Asistente asistente)
    {
        if (!string.IsNullOrWhiteSpace(asistente.Dni) &&
            await _repositorio.ExisteDniAsync(asistente.Dni))
        {
            ModelState.AddModelError(
                nameof(asistente.Dni),
                "Este DNI ya se encuentra registrado.");
        }

        if (!string.IsNullOrWhiteSpace(asistente.Correo) &&
            await _repositorio.ExisteCorreoAsync(asistente.Correo))
        {
            ModelState.AddModelError(
                nameof(asistente.Correo),
                "Este correo ya se encuentra registrado.");
        }

        if (!ModelState.IsValid)
            return View(asistente);

        asistente.Activo = true;

        await _repositorio.CrearAsync(asistente);

        TempData["Mensaje"] = "Asistente registrado correctamente.";

        return RedirectToAction(nameof(Index));
    }
}