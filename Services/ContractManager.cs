using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Services;

public class ContractManager
{
    private readonly PtichkaContext _context;

    public ContractManager(PtichkaContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Создаёт новый договор.
    /// </summary>
    public Contract Create(int clientId, int employeeId, int hallId,
                           string contractNumber, DateOnly eventDate,
                           TimeOnly start, TimeOnly end)
    {
        var contract = new Contract
        {
            ContractNumber = contractNumber,
            ClientId = clientId,
            EmployeeId = employeeId,
            HallId = hallId,
            EventDate = eventDate,
            StartTime = start,
            EndTime = end,
            TotalAmount = 0,
            ContractStatus = "Draft",
            CreatedDate = DateTime.Now
        };

        _context.Contracts.Add(contract);
        _context.SaveChanges();
        return contract;
    }

    /// <summary>
    /// Добавляет услугу в договор и пересчитывает итог.
    /// </summary>
    public void AddService(int contractId, int serviceId)
    {
        var service = _context.Services.Find(serviceId);
        if (service == null) return;

        _context.ContractServices.Add(new ContractService
        {
            ContractId = contractId,
            ServiceId = serviceId,
            Price = service.ServicePrice
        });
        _context.SaveChanges();

        RecalculateTotal(contractId);
    }

    /// <summary>
    /// Пересчитывает TotalAmount договора.
    /// </summary>
    public void RecalculateTotal(int contractId)
    {
        var total = _context.ContractServices
            .Where(cs => cs.ContractId == contractId)
            .Sum(cs => cs.Price);

        var contract = _context.Contracts.Find(contractId);
        if (contract != null)
        {
            contract.TotalAmount = total;
            _context.SaveChanges();
        }
    }

    /// <summary>
    /// Подписывает договор (перевод в статус Active).
    /// </summary>
    public bool Sign(int contractId)
    {
        var contract = _context.Contracts.Find(contractId);
        if (contract == null) return false;

        contract.ContractStatus = "Active";
        _context.SaveChanges();
        return true;
    }

    /// <summary>
    /// Полная информация о договоре (клиент, сотрудник, зал, услуги, счета).
    /// </summary>
    public Contract? GetFull(int contractId)
    {
        return _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.Employee)
            .Include(c => c.Hall)
            .Include(c => c.ContractServices)
                .ThenInclude(cs => cs.Service)
            .Include(c => c.Invoices)
            .Include(c => c.Schedules)
            .FirstOrDefault(c => c.Id == contractId);
    }
}