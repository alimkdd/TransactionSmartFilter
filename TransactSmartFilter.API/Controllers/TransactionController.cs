using Microsoft.AspNetCore.Mvc;
using TransactSmartFilter.Application.Dtos.Requests;
using TransactSmartFilter.Application.Dtos.Responses;
using TransactSmartFilter.Application.Interfaces;

namespace TransactSmartFilter.API.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("search")]
    public async Task<TransactionSearchResponse> Search([FromQuery] TransactionSearchRequest request)
        => await _transactionService.Search(request);


    [HttpGet("search/job/{jobId}")]
    public async Task<TransactionSearchResponse> Job(Guid jobId)
        => await _transactionService.Job(jobId);
}