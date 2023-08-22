using System.Linq;
using Microsoft.EntityFrameworkCore;
using SistemaContableCSG.Data;
using SistemaContableCSG.Models;

public interface IPeriodoHelper
{
    bool IsPeriodoIniciado();
}

public class PeriodoHelper : IPeriodoHelper
{
    private readonly ApplicationDbContext _dbContext;

    public PeriodoHelper(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool IsPeriodoIniciado()
    {
        return _dbContext.Periodo.Any(p => p.Iniciado);
    }
}

