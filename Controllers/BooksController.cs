using KitapTakasPlatformu.Data;
using KitapTakasPlatformu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitapTakasPlatformu.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var books = await _context.Books
                .Include(b => b.Owner)
                .Where(b => b.IsAvailable)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = NormalizeText(search);

                books = books.Where(b =>
                    NormalizeText(b.Title).Contains(normalizedSearch) ||
                    NormalizeText(b.Author).Contains(normalizedSearch) ||
                    NormalizeText(b.Category ?? "").Contains(normalizedSearch))
                    .ToList();
            }

            ViewBag.Search = search;
            return View(books);
        }

        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text
                .Trim()
                .ToLowerInvariant()
                .Replace("ı", "i")
                .Replace("ş", "s")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ö", "o")
                .Replace("ç", "c");
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        public IActionResult Create()
        {
            return View(new Book());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            ModelState.Remove("OwnerId");
            ModelState.Remove("Owner");

            if (!ModelState.IsValid)
                return View(book);

            var demoUser = await _userManager.FindByEmailAsync("demo@kitaptakas.com");
            if (demoUser == null)
            {
                ModelState.AddModelError("", "Demo kullanıcı bulunamadı.");
                return View(book);
            }

            book.OwnerId = demoUser.Id;
            book.CreatedAt = DateTime.Now;
            book.IsAvailable = true;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kitap başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MyBooks()
        {
            var demoUser = await _userManager.FindByEmailAsync("demo@kitaptakas.com");
            if (demoUser == null)
                return Content("Demo kullanıcı bulunamadı.");

            var books = await _context.Books
                .Where(b => b.OwnerId == demoUser.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(books);
        }
    }
}