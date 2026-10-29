using Eventos.Web.Data;
using Eventos.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Web.Repositorios;

public class EventoRepositorio
{
    private readonly EventosContext _context;

    public EventoRepositorio(EventosContext context)
    {
        _context = context;
    }

    public async Task<List<Evento>> ListarActivosAsync(
        int? categoriaId = null)
    {
        var consulta = _context.Eventos
            .AsNoTracking()
            .Include(e => e.Categoria)
            .Where(e => e.Activo);

        if (categoriaId.HasValue)
        {
            consulta = consulta.Where(
                e => e.CategoriaId == categoriaId.Value);
        }

        return await consulta
            .OrderBy(e => e.Fecha)
            .ToListAsync();
    }

    public async Task<Evento?> ObtenerAsync(int id)
    {
        return await _context.Eventos
            .AsNoTracking()
            .Include(e => e.Categoria)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<int> ContarConfirmadasAsync(int eventoId)
    {
        return await _context.Inscripciones
            .AsNoTracking()
            .CountAsync(i =>
                i.EventoId == eventoId &&
                i.Estado == "Confirmada");
    }

    public async Task<List<SelectListItem>>
        ObtenerCategoriasActivasAsync()
    {
        return await _context.Categorias
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nombre
            })
            .ToListAsync();
    }

    public async Task<bool> CategoriaActivaAsync(int categoriaId)
    {
        return await _context.Categorias
            .AsNoTracking()
            .AnyAsync(c =>
                c.Id == categoriaId &&
                c.Activo);
    }

    public async Task<bool> ExisteAsync(int id)
    {
        return await _context.Eventos
            .AnyAsync(e => e.Id == id && e.Activo);
    }

    public async Task CrearAsync(Evento evento)
    {
        _context.Eventos.Add(evento);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ActualizarAsync(Evento evento)
    {
        var existente = await _context.Eventos
            .FirstOrDefaultAsync(e =>
                e.Id == evento.Id && e.Activo);

        if (existente == null)
            return false;

        existente.Titulo = evento.Titulo;
        existente.Fecha = evento.Fecha;
        existente.Lugar = evento.Lugar;
        existente.Aforo = evento.Aforo;
        existente.Precio = evento.Precio;
        existente.Modalidad = evento.Modalidad;
        existente.CategoriaId = evento.CategoriaId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarLogicamenteAsync(int id)
    {
        var evento = await _context.Eventos
            .FirstOrDefaultAsync(e =>
                e.Id == id && e.Activo);

        if (evento == null)
            return false;

        evento.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }
}