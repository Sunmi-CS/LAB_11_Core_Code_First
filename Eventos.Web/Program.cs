using Eventos.Web.Data;

using Eventos.Web.Models;
using Eventos.Web.Repositorios;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<EventosContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

builder.Services.AddScoped<EventoRepositorio>();
builder.Services.AddScoped<AsistenteRepositorio>();
builder.Services.AddScoped<InscripcionRepositorio>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<EventosContext>();

    await context.Database.MigrateAsync();

    if (!await context.Categorias.AnyAsync())
    {
        context.Categorias.AddRange(
            new Categoria { Nombre = "Tecnología", Activo = true },
            new Categoria { Nombre = "Educación", Activo = true },
            new Categoria { Nombre = "Emprendimiento", Activo = true }
        );

        await context.SaveChangesAsync();
    }

    if (!await context.Eventos.AnyAsync())
    {
        var categorias = await context.Categorias
            .OrderBy(c => c.Id)
            .ToListAsync();

        context.Eventos.AddRange(
            new Evento
            {
                Titulo = "Feria de Innovación",
                Fecha = DateTime.Today.AddDays(10),
                Lugar = "Auditorio principal",
                Aforo = 200,
                Precio = 0,
                CategoriaId = categorias[0].Id,
                Modalidad = "Presencial",
                Activo = true
            },
            new Evento
            {
                Titulo = "Desarrollo Web",
                Fecha = DateTime.Today.AddDays(15),
                Lugar = "Laboratorio 1",
                Aforo = 80,
                Precio = 25,
                CategoriaId = categorias[0].Id,
                Modalidad = "Presencial",
                Activo = true
            },
            new Evento
            {
                Titulo = "Educación Digital",
                Fecha = DateTime.Today.AddDays(20),
                Lugar = "Plataforma virtual",
                Aforo = 500,
                Precio = 10,
                CategoriaId = categorias[1].Id,
                Modalidad = "Virtual",
                Activo = true
            },
            new Evento
            {
                Titulo = "Ideas de Negocio",
                Fecha = DateTime.Today.AddDays(25),
                Lugar = "Sala de conferencias",
                Aforo = 100,
                Precio = 15,
                CategoriaId = categorias[2].Id,
                Modalidad = "Presencial",
                Activo = true
            }
        );

        await context.SaveChangesAsync();
    }

    if (!await context.Asistentes.AnyAsync())
    {
        context.Asistentes.AddRange(
            new Asistente
            {
                Nombres = "Sunmi Casaño",
                Dni = "12345678",
                Correo = "sunmi@gmail.com",
                Telefono = "987654321",
                Activo = true
            },
            new Asistente
            {
                Nombres = "Luis Pérez",
                Dni = "23456789",
                Correo = "luis@gmail.com",
                Telefono = "987654322",
                Activo = true
            },
            new Asistente
            {
                Nombres = "María López",
                Dni = "34567890",
                Correo = "maria@gmail.com",
                Telefono = null,
                Activo = true
            },
            new Asistente
            {
                Nombres = "Carlos Ruiz",
                Dni = "45678901",
                Correo = "carlos@gmail.com",
                Telefono = "987654324",
                Activo = true
            },
            new Asistente
            {
                Nombres = "Sofía Díaz",
                Dni = "56789012",
                Correo = "sofia@gmail.com",
                Telefono = "987654325",
                Activo = true
            }
        );

        await context.SaveChangesAsync();
    }
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Eventos}/{action=Index}/{id?}");

app.Run();