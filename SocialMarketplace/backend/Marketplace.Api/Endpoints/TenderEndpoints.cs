using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Database;
using Marketplace.Database.Entities;
using Marketplace.Database.Enums;

namespace Marketplace.Api.Endpoints;

public static class TenderEndpoints
{
    public static void MapTenderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenders")
            .WithTags("Tenders")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context, MarketplaceDbContext db,
            [FromQuery] TenderStatus? status,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var skip = (page - 1) * pageSize;
            var query = db.Tenders.AsNoTracking().AsQueryable();
            if (status.HasValue) query = query.Where(t => t.Status == status.Value);
            var tenders = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip).Take(pageSize)
                .Select(t => new
                {
                    t.Id, t.TenderNumber, t.Title, t.Slug, t.Description,
                    t.Status, t.TenderType, t.EstimatedBudget, t.Currency,
                    t.PublishDate, t.SubmissionDeadline, t.Location,
                    t.BidCount, t.ViewCount, t.IsUrgent, t.IsFeatured, t.CreatedAt
                })
                .ToListAsync();
            var total = await query.CountAsync();
            return Results.Ok(new { items = tenders, total, page, pageSize });
        }).WithName("GetTenders").WithSummary("List tenders paginated with optional status filter");

        group.MapGet("/{id:guid}", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var tender = await db.Tenders.AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.Documents2)
                .Include(t => t.Award)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id, t.OrganizationId, t.OrganizationType, t.TenderNumber,
                    t.Title, t.Slug, t.Description, t.Scope, t.Requirements,
                    t.Eligibility, t.Status, t.TenderType, t.CategoryId,
                    CategoryName = t.Category.Name,
                    t.EstimatedBudget, t.Currency, t.PublishDate, t.SubmissionDeadline,
                    t.OpeningDate, t.AwardDate, t.ProjectStartDate, t.ProjectEndDate,
                    t.Location, t.ContactPerson, t.ContactEmail, t.ContactPhone,
                    t.DocumentFee, t.BidBond, t.PerformanceBond,
                    t.EvaluationCriteria, t.BidCount, t.ViewCount,
                    t.IsUrgent, t.IsFeatured, t.CreatedAt,
                    Documents = t.Documents2.Select(d => new
                    {
                        d.Id, d.Title, d.Description, d.Url, d.FileType,
                        d.FileSize, d.DocumentType, d.SortOrder, d.IsPublic, d.IsMandatory
                    }),
                    Award = t.Award != null ? new
                    {
                        t.Award.Id, t.Award.WinningBidId, t.Award.WinnerId,
                        t.Award.AwardedAmount, t.Award.AwardedAt, t.Award.AwardNotes
                    } : null
                })
                .FirstOrDefaultAsync();
            return tender == null ? Results.NotFound() : Results.Ok(tender);
        }).WithName("GetTenderById").WithSummary("Get tender by ID");

        group.MapPost("/", async ([FromBody] CreateTenderRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var tender = new Tender
            {
                OrganizationId = req.OrganizationId, OrganizationType = req.OrganizationType,
                TenderNumber = req.TenderNumber, Title = req.Title,
                Slug = req.Title.ToLower().Replace(" ", "-"),
                Description = req.Description, Scope = req.Scope,
                Requirements = req.Requirements, Eligibility = req.Eligibility,
                Status = TenderStatus.Draft, TenderType = req.TenderType ?? "Open",
                CategoryId = req.CategoryId, EstimatedBudget = req.EstimatedBudget,
                Currency = req.Currency ?? "USD",
                PublishDate = req.PublishDate ?? DateTime.UtcNow,
                SubmissionDeadline = req.SubmissionDeadline,
                Location = req.Location, ContactPerson = req.ContactPerson,
                ContactEmail = req.ContactEmail, ContactPhone = req.ContactPhone,
                DocumentFee = req.DocumentFee, BidBond = req.BidBond,
                PerformanceBond = req.PerformanceBond
            };
            tender.CreatedBy = userId;
            db.Tenders.Add(tender);
            await db.SaveChangesAsync();
            return Results.Created($"/api/tenders/{tender.Id}",
                new { tender.Id, tender.TenderNumber, tender.Title, tender.Status, tender.CreatedAt });
        }).WithName("CreateTender").WithSummary("Create a tender");

        group.MapPatch("/{id:guid}", async (Guid id, [FromBody] UpdateTenderRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var tender = await db.Tenders.FirstOrDefaultAsync(t => t.Id == id);
            if (tender == null) return Results.NotFound();

            if (req.Title != null) { tender.Title = req.Title; tender.Slug = req.Title.ToLower().Replace(" ", "-"); }
            if (req.Description != null) tender.Description = req.Description;
            if (req.Scope != null) tender.Scope = req.Scope;
            if (req.Requirements != null) tender.Requirements = req.Requirements;
            if (req.Eligibility != null) tender.Eligibility = req.Eligibility;
            if (req.Status.HasValue) tender.Status = req.Status.Value;
            if (req.EstimatedBudget.HasValue) tender.EstimatedBudget = req.EstimatedBudget;
            if (req.SubmissionDeadline.HasValue) tender.SubmissionDeadline = req.SubmissionDeadline.Value;
            if (req.Location != null) tender.Location = req.Location;
            tender.UpdatedBy = userId;

            await db.SaveChangesAsync();
            return Results.Ok(new { tender.Id, tender.Title, tender.Status, tender.UpdatedAt });
        }).WithName("UpdateTender").WithSummary("Update a tender");

        group.MapGet("/{id:guid}/bids", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var bids = await db.TenderBids.AsNoTracking()
                .Where(b => b.TenderId == id)
                .OrderByDescending(b => b.SubmittedAt)
                .Join(db.Users.AsNoTracking(), b => b.BidderId, u => u.Id,
                    (b, u) => new
                    {
                        b.Id, b.TenderId, b.BidderId, b.BidderType, b.BidNumber,
                        b.BidAmount, b.Currency, b.Status, b.ProposedDurationDays,
                        b.SubmittedAt, b.TechnicalScore, b.FinancialScore,
                        b.TotalScore, b.Rank,
                        Bidder = new { u.Id, u.FirstName, u.LastName, u.Username }
                    })
                .ToListAsync();
            return Results.Ok(bids);
        }).WithName("GetTenderBids").WithSummary("Get bids for a tender");

        group.MapPost("/{id:guid}/bids", async (Guid id, [FromBody] CreateTenderBidRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var tender = await db.Tenders.FirstOrDefaultAsync(t => t.Id == id);
            if (tender == null) return Results.NotFound();
            if (tender.Status != TenderStatus.Open && tender.Status != TenderStatus.Published)
                return Results.BadRequest(new { error = "Tender is not accepting bids" });
            if (DateTime.UtcNow > tender.SubmissionDeadline)
                return Results.BadRequest(new { error = "Submission deadline has passed" });

            var existingBid = await db.TenderBids.AsNoTracking()
                .AnyAsync(b => b.TenderId == id && b.BidderId == userId && b.Status != BidStatus.Withdrawn);
            if (existingBid) return Results.Conflict(new { error = "You already have an active bid for this tender" });

            var bid = new TenderBid
            {
                TenderId = id, BidderId = userId, BidderType = req.BidderType ?? "Individual",
                BidNumber = $"BID-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                BidAmount = req.BidAmount, Currency = tender.Currency,
                Status = BidStatus.Submitted,
                TechnicalProposal = req.TechnicalProposal,
                FinancialProposal = req.FinancialProposal,
                ExecutionPlan = req.ExecutionPlan,
                ProposedDurationDays = req.ProposedDurationDays,
                SubmittedAt = DateTime.UtcNow
            };
            bid.CreatedBy = userId;
            db.TenderBids.Add(bid);
            tender.BidCount++;
            await db.SaveChangesAsync();
            return Results.Created($"/api/tenders/{id}/bids/{bid.Id}",
                new { bid.Id, bid.BidNumber, bid.BidAmount, bid.Status, bid.SubmittedAt });
        }).WithName("SubmitTenderBid").WithSummary("Submit a bid for a tender");

        group.MapPost("/{id:guid}/award", async (Guid id, [FromBody] AwardTenderRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var tender = await db.Tenders.FirstOrDefaultAsync(t => t.Id == id);
            if (tender == null) return Results.NotFound();

            var bid = await db.TenderBids.FirstOrDefaultAsync(b => b.Id == req.BidId && b.TenderId == id);
            if (bid == null) return Results.BadRequest(new { error = "Bid not found for this tender" });

            var award = new TenderAward
            {
                TenderId = id, WinningBidId = bid.Id, WinnerId = bid.BidderId,
                AwardedAmount = bid.BidAmount, Currency = tender.Currency,
                AwardedAt = DateTime.UtcNow, AwardNotes = req.Notes, AwardedBy = userId
            };
            award.CreatedBy = userId;
            db.TenderAwards.Add(award);

            tender.Status = TenderStatus.Awarded;
            tender.AwardDate = DateTime.UtcNow;
            bid.Status = BidStatus.Accepted;

            await db.SaveChangesAsync();
            return Results.Ok(new { award.Id, award.TenderId, award.WinningBidId, award.WinnerId, award.AwardedAmount, award.AwardedAt });
        }).WithName("AwardTender").WithSummary("Award a tender contract to a bid");
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : throw new UnauthorizedAccessException();
    }
}

public record CreateTenderRequest(Guid OrganizationId, string OrganizationType,
    string TenderNumber, string Title, string? Description, string? Scope,
    string? Requirements, string? Eligibility, Guid CategoryId,
    decimal? EstimatedBudget, string? Currency, DateTime? PublishDate,
    DateTime SubmissionDeadline, string? TenderType, string? Location,
    string? ContactPerson, string? ContactEmail, string? ContactPhone,
    decimal? DocumentFee = null, decimal? BidBond = null, decimal? PerformanceBond = null);
public record UpdateTenderRequest(string? Title = null, string? Description = null,
    string? Scope = null, string? Requirements = null, string? Eligibility = null,
    TenderStatus? Status = null, decimal? EstimatedBudget = null,
    DateTime? SubmissionDeadline = null, string? Location = null);
public record CreateTenderBidRequest(decimal BidAmount, string? BidderType = null,
    string? TechnicalProposal = null, string? FinancialProposal = null,
    string? ExecutionPlan = null, int? ProposedDurationDays = null);
public record AwardTenderRequest(Guid BidId, string? Notes = null);