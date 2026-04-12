using KitapTakasPlatformu.Data;
using KitapTakasPlatformu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitapTakasPlatformu.Controllers
{
    public class ExchangesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExchangesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> SendRequest(int requestedBookId)
        {
            var demoUser = await _userManager.FindByEmailAsync("demo@kitaptakas.com");
            if (demoUser == null)
                return Content("Demo kullanıcı bulunamadı.");

            var requestedBook = await _context.Books
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.Id == requestedBookId);

            if (requestedBook == null)
                return NotFound();

            var myBooks = await _context.Books
                .Where(b => b.OwnerId == demoUser.Id && b.IsAvailable)
                .ToListAsync();

            ViewBag.RequestedBook = requestedBook;
            return View(myBooks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(int requestedBookId, int offeredBookId)
        {
            var demoUser = await _userManager.FindByEmailAsync("demo@kitaptakas.com");
            if (demoUser == null)
                return Content("Demo kullanıcı bulunamadı.");

            var requestedBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == requestedBookId);
            var offeredBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == offeredBookId);

            if (requestedBook == null || offeredBook == null)
                return NotFound();

            if (requestedBook.OwnerId == demoUser.Id)
            {
                TempData["ErrorMessage"] = "Kendi kitabınıza takas isteği gönderemezsiniz.";
                return RedirectToAction("Details", "Books", new { id = requestedBookId });
            }

            var exists = await _context.ExchangeRequests.AnyAsync(x =>
                x.RequestedBookId == requestedBookId &&
                x.OfferedBookId == offeredBookId &&
                x.RequesterId == demoUser.Id &&
                x.Status == "Bekliyor");

            if (exists)
            {
                TempData["ErrorMessage"] = "Bu takas isteği zaten gönderilmiş.";
                return RedirectToAction("Details", "Books", new { id = requestedBookId });
            }

            var exchange = new ExchangeRequest
            {
                RequesterId = demoUser.Id,
                OwnerId = requestedBook.OwnerId,
                RequestedBookId = requestedBookId,
                OfferedBookId = offeredBookId,
                Status = "Bekliyor",
                RequestedAt = DateTime.Now
            };

            _context.ExchangeRequests.Add(exchange);

            _context.Notifications.Add(new Notification
            {
                UserId = requestedBook.OwnerId,
                Message = $"'{offeredBook.Title}' kitabı ile '{requestedBook.Title}' için takas isteği gönderildi.",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Takas isteği başarıyla gönderildi.";
            return RedirectToAction("Details", "Books", new { id = requestedBookId });
        }

        public async Task<IActionResult> IncomingRequests()
        {
            var demoUser = await _userManager.FindByEmailAsync("demo@kitaptakas.com");
            if (demoUser == null)
                return Content("Demo kullanıcı bulunamadı.");

            var requests = await _context.ExchangeRequests
                .Include(x => x.RequestedBook)
                .Include(x => x.OfferedBook)
                .Include(x => x.Requester)
                .Where(x => x.OwnerId == demoUser.Id)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> Accept(int id)
        {
            var request = await _context.ExchangeRequests
                .Include(x => x.RequestedBook)
                .Include(x => x.OfferedBook)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
                return NotFound();

            request.Status = "Kabul Edildi";
            request.RespondedAt = DateTime.Now;

            if (request.RequestedBook != null)
                request.RequestedBook.IsAvailable = false;

            if (request.OfferedBook != null)
                request.OfferedBook.IsAvailable = false;

            _context.Notifications.Add(new Notification
            {
                UserId = request.RequesterId,
                Message = "Takas isteğiniz kabul edildi.",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Takas isteği kabul edildi.";
            return RedirectToAction(nameof(IncomingRequests));
        }

        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.ExchangeRequests.FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
                return NotFound();

            request.Status = "Reddedildi";
            request.RespondedAt = DateTime.Now;

            _context.Notifications.Add(new Notification
            {
                UserId = request.RequesterId,
                Message = "Takas isteğiniz reddedildi.",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Takas isteği reddedildi.";
            return RedirectToAction(nameof(IncomingRequests));
        }
    }
}