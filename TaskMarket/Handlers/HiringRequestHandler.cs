using ErrorOr;
using System.Collections.Generic;
using TaskMarket.IRepositories;
using TaskMarket.Models;
namespace TaskMarket.Handlers;

public sealed class HiringRequestHandler
{
    private readonly IHiringRequestRepository _hiringRequestRepository;

    public HiringRequestHandler(IHiringRequestRepository hiringRequestRepository)
    {
        _hiringRequestRepository = hiringRequestRepository;
    }

    // From HiringRequestsController: POST /api/hiring-requests
    public async Task<ErrorOr<int>> CreateAsync(HiringRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Error.Validation(
                code: "HiringRequest.Null",
                description: "Hiring request cannot be null.");
        }

        var rowsAffected = await _hiringRequestRepository.AddAsync(request, cancellationToken);
        if (rowsAffected == 0)
        {
            return Error.Failure(
                code: "HiringRequest.CreateFailed",
                description: "Failed to create the hiring request.");
        }
        return rowsAffected;
    }

    // From HiringRequestsController: GET /api/hiring-requests
    public async Task<ErrorOr<List<HiringRequest>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var hiringRequests = await _hiringRequestRepository.GetAllAsync(cancellationToken);

        if (hiringRequests is null)
        {
            return Error.Failure(
                code: "GettingAllHiringRequest.Null",
                description: "Failed to Get All  the hiring requests And returned Null"
                );
        }
        if(hiringRequests.Count == 0)
        {
            return Error.Failure(
                code: "GettingAllHiringRequest.Empty",
                description: "No hiring requests found.");
        }


        return hiringRequests;


    }

    // From HiringRequestsController: PUT /api/hiring-requests/{requestId}/status
    public async Task<ErrorOr<int>> UpdateStatusAsync(int requestId, string status, CancellationToken cancellationToken = default)
    {
        var HiringRequest= await _hiringRequestRepository.GetByIdAsync(requestId);
        if(HiringRequest is null)
        {
            return Error.Validation(
                code: "HiringRequest.NotFound",
                description: "Hiring request not found."
                );
        }
        //check if the provided status is valid
        if (!Enum.TryParse(status, true, out HiringRequestStatus newStatus))
        {
            return Error.Validation(
                code: "HiringRequest.InvalidStatus",
                description: "Invalid status value."
                );
        }
        //update the status of the hiring request
        HiringRequest.Status = newStatus;
        //finally, update the hiring request in the database
        var rowsAffected = await _hiringRequestRepository.Update(HiringRequest, cancellationToken);
        if (rowsAffected == 0)
        {
            return Error.Failure(
                code: "HiringRequest.UpdateFailed",
                description: "Failed to update the hiring request status.");
        }
        return rowsAffected;
    }
}